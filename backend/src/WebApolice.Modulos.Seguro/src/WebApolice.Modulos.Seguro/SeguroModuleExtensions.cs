using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApolice;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro;

public static class SeguroModuleExtensions
{
    public static IServiceCollection AddSeguroModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql");
        
        services.AddDbContext<SeguroDbContext>(options =>
            options.UseNpgsql(connectionString, o => 
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "seguro");
            })
            .UseSnakeCaseNamingConvention());

        // Repositories & Queries
        services.AddScoped<IApoliceRepository, ApoliceRepository>();
        services.AddScoped<IApolicesQueries, ApolicesQueries>();

        // UseCases / Handlers
        services.AddScoped<CriarApoliceHandler>();

        return services;
    }
}
