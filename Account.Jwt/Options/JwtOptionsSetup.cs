using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Jwt.Options;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    public JwtOptionsSetup() { }

    public void Configure(JwtOptions options)
    {
        int minutes;
        int days;
        string? secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        string? issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        string? audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        string? expires = Environment.GetEnvironmentVariable("JWT_EXPIRES");
        string? refreshExpires = Environment.GetEnvironmentVariable("JWT_REFEXPIRES");

        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentNullException("Missing JWT_SECRET");
        }

        if (string.IsNullOrEmpty(issuer))
        {
            throw new ArgumentNullException("Missing JWT_ISSUER");
        }

        if (string.IsNullOrEmpty(audience))
        {
            throw new ArgumentNullException("Missing JWT_AUDIENCE");
        }

        if (string.IsNullOrEmpty(expires))
        {
            throw new ArgumentNullException("Missing JWT_EXPIRES");
        }

        if (string.IsNullOrEmpty(refreshExpires))
        {
            throw new ArgumentNullException("Missing JWT_REFEXPIRES");
        }

        if (int.TryParse(expires, out minutes) == false || minutes <= 0)
        {
            minutes = 15;
        }

        if (int.TryParse(refreshExpires, out days) == false || days <= 0)
        {
            days = 7;
        }

        options.Secret = secret;
        options.Issuer = issuer;
        options.Audience = audience;
        options.Expires = minutes;
        options.RefreshExpires = days;
    }
}