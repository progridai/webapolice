using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ConfirmarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EfetivarVigencia;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private sealed class RelogioPropostas : TimeProvider
    {
        public DateTimeOffset Agora { get; set; } = new(2026, 11, 12, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Agora;
    }
    private static IOptions<ConfiguracaoPropostas> ConfigConfirmacao(string codigo, bool processar = true) =>
        Options.Create(new ConfiguracaoPropostas { StatusInicialCodigo = codigo, StatusCanceladaCodigo = "CANCELADA", StatusDescartadaCodigo = "DESCARTADA", StatusConfirmadaCodigo = "CONFIRMADA", ProcessarAgendamentos = processar });
    private ConfirmarPropostaHandler Confirmador(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)),
        new ClientesPropostas(fixture.Cadastro), Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), ConfigConfirmacao(codigo), relogio);
    private EfetivarVigenciaPropostaHandler Efetivador(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)),
        new ClientesPropostas(fixture.Cadastro), Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), ConfigConfirmacao(codigo), relogio);
    private async Task<(Guid Id, string Codigo, Guid Cobertura)> ProntaConfirmarAsync()
    {
        var r = await RascunhoAsync();
        await Preenchedor(r.Codigo).Handle(new(r.Id, Dados(r.Cobertura) with { Beneficiarios = [new(null, "MARIA d’ÁVILA", "FILHA", 100)] }), default);
        fixture.DbContext.ChangeTracker.Clear();
        return r;
    }

    [Fact]
    public async Task ConfirmaComDataInformadaSemFinanceiroENaoPermiteRepetirOuEditar()
    {
        var (id, codigo, cobertura) = await ProntaConfirmarAsync(); var relogio = new RelogioPropostas();
        var comando = new ConfirmarPropostaCommand(id, new(1, new(2026, 11, 1)));
        var resultado = await Confirmador(codigo, relogio).Handle(comando, default);
        Assert.True(resultado.Vigente); Assert.False(resultado.Agendada); Assert.Equal(2, resultado.RevisaoPreenchimento);
        fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => Confirmador(codigo, relogio).Handle(comando, default));
        await Assert.ThrowsAsync<ValidacaoException>(() => Preenchedor(codigo).Handle(new(id, Dados(cobertura, 2)), default));
        await using var leitura = LeituraNovaConexao();
        var p = await leitura.Proposta.Include(p => p.Status).Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        Assert.Equal("CONFIRMADA", p.Status.Codigo); Assert.True(p.Vigente); Assert.Equal(new DateOnly(2026, 11, 1), p.DataInicioVigencia);
        Assert.Equal(relogio.Agora.UtcDateTime, p.ConfirmadaEm); Assert.Null(p.DataInclusao); Assert.Null(p.DataMovimento);
        Assert.True(p.ApoliceVida!.Ativo); Assert.Equal("ativa", p.ApoliceVida.Status); Assert.Null(p.ApoliceVida.DataInicioVigencia);
        Assert.Null(p.PremioLiquido); Assert.Null(p.ValorParcela); Assert.Null(p.IofValor);
        Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.PropostaId == p.Id));
        Assert.Equal(3, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString()));
        Assert.Equal(15.12m, (await leitura.PropostaCoberturas.SingleAsync(c => c.PublicId == cobertura)).PremioTitular);
    }

    [Theory]
    [InlineData("beneficiarios")] [InlineData("plano")] [InlineData("cobertura")]
    [InlineData("modulo")] [InlineData("vida")] [InlineData("apolice")]
    [InlineData("vinculo")] [InlineData("revisao")] [InlineData("data")]
    public async Task RecusaConfirmacaoIncompletaOuIncompativelSemMudarSituacao(string tipo)
    {
        var (id, codigo, _) = await ProntaConfirmarAsync();
        var p = await fixture.DbContext.Proposta.Include(p => p.PropostaBeneficiarios).Include(p => p.PropostaCoberturas)
            .Include(p => p.Apolice).Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        if (tipo == "beneficiarios") p.PropostaBeneficiarios.Single().PercentualParticipacao = 99.9999m;
        if (tipo == "plano") (await fixture.DbContext.ApoliceModuloPlanos.SingleAsync(x => x.ApoliceModuloId == p.ApoliceModuloId)).Ativo = false;
        if (tipo == "cobertura") p.PropostaCoberturas.Single().Ativo = false;
        if (tipo == "modulo") p.ApoliceVida!.ApoliceModuloId = null;
        if (tipo == "vida") p.ApoliceVida!.Ativo = false;
        if (tipo == "apolice") p.Apolice!.Ativo = false;
        if (tipo == "vinculo")
        {
            var vinculo = await fixture.Cadastro.Vinculos.SingleAsync(v => v.Id == p.ClienteVinculoId);
            fixture.Cadastro.Entry(vinculo).Property(v => v.Ativo).CurrentValue = false; await fixture.Cadastro.SaveChangesAsync();
        }
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var dados = new ConfirmarPropostaDados(tipo == "revisao" ? 0 : 1, tipo == "data" ? new(2025, 12, 31) : new(2026, 11, 12));
        await Assert.ThrowsAsync<ValidacaoException>(() => Confirmador(codigo, new RelogioPropostas()).Handle(new(id, dados), default));
        await using var leitura = LeituraNovaConexao();
        p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.False(p.Vigente); Assert.Null(p.ConfirmadaEm); Assert.Null(p.DataInicioVigencia); Assert.Equal(1, p.RevisaoPreenchimento);
    }

    private async Task<(Guid Origem, Guid Nova, string Codigo)> SubstituicaoAsync(RelogioPropostas relogio)
    {
        var (id, codigo, _) = await ProntaConfirmarAsync();
        await Confirmador(codigo, relogio).Handle(new(id, new(1, new(2026, 11, 1))), default);
        var anterior = await fixture.DbContext.Proposta.Include(p => p.Apolice).Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        var comando = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta.CriarPropostaCommand {
            ClienteId = anterior.ClienteId, ApoliceId = anterior.Apolice!.PublicId, ApoliceVidaId = anterior.ApoliceVida!.PublicId, PropostaOrigemPublicId = id };
        var nova = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(comando, default);
        fixture.DbContext.ChangeTracker.Clear();
        var cobertura = await fixture.DbContext.PropostaCoberturas.Where(c => c.Proposta.PublicId == nova).Select(c => c.PublicId).SingleAsync();
        await Preenchedor(codigo).Handle(new(nova, Dados(cobertura) with { Beneficiarios = [new(null, "Outro Beneficiário", "Filho", 100)] }), default);
        fixture.DbContext.ChangeTracker.Clear();
        return (id, nova, codigo);
    }

    [Fact]
    public async Task AgendaPreservaAnteriorEApenasAtivaNaDataNoFusoDeSaoPaulo()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        var resultado = await Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 13))), default);
        Assert.True(resultado.Agendada); Assert.False(resultado.Vigente);
        fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
        relogio.Agora = new(2026, 11, 13, 2, 59, 0, TimeSpan.Zero); // Ainda 12/11 em São Paulo.
        await Assert.ThrowsAsync<ValidacaoException>(() => Efetivador(codigo, relogio).Handle(new(nova, 2), default));
        relogio.Agora = new(2026, 11, 13, 3, 0, 0, TimeSpan.Zero);
        var ativacao = await Efetivador(codigo, relogio).Handle(new(nova, 2), default);
        Assert.True(ativacao.Ativada); Assert.False(ativacao.JaEfetivada);
        fixture.DbContext.ChangeTracker.Clear();
        var repetida = await Efetivador(codigo, relogio).Handle(new(nova, 2), default);
        Assert.True(repetida.JaEfetivada);
        await using var leitura = LeituraNovaConexao();
        var anterior = await leitura.Proposta.Include(p => p.Status).SingleAsync(p => p.PublicId == origem);
        var atual = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.False(anterior.Vigente); Assert.True(atual.Vigente); Assert.Equal(new DateOnly(2026, 11, 13), anterior.DataFimVigencia);
        Assert.Equal(relogio.Agora.UtcDateTime, anterior.SubstituidaEm); Assert.Equal("CONFIRMADA", anterior.Status.Codigo);
        Assert.Equal(3, atual.RevisaoPreenchimento); Assert.Equal(3, anterior.RevisaoPreenchimento);
        Assert.Equal(3, await leitura.PropostaHistoricos.CountAsync(h => h.PropostaNovaId == atual.Id));
        Assert.Equal(4, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == nova.ToString()));
    }

    [Fact]
    public async Task PendenciaPreservaAnteriorNaoDuplicaAuditoriaEPermiteNovaTentativa()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        await Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 13))), default);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Propostas.Any(p => p.PublicId == nova));
        vida.Ativo = false; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        relogio.Agora = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero); // Processa atraso após reinício.
        var pendente = await Efetivador(codigo, relogio).Handle(new(nova, 2), default);
        Assert.False(pendente.Ativada); Assert.NotNull(pendente.PendenciaVigencia); Assert.Equal(3, pendente.RevisaoPreenchimento);
        fixture.DbContext.ChangeTracker.Clear();
        var repetida = await Efetivador(codigo, relogio).Handle(new(nova, 3), default);
        Assert.Equal(3, repetida.RevisaoPreenchimento);
        Assert.Equal(4, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == nova.ToString()));
        await using (var leitura = LeituraNovaConexao()) Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
        fixture.DbContext.ChangeTracker.Clear();
        vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Id == vida.Id); vida.Ativo = true; await fixture.DbContext.SaveChangesAsync();
        fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await Efetivador(codigo, relogio).Handle(new(nova, 3), default)).Ativada);
        await using var fim = LeituraNovaConexao();
        var atual = await fim.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.Null(atual.MotivoPendenciaVigencia); Assert.Null(atual.PendenciaVigenciaEm); Assert.True(atual.Vigente);
    }

    [Fact]
    public async Task AuditoriaFalhaReverteAtivacaoEEncerramentoDaAnterior()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Confirmador(codigo, relogio, falha.Object).Handle(new(nova, new(1, new(2026, 11, 12))), default));
        await using var leitura = LeituraNovaConexao();
        var anterior = await leitura.Proposta.SingleAsync(p => p.PublicId == origem);
        var atual = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.True(anterior.Vigente); Assert.Null(anterior.SubstituidaEm); Assert.Null(anterior.DataFimVigencia);
        Assert.False(atual.Vigente); Assert.Null(atual.ConfirmadaEm); Assert.Equal(1, atual.RevisaoPreenchimento);
        Assert.Equal(1, await leitura.PropostaHistoricos.CountAsync(h => h.PropostaNovaId == atual.Id));
    }

    private ServiceProvider ServicosAgendamento()
    {
        var services = new ServiceCollection();
        services.AddScoped<System.Data.Common.DbConnection>(_ => new NpgsqlConnection(fixture.ConnectionString));
        services.AddScoped(sp => new SeguroDbContext(new DbContextOptionsBuilder<SeguroDbContext>().UseNpgsql(sp.GetRequiredService<System.Data.Common.DbConnection>()).UseSnakeCaseNamingConvention().Options));
        services.AddScoped(sp => new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(sp.GetRequiredService<System.Data.Common.DbConnection>()).UseSnakeCaseNamingConvention().Options));
        services.AddScoped(sp => new AuditoriaDbContext(new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(sp.GetRequiredService<System.Data.Common.DbConnection>()).UseSnakeCaseNamingConvention().Options));
        services.AddScoped<IClientesPropostas, ClientesPropostas>(); services.AddScoped<IPropostasRepository, PropostasRepository>();
        services.AddScoped<IRegistradorAuditoria, RegistradorAuditoria>();
        services.AddScoped<ICadastroSeguroQueries>(sp => Setup.SeguroPersistenciaTestes.ConsultasCadastro(sp.GetRequiredService<SeguroDbContext>()));
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SubstituicaoImediataPreservaCamposLegadosETextosIntocados()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        await fixture.DbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE seguro.proposta SET legado_id=772211,observacao='EVIDÊNCIA ORIGINAL',confirmada_em=NULL,data_inicio_vigencia=NULL WHERE public_id={origem}");
        fixture.DbContext.ChangeTracker.Clear();
        var antes = await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5((to_jsonb(p)-'vigente'-'substituida_em'-'data_fim_vigencia'-'updated_at'-'revisao_preenchimento')::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={origem}").SingleAsync();
        Assert.True((await Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 10))), default)).Vigente);
        await using var leitura = LeituraNovaConexao();
        var depois = await leitura.Database.SqlQuery<string>($"SELECT md5((to_jsonb(p)-'vigente'-'substituida_em'-'data_fim_vigencia'-'updated_at'-'revisao_preenchimento')::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={origem}").SingleAsync();
        Assert.Equal(antes, depois);
        var anterior = await leitura.Proposta.SingleAsync(p => p.PublicId == origem);
        Assert.Equal("EVIDÊNCIA ORIGINAL", anterior.Observacao); Assert.Null(anterior.ConfirmadaEm); Assert.False(anterior.Vigente);
        Assert.Equal(new DateOnly(2026, 11, 10), anterior.DataFimVigencia);
    }

    [Fact]
    public async Task BancoImpedeDuasNativasVigentesNaMesmaVidaMesmoPorSqlDireto()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, _) = await SubstituicaoAsync(relogio);
        await using var escrita = LeituraNovaConexao();
        var ex = await Assert.ThrowsAsync<PostgresException>(() => escrita.Database.ExecuteSqlInterpolatedAsync($"UPDATE seguro.proposta SET vigente=true WHERE public_id={nova}"));
        Assert.Equal("ux_proposta_nativa_vigente_vida", ex.ConstraintName);
        Assert.True((await escrita.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
        Assert.False((await escrita.Proposta.SingleAsync(p => p.PublicId == nova)).Vigente);
    }

    [Fact]
    public async Task NaoPermiteRetroagirAntesDaOrigemNemAtivarForaDoPeriodoAtual()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        await Assert.ThrowsAsync<ValidacaoException>(() => Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 10, 31))), default));
        fixture.DbContext.ChangeTracker.Clear();
        await Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 13))), default);
        var apolice = await fixture.DbContext.Apolices.SingleAsync(a => a.Propostas.Any(p => p.PublicId == nova));
        apolice.DataFimVigencia = new(2026, 11, 13); await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        relogio.Agora = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
        var resultado = await Efetivador(codigo, relogio).Handle(new(nova, 2), default);
        Assert.False(resultado.Ativada); Assert.NotNull(resultado.PendenciaVigencia);
        await using var leitura = LeituraNovaConexao();
        Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
        Assert.False((await leitura.Proposta.SingleAsync(p => p.PublicId == nova)).Vigente);
    }

    [Fact]
    public async Task ProcessadorReiniciavelAtivaAgendamentoComIdentidadeTecnicaSemDuplicar()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        await Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 13))), default);
        relogio.Agora = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
        await using var services = ServicosAgendamento();
        var processador = new ProcessadorVigenciasPropostas(services.GetRequiredService<IServiceScopeFactory>(), ConfigConfirmacao(codigo), relogio, NullLogger<ProcessadorVigenciasPropostas>.Instance);
        await Task.WhenAll(processador.ProcessarAsync(default), processador.ProcessarAsync(default));
        await processador.ProcessarAsync(default);
        await using var leitura = LeituraNovaConexao();
        Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == nova)).Vigente);
        Assert.False((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
        Assert.Equal(1, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == nova.ToString() && a.UsuarioIdExterno == "sistema:vigencias-propostas"));
    }

    [Fact]
    public async Task ConfirmacaoBloqueiaMudancaConcorrenteDeVidaIdentidadeEModuloAteAuditoriaConcluir()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync(); var relogio = new RelogioPropostas();
        var p = await fixture.DbContext.Proposta.Include(p => p.ApoliceVida).ThenInclude(v => v!.ApoliceModulo).SingleAsync(p => p.PublicId == id);
        var vidaId = p.ApoliceVidaId!.Value; var vinculoId = p.ClienteVinculoId; var moduloId = p.ApoliceVida!.ApoliceModulo!.ModuloId;
        fixture.DbContext.ChangeTracker.Clear();
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var registrador = new Mock<IRegistradorAuditoria>();
        registrador.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) => {
                chegou.TrySetResult(); await liberar.Task.WaitAsync(ct); await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct);
            });
        var confirmacao = Confirmador(codigo, relogio, registrador.Object).Handle(new(id, new(1, new(2026, 11, 12))), default);
        try
        {
            await chegou.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await using var escrita = LeituraNovaConexao();
            await escrita.Database.OpenConnectionAsync();
            await escrita.Database.ExecuteSqlRawAsync("SET lock_timeout='200ms'");
            var vida = await Assert.ThrowsAsync<PostgresException>(() => escrita.Database.ExecuteSqlInterpolatedAsync($"UPDATE seguro.apolice_vida SET ativo=false WHERE id={vidaId}"));
            Assert.Equal("55P03", vida.SqlState);
            var vinculo = await Assert.ThrowsAsync<PostgresException>(() => escrita.Database.ExecuteSqlInterpolatedAsync($"UPDATE cadastro.cliente_vinculo SET ativo=false WHERE id={vinculoId}"));
            Assert.Equal("55P03", vinculo.SqlState);
            var modulo = await Assert.ThrowsAsync<PostgresException>(() => escrita.Database.ExecuteSqlInterpolatedAsync($"UPDATE cadastro.modulo SET ativo=false WHERE id={moduloId}"));
            Assert.Equal("55P03", modulo.SqlState);
        }
        finally { liberar.TrySetResult(); await confirmacao; }
        await using var leitura = LeituraNovaConexao();
        Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == id)).Vigente);
        Assert.True((await leitura.ApoliceVidas.SingleAsync(v => v.Id == vidaId)).Ativo);
    }
}
