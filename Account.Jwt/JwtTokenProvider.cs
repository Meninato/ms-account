using Account.Jwt.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Account.Jwt;

public interface IJwtTokenProvider
{
    string GenerateJwtToken(string id, params string[] roles);
    bool ValidateJwtToken(string? token);
    Task<bool> ValidateJwtTokenAsync(string? token);
    JwtRefreshToken GenerateRefreshToken(string? ipAddress);
}

public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenProvider(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateJwtToken(string id, params string[] roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaim(new Claim("pst", id));
        foreach (string role in roles)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.Expires),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateJwtToken(string? token)
    {
        if (token == null)
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, new JwtTokenValidationParameter(_jwtOptions),
                out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateJwtTokenAsync(string? token)
    {
        if (token == null)
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenResult = await tokenHandler.ValidateTokenAsync(token, new JwtTokenValidationParameter(_jwtOptions));

        return tokenResult.IsValid;
    }

    public JwtRefreshToken GenerateRefreshToken(string? ipAddress)
    {
        var refreshToken = new JwtRefreshToken
        {
            // token is a cryptographically strong random sequence of values
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshExpires),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        return refreshToken;
    }
}
