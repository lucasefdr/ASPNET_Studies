using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Studies.Domain.Interfaces;
using Studies.Infrastructure.Persistence;
using Studies.Infrastructure.Persistence.Context;
using Studies.Infrastructure.Persistence.Repositories;

namespace Studies.Infrastructure.DependencyInjection;

public static class InfrastructureCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("SQLite"), sqlOptions =>
           {
               // Retry automático para falhas transitórias (ex: SQL Server reiniciando)
               //sqlOptions.EnableRetryOnFailure(
               //    maxRetryCount: 3,
               //    maxRetryDelay: TimeSpan.FromSeconds(5),
               //    errorNumbersToAdd: null);

               // Tempo máximo de um comando SQL antes de timeout
               sqlOptions.CommandTimeout(60);

               // Melhora performance em queries com múltiplos resultsets
               sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
           });

            // Em desenvolvimento, loga as queries geradas — NUNCA em produção (log sensível)
            //if (configuration["ASPNETCORE_ENVIRONMENT"] == "Development")
            //    options.EnableSensitiveDataLogging().EnableDetailedErrors();
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>()
                .AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
