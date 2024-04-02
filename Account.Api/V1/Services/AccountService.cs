using Account.Api.V1.Models.Requests;
using Account.Api.V1.Models.Responses;
using Account.Jwt.Options;
using Account.Jwt;
using AutoMapper;
using Account.Data;
using MassTransit;
using Microsoft.Extensions.Options;
using Account.Api.Core;
using Account.Data.Entities;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Account.Api.V1.Services;

public interface IAccountService
{
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, string? ipAddress);
    Task<AuthenticateResponse> RefreshTokenAsync(string? token, string? ipAddress);
    Task RevokeTokenAsync(string? token, string? ipAddress);
    Task RegisterAsync(RegisterAccountRequest request, string? origin);
}

public class AccountService : IAccountService
{
    private readonly DataContext _context;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IMapper _mapper;
    private readonly JwtOptions _jwtOptions;
    private readonly IPublishEndpoint _publishEndpoint;

    public AccountService(DataContext dbContext,
    IJwtTokenProvider tokenProvider,
    IMapper mapper,
    IOptions<JwtOptions> jwtOptions,
    IPublishEndpoint publishEndpoint)
    {
        _context = dbContext;
        _jwtTokenProvider = tokenProvider;
        _mapper = mapper;
        _jwtOptions = jwtOptions.Value;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string? ipAddress)
    {
        var account = await _context.Accounts.Include(acc => acc.Roles)
            .SingleOrDefaultAsync(x => x.Email == model.Email);

        // validate
        if (account == null || !account.IsVerified || !BCryptNet.Verify(model.Password, account.PasswordHash))
            throw new AppException("Email or password is wrong", StatusCodes.Status401Unauthorized);

        // authentication successful so generate jwt and refresh tokens
        string id = account.Id.ToString();
        string[] roles = account.Roles.Select(role => role.Name).ToArray();
        var jwtToken = _jwtTokenProvider.GenerateJwtToken(id, roles);

        var refreshToken = await GenerateRefreshToken(ipAddress);
        account.RefreshTokens.Add(refreshToken);

        // remove old refresh tokens from account
        RemoveOldRefreshTokens(account);

        // save changes to db
        _context.Update(account);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;
        response.RefreshToken = refreshToken.Token;
        return response;
    }

    public async Task<AuthenticateResponse> RefreshTokenAsync(string? token, string? ipAddress)
    {
        var account = await GetAccountByRefreshToken(token);
        var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            RevokeDescendantRefreshTokens(refreshToken, account, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
            _context.Update(account);
            await _context.SaveChangesAsync();
        }

        if (!refreshToken.IsActive)
            throw new AppException("Invalid Token");

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = await RotateRefreshToken(refreshToken, ipAddress);
        account.RefreshTokens.Add(newRefreshToken);

        // remove old refresh tokens from account
        RemoveOldRefreshTokens(account);

        // save changes to db
        _context.Update(account);
        await _context.SaveChangesAsync();

        // generate new jwt
        string id = account.Id.ToString();
        string[] roles = account.Roles.Select(role => role.Name).ToArray();
        var jwtToken = _jwtTokenProvider.GenerateJwtToken(id, roles);

        // return data in authenticate response object
        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;
        response.RefreshToken = newRefreshToken.Token;
        return response;
    }

    public async Task RevokeTokenAsync(string? token, string? ipAddress)
    {
        if (string.IsNullOrEmpty(token))
            throw new AppException("Token is required and cannot be empty");

        var account = await GetAccountByRefreshToken(token);
        var adminRole = await GetAdminRole();

        // users can revoke their own tokens and admins can revoke any tokens
        if (!account.OwnsToken(token) && !account.Roles.Any(r => r.Id == adminRole.Id))
            throw new AppException("Forbidden", StatusCodes.Status403Forbidden);

        var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        // revoke token and save
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        _context.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task RegisterAsync(RegisterAccountRequest request, string? origin)
    {
        // validate if e-mail exists and prevent account enumeration 
        var emailExists = await _context.Accounts.AnyAsync(x => x.Email == request.Email);
        if (emailExists) return;

        var roles = await _context.Roles
            .Where(role => request.Roles.Any(addRole => addRole.ToLower() == role.Name.ToLower()))
            .ToListAsync();

        var account = _mapper.Map<UserAccount>(request);

        account.Roles = roles;
        account.Created = DateTime.UtcNow;
        account.VerificationToken = await GenerateVerificationToken();

        // hash password
        account.PasswordHash = BCryptNet.HashPassword(request.Password);

        // save account
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // send email
        //await PublishEmailAsync(account, origin);
    }

    private async Task<Role> GetAdminRole()
    {
        return await _context.Roles.SingleAsync(r => r.Name.ToLower() == "admin");
    }

    //private async Task PublishEmailAsync(UserAccount account, string? origin)
    //{
    //    //TODO: logic on event drive
    //    await _publishEndpoint.Publish<AccountCreated>(new AccountCreated()
    //    {
    //        Name = account.FirstName,
    //        VerificationToken = account.VerificationToken!
    //    });
    //}

    private async Task<string> GenerateVerificationToken()
    {
        // token is a cryptographically strong random sequence of values
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // ensure token is unique by checking against db
        var tokenIsUnique = !await _context.Accounts.AnyAsync(x => x.VerificationToken == token);
        if (!tokenIsUnique)
            return await GenerateVerificationToken();

        return token;
    }

    private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, UserAccount account, string? ipAddress, string reason)
    {
        // recursively traverse the refresh token chain and ensure all descendants are revoked
        if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            var childToken = account.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
            if (childToken != null)
            {
                if (childToken.IsActive)
                    RevokeRefreshToken(childToken, ipAddress, reason);
                else
                    RevokeDescendantRefreshTokens(childToken, account, ipAddress, reason);
            }
        }
    }

    private void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }

    private async Task<RefreshToken> RotateRefreshToken(RefreshToken refreshToken, string? ipAddress)
    {
        var newRefreshToken = await GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private async Task<RefreshToken> GenerateRefreshToken(string? ipAddress)
    {
        var jwtRefreshToken = _jwtTokenProvider.GenerateRefreshToken(ipAddress);

        // ensure token is unique by checking against db
        var tokenIsUnique = !await _context.Accounts.AnyAsync(a => a.RefreshTokens.Any(t => t.Token == jwtRefreshToken.Token));

        if (!tokenIsUnique)
            return await GenerateRefreshToken(ipAddress);

        return _mapper.Map<RefreshToken>(jwtRefreshToken);
    }

    private void RemoveOldRefreshTokens(UserAccount account)
    {
        account.RefreshTokens.RemoveAll(x =>
            !x.IsActive &&
            x.Created.AddDays(_jwtOptions.RefreshExpires) <= DateTime.UtcNow);
    }

    private async Task<UserAccount> GetAccountByRefreshToken(string? token)
    {
        var account = await _context.Accounts.Include(acc => acc.Roles)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (account == null) throw new AppException("Invalid token");
        return account;
    }
}
