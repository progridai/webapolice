using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.SimularValorMensal;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.Financeiro;

public sealed record FinanceiroConfirmacaoDados(int? CompetenciaInicial, DateOnly? VencimentoInicial, Guid? RamoPublicId, string? Conferencia);
public sealed record ConferenciaFinanceiraDados(int? RevisaoEsperada, DateOnly? DataInicioVigencia, int? CompetenciaInicial, DateOnly? VencimentoInicial, Guid? RamoPublicId);
public sealed record ConferenciaFinanceiraResultado(string Conferencia, int CompetenciaInicial, DateOnly VencimentoInicial,
    int CompetenciaReferencia, DateOnly VencimentoReferencia, SimulacaoMensalResultado Valores);
public sealed record ConferirFinanceiroQuery(Guid PublicId, ConferenciaFinanceiraDados Dados) : IRequest<ConferenciaFinanceiraResultado>;
public sealed record AtualizarFinanceiroDados(int? RevisaoEsperada, FinanceiroConfirmacaoDados Financeiro);
public sealed record AtualizarFinanceiroCommand(Guid PublicId, AtualizarFinanceiroDados Dados) : IRequest<int>;

public static class CalendarioParcelas
{
    public static int Competencia(DateOnly data) => data.Year * 100 + data.Month;
    public static DateOnly Inicio(int competencia)
    {
        if (competencia is < 101 or > 999912 || competencia % 100 is < 1 or > 12)
            throw new ValidacaoException("Informe uma competência válida.");
        return new(competencia / 100, competencia % 100, 1);
    }
    public static int Proxima(int competencia)
    {
        var inicio = Inicio(competencia);
        if (competencia == 999912) throw new ValidacaoException("A competência atingiu o limite do calendário.");
        return Competencia(inicio.AddMonths(1));
    }
    public static DateOnly Vencimento(int competencia, int referencia, DateOnly vencimento)
    {
        var alvo = Inicio(competencia); var origem = Inicio(referencia);
        var meses = (alvo.Year - origem.Year) * 12 + alvo.Month - origem.Month;
        try
        {
            var mes = new DateOnly(vencimento.Year, vencimento.Month, 1).AddMonths(meses);
            return new(mes.Year, mes.Month, Math.Min(vencimento.Day, DateTime.DaysInMonth(mes.Year, mes.Month)));
        }
        catch (ArgumentOutOfRangeException) { throw new ValidacaoException("O vencimento projetado ultrapassa o calendário permitido."); }
    }
}

internal static class FinanceiroConferencia
{
    internal static string Assinar(object valor) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(valor))));
    internal static string Condicoes(Propostum p, DateOnly inicio) => Assinar(new { p.PublicId, Inicio = inicio, p.IncluiConjuge,
        p.ApoliceModuloId, p.ApoliceSubgrupoId,
        Itens = p.PropostaItems.Where(i => i.Ativo).OrderBy(i => i.PublicId).Select(i => new { i.PublicId, i.ApoliceModuloPlanoId }),
        Coberturas = p.PropostaCoberturas.Where(c => c.Ativo).OrderBy(c => c.PublicId).Select(c => new { c.PublicId, c.CoberturaId, PremioTitular = c.PremioTitular?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), PremioConjuge = c.PremioConjuge?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) }) });
    internal static void Elegivel(Propostum p)
    {
        if (p.LegadoId != null || p.ApoliceId == null || p.ApoliceVidaId == null || p.DeletedAt != null || !p.VisivelOperacional ||
            p.Vigente || p.SubstituidaEm != null || p.CanceladaEm != null || p.DescartadaEm != null || p.CancelamentoSolicitadoEm != null ||
            p.Status.Finalizador || p.Status.PermiteMovimentacao || !p.Status.VisivelOperacional)
            throw new ValidacaoException("Confira o financeiro de uma pré proposta nativa ou proposta confirmada ainda sem vigência.");
    }
    internal static async Task<ConferenciaFinanceiraResultado> ConferirAsync(Propostum p, ConferenciaFinanceiraDados dados,
        IPropostasRepository propostas, IParcelasPropostasRepository financeiro, TimeProvider relogio, CancellationToken ct)
    {
        Elegivel(p);
        if (dados.DataInicioVigencia == null || dados.CompetenciaInicial == null || dados.VencimentoInicial == null ||
            dados.RevisaoEsperada != p.RevisaoPreenchimento || dados.RevisaoEsperada == null)
            throw new ValidacaoException("Informe início, competência, vencimento e revisão para conferir os valores.");
        if (p.ConfirmadaEm != null && p.DataInicioVigencia != dados.DataInicioVigencia)
            throw new ValidacaoException("Use o início confirmado; altere o agendamento pela operação própria.");
        VigenciaPropostas.ValidarDatas(p, dados.DataInicioVigencia.Value, VigenciaPropostas.Hoje(relogio), false);
        var competencia = dados.CompetenciaInicial.Value;
        CalendarioParcelas.Inicio(competencia);
        if (competencia < CalendarioParcelas.Competencia(dados.DataInicioVigencia.Value))
            throw new ValidacaoException("A competência não pode anteceder o mês de início da cobertura.");
        if ((await financeiro.ObterCompetenciasAsync(p.ApoliceId!.Value, ct)).Any(c => c.FechadaEm != null && c.Competencia >= competencia))
            throw new ValidacaoException("A competência inicial já pertence a um ciclo fechado. Escolha uma competência posterior ao último fechamento.");
        var anteriores = await financeiro.ObterPlanosAsync(p.ApoliceId!.Value, ct);
        var referencia = anteriores.Where(f => f.Proposta.ApoliceVidaId == p.ApoliceVidaId && f.PropostaId != p.Id && f.Proposta.ConfirmadaEm != null)
            .OrderBy(f => f.ConferidaEm).ThenBy(f => f.Id).FirstOrDefault();
        var competenciaReferencia = referencia?.CompetenciaReferencia ?? competencia;
        var vencimentoReferencia = referencia?.VencimentoReferencia ?? dados.VencimentoInicial.Value;
        if (competencia < competenciaReferencia || CalendarioParcelas.Vencimento(competencia, competenciaReferencia, vencimentoReferencia) != dados.VencimentoInicial)
            throw new ValidacaoException("Preserve a competência inicial de controle e a referência original de vencimentos da Vida.");
        var valores = await new SimularValorMensalHandler(propostas, relogio).Handle(new(p.PublicId, new(dados.RevisaoEsperada, dados.RamoPublicId)), ct);
        var token = Assinar(new { p.PublicId, p.RevisaoPreenchimento, Condicoes = Condicoes(p, dados.DataInicioVigencia.Value),
            competencia, dados.VencimentoInicial, competenciaReferencia, vencimentoReferencia, valores.Ramo,
            valores.LiquidoTotal, valores.Iof, valores.TotalMensal });
        return new(token, competencia, dados.VencimentoInicial.Value, competenciaReferencia, vencimentoReferencia, valores);
    }
    internal static async Task SalvarAsync(Propostum p, DateOnly inicio, FinanceiroConfirmacaoDados dados, IPropostasRepository propostas,
        IParcelasPropostasRepository financeiro, TimeProvider relogio, CancellationToken ct)
    {
        var conferido = await ConferirAsync(p, new(p.RevisaoPreenchimento, inicio, dados.CompetenciaInicial, dados.VencimentoInicial, dados.RamoPublicId), propostas, financeiro, relogio, ct);
        if (conferido.Conferencia != dados.Conferencia) throw new ValidacaoException("As condições ou o Ramo mudaram. Confira os valores novamente antes de salvar.");
        var movimentos = await financeiro.ObterMovimentosAsync(p.ApoliceId!.Value, ct);
        if (movimentos.Any(m => m.PropostaFinanceiroId != null && m.PropostaId == p.Id))
            throw new ValidacaoException("O plano financeiro já possui parcelas. Prepare nova versão para alterar seus valores.");
        var ramo = (await propostas.ObterRamosSimulacaoAsync(p.ApoliceId.Value, ct)).Single(r => r.PublicId == dados.RamoPublicId);
        var plano = await financeiro.ObterPlanoAsync(p.Id, ct);
        if (plano == null) { plano = new PropostaFinanceiro { PropostaId = p.Id }; financeiro.Adicionar(plano); }
        plano.ApoliceRamoId = ramo.Id; plano.InicioConferido = inicio;
        plano.CompetenciaInicial = conferido.CompetenciaInicial; plano.VencimentoInicial = conferido.VencimentoInicial;
        plano.CompetenciaReferencia = conferido.CompetenciaReferencia; plano.VencimentoReferencia = conferido.VencimentoReferencia;
        plano.PremioLiquido = decimal.Parse(conferido.Valores.LiquidoTotal, System.Globalization.CultureInfo.InvariantCulture);
        plano.IofPercentual = conferido.Valores.Ramo.IofPercentual!.Value;
        plano.IofValor = decimal.Parse(conferido.Valores.Iof, System.Globalization.CultureInfo.InvariantCulture);
        plano.TotalMensal = decimal.Parse(conferido.Valores.TotalMensal, System.Globalization.CultureInfo.InvariantCulture);
        plano.CondicoesHash = Condicoes(p, inicio); plano.ConferidaEm = relogio.GetUtcNow().UtcDateTime;
        p.PremioLiquido = plano.PremioLiquido; p.IofPercentual = plano.IofPercentual; p.IofValor = plano.IofValor;
        p.ValorParcela = plano.TotalMensal; p.DataPrimeiroVencimento = plano.VencimentoInicial;
    }
}

public sealed class ConferirFinanceiroHandler(IPropostasRepository propostas, IParcelasPropostasRepository financeiro, TimeProvider relogio)
    : IRequestHandler<ConferirFinanceiroQuery, ConferenciaFinanceiraResultado>
{
    public async Task<ConferenciaFinanceiraResultado> Handle(ConferirFinanceiroQuery request, CancellationToken ct)
    {
        var p = await propostas.ObterParaComparacaoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
        return await FinanceiroConferencia.ConferirAsync(p, request.Dados, propostas, financeiro, relogio, ct);
    }
}

public sealed class AtualizarFinanceiroHandler(IPropostasRepository propostas, IParcelasPropostasRepository financeiro,
    IOperacaoPropostas operacao, TimeProvider relogio) : IRequestHandler<AtualizarFinanceiroCommand, int>
{
    public Task<int> Handle(AtualizarFinanceiroCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Conferir Financeiro da Proposta", async ct =>
        {
            var p = await propostas.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            if (p.ConfirmadaEm == null || p.DataInicioVigencia == null || p.RevisaoPreenchimento != request.Dados.RevisaoEsperada)
                throw new ValidacaoException("Recarregue uma proposta confirmada ainda sem vigência para conferir novamente.");
            await FinanceiroConferencia.SalvarAsync(p, p.DataInicioVigencia.Value, request.Dados.Financeiro, propostas, financeiro, relogio, ct);
            p.RevisaoPreenchimento = checked(p.RevisaoPreenchimento + 1); p.UpdatedAt = relogio.GetUtcNow().UtcDateTime;
            await propostas.SalvarAlteracoesAsync(ct);
            return (p.RevisaoPreenchimento, p.PublicId.ToString(), (object?)null, (object?)new { p.RevisaoPreenchimento, p.PremioLiquido, p.IofPercentual, p.IofValor, p.ValorParcela });
        }, cancellationToken);
}

public sealed record FinanceiroEstado(Guid? RamoPublicId, int? CompetenciaInicial, DateOnly? VencimentoInicial,
    int? CompetenciaReferencia, DateOnly? VencimentoReferencia, string? PremioLiquido, decimal? IofPercentual,
    string? Iof, string? TotalMensal, bool ConferenciaAtual);
public sealed record ObterFinanceiroQuery(Guid PublicId) : IRequest<FinanceiroEstado>;
public sealed class ObterFinanceiroHandler(IPropostasRepository propostas, IParcelasPropostasRepository repo) : IRequestHandler<ObterFinanceiroQuery, FinanceiroEstado>
{
    public async Task<FinanceiroEstado> Handle(ObterFinanceiroQuery request, CancellationToken ct)
    {
        var p = await propostas.ObterParaComparacaoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
        if (p.DeletedAt != null || !p.VisivelOperacional || !p.Status.VisivelOperacional) throw new ValidacaoException("Proposta indisponível.");
        var f = await repo.ObterPlanoAsync(p.Id, ct);
        var referencia = p.ApoliceId == null ? null : (await repo.ObterPlanosAsync(p.ApoliceId.Value, ct))
            .Where(v => v.Proposta.ApoliceVidaId == p.ApoliceVidaId && v.Proposta.ConfirmadaEm != null)
            .OrderBy(v => v.ConferidaEm).ThenBy(v => v.Id).FirstOrDefault();
        return new(f?.ApoliceRamo.PublicId, f?.CompetenciaInicial, f?.VencimentoInicial, referencia?.CompetenciaReferencia,
            referencia?.VencimentoReferencia, f == null ? null : CalculoMensalProposta.Moeda(f.PremioLiquido), f?.IofPercentual,
            f == null ? null : CalculoMensalProposta.Moeda(f.IofValor), f == null ? null : CalculoMensalProposta.Moeda(f.TotalMensal),
            f != null && p.DataInicioVigencia == f.InicioConferido && f.CondicoesHash == FinanceiroConferencia.Condicoes(p, f.InicioConferido));
    }
}
