using Account.Jwt.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Jwt;

public class JwtTokenValidationParameter : TokenValidationParameters
{
    public JwtTokenValidationParameter(JwtOptions options)
    {
        ValidateIssuer = true;
        ValidateAudience = true;
        ValidateIssuerSigningKey = true;
        ValidateLifetime = true;
        ValidIssuer = options.Issuer;
        ValidAudience = options.Audience;
        ClockSkew = TimeSpan.Zero;
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.Secret));
    }
}
