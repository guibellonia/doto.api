using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Doto.Application.Interfaces;
using Doto.Domain.Interfaces;
using Doto.Infrastructure.Auth;
using Doto.Infrastructure.Persistence;
using Doto.Infrastructure.Repositories;

namespace Doto.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DotoDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddSingleton(sp =>
        {
            var url = config["Supabase:Url"];
            var key = config["Supabase:ServiceRoleKey"]; 

            var options = new Supabase.Gotrue.ClientOptions
            {
                Url = $"{url}/auth/v1",
                Headers = new Dictionary<string, string> { { "apikey", key! } }
            };

            return new Supabase.Gotrue.AdminClient(options.ToString());
        });


        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IMedicineRepository, MedicineRepository>(); 
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IAdminAuthService, SupabaseAdminService>();
        services.AddScoped<IMedicineDoseOccurrenceRepository, MedicineDoseOccurrenceRepository>();
        services.AddScoped<IVitalSignRecordRepository, VitalSignRecordRepository>();
        services.AddScoped<ISymptomRecordRepository, SymptomRecordRepository>();

        return services;
    }
}
