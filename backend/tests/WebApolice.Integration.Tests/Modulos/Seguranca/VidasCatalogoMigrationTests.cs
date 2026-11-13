using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class VidasCatalogoMigrationTests(SegurancaIntegrationTestFixture fixture) : IClassFixture<SegurancaIntegrationTestFixture>
{
    private static readonly string[] Codigos = ["apolices.vidas.inserir", "apolices.vidas.alterar", "apolices.vidas.inativar"];

    [Fact]
    public async Task CatalogoHierarquicoPossuiCincoPermissoesSemConcessoesEReexecucaoPreservaInativacao()
    {
        string[] codigos = ["apolices.ramos.excluir", "apolices.subgrupos.excluir", "apolices.subestipulantes.excluir", "apolices.modulos.excluir", "apolices.universo.excluir"];
        var db = fixture.DbContext;
        Assert.Contains("20261111220000_AdicionarExclusaoVinculosApolice", await db.Database.GetAppliedMigrationsAsync());
        var antes = await db.Permissoes.AsNoTracking().Where(p => codigos.Contains(p.Codigo)).OrderBy(p => p.Codigo).ToListAsync();
        Assert.Equal(5, antes.Count);
        Assert.False(await db.PerfisPermissoes.AnyAsync(p => codigos.Contains(p.Permissao.Codigo)));
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Permissoes.Where(p => codigos.Contains(p.Codigo)).ExecuteUpdateAsync(s => s.SetProperty(p => p.Ativo, false));
        var migration = new AdicionarExclusaoVinculosApolice();
        foreach (var comando in db.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, db.Model))
            await db.Database.ExecuteSqlRawAsync(comando.CommandText);
        var depois = await db.Permissoes.AsNoTracking().Where(p => codigos.Contains(p.Codigo)).OrderBy(p => p.Codigo).ToListAsync();
        Assert.Equal(antes.Select(p => p.PublicId), depois.Select(p => p.PublicId)); Assert.All(depois, p => Assert.False(p.Ativo));
        await tx.RollbackAsync();
    }

    [Fact]
    public async Task CatalogoExclusaoDescobertoSemConcessaoAutomaticaEReexecucaoPreservaInativacao()
    {
        var db = fixture.DbContext;
        Assert.Contains("20261111190000_AdicionarPermissaoExcluirVida", await db.Database.GetAppliedMigrationsAsync());
        const string codigo = "apolices.vidas.excluir";
        var permissao = await db.Permissoes.AsNoTracking().SingleAsync(p => p.Codigo == codigo);
        Assert.True(await new DisponibilidadeCatalogo(db).PermissaoDisponivelAsync(codigo, default));
        Assert.False(await db.PerfisPermissoes.AnyAsync(p => p.PermissaoId == permissao.Id));
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Permissoes.Where(p => p.Codigo == codigo).ExecuteUpdateAsync(s => s.SetProperty(p => p.Ativo, false));
        var migration = new AdicionarPermissaoExcluirVida();
        foreach (var comando in db.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, db.Model))
            await db.Database.ExecuteSqlRawAsync(comando.CommandText);
        var depois = await db.Permissoes.AsNoTracking().SingleAsync(p => p.Codigo == codigo);
        Assert.Equal(permissao.PublicId, depois.PublicId); Assert.False(depois.Ativo);
        await tx.RollbackAsync();
    }

    [Fact]
    public async Task EfDescobreMigrationECriaCatalogoDeVidasNaBaseNova()
    {
        var db = fixture.DbContext;
        Assert.Contains("20260822210000_AdicionarPermissoesVidasApolice", db.Database.GetMigrations());
        Assert.Contains("20260822210000_AdicionarPermissoesVidasApolice", await db.Database.GetAppliedMigrationsAsync());
        var permissoes = await db.Permissoes.AsNoTracking().Where(p => Codigos.Contains(p.Codigo))
            .Select(p => new { p.Codigo, p.Ativo, Recurso = p.Recurso.Codigo, Modulo = p.Recurso.Modulo.Codigo }).ToListAsync();
        Assert.Equal(3, permissoes.Count);
        Assert.All(permissoes, p => { Assert.True(p.Ativo); Assert.Equal("APOLICES", p.Recurso); Assert.Equal("APOLICES", p.Modulo); });
        var catalogo = new DisponibilidadeCatalogo(db);
        foreach (var codigo in Codigos) Assert.True(await catalogo.PermissaoDisponivelAsync(codigo, default));
    }

    [Fact]
    public async Task CargaRestauraAusentesSemConcederPerfisEReexecucaoPreservaIdsEInativacao()
    {
        var db = fixture.DbContext;
        await using var tx = await db.Database.BeginTransactionAsync();
        var perfisAntes = await db.Perfis.AsNoTracking().OrderBy(p => p.Id).Select(p => new {p.Id,p.Ativo,p.AcessoTotal}).ToListAsync();
        var vinculosAntes = await db.PerfisPermissoes.AsNoTracking().CountAsync();
        var usuariosAntes = await db.UsuariosPerfis.AsNoTracking().CountAsync();
        await db.Permissoes.Where(p => Codigos.Contains(p.Codigo)).ExecuteDeleteAsync();
        var catalogo = new DisponibilidadeCatalogo(db);
        Assert.False(await catalogo.PermissaoDisponivelAsync("apolices.vidas.alterar", default));
        var migration = new AdicionarPermissoesVidasApolice();
        var comandos = db.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, db.Model);
        async Task Aplicar()
        {
            foreach (var comando in comandos) await db.Database.ExecuteSqlRawAsync(comando.CommandText);
        }
        await Aplicar();
        Assert.True(await catalogo.PermissaoDisponivelAsync("apolices.vidas.alterar", default));
        var ids = await db.Permissoes.AsNoTracking().Where(p => Codigos.Contains(p.Codigo)).OrderBy(p => p.Codigo).Select(p => p.PublicId).ToListAsync();
        await db.Permissoes.Where(p => p.Codigo == "apolices.vidas.alterar").ExecuteUpdateAsync(s => s.SetProperty(p => p.Ativo, false));
        await Aplicar();
        Assert.Equal(ids, await db.Permissoes.AsNoTracking().Where(p => Codigos.Contains(p.Codigo)).OrderBy(p => p.Codigo).Select(p => p.PublicId).ToListAsync());
        Assert.False(await catalogo.PermissaoDisponivelAsync("apolices.vidas.alterar", default));
        Assert.Equal(perfisAntes, await db.Perfis.AsNoTracking().OrderBy(p => p.Id).Select(p => new {p.Id,p.Ativo,p.AcessoTotal}).ToListAsync());
        Assert.Equal(vinculosAntes, await db.PerfisPermissoes.CountAsync());
        Assert.Equal(usuariosAntes, await db.UsuariosPerfis.CountAsync());
        await tx.RollbackAsync();
    }
}
