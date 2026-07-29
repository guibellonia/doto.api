using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Doto.Infrastructure.Persistence;

public class DotoDbContextFactory : IDesignTimeDbContextFactory<DotoDbContext>
{
    private const string ApiUserSecretsId = "89934851-f4dc-4539-828f-5b78b95cbaa3";

    private const string ConnectionStringName = "DotoDb";

    public DotoDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"A connection string '{ConnectionStringName}' não foi encontrada. Configure-a com:\n" +
                $"  dotnet user-secrets set \"ConnectionStrings:{ConnectionStringName}\" \"<conn>\" --project src/Doto.Api\n" +
                $"ou exporte ConnectionStrings__{ConnectionStringName}. " +
                "Use o Session pooler do Supabase (porta 5432), não o Transaction pooler (6543).");
        }

        var options = new DbContextOptionsBuilder<DotoDbContext>()
            .UseNpgsql(connectionString, npg => npg.MigrationsAssembly(typeof(DotoDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DotoDbContext(options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var apiProjectPath = ResolveApiProjectPath();

        var builder = new ConfigurationBuilder().AddEnvironmentVariables();

        if (apiProjectPath is not null)
        {
            builder
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true);
        }

        builder.AddUserSecrets(ApiUserSecretsId);

        return builder.Build();
    }

    private static string? ResolveApiProjectPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Doto.Api");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            if (directory.Name == "Doto.Api")
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
