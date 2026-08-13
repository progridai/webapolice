using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Estipulantes.Application.Ports;
using WebApolice.Modulos.Estipulantes.Application.UseCases.ConsultarEstipulante;
using WebApolice.Modulos.Estipulantes.Application.UseCases.ConsultarEstipulanteConfiguracao;
using WebApolice.Modulos.Estipulantes.Application.UseCases.CriarEstipulante;
using WebApolice.Modulos.Estipulantes.Application.UseCases.AtualizarEstipulante;
using WebApolice.Modulos.Estipulantes.Application.UseCases.InativarEstipulante;
using WebApolice.Modulos.Estipulantes.Application.UseCases.ExcluirEstipulante;
using WebApolice.Modulos.Estipulantes.Application.UseCases.ReativarEstipulante;
using WebApolice.Modulos.Estipulantes.Application.UseCases.ListarEstipulantes;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Repositories;

namespace WebApolice.Modulos.Estipulantes;

public static class EstipulantesModuleExtensions
{
    public static IServiceCollection AddEstipulantesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EstipulantesDbContext>((sp, options) =>
        {
            var connection = sp.GetRequiredService<System.Data.Common.DbConnection>();
            options.UseNpgsql(connection, npgsqlOptions => 
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "cadastro");
            }).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IEstipulantesQueries, EstipulantesQueries>();
        services.AddScoped<IEstipulanteRepository, EstipulanteRepository>();

        services.AddScoped<CriarEstipulanteHandler>();
        services.AddScoped<AtualizarEstipulanteHandler>();
        services.AddScoped<InativarEstipulanteHandler>();
        services.AddScoped<ExcluirEstipulanteHandler>();
        services.AddScoped<ReativarEstipulanteHandler>();
        services.AddScoped<ListarEstipulantesHandler>();
        services.AddScoped<ConsultarEstipulantePorIdHandler>();
        services.AddScoped<ConsultarEstipulanteConfiguracaoHandler>();
        services.AddScoped<WebApolice.Modulos.Estipulantes.Application.UseCases.CriarEstipulante.CriarEstipulanteHandler>();
        
        return services;
    }
}
