using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApolice;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;
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

        // Handlers - Apólice
        services.AddScoped<ListarApolicesHandler>();
        services.AddScoped<CriarApoliceHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApolice.AlterarApoliceHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ObterApoliceHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ListarApoliceVidasHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ObterApoliceUniversoPermitidoHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ListarApoliceSubestipulantesHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ListarApoliceHistoricoHandler>();
        
        services.AddScoped<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularRamo.VincularRamoApoliceHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarRamo.AtualizarRamoApoliceHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarRamo.InativarRamoApoliceHandler>();

        // Handlers - Ramo
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ListarRamos.ListarRamosHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ObterRamo.ObterRamoHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Ramos.CriarRamo.CriarRamoHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarRamo.AlterarRamoHandler>();
        services.AddScoped<WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarStatusRamo.AlterarStatusRamoHandler>();

        return services;
    }
}
