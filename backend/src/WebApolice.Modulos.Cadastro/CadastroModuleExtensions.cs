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
        services.AddScoped<ICooperadoRepository, CooperadoRepository>();
        services.AddScoped<ICooperadosQueries, CooperadosQueries>();
        services.AddScoped<ICadastroTransactionManager, CadastroTransactionManager>();
        
        services.AddScoped<IEstipulantesQueries, EstipulantesQueries>();
        services.AddScoped<IEstipulanteRepository, EstipulanteRepository>();
        services.AddScoped<ISeguradorasQueries, SeguradorasQueries>();
        services.AddScoped<ISeguradoraRepository, SeguradoraRepository>();
        
        services.AddScoped<ICorretorasQueries, CorretorasQueries>();
        services.AddScoped<ICorretoraRepository, CorretoraRepository>();

        services.AddScoped<ISubestipulantesQueries, SubestipulantesQueries>();
        services.AddScoped<ISubestipulanteRepository, SubestipulanteRepository>();

        // Clientes Handlers
        services.AddScoped<CadastrarClienteHandler>();
        services.AddScoped<AlterarClienteHandler>();
        services.AddScoped<ConsultarClientePorIdHandler>();
        services.AddScoped<ListarClientesHandler>();
        services.AddScoped<AtivarClienteHandler>();
        services.AddScoped<InativarClienteHandler>();
        
        // Cooperados Handlers
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCooperado.CadastrarCooperadoHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCooperado.AlterarCooperadoHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCooperado.ConsultarCooperadoHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ListarCooperados.ListarCooperadosHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.AtivarCooperado.AtivarCooperadoHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.InativarCooperado.InativarCooperadoHandler>();
        
        // Estipulantes Handlers
        services.AddScoped<CriarEstipulanteHandler>();
        services.AddScoped<AtualizarEstipulanteHandler>();
        services.AddScoped<InativarEstipulanteHandler>();
        services.AddScoped<ExcluirEstipulanteHandler>();
        services.AddScoped<ReativarEstipulanteHandler>();
        services.AddScoped<ListarEstipulantesHandler>();
        services.AddScoped<ConsultarEstipulantePorIdHandler>();
        services.AddScoped<ConsultarEstipulanteConfiguracaoHandler>();

        // Seguradoras Handlers
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.CriarSeguradora.CriarSeguradoraHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSeguradora.AlterarSeguradoraHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.InativarSeguradora.InativarSeguradoraHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ReativarSeguradora.ReativarSeguradoraHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ListarSeguradoras.ListarSeguradorasHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSeguradora.ConsultarSeguradoraPorIdHandler>();

        // Corretoras Handlers
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.CriarCorretora.CriarCorretoraHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCorretora.AlterarCorretoraHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.InativarCorretora.InativarCorretoraHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ReativarCorretora.ReativarCorretoraHandler>();

        // Subestipulantes Handlers
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.CriarSubestipulante.CriarSubestipulanteHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSubestipulante.AlterarSubestipulanteHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.InativarSubestipulante.InativarSubestipulanteHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ReativarSubestipulante.ReativarSubestipulanteHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ListarSubestipulantes.ListarSubestipulantesHandler>();
        services.AddScoped<WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSubestipulante.ConsultarSubestipulantePorIdHandler>();

        return services;
    }
}
