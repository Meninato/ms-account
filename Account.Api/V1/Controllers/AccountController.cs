using Account.Api.Core;
using Account.Api.V1.Models.Requests;
using Account.Api.V1.Services;
using Account.Jwt.Options;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.Results;
using MassTransit.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Account.Api.V1.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:apiVersion}/accounts")]
public class AccountController : ControllerBase
{
    private readonly IValidator<AuthenticateRequest> _authReqValidator;
    private readonly IValidator<RegisterAccountRequest> _registerReqValidator;
    private readonly IAccountService _accountService;
    private readonly JwtOptions _jwtOptions;

    public AccountController(IValidator<AuthenticateRequest> authReqValidator,
    IValidator<RegisterAccountRequest> registerReqValidator,
    IAccountService accountService,
    IOptions<JwtOptions> jwtOptions)
    {
        _authReqValidator = authReqValidator;
        _registerReqValidator = registerReqValidator;
        _accountService = accountService;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("authenticate")]
    public async Task<IResult> Authenticate(AuthenticateRequest request)
    {
        ValidationResult validationResult = await _authReqValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new AppException(validationResult.ToDictionary());
        }

        var response = await _accountService.AuthenticateAsync(request, IpAddress());
        SetTokenCookie(response.RefreshToken);
        return Results.Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IResult> RefreshToken()
    {
        Request.Cookies.TryGetValue("refreshToken", out string? refreshToken);
        var response = await _accountService.RefreshTokenAsync(refreshToken, IpAddress());
        SetTokenCookie(response.RefreshToken);
        return Results.Ok(response);
    }

    [HttpPost("revoke-token")]
    public async Task<IResult> RevokeToken(RevokeTokenRequest? request)
    {
        // accept token from request body or cookie
        string? token = request?.Token;
        if (string.IsNullOrEmpty(token))
        {
            Request.Cookies.TryGetValue("refreshToken", out token);
        }

        await _accountService.RevokeTokenAsync(token, IpAddress());

        return Results.Ok(new MessageResponse() { Message = "Token revoked" });
    }

    [HttpPost("register")]
    public async Task<IResult> Register(RegisterAccountRequest request)
    {
        ValidationResult validationResult = await _registerReqValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new AppException(validationResult.ToDictionary());
        }

        await _accountService.RegisterAsync(request, Request.Headers["origin"]);

        return Results.Ok(new MessageResponse()
        {
            Message = "If that email is not in use, a confirmation link has been sent."
        });
    }

    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshExpires)
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string? IpAddress()
    {
        string? ip;

        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            ip = Request.Headers["X-Forwarded-For"];
        else
            ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

        return ip;
    }
}