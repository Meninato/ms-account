using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Data;

public static class AccountDataExtension
{
    public static void AddAccountDbContext(this IServiceCollection services)
    {
        services.AddDbContext<DataContext>();
        services.AddScoped<IDbSeeder, DbSeeder>();
    }
}
