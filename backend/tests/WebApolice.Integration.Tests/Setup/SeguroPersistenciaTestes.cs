using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Integration.Tests.Setup;

internal static class SeguroPersistenciaTestes
{
    public static ICadastroSeguroQueries ConsultasCadastro(SeguroDbContext db) => new CadastroSeguroQueries(
        new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql(db.Database.GetDbConnection()).UseSnakeCaseNamingConvention().Options));
}
