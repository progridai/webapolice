using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.PreencherRascunho;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private PreencherRascunhoHandler Preenchedor(string codigo, IRegistradorAuditoria? registrador = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)),
        Operacao(registrador ?? new RegistradorAuditoria(fixture.Auditoria)), Options.Create(new ConfiguracaoPropostas { StatusInicialCodigo = codigo }));

    private async Task<(Guid Id, string Codigo, Guid Cobertura)> RascunhoAsync()
    {
        var (command, codigo, _) = await PrepararModuloAsync();
        var id = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(command, default);
        fixture.DbContext.ChangeTracker.Clear();
        var cobertura = await fixture.DbContext.PropostaCoberturas.Where(c => c.Proposta.PublicId == id).Select(c => c.PublicId).SingleAsync();
        return (id, codigo, cobertura);
    }

    private static PreencherRascunhoDados Dados(Guid cobertura, int revisao = 0, decimal premio = 15.12m) => new(revisao, "OBSERVAÇÃO DA FAMÍLIA",
        [new(null, "MARIA d’ÁVILA", "FILHA DO TITULAR", 25.1234m)], [new(cobertura, premio, 0, "AJUSTE DE PROTEÇÃO")], false);

    [Fact]
    public async Task PreencheRascunhoAjustaComJustificativaPreservaBaseENaoAtivaFinanceiro()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        Assert.Equal(1, await Preenchedor(codigo).Handle(new(id, Dados(cobertura)), default));
        fixture.DbContext.ChangeTracker.Clear();
        var detalhe = await new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)).ObterDetalheAsync(id, default, codigo);
        Assert.True(detalhe!.PodeEditarRascunho); Assert.Equal(1, detalhe.RevisaoPreenchimento);
        var b = Assert.Single(detalhe.Beneficiarios);
        Assert.Equal("Maria d’Ávila", b.Nome); Assert.Equal("Filha do Titular", b.Parentesco); Assert.True(b.PodeAlterar);
        Assert.NotEqual(Guid.Empty, b.PublicId); Assert.Equal(25.1234m, b.PercentualParticipacao);
        var c = Assert.Single(detalhe.Coberturas);
        Assert.Equal(15.12m, c.PremioTitular); Assert.Equal(12.34m, c.PremioTitularInicial);
        Assert.Equal(0m, c.PremioConjugeInicial); Assert.Equal("Ajuste de Proteção", c.JustificativaAjustePremio);
        Assert.True(c.PremioAjustado); Assert.Equal("Proteção da Família", c.NomeRegistrado);
        Assert.False(detalhe.Vigente); Assert.Null(detalhe.PremioLiquido); Assert.Null(detalhe.ValorParcela);
        var segunda = Dados(cobertura, 1, 20m) with { Beneficiarios = [new(b.PublicId, "MARIA d’ÁVILA", "FILHA", 50)] };
        await Preenchedor(codigo).Handle(new(id, segunda), default);
        await using var leitura = LeituraNovaConexao();
        var atual = await leitura.PropostaCoberturas.SingleAsync(c => c.PublicId == cobertura);
        Assert.Equal(12.34m, atual.PremioTitularInicial); Assert.Equal(20m, atual.PremioTitular);
        Assert.Equal(12.34m, (await leitura.ApoliceModuloCoberturas.SingleAsync(c => c.Id == atual.ApoliceModuloCoberturaId)).PremioTitular);
        Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.Proposta != null && m.Proposta.PublicId == id));
        Assert.Equal(3, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString()));
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(100.0001)] [InlineData(0.00001)]
    public async Task RecusaPercentuaisInvalidosSemAlterarRevisao(decimal percentual)
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        var dados = Dados(cobertura) with { Beneficiarios = [new(null, "Nome", "Filha", percentual)] };
        await Assert.ThrowsAsync<ValidacaoException>(() => Preenchedor(codigo).Handle(new(id, dados), default));
        await using var leitura = LeituraNovaConexao();
        Assert.Equal(0, (await leitura.Proposta.SingleAsync(p => p.PublicId == id)).RevisaoPreenchimento);
        Assert.False(await leitura.PropostaBeneficiarios.AnyAsync(b => b.Proposta.PublicId == id));
    }

    [Fact]
    public async Task ValidaSomaCamposObrigatoriosCoberturaEJustificativa()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        var original = Dados(cobertura);
        var invalidos = new[] {
            original with { Beneficiarios = [new(null, "Nome", "Filha", 60), new(null, "Outro", "Filho", 50)] },
            original with { Beneficiarios = [new(null, " ", "Filha", 50)] },
            original with { Beneficiarios = [new(null, "Nome", " ", 50)] },
            original with { Beneficiarios = [new(Guid.NewGuid(), "Nome", "Filha", 50)] },
            original with { Coberturas = [new(Guid.NewGuid(), 20, 0, "Motivo")] },
            original with { Coberturas = [new(cobertura, 20, 0, " ")] },
            original with { Coberturas = [new(cobertura, -1, 0, "Motivo")] },
            original with { Coberturas = [new(cobertura, 1.001m, 0, "Motivo")] },
            original with { RevisaoEsperada = null },
            original with { Beneficiarios = null }, original with { Coberturas = null }
        };
        foreach (var dados in invalidos)
        {
            fixture.DbContext.ChangeTracker.Clear();
            await Assert.ThrowsAsync<ValidacaoException>(() => Preenchedor(codigo).Handle(new(id, dados), default));
        }
        await using var leitura = LeituraNovaConexao();
        Assert.Equal(12.34m, (await leitura.PropostaCoberturas.SingleAsync(c => c.PublicId == cobertura)).PremioTitular);
        Assert.Equal(0, (await leitura.Proposta.SingleAsync(p => p.PublicId == id)).RevisaoPreenchimento);
    }

    [Fact]
    public async Task RemoveSomenteNativosAoSalvarEPreservaLegadoContandoSuaParticipacao()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id);
        var legado = new PropostaBeneficiario { PropostaId = p.Id, LegadoId = 991, Nome = "Beneficiário Anterior", ParentescoOriginal = "FILHA ORIGINAL", PercentualParticipacao = 80, CpfOriginal = "CPF ORIGINAL" };
        fixture.DbContext.Add(legado); await fixture.DbContext.SaveChangesAsync();
        fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => Preenchedor(codigo).Handle(new(id, Dados(cobertura)), default));
        var dados = Dados(cobertura) with { Beneficiarios = [new(null, "Nome Novo", "Filho", 20)] };
        await Preenchedor(codigo).Handle(new(id, dados), default);
        fixture.DbContext.ChangeTracker.Clear();
        var novo = await fixture.DbContext.PropostaBeneficiarios.SingleAsync(b => b.PropostaId == p.Id && b.LegadoId == null);
        await Assert.ThrowsAsync<ValidacaoException>(() => Preenchedor(codigo).Handle(new(id, Dados(cobertura, 1) with { Beneficiarios = [new(legado.PublicId, "Outro", "Filho", 20)] }), default));
        fixture.DbContext.ChangeTracker.Clear();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura, 1) with { Beneficiarios = [] }), default);
        await using var leitura = LeituraNovaConexao();
        Assert.NotNull((await leitura.PropostaBeneficiarios.SingleAsync(b => b.PublicId == novo.PublicId)).DeletedAt);
        var preservado = await leitura.PropostaBeneficiarios.SingleAsync(b => b.PublicId == legado.PublicId);
        Assert.Null(preservado.DeletedAt); Assert.Equal("FILHA ORIGINAL", preservado.ParentescoOriginal); Assert.Equal("CPF ORIGINAL", preservado.CpfOriginal);
    }

    [Fact]
    public async Task FalhaAuditoriaReverteBeneficiariosAjustesERevisao()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Preenchedor(codigo, falha.Object).Handle(new(id, Dados(cobertura)), default));
        await using var leitura = LeituraNovaConexao();
        Assert.Equal(0, (await leitura.Proposta.SingleAsync(p => p.PublicId == id)).RevisaoPreenchimento);
        var c = await leitura.PropostaCoberturas.SingleAsync(c => c.PublicId == cobertura);
        Assert.Equal(12.34m, c.PremioTitular); Assert.False(c.PremioAjustado); Assert.Null(c.JustificativaAjustePremio);
        Assert.False(await leitura.PropostaBeneficiarios.AnyAsync(b => b.Proposta.PublicId == id));
    }

    [Fact]
    public async Task RecusaRevisaoAntigaEVigenteLegadoOuStatusForaDoRascunho()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura)), default);
        fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => Preenchedor(codigo).Handle(new(id, Dados(cobertura)), default));
        foreach (var tipo in new[] { "vigente", "legado", "invisivel", "excluida", "finalizador", "movimentacao", "configuracao" })
        {
            fixture.DbContext.ChangeTracker.Clear();
            var p = await fixture.DbContext.Proposta.Include(p => p.Status).SingleAsync(p => p.PublicId == id);
            p.Vigente = tipo == "vigente"; p.LegadoId = tipo == "legado" ? 887 : null;
            p.VisivelOperacional = tipo != "invisivel"; p.DeletedAt = tipo == "excluida" ? DateTime.UtcNow : null;
            p.Status.Finalizador = tipo == "finalizador"; p.Status.PermiteMovimentacao = tipo == "movimentacao";
            await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
            await Assert.ThrowsAsync<ValidacaoException>(() => Preenchedor(tipo == "configuracao" ? "" : codigo).Handle(new(id, Dados(cobertura, 1)), default));
        }
    }

    [Fact]
    public async Task DuasConexoesComMesmaRevisaoNaoSobrescrevemPreenchimento()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        async Task<bool> Salvar(string observacao)
        {
            await using var connection = new Npgsql.NpgsqlConnection(fixture.ConnectionString); await connection.OpenAsync();
            await using var db = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var cadastro = new WebApolice.Modulos.Cadastro.Infrastructure.Persistence.CadastroDbContext(new DbContextOptionsBuilder<WebApolice.Modulos.Cadastro.Infrastructure.Persistence.CadastroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var auditoria = new AuditoriaDbContext(new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            var handler = new PreencherRascunhoHandler(new PropostasRepository(db, new ClientesPropostas(cadastro)),
                new WebApolice.Modulos.Seguro.Infrastructure.Persistence.OperacaoPropostas(db, new OperacaoAuditada(auditoria, new RegistradorAuditoria(auditoria), Mock.Of<IContextoAuditoria>())),
                Options.Create(new ConfiguracaoPropostas { StatusInicialCodigo = codigo }));
            try { await handler.Handle(new(id, Dados(cobertura) with { Observacao = observacao }), default); return true; }
            catch (ValidacaoException) { return false; }
        }
        var resultados = await Task.WhenAll(Salvar("Primeira Tela"), Salvar("Segunda Tela"));
        Assert.Single(resultados, r => r); Assert.Single(resultados, r => !r);
        await using var leitura = LeituraNovaConexao();
        Assert.Equal(1, (await leitura.Proposta.SingleAsync(p => p.PublicId == id)).RevisaoPreenchimento);
        Assert.Equal(1, await leitura.PropostaBeneficiarios.CountAsync(b => b.Proposta.PublicId == id));
        Assert.Equal(2, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString()));
    }
}
