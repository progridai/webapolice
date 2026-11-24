using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ConfirmarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EfetivarVigencia;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.Financeiro;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private ParcelasPropostasRepository FinanceiroRepo() => new(fixture.DbContext, new ClientesPropostas(fixture.Cadastro));
    private ConfirmarPropostaHandler ConfirmadorFinanceiro(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null)
    {
        var config = ConfigConfirmacao(codigo); config.Value.ExigirConferenciaFinanceira = true;
        return new(RepositorySimulacao(), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)), new ClientesPropostas(fixture.Cadastro),
            Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), config, relogio, FinanceiroRepo());
    }
    private EfetivarVigenciaPropostaHandler EfetivadorFinanceiro(string codigo, TimeProvider relogio) => new(RepositorySimulacao(),
        Operacao(new RegistradorAuditoria(fixture.Auditoria)), new ClientesPropostas(fixture.Cadastro),
        Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), ConfigConfirmacao(codigo), relogio, FinanceiroRepo());
    private Task<ConferenciaFinanceiraResultado> ConferirAsync(Guid id, Guid ramo, DateOnly inicio, int competencia = 202611, DateOnly? vencimento = null, int revisao = 1) =>
        new ConferirFinanceiroHandler(RepositorySimulacao(), FinanceiroRepo(), new RelogioPropostas()).Handle(new(id, new(revisao, inicio, competencia, vencimento ?? new DateOnly(2026, 11, 30), ramo)), default);
    private static FinanceiroConfirmacaoDados FinanceiroDados(ConferenciaFinanceiraResultado c, Guid ramo) => new(c.CompetenciaInicial, c.VencimentoInicial, ramo, c.Conferencia);
    private async Task<(Guid Id, Guid Apolice, string Codigo, Guid Ramo)> AtivaFinanceiroAsync()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync(); var ramo = await RamoSimulacaoAsync(id);
        var c = await ConferirAsync(id, ramo, new(2026, 11, 1));
        await ConfirmadorFinanceiro(codigo, new RelogioPropostas()).Handle(new(id, new(1, new(2026, 11, 1), FinanceiroDados(c, ramo))), default);
        var apolice = await fixture.DbContext.Proposta.Where(p => p.PublicId == id).Select(p => p.Apolice!.PublicId).SingleAsync();
        fixture.DbContext.ChangeTracker.Clear(); return (id, apolice, codigo, ramo);
    }
    private Task<LoteMensalResultado> LoteAsync(Guid apolice, int competencia) => new ConferirLoteHandler(RepositorySimulacao(), FinanceiroRepo()).Handle(new(apolice, competencia), default);
    private Task<LoteMensalResultado> GerarAsync(LoteMensalResultado lote) => new GerarLoteHandler(RepositorySimulacao(), FinanceiroRepo(), Operacao(new RegistradorAuditoria(fixture.Auditoria)), new RelogioPropostas()).Handle(new(lote.ApolicePublicId, new(lote.Competencia, lote.Conferencia)), default);
    private Task<LoteMensalResultado> FecharAsync(LoteMensalResultado lote) => new FecharLoteHandler(RepositorySimulacao(), FinanceiroRepo(), Operacao(new RegistradorAuditoria(fixture.Auditoria)), new RelogioPropostas()).Handle(new(lote.ApolicePublicId, new(lote.Competencia, lote.Conferencia)), default);

    [Fact]
    public async Task FinanceiroPrimeiraIntegralComIofMovimentoESequenciaSemTitulo()
    {
        var (id, apolice, _, _) = await AtivaFinanceiroAsync();
        var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id);
        var f = await fixture.DbContext.PropostasFinanceiro.SingleAsync(f => f.PropostaId == p.Id);
        var m = await fixture.DbContext.PropostaMovimentos.SingleAsync(m => m.PropostaId == p.Id);
        Assert.Equal(f.TotalMensal, m.PremioTotal); Assert.Equal(1, m.Parcela); Assert.Equal(202611, m.CompetenciaInt);
        Assert.Equal("PARCELA_MENSAL", m.Classificacao); Assert.NotNull(m.PublicId); Assert.Null(m.LegadoId);
        Assert.Null(m.TituloGerado); Assert.Null(m.MovimentoTipoId); Assert.Equal(f.IofValor, m.Iof);
        Assert.Equal(f.PremioLiquido + f.IofValor, m.PremioTotal);
        var lote = await LoteAsync(apolice, 202611); Assert.True(lote.PodeGerar); Assert.Equal("EXISTENTE", Assert.Single(lote.Vidas).Situacao);
        await GerarAsync(lote); Assert.Single(await fixture.DbContext.PropostaMovimentos.Where(m => m.PropostaId == p.Id).ToArrayAsync());
    }
    [Fact]
    public async Task FinanceiroCongelaAliquotaFuturaMasExigeNovaConferenciaSeInicioMudar()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync(); var ramo = await RamoSimulacaoAsync(id);
        var c = await ConferirAsync(id, ramo, new(2026, 11, 20));
        await ConfirmadorFinanceiro(codigo, new RelogioPropostas()).Handle(new(id, new(1, new(2026, 11, 20), FinanceiroDados(c, ramo))), default);
        Assert.False(await fixture.DbContext.PropostaMovimentos.AnyAsync(m => m.Proposta!.PublicId == id));
        var vinculo = await fixture.DbContext.ApoliceRamos.SingleAsync(v => v.PublicId == ramo); vinculo.IofPercentual = 10;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var relogio = new RelogioPropostas { Agora = new(2026, 11, 21, 12, 0, 0, TimeSpan.Zero) };
        var r = await EfetivadorFinanceiro(codigo, relogio).Handle(new(id, 2), default); Assert.True(r.Ativada);
        var m = await fixture.DbContext.PropostaMovimentos.SingleAsync(m => m.Proposta!.PublicId == id);
        Assert.Equal(decimal.Parse(c.Valores.TotalMensal, System.Globalization.CultureInfo.InvariantCulture), m.PremioTotal);
        var repetida = await EfetivadorFinanceiro(codigo, relogio).Handle(new(id, r.RevisaoPreenchimento), default); Assert.True(repetida.JaEfetivada);
        Assert.Single(await fixture.DbContext.PropostaMovimentos.Where(m => m.Proposta!.PublicId == id).ToArrayAsync());
    }
    [Fact]
    public async Task FinanceiroRecusaConferenciaAntigaEConfirmacaoSemConferencia()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync(); var ramo = await RamoSimulacaoAsync(id);
        await Assert.ThrowsAsync<ValidacaoException>(() => ConfirmadorFinanceiro(codigo, new RelogioPropostas()).Handle(new(id, new(1, new(2026, 11, 1))), default));
        var c = await ConferirAsync(id, ramo, new(2026, 11, 1));
        var vinculo = await fixture.DbContext.ApoliceRamos.SingleAsync(v => v.PublicId == ramo); vinculo.IofPercentual = 7;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => ConfirmadorFinanceiro(codigo, new RelogioPropostas()).Handle(new(id, new(1, new(2026, 11, 1), FinanceiroDados(c, ramo))), default));
        Assert.False(await fixture.DbContext.PropostasFinanceiro.AnyAsync(f => f.Proposta.PublicId == id));
    }
    [Fact]
    public async Task FinanceiroLoteExigeFechamentoAnteriorPreservaInadimplenciaSemDuplicar()
    {
        var (_, apolice, _, _) = await AtivaFinanceiroAsync();
        var dezembro = await LoteAsync(apolice, 202612); Assert.False(dezembro.PodeGerar);
        var novembro = await GerarAsync(await LoteAsync(apolice, 202611));
        await FecharAsync(novembro);
        dezembro = await LoteAsync(apolice, 202612); Assert.True(dezembro.PodeGerar);
        var vida = Assert.Single(dezembro.Vidas); Assert.Equal("INCLUIR", vida.Situacao); Assert.Equal(vida.NovaMensalidade, vida.SaldoAnterior);
        var gerado = await GerarAsync(dezembro); var parcelas = Assert.Single(gerado.Vidas).Parcelas;
        Assert.Equal(new[] { 1, 2 }, parcelas.Select(p => p.Parcela)); Assert.Equal(parcelas[0].Total, parcelas[1].Total);
        await Assert.ThrowsAsync<ValidacaoException>(() => GerarAsync(dezembro));
        Assert.False((await LoteAsync(apolice, 202702)).PodeGerar);
        var baixa = new BaixarParcelaHandler(RepositorySimulacao(), FinanceiroRepo(), Operacao(new RegistradorAuditoria(fixture.Auditoria)), new RelogioPropostas());
        var antiga = parcelas[0]; await baixa.Handle(new(apolice, antiga.PublicId, new(antiga.RevisaoBaixa, new(2026, 12, 15))), default);
        var atual = await LoteAsync(apolice, 202612); Assert.Equal("0.00", Assert.Single(atual.Vidas).SaldoAnterior);
        Assert.Equal(parcelas[1].Total, Assert.Single(atual.Vidas).TotalAcumulado);
        await Assert.ThrowsAsync<ValidacaoException>(() => baixa.Handle(new(apolice, antiga.PublicId, new(antiga.RevisaoBaixa, new(2026, 12, 15))), default));
        await Assert.ThrowsAsync<ValidacaoException>(() => FecharAsync(gerado)); // Baixa mudou a conferência.
        Assert.NotNull((await fixture.DbContext.ParcelasCompetencias.SingleAsync(c => c.Apolice.PublicId == apolice && c.Competencia == 202611)).FechadaEm);
    }
    [Fact]
    public async Task FinanceiroReagendamentoExigeConferirNovamenteAntesDaAtivacao()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync(); var ramo = await RamoSimulacaoAsync(id);
        var relogio = new RelogioPropostas(); var c = await ConferirAsync(id, ramo, new(2026,11,20));
        await ConfirmadorFinanceiro(codigo,relogio).Handle(new(id,new(1,new(2026,11,20),FinanceiroDados(c,ramo))),default);
        await Reagendador(codigo,relogio).Handle(new(id,new(2,new(2026,11,21),"Nova Data")),default);
        relogio.Agora = new(2026,11,22,12,0,0,TimeSpan.Zero);
        var pendente = await EfetivadorFinanceiro(codigo,relogio).Handle(new(id,3),default);
        Assert.False(pendente.Ativada); Assert.Contains("Financeiro",pendente.PendenciaVigencia!);
        var nova = await ConferirAsync(id,ramo,new(2026,11,21),revisao:pendente.RevisaoPreenchimento);
        var revisao = await new AtualizarFinanceiroHandler(RepositorySimulacao(),FinanceiroRepo(),Operacao(new RegistradorAuditoria(fixture.Auditoria)),relogio)
            .Handle(new(id,new(pendente.RevisaoPreenchimento,FinanceiroDados(nova,ramo))),default);
        Assert.True((await EfetivadorFinanceiro(codigo,relogio).Handle(new(id,revisao),default)).Ativada);
    }
    [Fact]
    public async Task FinanceiroNovaVersaoPreservaParcelaDoMesESequenciaIncluindoLegado()
    {
        var (id,apolice,codigo,ramo) = await AtivaFinanceiroAsync();
        var anterior = await fixture.DbContext.Proposta.Include(p=>p.Apolice).Include(p=>p.ApoliceVida).SingleAsync(p=>p.PublicId==id);
        var primeira = await fixture.DbContext.PropostaMovimentos.AsNoTracking().SingleAsync(m=>m.PropostaId==anterior.Id);
        fixture.DbContext.PropostaMovimentos.Add(new() { PropostaId=anterior.Id, LegadoId=Random.Shared.Next(1000000,int.MaxValue), Parcela=27, Classificacao="LEGADO" });
        await fixture.DbContext.SaveChangesAsync();
        var nova = await CriarHandler(codigo,new RegistradorAuditoria(fixture.Auditoria)).Handle(new() {
            ClienteId=anterior.ClienteId,ApoliceId=apolice,ApoliceVidaId=anterior.ApoliceVida!.PublicId,PropostaOrigemPublicId=id },default);
        fixture.DbContext.ChangeTracker.Clear();
        var cobertura = await fixture.DbContext.PropostaCoberturas.Where(c=>c.Proposta.PublicId==nova).Select(c=>c.PublicId).SingleAsync();
        await Preenchedor(codigo).Handle(new(nova,Dados(cobertura,premio:30) with { Beneficiarios=[new(null,"Filha","Filha",100)] }),default);
        var conferencia = await ConferirAsync(nova,ramo,new(2026,11,10));
        await ConfirmadorFinanceiro(codigo,new RelogioPropostas()).Handle(new(nova,new(1,new(2026,11,10),FinanceiroDados(conferencia,ramo))),default);
        Assert.Equal(1,await fixture.DbContext.PropostaMovimentos.CountAsync(m=>m.Proposta!.ApoliceId==anterior.ApoliceId && m.Classificacao=="PARCELA_MENSAL"));
        var preservada = await fixture.DbContext.PropostaMovimentos.AsNoTracking().SingleAsync(m=>m.Id==primeira.Id);
        Assert.Equal(primeira.PremioTotal,preservada.PremioTotal); Assert.Equal(primeira.PropostaId,preservada.PropostaId);
        await FecharAsync(await GerarAsync(await LoteAsync(apolice,202611)));
        var dezembro = await GerarAsync(await LoteAsync(apolice,202612));
        var ultima = Assert.Single(dezembro.Vidas).Parcelas.Last(); Assert.Equal(28,ultima.Parcela);
        Assert.Equal(conferencia.Valores.TotalMensal,ultima.Total);
    }
    [Fact]
    public async Task FinanceiroCancelamentoExcluiProximaMensalidadePreservandoSaldoAnterior()
    {
        var (id,apolice,codigo,_) = await AtivaFinanceiroAsync(); var relogio=new RelogioPropostas();
        await SolicitadorCancelamento(codigo,relogio).Handle(new(id,new(2,new(2026,11,12),"Fim da Participação")),default);
        await FecharAsync(await GerarAsync(await LoteAsync(apolice,202611)));
        var dezembro = await LoteAsync(apolice,202612); var vida=Assert.Single(dezembro.Vidas);
        Assert.Equal("EXCLUIDA",vida.Situacao); Assert.Equal("0.00",vida.NovaMensalidade); Assert.NotEqual("0.00",vida.SaldoAnterior);
        var gerada=await GerarAsync(dezembro); Assert.Single(Assert.Single(gerada.Vidas).Parcelas);
    }
    [Fact]
    public async Task FinanceiroRecusaBaixaEmOutraApoliceEFechamentoComTokenAntigo()
    {
        var (_,apolice,_,_) = await AtivaFinanceiroAsync(); var outra=await AtivaFinanceiroAsync();
        var lote=await LoteAsync(apolice,202611); var parcela=Assert.Single(Assert.Single(lote.Vidas).Parcelas);
        var baixa=new BaixarParcelaHandler(RepositorySimulacao(),FinanceiroRepo(),Operacao(new RegistradorAuditoria(fixture.Auditoria)),new RelogioPropostas());
        await Assert.ThrowsAsync<ValidacaoException>(()=>baixa.Handle(new(outra.Apolice,parcela.PublicId,new(0,new(2026,11,30))),default));
        var gerado=await GerarAsync(lote);
        await baixa.Handle(new(apolice,parcela.PublicId,new(0,new(2026,11,30))),default);
        await Assert.ThrowsAsync<ValidacaoException>(()=>FecharAsync(gerado));
    }
    [Fact]
    public async Task FinanceiroBancoProtegeIdentidadeMensalValoresESequencia()
    {
        var (id,_,_,_) = await AtivaFinanceiroAsync(); var db=fixture.DbContext;
        var m=await db.PropostaMovimentos.SingleAsync(m=>m.Proposta!.PublicId==id);
        foreach (var caso in new[] { "parcela=NULL", "competencia_int=NULL", "premio_liquido=-1", "valor_pago=0.01" })
        {
            // Fragmentos técnicos constantes; UUID parametrizado e nenhuma informação do usuário no SQL.
            var sql=$"UPDATE seguro.proposta_movimento SET {caso} WHERE public_id={{0}}";
            var erro=await Assert.ThrowsAsync<PostgresException>(()=>db.Database.ExecuteSqlRawAsync(sql,m.PublicId!.Value));
            Assert.Equal(PostgresErrorCodes.CheckViolation,erro.SqlState);
        }
        var duplicada=await Assert.ThrowsAsync<PostgresException>(()=>db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO seguro.proposta_movimento(public_id,proposta_id,apolice_vida_id,proposta_financeiro_id,parcela_competencia_id,classificacao,competencia_int,parcela,data_vencimento,premio_liquido,iof,premio_total) SELECT {Guid.NewGuid()},proposta_id,apolice_vida_id,proposta_financeiro_id,parcela_competencia_id,classificacao,competencia_int,parcela+1,data_vencimento,premio_liquido,iof,premio_total FROM seguro.proposta_movimento WHERE id={m.Id}"));
        Assert.Equal("ux_parcela_vida_competencia",duplicada.ConstraintName);
        var numero=await Assert.ThrowsAsync<PostgresException>(()=>db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO seguro.proposta_movimento(public_id,proposta_id,apolice_vida_id,proposta_financeiro_id,parcela_competencia_id,classificacao,competencia_int,parcela,data_vencimento,premio_liquido,iof,premio_total) SELECT {Guid.NewGuid()},proposta_id,apolice_vida_id,proposta_financeiro_id,parcela_competencia_id,classificacao,202612,parcela,data_vencimento,premio_liquido,iof,premio_total FROM seguro.proposta_movimento WHERE id={m.Id}"));
        Assert.Equal("ux_parcela_vida_numero",numero.ConstraintName);
        var rollback=await Assert.ThrowsAsync<PostgresException>(()=>db.GetService<IMigrator>().MigrateAsync("20261118140000_IdentidadeMovimentoNativo"));
        Assert.Contains("Rollback bloqueado",rollback.MessageText);
        Assert.Equal(m.PremioTotal,(await db.PropostaMovimentos.AsNoTracking().SingleAsync(v=>v.Id==m.Id)).PremioTotal);
    }
    [Fact]
    public async Task FinanceiroSerializaLotesConcorrentesSemDuplicarMensalidades()
    {
        var (_,apolice,_,_) = await AtivaFinanceiroAsync(); await FecharAsync(await GerarAsync(await LoteAsync(apolice,202611)));
        var lote=await LoteAsync(apolice,202612); var chegou=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var liberar=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var auditor=new Mock<IRegistradorAuditoria>();
        auditor.Setup(a=>a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(),It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro,CancellationToken ct)=> { chegou.TrySetResult(); await liberar.Task.WaitAsync(ct); await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro,ct); });
        var primeira=new GerarLoteHandler(RepositorySimulacao(),FinanceiroRepo(),Operacao(auditor.Object),new RelogioPropostas())
            .Handle(new(apolice,new(202612,lote.Conferencia)),default);
        Task<LoteMensalResultado>? segunda=null;
        await using var services=ServicosAgendamento(); await using var scope=services.CreateAsyncScope(); var sp=scope.ServiceProvider;
        try
        {
            await chegou.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var db=sp.GetRequiredService<SeguroDbContext>();
            var operacao=new OperacaoPropostas(db,new OperacaoAuditada(sp.GetRequiredService<AuditoriaDbContext>(),sp.GetRequiredService<IRegistradorAuditoria>(),new Mock<IContextoAuditoria>().Object));
            segunda=new GerarLoteHandler(sp.GetRequiredService<IPropostasRepository>(),new ParcelasPropostasRepository(db,sp.GetRequiredService<IClientesPropostas>()),operacao,new RelogioPropostas())
                .Handle(new(apolice,new(202612,lote.Conferencia)),default);
            Assert.NotSame(segunda,await Task.WhenAny(segunda,Task.Delay(150)));
        }
        finally { liberar.TrySetResult(); await primeira; }
        Assert.NotNull(segunda); await Assert.ThrowsAsync<ValidacaoException>(async()=>await segunda);
        await using var leitura=LeituraNovaConexao();
        Assert.Equal(2,await leitura.PropostaMovimentos.CountAsync(m=>m.Proposta!.Apolice!.PublicId==apolice && m.Classificacao=="PARCELA_MENSAL"));
    }
    [Fact]
    public async Task FinanceiroProcessadorGeraSomentePrimeiraParcelaNaAtivacaoAgendada()
    {
        var (id,codigo,_) = await ProntaConfirmarAsync(); var ramo=await RamoSimulacaoAsync(id); var relogio=new RelogioPropostas();
        var c=await ConferirAsync(id,ramo,new(2026,11,20));
        await ConfirmadorFinanceiro(codigo,relogio).Handle(new(id,new(1,new(2026,11,20),FinanceiroDados(c,ramo))),default);
        relogio.Agora=new(2026,11,21,12,0,0,TimeSpan.Zero);
        await using var services=ServicosAgendamento();
        var processador=new ProcessadorVigenciasPropostas(services.GetRequiredService<IServiceScopeFactory>(),ConfigConfirmacao(codigo),relogio,Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessadorVigenciasPropostas>.Instance);
        await processador.ProcessarAsync(default); await processador.ProcessarAsync(default);
        await using var leitura=LeituraNovaConexao();
        Assert.True((await leitura.Proposta.SingleAsync(p=>p.PublicId==id)).Vigente);
        var primeira=Assert.Single(await leitura.PropostaMovimentos.Where(m=>m.Proposta!.PublicId==id).ToArrayAsync());
        Assert.Equal(202611,primeira.CompetenciaInt); Assert.Equal(1,primeira.Parcela);
        Assert.Equal(1,await fixture.Auditoria.RegistrosAuditoria.CountAsync(a=>a.RecursoId==id.ToString() && a.UsuarioIdExterno=="sistema:vigencias-propostas"));
    }
    [Fact]
    public async Task FinanceiroNovaVersaoAposFechamentoExigeCompetenciaPosteriorSemReabrirHistorico()
    {
        var (id,apolice,codigo,ramo)=await AtivaFinanceiroAsync();
        await FecharAsync(await GerarAsync(await LoteAsync(apolice,202611)));
        var nova=await CriarHandler(codigo,new RegistradorAuditoria(fixture.Auditoria)).Handle(await CriacaoDaOrigemAsync(id),default);
        fixture.DbContext.ChangeTracker.Clear();
        var cobertura=await fixture.DbContext.PropostaCoberturas.Where(c=>c.Proposta.PublicId==nova).Select(c=>c.PublicId).SingleAsync();
        await Preenchedor(codigo).Handle(new(nova,Dados(cobertura,premio:30) with { Beneficiarios=[new(null,"Filha","Filha",100)] }),default);
        await Assert.ThrowsAsync<ValidacaoException>(()=>ConferirAsync(nova,ramo,new(2026,11,10)));
        var c=await ConferirAsync(nova,ramo,new(2026,11,10),202612,new(2026,12,30));
        await ConfirmadorFinanceiro(codigo,new RelogioPropostas()).Handle(new(nova,new(1,new(2026,11,10),FinanceiroDados(c,ramo))),default);
        var parcelas=await fixture.DbContext.PropostaMovimentos.Where(m=>m.Proposta!.Apolice!.PublicId==apolice).OrderBy(m=>m.Parcela).ToArrayAsync();
        Assert.Equal(new[] { 202611,202612 },parcelas.Select(m=>m.CompetenciaInt!.Value));
        Assert.Equal(new[] { 1,2 },parcelas.Select(m=>m.Parcela!.Value));
        Assert.Equal(c.Valores.TotalMensal,parcelas[1].PremioTotal!.Value.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
        Assert.NotNull((await fixture.DbContext.ParcelasCompetencias.SingleAsync(c=>c.Apolice.PublicId==apolice && c.Competencia==202611)).FechadaEm);
    }
    [Fact]
    public async Task FinanceiroAuditoriaFalhaRevertePlanoParcelaEConfirmacao()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync(); var ramo = await RamoSimulacaoAsync(id);
        var c = await ConferirAsync(id, ramo, new(2026, 11, 1));
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ConfirmadorFinanceiro(codigo, new RelogioPropostas(), falha.Object).Handle(new(id, new(1, new(2026, 11, 1), FinanceiroDados(c, ramo))), default));
        await using var db = LeituraNovaConexao();
        Assert.False(await db.PropostasFinanceiro.AnyAsync(f => f.Proposta.PublicId == id));
        Assert.False(await db.PropostaMovimentos.AnyAsync(m => m.Proposta!.PublicId == id));
        Assert.Null((await db.Proposta.SingleAsync(p => p.PublicId == id)).ConfirmadaEm);
    }
}

public sealed class CalendarioParcelasTests
{
    [Theory]
    [InlineData(202602, 202601, "2026-01-31", "2026-02-28")]
    [InlineData(202603, 202601, "2026-01-31", "2026-03-31")]
    [InlineData(202802, 202801, "2028-02-29", "2028-03-29")]
    [InlineData(202612, 202611, "2026-10-31", "2026-11-30")]
    public void ProjetaSemPerderDiaOriginal(int alvo, int referencia, string vencimento, string esperado) =>
        Assert.Equal(DateOnly.Parse(esperado), CalendarioParcelas.Vencimento(alvo, referencia, DateOnly.Parse(vencimento)));
    [Theory]
    [InlineData(0)][InlineData(202600)][InlineData(202613)][InlineData(1000001)]
    public void RecusaCompetenciaInvalida(int competencia) => Assert.Throws<ValidacaoException>(() => CalendarioParcelas.Inicio(competencia));
}
