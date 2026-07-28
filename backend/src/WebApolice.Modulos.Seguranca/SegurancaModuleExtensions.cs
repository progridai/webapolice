using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.Services;
using WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;
using WebApolice.Modulos.Seguranca.Infrastructure.Authentication;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;

namespace WebApolice.Modulos.Seguranca;

public static class SegurancaModuleExtensions
{
    public static IServiceCollection AddModuloSeguranca(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SegurancaDbContext>((sp, options) =>
        {
            var connection = sp.GetRequiredService<System.Data.Common.DbConnection>();
            options.UseNpgsql(connection, npgsqlOptions => 
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "seguranca");
            }).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPermissoesEfetivasService, PermissoesEfetivasService>();
        services.AddScoped<IAcessoOperadorSistemaService, AcessoOperadorSistemaService>();
        services.AddScoped<IContextoUsuarioAutenticado, ContextoUsuarioAutenticado>();
        services.AddScoped<IUsuarioProvisionamentoRepository, UsuarioProvisionamentoRepository>();
        services.AddScoped<IProvisionamentoUsuarioService, ProvisionamentoUsuarioService>();
        services.AddScoped<ProvisionamentoUsuarioMiddleware>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissaoPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissaoAuthorizationHandler>();

        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios.ListarUsuariosUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios.ObterUsuarioUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios.CriarUsuarioUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios.AtualizarUsuarioUseCase>();

        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Perfis.ListarPerfisUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Perfis.ObterPerfilUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Perfis.CriarPerfilUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Perfis.AtualizarPerfilUseCase>();

        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Catalogo.ListarCatalogoUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Catalogo.ListarPermissoesUseCase>();

        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Auditoria.ListarAuditoriaUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Auditoria.ObterAuditoriaUseCase>();

        services.AddScoped<ListarModulosUseCase>();
        services.AddScoped<AlterarHabilitacaoModuloUseCase>();
        services.AddScoped<WebApolice.Modulos.Seguranca.Application.UseCases.Me.ObterUsuarioAutenticadoUseCase>();

        services.Configure<WebApolice.Modulos.Seguranca.Infrastructure.Keycloak.KeycloakAdminOptions>(
            configuration.GetSection(WebApolice.Modulos.Seguranca.Infrastructure.Keycloak.KeycloakAdminOptions.SectionName));

        services.AddHttpClient<IKeycloakUsuariosAdminClient, WebApolice.Modulos.Seguranca.Infrastructure.Keycloak.KeycloakUsuariosAdminClient>();

        return services;
    }
}
