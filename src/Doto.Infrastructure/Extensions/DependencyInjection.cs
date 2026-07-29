using Doto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Doto.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DotoDb");

        services.AddDbContext<DotoDbContext>(options => options
            .UseNpgsql(
                connectionString,
                npg => npg.MigrationsAssembly(typeof(DotoDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention());

        return services;
    }
}
