using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Queries;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public sealed class AssertivaCatalogoMigrationTests(SegurancaIntegrationTestFixture fixture)
    : IClassFixture<SegurancaIntegrationTestFixture>
{
    [Fact]
    public async Task AssertivaApareceEmModulosEPerfisSemHabilitacaoOuConcessaoInicial()
    {
        var db = fixture.DbContext;
        var consultas = new ConsultasSeguranca(db);
        var modulos = await consultas.ListarModulosAsync(default);
        var crm = Assert.Single(modulos, m => m.Codigo == "CRM");
        var recurso = Assert.Single(crm.Recursos, r => r.Codigo == "ASSERTIVA");
        Assert.True(recurso.Ativo);
        Assert.False(recurso.Habilitado);

        var catalogo = await consultas.ListarCatalogoAsync(default);
        var crmNoCatalogo = Assert.Single(catalogo, m => m.Codigo == "CRM");
        var recursoNoCatalogo = Assert.Single(crmNoCatalogo.Recursos, r => r.PublicId == recurso.PublicId);
        var permissao = Assert.Single(recursoNoCatalogo.Permissoes);
        Assert.Equal(PermissoesSeguranca.Crm.Assertiva.Utilizar, permissao.Codigo);
        Assert.Equal("Utilizar", permissao.Nome);
        Assert.False(await db.PerfisPermissoes.AnyAsync(pp => pp.Permissao.PublicId == permissao.PublicId));
    }

    [Fact]
    public async Task ReexecutarCargaPreservaHabilitacaoERollbackRemoveApenasAssertiva()
    {
        var db = fixture.DbContext;
        await using var transacao = await db.Database.BeginTransactionAsync();
        var recursos = db.Recursos.Where(r => r.Modulo.Codigo == "CRM" && r.Codigo == "ASSERTIVA");
        await db.Modulos.Where(m => m.Codigo == "CRM")
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Habilitado, true));
        await recursos.ExecuteUpdateAsync(s => s.SetProperty(r => r.Habilitado, true));

        var migration = new AdicionarRecursoAssertivaCrm();
        var geradorSql = db.GetService<IMigrationsSqlGenerator>();
        foreach (var comando in geradorSql.Generate(migration.UpOperations, db.Model))
            await db.Database.ExecuteSqlRawAsync(comando.CommandText);

        var catalogo = new DisponibilidadeCatalogo(db);
        Assert.True(await catalogo.PermissaoDisponivelAsync(PermissoesSeguranca.Crm.Assertiva.Utilizar, default));
        foreach (var comando in geradorSql.Generate(migration.DownOperations, db.Model))
            await db.Database.ExecuteSqlRawAsync(comando.CommandText);

        Assert.False(await recursos.AnyAsync());
        Assert.False(await db.Permissoes.AnyAsync(p => p.Codigo == PermissoesSeguranca.Crm.Assertiva.Utilizar));
        Assert.True(await db.Permissoes.AnyAsync(p => p.Codigo == PermissoesSeguranca.Crm.Acessar));
        await transacao.RollbackAsync();
    }
}
