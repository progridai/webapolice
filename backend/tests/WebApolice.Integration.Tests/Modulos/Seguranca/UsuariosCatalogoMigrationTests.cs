using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public sealed class UsuariosCatalogoMigrationTests(SegurancaIntegrationTestFixture fixture)
    : IClassFixture<SegurancaIntegrationTestFixture>
{
    [Fact]
    public async Task PermissaoInserirUsuariosExisteDisponivelSemConcessaoAutomaticaEReexecucaoPreservaInativacao()
    {
        var db = fixture.DbContext;
        Assert.Contains("20261126130000_AdicionarPermissaoInserirUsuarios", db.Database.GetMigrations());
        Assert.Contains("20261126130000_AdicionarPermissaoInserirUsuarios", await db.Database.GetAppliedMigrationsAsync());

        const string codigo = "seguranca.usuarios.inserir";
        var permissao = await db.Permissoes.AsNoTracking().SingleAsync(p => p.Codigo == codigo);
        Assert.True(permissao.Ativo);
        Assert.True(await new DisponibilidadeCatalogo(db).PermissaoDisponivelAsync(codigo, default));
        Assert.False(await db.PerfisPermissoes.AnyAsync(pp => pp.PermissaoId == permissao.Id));

        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Permissoes.Where(p => p.Codigo == codigo).ExecuteUpdateAsync(s => s.SetProperty(p => p.Ativo, false));

        var migration = new AdicionarPermissaoInserirUsuarios();
        foreach (var comando in db.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, db.Model))
            await db.Database.ExecuteSqlRawAsync(comando.CommandText);

        var depois = await db.Permissoes.AsNoTracking().SingleAsync(p => p.Codigo == codigo);
        Assert.Equal(permissao.PublicId, depois.PublicId);
        Assert.False(depois.Ativo);
        await tx.RollbackAsync();
    }
}
