using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApolice.Integration.Tests.Setup;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class ExclusaoHierarquiaMigrationTests
{
    [Fact]
    public async Task MigrationEmBasePopuladaPreservaCamposAnterioresEGeraUuidsUnicos()
    {
        await using var ambiente = new Ambiente();
        await ambiente.Fixture.InitializeAsync();
        var db = ambiente.Fixture.DbContext;
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO seguro.apolice(public_id,estipulante_id,seguradora_id,nome,data_inicio_vigencia,status,ativo,versao,created_at,updated_at)
              VALUES ('11111111-1111-1111-1111-111111111111',1,1,'Nome Original',DATE '2026-01-01','ativa',true,1,now(),now());
            INSERT INTO seguro.ramo(public_id,codigo,nome,ativo,created_at,updated_at)
              VALUES ('22222222-2222-2222-2222-222222222222','COD-Teste','Ramo Original',true,now(),now());
            INSERT INTO seguro.apolice_ramo(apolice_id,ramo_id,numero_apolice,ativo,created_at,updated_at)
              SELECT a.id,r.id,'aBc-009',true,now(),now() FROM seguro.apolice a CROSS JOIN seguro.ramo r;
            INSERT INTO seguro.apolice_ramo(apolice_id,ramo_id,numero_apolice,ativo,created_at,updated_at)
              SELECT a.id,r.id,'aBc-010',false,now(),now() FROM seguro.apolice a CROSS JOIN seguro.ramo r;
            INSERT INTO seguro.apolice_subestipulante(apolice_id,subestipulante_id,ativo,created_at,updated_at)
              SELECT id,100,false,now(),now() FROM seguro.apolice;
            INSERT INTO seguro.produto(nome,ativo,created_at,updated_at) VALUES ('Produto Original',true,now(),now());
            INSERT INTO seguro.plano(public_id,nome,ativo,created_at,updated_at) VALUES ('33333333-3333-3333-3333-333333333333','Plano Original',true,now(),now());
            INSERT INTO seguro.apolice_produto(apolice_id,produto_id,ativo,created_at,updated_at)
              SELECT a.id,p.id,true,now(),now() FROM seguro.apolice a CROSS JOIN seguro.produto p;
            INSERT INTO seguro.apolice_plano(apolice_produto_id,plano_id,ativo,created_at,updated_at)
              SELECT a.id,p.id,false,now(),now() FROM seguro.apolice_produto a CROSS JOIN seguro.plano p;
            """);
        string[] tabelas = ["apolice_ramo", "apolice_subestipulante", "apolice_produto", "apolice_plano"];
        async Task<string[]> Snapshot()
        {
            var lista = new List<string>();
            foreach (var tabela in tabelas)
            {
                var remover = tabela == "apolice_subestipulante" ? " - 'public_id'" : " - 'public_id' - 'deleted_at'";
                await using var cmd = new NpgsqlCommand($"SELECT jsonb_agg(to_jsonb(x){remover} ORDER BY id)::text FROM seguro.{tabela} x", ambiente.Fixture.Connection);
                lista.Add((string)(await cmd.ExecuteScalarAsync())!);
            }
            return lista.ToArray();
        }
        var antes = await Snapshot();
        await db.Database.MigrateAsync();
        Assert.Equal(antes, await Snapshot());
        foreach (var tabela in tabelas)
        {
            await using var cmd = new NpgsqlCommand($"SELECT count(*)=count(DISTINCT public_id) AND bool_and(public_id <> '00000000-0000-0000-0000-000000000000'::uuid) FROM seguro.{tabela}", ambiente.Fixture.Connection);
            Assert.True((bool)(await cmd.ExecuteScalarAsync())!);
        }
        Assert.False(db.Database.HasPendingModelChanges());
    }
    private sealed class Ambiente : IAsyncDisposable
    {
        public SeguroIntegrationTestFixture Fixture { get; } = new("20261109232500_ProtegerVinculosVidas");
        public async ValueTask DisposeAsync() => await Fixture.DisposeAsync();
    }
}
