using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Financeiro.Application.Ports;
using WebApolice.Modulos.Financeiro.Application.UseCases.ConveniosCobranca;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.Modulos.Financeiro.Infrastructure;
using WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence;
namespace WebApolice.Modulos.Financeiro;

public static class FinanceiroModuleExtensions
{
    public static IServiceCollection AddFinanceiroModule(this IServiceCollection services)
    {
        services.AddDbContext<FinanceiroDbContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<DbConnection>(), o => o.MigrationsHistoryTable("__EFMigrationsHistory", "financeiro")).UseSnakeCaseNamingConvention());
        services.AddScoped<ConveniosCobrancaRepository>();
        services.AddScoped<IConveniosCobrancaCadastro>(sp => sp.GetRequiredService<ConveniosCobrancaRepository>());
        services.AddScoped<IConveniosCobrancaConsulta>(sp => sp.GetRequiredService<ConveniosCobrancaRepository>());
        services.AddScoped<ConveniosCobrancaHandler>();
        return services;
    }
}
