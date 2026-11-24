using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.SimularValorMensal;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.Financeiro;

public sealed record ParcelaLoteDto(Guid PublicId, int Parcela, int Competencia, DateOnly Vencimento, string Total, string Aberto, int RevisaoBaixa, DateOnly? DataPagamento);
public sealed record VidaLoteDto(Guid? VidaPublicId, Guid PropostaPublicId, string? Numero, string? ClienteNome, string? Matricula, int Versao, string Situacao, string? Motivo,
    string NovaMensalidade, string SaldoAnterior, string TotalAcumulado, IReadOnlyList<ParcelaLoteDto> Parcelas);
public sealed record LoteMensalResultado(Guid ApolicePublicId, int Competencia, string Conferencia, DateTime? GeradaEm, DateTime? FechadaEm,
    bool PodeGerar, bool PodeFechar, IReadOnlyList<string> Impedimentos, IReadOnlyList<VidaLoteDto> Vidas);
public sealed record ConferirLoteQuery(Guid ApolicePublicId, int Competencia) : IRequest<LoteMensalResultado>;
public sealed record LoteMensalDados(int Competencia, string? Conferencia);
public sealed record GerarLoteCommand(Guid ApolicePublicId, LoteMensalDados Dados) : IRequest<LoteMensalResultado>;
public sealed record FecharLoteCommand(Guid ApolicePublicId, LoteMensalDados Dados) : IRequest<LoteMensalResultado>;
public sealed record BaixarParcelaDados(int? RevisaoEsperada, DateOnly? DataPagamento);
public sealed record BaixarParcelaCommand(Guid ApolicePublicId, Guid ParcelaPublicId, BaixarParcelaDados Dados) : IRequest<bool>;
internal sealed record EstadoLote(ApoliceModel Apolice, IReadOnlyList<PropostaFinanceiro> Planos, IReadOnlyList<PropostaMovimento> Movimentos,
    IReadOnlyList<ParcelaCompetencia> Competencias, IReadOnlyList<PropostaFinanceiro> Incluir, LoteMensalResultado Resultado);

internal static class ConferenciaLote
{
    internal static async Task<EstadoLote> CarregarAsync(Guid id, int competencia, IPropostasRepository propostas, IParcelasPropostasRepository repo, CancellationToken ct)
    {
        var inicio = CalendarioParcelas.Inicio(competencia);
        var fim = competencia == 999912 ? DateOnly.MaxValue : inicio.AddMonths(1).AddDays(-1);
        var apolice = await propostas.ObterApoliceAsync(id, ct) ?? throw new ValidacaoException("Apólice não encontrada.");
        if (apolice.DeletedAt != null) throw new ValidacaoException("Apólice indisponível.");
        var planos = await repo.ObterPlanosAsync(apolice.Id, ct);
        var todas = await repo.ObterPropostasAsync(apolice.Id, ct);
        var referencias = await repo.ObterReferenciasAsync(todas, ct);
        var movimentos = await repo.ObterMovimentosAsync(apolice.Id, ct);
        var headers = await repo.ObterCompetenciasAsync(apolice.Id, ct);
        var header = headers.SingleOrDefault(c => c.Competencia == competencia);
        var impedimentos = new List<string>(); var linhas = new List<VidaLoteDto>(); var incluir = new List<PropostaFinanceiro>();
        var anterior = headers.Where(c => c.GeradaEm != null && c.Competencia < competencia).OrderByDescending(c => c.Competencia).FirstOrDefault();
        if (header?.GeradaEm == null)
        {
            if (!apolice.Ativo) impedimentos.Add("A Apólice está inativa.");
            if (anterior != null && (anterior.FechadaEm == null || CalendarioParcelas.Proxima(anterior.Competencia) != competencia))
                impedimentos.Add("Feche a competência anterior e gere somente o mês seguinte.");
            if (headers.Any(c => c.GeradaEm != null && c.Competencia > competencia)) impedimentos.Add("Há um lote posterior já gerado.");
            var primeira = headers.Select(c => (int?)c.Competencia).Min();
            if (anterior == null && (primeira == null || primeira != competencia)) impedimentos.Add("Inicie pela primeira competência com inclusão automática.");
        }
        foreach (var grupo in todas.Where(p => p.ApoliceVidaId != null).GroupBy(p => p.ApoliceVidaId).OrderBy(g => g.Key))
        {
            var nativas = movimentos.Where(m => ParcelasNativas.Nativa(m) && m.ApoliceVidaId == grupo.Key).ToArray();
            var gerada = nativas.SingleOrDefault(m => m.CompetenciaInt == competencia);
            var candidatos = planos.Where(f => f.Proposta.ApoliceVidaId == grupo.Key && f.CompetenciaInicial <= competencia &&
                f.Proposta.ConfirmadaEm != null && f.Proposta.DataInicioVigencia <= fim &&
                (f.Proposta.DataFimVigencia == null || f.Proposta.DataFimVigencia > inicio) &&
                (f.Proposta.DataCancelamentoVigencia == null || f.Proposta.DataCancelamentoVigencia > inicio))
                .OrderByDescending(f => f.Proposta.DataInicioVigencia).ThenByDescending(f => f.Proposta.Versao).ThenByDescending(f => f.Id).ToArray();
            var plano = candidatos.FirstOrDefault();
            var p = plano?.Proposta ?? grupo.OrderByDescending(v => v.Versao).ThenByDescending(v => v.Id).First();
            string situacao; string? motivo = null; decimal novo = 0;
            if (gerada != null) { situacao = "EXISTENTE"; p = gerada.Proposta!; }
            else if (plano == null) { situacao = "EXCLUIDA"; motivo = "Sem plano financeiro confirmado ou sem cobertura nesta competência."; }
            else if (p.DeletedAt != null || !p.VisivelOperacional || p.ApoliceVida is not { Ativo: true, DeletedAt: null } ||
                p.ApoliceVida.DataInicioVigencia > fim || p.ApoliceVida.DataFimVigencia < inicio || apolice.DataInicioVigencia > fim || apolice.DataFimVigencia < inicio)
            { situacao = "EXCLUIDA"; motivo = "Proposta, Vida ou Apólice indisponível no período."; }
            else if (!p.Vigente && p.SubstituidaEm == null && p.CanceladaEm == null)
            { situacao = "PENDENTE"; motivo = "A vigência ainda não foi efetivada."; }
            else
            {
                try { ParcelasNativas.ValidarPlano(plano); CalendarioParcelas.Vencimento(competencia, plano.CompetenciaReferencia, plano.VencimentoReferencia); situacao = "INCLUIR"; novo = plano.TotalMensal; incluir.Add(plano); }
                catch (ValidacaoException ex) { situacao = "PENDENTE"; motivo = ex.Message; }
            }
            if (situacao == "PENDENTE" && header?.GeradaEm == null) impedimentos.Add($"Proposta {p.PublicId}: {motivo}");
            var saldo = nativas.Where(m => m.CompetenciaInt < competencia).Sum(ParcelasNativas.Aberto);
            var atual = gerada == null ? novo : ParcelasNativas.Aberto(gerada);
            linhas.Add(new(p.ApoliceVida?.PublicId, p.PublicId, p.Numero, referencias.Clientes.SingleOrDefault(c => c.ClienteId == p.ClienteId)?.Nome,
                referencias.Vinculos.SingleOrDefault(v => v.ClienteId == p.ClienteId && v.VinculoId == p.ClienteVinculoId)?.Matricula, p.Versao, situacao, motivo,
                CalculoMensalProposta.Moeda(novo), CalculoMensalProposta.Moeda(saldo), CalculoMensalProposta.Moeda(saldo + atual),
                nativas.Where(m => m.CompetenciaInt <= competencia && (ParcelasNativas.Aberto(m) > 0 || m.CompetenciaInt == competencia))
                    .OrderBy(m => m.Parcela).Select(m => new ParcelaLoteDto(m.PublicId!.Value, m.Parcela!.Value, m.CompetenciaInt!.Value,
                        m.DataVencimento!.Value, CalculoMensalProposta.Moeda(m.PremioTotal!.Value), CalculoMensalProposta.Moeda(ParcelasNativas.Aberto(m)), m.RevisaoBaixa, m.DataPagamento)).ToArray()));
        }
        var token = FinanceiroConferencia.Assinar(new { id, competencia, apolice.Ativo, apolice.DeletedAt, apolice.DataInicioVigencia, apolice.DataFimVigencia,
            Headers = headers.Select(c => new { c.PublicId, c.Competencia, c.Revisao, c.GeradaEm, c.FechadaEm }),
            Movimentos = movimentos.Select(m => new { m.PublicId, m.LegadoId, m.PropostaId, m.Parcela, m.CompetenciaInt, m.PremioTotal, m.ValorPago, m.DataPagamento, m.RevisaoBaixa }),
            Planos = planos.Select(f => new { f.PublicId, f.ConferidaEm, f.CondicoesHash, f.TotalMensal, f.Proposta.RevisaoPreenchimento }), Linhas = linhas, impedimentos });
        return new(apolice, planos, movimentos, headers, incluir, new(id, competencia, token, header?.GeradaEm, header?.FechadaEm,
            header?.GeradaEm == null && impedimentos.Count == 0, header?.GeradaEm != null && header.FechadaEm == null, impedimentos, linhas));
    }
}

public sealed class ConferirLoteHandler(IPropostasRepository propostas, IParcelasPropostasRepository repo) : IRequestHandler<ConferirLoteQuery, LoteMensalResultado>
{
    public async Task<LoteMensalResultado> Handle(ConferirLoteQuery request, CancellationToken ct) =>
        (await ConferenciaLote.CarregarAsync(request.ApolicePublicId, request.Competencia, propostas, repo, ct)).Resultado;
}
public sealed class GerarLoteHandler(IPropostasRepository propostas, IParcelasPropostasRepository repo, IOperacaoPropostas operacao, TimeProvider relogio)
    : IRequestHandler<GerarLoteCommand, LoteMensalResultado>
{
    public Task<LoteMensalResultado> Handle(GerarLoteCommand request, CancellationToken cancellationToken) => operacao.ExecutarAsync("Gerar Lote Mensal", async ct =>
    {
        await propostas.BloquearApoliceAsync(request.ApolicePublicId, ct);
        var estado = await ConferenciaLote.CarregarAsync(request.ApolicePublicId, request.Dados.Competencia, propostas, repo, ct);
        if (!estado.Resultado.PodeGerar || estado.Resultado.Conferencia != request.Dados.Conferencia)
            throw new ValidacaoException("O lote mudou ou possui pendências. Confira novamente antes de gerar.");
        var agora = relogio.GetUtcNow().UtcDateTime;
        var header = estado.Competencias.SingleOrDefault(c => c.Competencia == request.Dados.Competencia);
        if (header == null) { header = new() { ApoliceId = estado.Apolice.Id, Competencia = request.Dados.Competencia }; repo.Adicionar(header); }
        foreach (var plano in estado.Incluir) repo.Adicionar(ParcelasNativas.Criar(plano, header, estado.Movimentos, agora));
        header.GeradaEm = agora; header.Revisao = checked(header.Revisao + 1);
        await propostas.SalvarAlteracoesAsync(ct);
        var depois = (await ConferenciaLote.CarregarAsync(request.ApolicePublicId, request.Dados.Competencia, propostas, repo, ct)).Resultado;
        return (depois, header.PublicId.ToString(), (object?)estado.Resultado, (object?)depois);
    }, cancellationToken);
}
public sealed class FecharLoteHandler(IPropostasRepository propostas, IParcelasPropostasRepository repo, IOperacaoPropostas operacao, TimeProvider relogio)
    : IRequestHandler<FecharLoteCommand, LoteMensalResultado>
{
    public Task<LoteMensalResultado> Handle(FecharLoteCommand request, CancellationToken cancellationToken) => operacao.ExecutarAsync("Fechar Competência Mensal", async ct =>
    {
        await propostas.BloquearApoliceAsync(request.ApolicePublicId, ct);
        var estado = await ConferenciaLote.CarregarAsync(request.ApolicePublicId, request.Dados.Competencia, propostas, repo, ct);
        if (!estado.Resultado.PodeFechar || estado.Resultado.Conferencia != request.Dados.Conferencia)
            throw new ValidacaoException("O lote mudou ou não permite fechamento. Confira os pagamentos e saldos novamente.");
        var header = estado.Competencias.Single(c => c.Competencia == request.Dados.Competencia);
        header.FechadaEm = relogio.GetUtcNow().UtcDateTime; header.Revisao = checked(header.Revisao + 1);
        await propostas.SalvarAlteracoesAsync(ct);
        var depois = (await ConferenciaLote.CarregarAsync(request.ApolicePublicId, request.Dados.Competencia, propostas, repo, ct)).Resultado;
        return (depois, header.PublicId.ToString(), (object?)estado.Resultado, (object?)depois);
    }, cancellationToken);
}
public sealed class BaixarParcelaHandler(IPropostasRepository propostas, IParcelasPropostasRepository repo, IOperacaoPropostas operacao, TimeProvider relogio)
    : IRequestHandler<BaixarParcelaCommand, bool>
{
    public Task<bool> Handle(BaixarParcelaCommand request, CancellationToken cancellationToken) => operacao.ExecutarAsync("Baixar Parcela Integral", async ct =>
    {
        await propostas.BloquearApoliceAsync(request.ApolicePublicId, ct);
        var apolice = await propostas.ObterApoliceAsync(request.ApolicePublicId, ct) ?? throw new ValidacaoException("Apólice não encontrada.");
        var m = await repo.ObterMovimentoAsync(request.ParcelaPublicId, ct);
        if (m == null || m.Proposta?.ApoliceId != apolice.Id || !ParcelasNativas.Nativa(m) || m.PropostaFinanceiroId == null ||
            request.Dados.DataPagamento == null || request.Dados.RevisaoEsperada != m.RevisaoBaixa || ParcelasNativas.Aberto(m) == 0)
            throw new ValidacaoException("Recarregue uma parcela nativa em aberto e informe a data para registrar a baixa integral.");
        var antes = new { m.PublicId, m.ValorPago, m.DataPagamento, m.RevisaoBaixa };
        m.ValorPago = m.PremioTotal; m.DataPagamento = request.Dados.DataPagamento; m.RevisaoBaixa = checked(m.RevisaoBaixa + 1); m.UpdatedAt = relogio.GetUtcNow().UtcDateTime;
        var header = (await repo.ObterCompetenciasAsync(apolice.Id, ct)).Single(c => c.Id == m.ParcelaCompetenciaId);
        header.Revisao = checked(header.Revisao + 1);
        await propostas.SalvarAlteracoesAsync(ct);
        return (true, m.PublicId!.Value.ToString(), (object?)antes, (object?)new { m.PublicId, m.ValorPago, m.DataPagamento, m.RevisaoBaixa });
    }, cancellationToken);
}
