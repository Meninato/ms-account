using Account.Jwt.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Jwt;

public static class JwtAuthenticatorExtension
{

    public static void AddJwtAuthenticator(this IServiceCollection services)
    {
        AddJwtTokenProvider(services);
        services.ConfigureOptions<JwtBearerOptionsSetup>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();
    }

    public static void AddJwtTokenProvider(this IServiceCollection services)
    {
        AddJwtOptions(services);
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
    }

    public static void AddJwtOptions(this IServiceCollection services)
    {
        services.ConfigureOptions<JwtOptionsSetup>();
    }
}