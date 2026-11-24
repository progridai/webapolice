using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.Domain.Modelos;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class IdentidadeMovimentoMigrationTests
{
    [Fact]
    public async Task PreservaLegadoPermiteUuidNativoUnicoEProtegeRollback()
    {
        await using var fixture = new SeguroIntegrationTestFixture("20261118103000_IncluirConjugeProposta");
        await fixture.InitializeAsync(); var db = fixture.DbContext;
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO seguro.proposta_movimento(legado_id,observacao,premio_total)
            VALUES(900,'EVIDÊNCIA ORIGINAL DO LEGADO',123.45);
            """);
        var antes = await db.Database.SqlQueryRaw<string>("SELECT md5(to_jsonb(m)::text) AS \"Value\" FROM seguro.proposta_movimento m WHERE legado_id=900").SingleAsync();
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync();
        Assert.False(db.Database.HasPendingModelChanges());
        var legado = await db.PropostaMovimentos.AsNoTracking().SingleAsync(m => m.LegadoId == 900);
        Assert.Null(legado.PublicId);
        Assert.Equal(antes, await db.Database.SqlQueryRaw<string>("SELECT md5((to_jsonb(m)-'public_id'-'apolice_vida_id'-'parcela_competencia_id'-'proposta_financeiro_id'-'revisao_baixa')::text) AS \"Value\" FROM seguro.proposta_movimento m WHERE legado_id=900").SingleAsync());
        // Antes de usar a identidade nativa, o retorno ao baseline é seguro.
        await migrator.MigrateAsync("20261118103000_IncluirConjugeProposta");
        await migrator.MigrateAsync();
        var um = new PropostaMovimento { PublicId = Guid.NewGuid() };
        var dois = new PropostaMovimento { PublicId = Guid.NewGuid() };
        db.PropostaMovimentos.AddRange(um, dois); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        Assert.Null(um.LegadoId); Assert.Null(dois.LegadoId);
        Assert.Equal(2, await db.PropostaMovimentos.CountAsync(m => m.LegadoId == null && m.PublicId != null));
        var duplicado = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO seguro.proposta_movimento(public_id) VALUES({um.PublicId})"));
        Assert.Equal(PostgresErrorCodes.UniqueViolation, duplicado.SqlState);
        var rollback = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync("20261118103000_IncluirConjugeProposta"));
        Assert.Contains("Rollback bloqueado", rollback.MessageText);
        Assert.Equal(um.PublicId, (await db.PropostaMovimentos.AsNoTracking().SingleAsync(m => m.Id == um.Id)).PublicId);
        Assert.Equal("EVIDÊNCIA ORIGINAL DO LEGADO", (await db.PropostaMovimentos.AsNoTracking().SingleAsync(m => m.LegadoId == 900)).Observacao);
    }

    [Fact]
    public async Task RecusaMovimentoSemIdentidadeOuComUuidVazioPreservandoUnicidadeLegada()
    {
        await using var fixture = new SeguroIntegrationTestFixture();
        await fixture.InitializeAsync(); var db = fixture.DbContext;
        var ausente = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync("INSERT INTO seguro.proposta_movimento(legado_id) VALUES(NULL)"));
        Assert.Equal(PostgresErrorCodes.CheckViolation, ausente.SqlState);
        var vazio = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO seguro.proposta_movimento(public_id,legado_id) VALUES({Guid.Empty},901)"));
        Assert.Equal(PostgresErrorCodes.CheckViolation, vazio.SqlState);
        await db.Database.ExecuteSqlRawAsync("INSERT INTO seguro.proposta_movimento(legado_id) VALUES(901)");
        var duplicado = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync("INSERT INTO seguro.proposta_movimento(legado_id) VALUES(901)"));
        Assert.Equal(PostgresErrorCodes.UniqueViolation, duplicado.SqlState);
    }
}
