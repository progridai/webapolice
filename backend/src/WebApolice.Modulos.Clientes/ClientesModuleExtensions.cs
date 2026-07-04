using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.AtivarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.ConsultarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.InativarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;

namespace WebApolice.Modulos.Clientes;

public static class ClientesModuleExtensions
{
    public static IServiceCollection AddClientesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ClientesDbContext>((sp, options) =>
        {
            var connection = sp.GetRequiredService<System.Data.Common.DbConnection>();
            options.UseNpgsql(connection, npgsqlOptions => 
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "clientes");
            }).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IClientesRepository, ClientesRepository>();
        services.AddScoped<IClientesTransactionManager, ClientesTransactionManager>();

        // Handlers
        services.AddScoped<CadastrarClienteHandler>();
        services.AddScoped<AlterarClienteHandler>();
        services.AddScoped<ConsultarClientePorIdHandler>();
        services.AddScoped<ListarClientesHandler>();
        services.AddScoped<AtivarClienteHandler>();
        services.AddScoped<InativarClienteHandler>();

        return services;
    }
}
