using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Jwt.Options;

public class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;
    private readonly IServiceProvider _serviceProvider;

    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions, IServiceProvider serviceProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _serviceProvider = serviceProvider;
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(JwtBearerDefaults.AuthenticationScheme, options);
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        IHostEnvironment? hostEnv = _serviceProvider.GetService<IHostEnvironment>();

        options.RequireHttpsMetadata = hostEnv != null && hostEnv.IsProduction() ? true : false;
        options.TokenValidationParameters = new JwtTokenValidationParameter(_jwtOptions);
        options.Events = new JwtTokenBearerEvents();
    }
}