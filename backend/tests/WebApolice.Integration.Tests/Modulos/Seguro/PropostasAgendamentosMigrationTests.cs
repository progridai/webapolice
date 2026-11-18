using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using WebApolice.Integration.Tests.Setup;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class PropostasAgendamentosMigrationTests
{
    [Fact]
    public async Task AcrescentaMetadadoECatalogoSemReescreverDadosERecusaRollbackComCancelamento()
    {
        await using var fixture = new SeguroIntegrationTestFixture("20261112111200_DefinirConfirmadaProposta");
        await fixture.InitializeAsync(); var db = fixture.DbContext;
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO seguro.proposta(id,public_id,pessoa_id,cliente_id,cliente_vinculo_id,estipulante_id,status_id,versao,vigente,visivel_operacional,legado_id,observacao)
              SELECT 900,'02a93d97-70f5-48df-838e-236cbbbf068c',10,20,30,40,id,2,true,true,900,'EVIDÊNCIA ORIGINAL'
              FROM seguro.proposta_status WHERE codigo='CONFIRMADA';
            """);
        var antes = await db.Database.SqlQueryRaw<string>("SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync();
        await db.GetService<IMigrator>().MigrateAsync();
        var depois = await db.Database.SqlQueryRaw<string>("SELECT md5((to_jsonb(p)-'inclui_conjuge'-'condicoes_proprias'-'cancelada_em'-'data_cancelamento_vigencia'-'cancelamento_solicitado_em'-'descartada_em')::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync();
        Assert.Equal(antes, depois); var p = await db.Proposta.SingleAsync(p => p.Id == 900); Assert.False(p.CondicoesProprias); Assert.Null(p.CanceladaEm);
        var status = await db.PropostaStatuses.SingleAsync(s => s.Codigo == "CANCELADA"); Assert.True(status.Finalizador); Assert.True(status.VisivelOperacional); Assert.False(status.PermiteMovimentacao);
        await db.Database.ExecuteSqlRawAsync("UPDATE seguro.proposta SET cancelada_em=now() WHERE id=900");
        var erro = await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync("20261112111200_DefinirConfirmadaProposta"));
        Assert.Contains("Rollback bloqueado", erro.MessageText);
        Assert.True(await db.Database.SqlQueryRaw<bool>("SELECT EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema='seguro' AND table_name='proposta' AND column_name='cancelada_em') AS \"Value\"").SingleAsync());
    }
}
