using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using WebApolice.Integration.Tests.Setup;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class ConjugePropostaMigrationTests
{
    [Theory]
    [InlineData(false)][InlineData(true)]
    public async Task PreservaOriginaisSemInferirContratacaoEProtegeRollback(bool escolha)
    {
        await using var fixture = new SeguroIntegrationTestFixture("20261117180000_CondicoesPropriasProposta");
        await fixture.InitializeAsync(); var db = fixture.DbContext;
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO seguro.proposta(id,public_id,pessoa_id,cliente_id,cliente_vinculo_id,estipulante_id,status_id,versao,vigente,visivel_operacional,legado_id,observacao)
              SELECT 900,'e17d4413-fd2d-41cf-92b3-7aebbf686bab',10,20,30,40,id,2,true,true,900,'EVIDÊNCIA ORIGINAL'
              FROM seguro.proposta_status WHERE codigo='CONFIRMADA';
            """);
        var antes = await db.Database.SqlQueryRaw<string>("SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync();
        await db.GetService<IMigrator>().MigrateAsync();
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.Null((await db.Proposta.AsNoTracking().SingleAsync(p => p.Id == 900)).IncluiConjuge);
        Assert.Equal(antes, await db.Database.SqlQueryRaw<string>("SELECT md5((to_jsonb(p)-'inclui_conjuge')::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync());
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE seguro.proposta SET inclui_conjuge={escolha} WHERE id=900");
        var erro = await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync("20261117180000_CondicoesPropriasProposta"));
        Assert.Contains("Rollback bloqueado", erro.MessageText);
    }
}
