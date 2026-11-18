using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Integration.Tests.Setup;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class PrePropostaMigrationTests
{
    [Fact]
    public async Task RenomeiaSomenteNomeInicialPreservandoIdentidadeConfiguracaoEOutrasSituacoes()
    {
        await using var fixture = new SeguroIntegrationTestFixture("20261112194300_DefinirDescartadaProposta");
        await fixture.InitializeAsync();
        var db = fixture.DbContext;
        var antes = await db.Database.SqlQueryRaw<string>("""
            SELECT md5(string_agg((CASE WHEN codigo='RASCUNHO' THEN to_jsonb(s)-'nome'
              ELSE to_jsonb(s) END)::text, ',' ORDER BY id)) AS "Value"
            FROM seguro.proposta_status s
            """).SingleAsync();
        Assert.Equal("Rascunho", (await db.PropostaStatuses.AsNoTracking().SingleAsync(s => s.Codigo == "RASCUNHO")).Nome);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync();
        await migrator.MigrateAsync();
        var depois = await db.Database.SqlQueryRaw<string>("""
            SELECT md5(string_agg((CASE WHEN codigo='RASCUNHO' THEN to_jsonb(s)-'nome'
              ELSE to_jsonb(s) END)::text, ',' ORDER BY id)) AS "Value"
            FROM seguro.proposta_status s
            """).SingleAsync();
        Assert.Equal(antes, depois);
        Assert.Equal("Pré Proposta", (await db.PropostaStatuses.AsNoTracking().SingleAsync(s => s.Codigo == "RASCUNHO")).Nome);
    }

    [Fact]
    public async Task RecusaConfiguracaoInicialIncompativelSemRegistrarMigration()
    {
        await using var fixture = new SeguroIntegrationTestFixture("20261112194300_DefinirDescartadaProposta");
        await fixture.InitializeAsync();
        var db = fixture.DbContext;
        await db.Database.ExecuteSqlRawAsync("UPDATE seguro.proposta_status SET finalizador=true WHERE codigo='RASCUNHO'");
        var erro = await Assert.ThrowsAsync<Npgsql.PostgresException>(() => db.GetService<IMigrator>().MigrateAsync());
        Assert.Equal("P0001", erro.SqlState);
        Assert.DoesNotContain("20261117120000_NomePreProposta", await db.Database.GetAppliedMigrationsAsync());
        Assert.Equal("Rascunho", (await db.PropostaStatuses.AsNoTracking().SingleAsync(s => s.Codigo == "RASCUNHO")).Nome);
    }
}
