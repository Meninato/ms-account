using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Jwt;

public class JwtTokenBearerEvents : JwtBearerEvents
{
    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
        {
            context.Response.Headers.Append("Is-Token-Expired", "true");
        }
        return Task.CompletedTask;
    }
}