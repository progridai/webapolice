using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Queries;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public sealed class DiscadorVoipCatalogoMigrationTests(SegurancaIntegrationTestFixture fixture)
    : IClassFixture<SegurancaIntegrationTestFixture>
{
    private const string CodigoRecurso = "DISCADOR_VOIP";
    private const string CodigoPermissao = PermissoesSeguranca.Crm.DiscadorVoip.Utilizar;

    [Fact]
    public async Task CatalogosPreservamGrafiaVoipSemHabilitacaoEConcessaoInicial()
    {
        var db = fixture.DbContext;
        await using var transacao = await db.Database.BeginTransactionAsync();
        await db.Recursos.Where(r => r.Codigo == CodigoRecurso)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Nome, "Discador Voip"));

        var consultas = new ConsultasSeguranca(db);
        var modulos = await consultas.ListarModulosAsync(default);
        var crm = Assert.Single(modulos, m => m.Codigo == "CRM");
        var recurso = Assert.Single(crm.Recursos, r => r.Codigo == CodigoRecurso);
        Assert.Equal("Discador VoIP", recurso.Nome);
        Assert.True(recurso.Ativo);
        Assert.False(recurso.Habilitado);

        var catalogo = await consultas.ListarCatalogoAsync(default);
        var crmNoCatalogo = Assert.Single(catalogo, m => m.Codigo == "CRM");
        var recursoNoCatalogo = Assert.Single(crmNoCatalogo.Recursos, r => r.PublicId == recurso.PublicId);
        Assert.Equal("Discador VoIP", recursoNoCatalogo.Nome);
        var permissao = Assert.Single(recursoNoCatalogo.Permissoes);
        Assert.Equal(CodigoPermissao, permissao.Codigo);
        Assert.Equal("Utilizar", permissao.Nome);
        Assert.False(await db.PerfisPermissoes.AnyAsync(pp => pp.Permissao.PublicId == permissao.PublicId));
        await transacao.RollbackAsync();
    }

    [Fact]
    public async Task ReexecutarCargaPreservaHabilitacaoERollbackPreservaAssertiva()
    {
        var db = fixture.DbContext;
        await using var transacao = await db.Database.BeginTransactionAsync();
        var recursos = db.Recursos.Where(r => r.Modulo.Codigo == "CRM" && r.Codigo == CodigoRecurso);
        await db.Modulos.Where(m => m.Codigo == "CRM")
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Habilitado, true));
        await recursos.ExecuteUpdateAsync(s => s.SetProperty(r => r.Habilitado, true));

        var migration = new AdicionarRecursoDiscadorVoipCrm();
        var geradorSql = db.GetService<IMigrationsSqlGenerator>();
        foreach (var comando in geradorSql.Generate(migration.UpOperations, db.Model))
            await db.Database.ExecuteSqlRawAsync(comando.CommandText);

        var catalogo = new DisponibilidadeCatalogo(db);
        Assert.True(await catalogo.PermissaoDisponivelAsync(CodigoPermissao, default));
        foreach (var comando in geradorSql.Generate(migration.DownOperations, db.Model))
            await db.Database.ExecuteSqlRawAsync(comando.CommandText);

        Assert.False(await recursos.AnyAsync());
        Assert.False(await db.Permissoes.AnyAsync(p => p.Codigo == CodigoPermissao));
        Assert.True(await db.Permissoes.AnyAsync(p => p.Codigo == PermissoesSeguranca.Crm.Assertiva.Utilizar));
        await transacao.RollbackAsync();
    }
}
