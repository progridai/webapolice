using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.AtivarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulanteConfiguracao;
using WebApolice.Modulos.Cadastro.Application.UseCases.CriarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.AtualizarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ExcluirEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ReativarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarEstipulantes;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

namespace WebApolice.Modulos.Cadastro;

public static class CadastroModuleExtensions
{
    public static IServiceCollection AddCadastroModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CadastroDbContext>((sp, options) =>
        {
            var connection = sp.GetRequiredService<System.Data.Common.DbConnection>();
            options.UseNpgsql(connection, npgsqlOptions => 
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "cadastro");
            }).UseSnakeCaseNamingConvention();
        });

        // Ports / Repositories
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IClientesQueries, ClientesQueries>();
        services.AddScoped<ICadastroTransactionManager, CadastroTransactionManager>();
        
        services.AddScoped<IEstipulantesQueries, EstipulantesQueries>();
        services.AddScoped<IEstipulanteRepository, EstipulanteRepository>();

        // Clientes Handlers
        services.AddScoped<CadastrarClienteHandler>();
        services.AddScoped<AlterarClienteHandler>();
        services.AddScoped<ConsultarClientePorIdHandler>();
        services.AddScoped<ListarClientesHandler>();
        services.AddScoped<AtivarClienteHandler>();
        services.AddScoped<InativarClienteHandler>();
        
        // Estipulantes Handlers
        services.AddScoped<CriarEstipulanteHandler>();
        services.AddScoped<AtualizarEstipulanteHandler>();
        services.AddScoped<InativarEstipulanteHandler>();
        services.AddScoped<ExcluirEstipulanteHandler>();
        services.AddScoped<ReativarEstipulanteHandler>();
        services.AddScoped<ListarEstipulantesHandler>();
        services.AddScoped<ConsultarEstipulantePorIdHandler>();
        services.AddScoped<ConsultarEstipulanteConfiguracaoHandler>();

        return services;
    }
}
