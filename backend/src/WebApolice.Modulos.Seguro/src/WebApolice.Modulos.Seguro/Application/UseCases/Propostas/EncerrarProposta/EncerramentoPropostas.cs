using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;

internal static class EncerramentoPropostas
{
    internal static void ValidarPedido(Propostum p, int? revisao, string? observacao)
    {
        if (revisao is null or < 0 || string.IsNullOrWhiteSpace(observacao) || observacao.Length > 1000)
            throw new ValidacaoException("Informe a revisão e uma justificativa de até 1.000 caracteres.");
        if (p.RevisaoPreenchimento != revisao)
            throw new ValidacaoException("A proposta foi alterada em outra operação. Recarregue antes de continuar.");
    }

    internal static async Task<PropostaStatus> ObterStatusAsync(IPropostasRepository repository, ConfiguracaoPropostas config, bool descarte, CancellationToken ct)
    {
        var codigo = descarte ? config.StatusDescartadaCodigo : config.StatusCanceladaCodigo;
        var outro = descarte ? config.StatusCanceladaCodigo : config.StatusDescartadaCodigo;
        if (string.IsNullOrWhiteSpace(codigo) || codigo == config.StatusInicialCodigo || codigo == config.StatusConfirmadaCodigo || codigo == outro)
            throw new ValidacaoException("A situação de encerramento ainda não foi configurada corretamente.");
        var status = await repository.ObterStatusAsync(codigo, ct);
        if (status is null || !status.VisivelOperacional || !status.Finalizador || status.PermiteMovimentacao)
            throw new ValidacaoException("A situação de encerramento deve ser visível, finalizadora e sem movimentação financeira.");
        return status;
    }

    internal static async Task ValidarVigenteAsync(Propostum p, IPropostasRepository repository, ConfiguracaoPropostas config, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(config.StatusConfirmadaCodigo) || p.Status.Codigo != config.StatusConfirmadaCodigo ||
            p.LegadoId != null || !p.Vigente || p.ConfirmadaEm == null || p.DataInicioVigencia == null || p.DataFimVigencia != null ||
            p.CanceladaEm != null || p.DescartadaEm != null || p.SubstituidaEm != null || p.DeletedAt != null || !p.VisivelOperacional ||
            !p.Status.VisivelOperacional || p.Status.Finalizador || p.Status.PermiteMovimentacao || p.Apolice is null || p.ApoliceVida is null)
            throw new ValidacaoException("Somente uma proposta nativa confirmada e vigente pode ter sua vigência cancelada.");
        var propostas = await repository.ObterPropostasDaVidaAsync(p.ApoliceVidaId!.Value, ct);
        var vigentes = propostas.Where(v => v.Vigente && v.DeletedAt == null).ToList();
        if (vigentes.Count != 1 || vigentes[0].Id != p.Id)
            throw new ValidacaoException("A proposta deve ser a única vigente da Vida para cancelar a vigência.");
        if (propostas.Any(v => v.PropostaOrigemId == p.Id &&
            !v.TentativaEncerradaSemVigencia(config.StatusCanceladaCodigo, config.StatusDescartadaCodigo)))
            throw new ValidacaoException("Existe uma versão sucessora não encerrada. Descarte o rascunho ou cancele o agendamento antes de cancelar a vigente.");
    }

    internal static void Concluir(Propostum p, PropostaStatus status, DateTime agora)
    {
        p.StatusId = status.Id; p.Status = status; p.Vigente = false;
        p.CanceladaEm = agora; p.DataFimVigencia = p.DataCancelamentoVigencia;
        p.MotivoPendenciaVigencia = null; p.PendenciaVigenciaEm = null;
    }
}
