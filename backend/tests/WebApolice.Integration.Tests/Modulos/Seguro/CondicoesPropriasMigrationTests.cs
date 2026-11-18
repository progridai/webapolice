using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using WebApolice.Integration.Tests.Setup;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class CondicoesPropriasMigrationTests
{
    [Fact]
    public async Task PreservaEvidenciaComPadraoFalsoERecusaRollbackAposUso()
    {
        await using var fixture = new SeguroIntegrationTestFixture("20261117120000_NomePreProposta");
        await fixture.InitializeAsync(); var db = fixture.DbContext;
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO seguro.proposta(id,public_id,pessoa_id,cliente_id,cliente_vinculo_id,estipulante_id,status_id,versao,vigente,visivel_operacional,legado_id,observacao)
              SELECT 900,'e17d4413-fd2d-41cf-92b3-7aebbf686bab',10,20,30,40,id,2,true,true,900,'EVIDÊNCIA ORIGINAL'
              FROM seguro.proposta_status WHERE codigo='CONFIRMADA';
            """);
        var antes = await db.Database.SqlQueryRaw<string>("SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync();
        await db.GetService<IMigrator>().MigrateAsync();
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.False((await db.Proposta.AsNoTracking().SingleAsync(p => p.Id == 900)).CondicoesProprias);
        var depois = await db.Database.SqlQueryRaw<string>("SELECT md5((to_jsonb(p)-'inclui_conjuge'-'condicoes_proprias')::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync();
        Assert.Equal(antes, depois);
        await db.Database.ExecuteSqlRawAsync("UPDATE seguro.proposta SET condicoes_proprias=true WHERE id=900");
        var erro = await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync("20261117120000_NomePreProposta"));
        Assert.Contains("Rollback bloqueado", erro.MessageText);
        Assert.Equal("EVIDÊNCIA ORIGINAL", await db.Database.SqlQueryRaw<string>("SELECT observacao AS \"Value\" FROM seguro.proposta WHERE id=900").SingleAsync());
    }
}
