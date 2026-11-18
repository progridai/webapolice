using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Integration.Tests.Setup;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class PropostasOperacionaisMigrationTests
{
    [Fact]
    public async Task EvoluiLegadoSemReescreverEInstalaTitleCaseSomenteParaNovasGravacoes()
    {
        await using var fixture = new SeguroIntegrationTestFixture("20261111213321_ExclusaoHierarquicaVinculosApolice");
        await fixture.InitializeAsync();
        var db = fixture.DbContext;
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO seguro.proposta_status(id,codigo,nome,permite_movimentacao,visivel_operacional,finalizador) VALUES(90,'LEGADO_FIXTURE','Legado da Fixture',false,true,false);
            INSERT INTO seguro.proposta(id,public_id,pessoa_id,cliente_id,cliente_vinculo_id,estipulante_id,status_id,versao,vigente,visivel_operacional,legado_id,observacao)
              VALUES(900,'2f09ad21-73f5-4587-928d-ce98b4d13bc4',10,20,30,40,90,3,true,true,900,'EVIDÊNCIA ORIGINAL');
            INSERT INTO seguro.proposta_item(id,proposta_id,public_id,legado_id,plano_nome_legado,valor) VALUES(900,900,'85f0feba-2676-4792-a6f0-2a25a5ed9101',900,'PLANO ORIGINAL',12.34);
            INSERT INTO seguro.proposta_cobertura(id,proposta_id,proposta_item_id,legado_id,cobertura_nome_legado,premio_titular,premio_conjuge) VALUES(900,900,900,900,'COBERTURA ORIGINAL',1.23,0);
            INSERT INTO seguro.proposta_beneficiario(id,proposta_id,legado_id,nome,parentesco_original,cpf_original,cpf_valido,percentual_participacao) VALUES(900,900,900,'NOME ORIGINAL','PARENTESCO ORIGINAL','CPF ORIGINAL',false,100);
            """);
        var antes = await db.Database.SqlQueryRaw<string>("SELECT md5((to_jsonb(p))::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync();
        await db.GetService<IMigrator>().MigrateAsync();
        var depois = await db.Database.SqlQueryRaw<string>("SELECT md5((to_jsonb(p)-'inclui_conjuge'-'condicoes_proprias'-'apolice_modulo_id'-'apolice_subgrupo_id'-'modulo_nome_registrado'-'subgrupo_nome_registrado'-'revisao_preenchimento'-'confirmada_em'-'substituida_em'-'data_inicio_vigencia'-'data_fim_vigencia'-'motivo_pendencia_vigencia'-'pendencia_vigencia_em'-'cancelada_em'-'data_cancelamento_vigencia'-'cancelamento_solicitado_em'-'descartada_em')::text) AS \"Value\" FROM seguro.proposta p WHERE id=900").SingleAsync();
        Assert.Equal(antes, depois);
        var item = await db.PropostaItems.SingleAsync(i => i.Id == 900);
        Assert.Equal(Guid.Parse("85f0feba-2676-4792-a6f0-2a25a5ed9101"), item.PublicId);
        Assert.Null(item.ApoliceModuloPlanoId); Assert.Null(item.PlanoNomeRegistrado); Assert.Equal("PLANO ORIGINAL", item.PlanoNomeLegado);
        var cobertura = await db.PropostaCoberturas.SingleAsync(c => c.Id == 900);
        Assert.NotEqual(Guid.Empty, cobertura.PublicId); Assert.Equal(900, cobertura.LegadoId); Assert.Equal("COBERTURA ORIGINAL", cobertura.CoberturaNomeLegado);
        Assert.Null(cobertura.ApoliceModuloCoberturaId); Assert.Null(cobertura.CoberturaNomeRegistrado); Assert.Equal(1.23m, cobertura.PremioTitular);
        var beneficiario = await db.PropostaBeneficiarios.SingleAsync(b => b.Id == 900);
        Assert.NotEqual(Guid.Empty, beneficiario.PublicId); Assert.Equal(900, beneficiario.LegadoId);
        Assert.Equal("NOME ORIGINAL", beneficiario.Nome); Assert.Equal("PARENTESCO ORIGINAL", beneficiario.ParentescoOriginal);
        Assert.Equal("CPF ORIGINAL", beneficiario.CpfOriginal); Assert.Null(beneficiario.ParentescoRegistrado);
        Assert.False(cobertura.PremioAjustado); Assert.Null(cobertura.PremioTitularInicial);
        var rascunho = await db.PropostaStatuses.SingleAsync(s => s.Codigo == "RASCUNHO");
        Assert.Equal("Pré Proposta", rascunho.Nome);
        Assert.NotEqual(90, rascunho.Id); Assert.False(rascunho.PermiteMovimentacao); Assert.False(rascunho.Finalizador); Assert.True(rascunho.VisivelOperacional);
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "backend", "scripts", "padronizar-title-case.sql"))) directory = directory.Parent;
        Assert.NotNull(directory);
        var sql = await File.ReadAllTextAsync(Path.Combine(directory.FullName, "backend", "scripts", "padronizar-title-case.sql"));
        await db.Database.ExecuteSqlRawAsync("SET webapolice.title_case_normalizar_existentes='off'");
        await db.Database.ExecuteSqlRawAsync(sql);
        await db.Database.ExecuteSqlRawAsync("RESET webapolice.title_case_normalizar_existentes");
        Assert.Equal("EVIDÊNCIA ORIGINAL", await db.Database.SqlQueryRaw<string>("SELECT observacao AS \"Value\" FROM seguro.proposta WHERE id=900").SingleAsync());
        await db.Database.ExecuteSqlRawAsync("""
            UPDATE seguro.proposta_item SET plano_nome_registrado='AÇÃO DE PROTEÇÃO' WHERE id=900;
            UPDATE seguro.proposta_cobertura SET cobertura_nome_registrado='AÇÃO DE PROTEÇÃO',cobertura_nome_reduzido_registrado='mARIA d’ÁVILA',cobertura_basica_registrada='S' WHERE id=900;
            UPDATE seguro.proposta SET vigente=false WHERE id=900;
            UPDATE seguro.proposta SET motivo_pendencia_vigencia='AÇÃO DE PROTEÇÃO' WHERE id=900;
            UPDATE seguro.proposta SET modulo_nome_registrado='MÓDULO DA FAMÍLIA',subgrupo_nome_registrado='AÇÃO E SAÚDE' WHERE id=900;
            UPDATE seguro.proposta_beneficiario SET parentesco_registrado='FILHA DO TITULAR' WHERE id=900;
            UPDATE seguro.proposta_cobertura SET justificativa_ajuste_premio='AJUSTE DE PROTEÇÃO' WHERE id=900;
            """);
        db.ChangeTracker.Clear();
        item = await db.PropostaItems.SingleAsync(i => i.Id == 900);
        Assert.Equal("Ação de Proteção", item.PlanoNomeRegistrado); Assert.Equal("PLANO ORIGINAL", item.PlanoNomeLegado);
        cobertura = await db.PropostaCoberturas.SingleAsync(c => c.Id == 900);
        Assert.Equal("Ação de Proteção", cobertura.CoberturaNomeRegistrado); Assert.Equal("Maria d’Ávila", cobertura.CoberturaNomeReduzidoRegistrado); Assert.Equal("S", cobertura.CoberturaBasicaRegistrada);
        var proposta = await db.Proposta.SingleAsync(p => p.Id == 900);
        Assert.Equal("EVIDÊNCIA ORIGINAL", proposta.Observacao);
        Assert.Equal("Ação de Proteção", proposta.MotivoPendenciaVigencia);
        Assert.Equal("Módulo da Família", proposta.ModuloNomeRegistrado); Assert.Equal("Ação e Saúde", proposta.SubgrupoNomeRegistrado);
        beneficiario = await db.PropostaBeneficiarios.SingleAsync(b => b.Id == 900);
        Assert.Equal("Filha do Titular", beneficiario.ParentescoRegistrado); Assert.Equal("PARENTESCO ORIGINAL", beneficiario.ParentescoOriginal);
        Assert.Equal("Ajuste de Proteção", cobertura.JustificativaAjustePremio);
    }
}
