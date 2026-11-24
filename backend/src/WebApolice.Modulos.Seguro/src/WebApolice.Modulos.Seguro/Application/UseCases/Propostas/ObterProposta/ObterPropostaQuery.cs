using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ReincluirProposta;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;

public sealed record ObterPropostaQuery(Guid PublicId) : IRequest<PropostaDetalheDto?>;

public sealed class ObterPropostaHandler(IPropostasRepository repository, IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio) : IRequestHandler<ObterPropostaQuery, PropostaDetalheDto?>
{
    public async Task<PropostaDetalheDto?> Handle(ObterPropostaQuery request, CancellationToken cancellationToken)
    {
        var dto = await repository.ObterDetalheAsync(request.PublicId, cancellationToken, configuracao.Value.StatusInicialCodigo);
        if (dto is null) return null;
        if (dto.PodePrepararReinclusao && dto.ApoliceVidaPublicId is Guid vidaPublicId)
        {
            var vida = await repository.ObterVidaAsync(vidaPublicId, cancellationToken);
            var propostas = vida is null ? [] : await repository.ObterPropostasDaVidaAsync(vida.Id, cancellationToken);
            var origem = propostas.SingleOrDefault(p => p.PublicId == dto.PublicId);
            dto.PodePrepararReinclusao = vida is { Ativo: true, DeletedAt: null } && origem is not null &&
                !string.IsNullOrWhiteSpace(configuracao.Value.StatusInicialCodigo) && ReinclusaoPropostas.SemImpedimentos(origem, propostas, configuracao.Value);
        }
        dto.PodeConfirmar = dto.PodeEditarRascunho && !string.IsNullOrWhiteSpace(configuracao.Value.StatusConfirmadaCodigo);
        var podeGerir = dto.PodeEfetivarVigencia && dto.IntegradaApolice && dto.CanceladaEm == null &&
            dto.ConfirmadaEm != null && dto.SubstituidaEm == null && !dto.Vigente &&
            !string.IsNullOrWhiteSpace(configuracao.Value.StatusConfirmadaCodigo) && dto.StatusCodigo == configuracao.Value.StatusConfirmadaCodigo;
        dto.ExigeConferenciaFinanceira = configuracao.Value.ExigirConferenciaFinanceira;
        dto.PodeConferirFinanceiro = podeGerir;
        dto.PodeReagendar = podeGerir && configuracao.Value.ProcessarAgendamentos;
        dto.PodeRevisarCondicoes = dto.PodeEditarRascunho || (podeGerir && dto.DataInicioVigencia > VigenciaPropostas.Hoje(relogio));
        dto.PodeCancelarAgendamento = podeGerir && !string.IsNullOrWhiteSpace(configuracao.Value.StatusCanceladaCodigo);
        dto.PodeEfetivarVigencia = dto.PodeEfetivarVigencia && dto.IntegradaApolice && dto.ConfirmadaEm != null && dto.SubstituidaEm == null &&
            !dto.Vigente && dto.StatusCodigo == configuracao.Value.StatusConfirmadaCodigo && dto.DataInicioVigencia <= VigenciaPropostas.Hoje(relogio);
        dto.PodeDescartarRascunho = dto.PodeEditarRascunho && !string.IsNullOrWhiteSpace(configuracao.Value.StatusDescartadaCodigo);
        var vigenteNativa = dto.PodeCancelarVigencia && dto.IntegradaApolice && dto.StatusCodigo == configuracao.Value.StatusConfirmadaCodigo;
        var pedidoAtivo = vigenteNativa && dto.CancelamentoSolicitadoEm != null && dto.DataCancelamentoVigencia != null;
        dto.PodeAlterarCancelamento = pedidoAtivo && !string.IsNullOrWhiteSpace(configuracao.Value.StatusCanceladaCodigo);
        dto.PodeRetirarCancelamento = pedidoAtivo;
        dto.PodeCancelarVigencia = vigenteNativa && dto.CancelamentoSolicitadoEm == null && !string.IsNullOrWhiteSpace(configuracao.Value.StatusCanceladaCodigo);
        dto.PodeEfetivarCancelamento = vigenteNativa && dto.CancelamentoSolicitadoEm != null && dto.DataCancelamentoVigencia <= VigenciaPropostas.Hoje(relogio);
        dto.SituacaoVigenciaCodigo = dto.DescartadaEm != null ? "DESCARTADA" : dto.CanceladaEm != null ? "CANCELADA" : dto.CancelamentoSolicitadoEm != null && dto.Vigente
            ? dto.DataCancelamentoVigencia > VigenciaPropostas.Hoje(relogio) ? "CANCELAMENTO_AGENDADO" : "CANCELAMENTO_PENDENTE"
            : dto.Vigente ? "VIGENTE" : dto.SubstituidaEm != null ? "SUBSTITUIDA" :
            dto.ConfirmadaEm == null ? "SEM_VIGENCIA" : dto.DataInicioVigencia > VigenciaPropostas.Hoje(relogio) ? "AGENDADA" : "ATIVACAO_PENDENTE";
        return dto;
    }
}

public sealed class PropostaDetalheDto : PropostaDto
{
    public bool ExigeConferenciaFinanceira { get; set; }
    public bool PodeConferirFinanceiro { get; set; }
    public bool PodeRevisarCondicoes { get; set; }
    public bool CondicoesProprias { get; set; }
    public bool? IncluiConjuge { get; set; }
    public bool PodeSimularValorMensal { get; set; }
    public bool PodePrepararReinclusao { get; set; }
    public bool Reinclusao { get; set; }
    public DateOnly? DataFimVigenciaOrigem { get; set; }
    public DateOnly? DataCancelamentoVigencia { get; set; }
    public DateTime? CancelamentoSolicitadoEm { get; set; }
    public DateTime? DescartadaEm { get; set; }
    public bool PodeDescartarRascunho { get; set; }
    public bool PodeCancelarVigencia { get; set; }
    public bool PodeEfetivarCancelamento { get; set; }
    public bool PodeAlterarCancelamento { get; set; }
    public bool PodeRetirarCancelamento { get; set; }
    public DateTime? CanceladaEm { get; set; }
    public bool PodeReagendar { get; set; }
    public bool PodeCancelarAgendamento { get; set; }
    public string SituacaoVigenciaCodigo { get; set; } = "SEM_VIGENCIA";
    public string? MotivoPendenciaVigencia { get; set; }
    public DateTime? PendenciaVigenciaEm { get; set; }
    public bool PodeConfirmar { get; set; }
    public bool PodeEfetivarVigencia { get; set; }
    public DateTime? ConfirmadaEm { get; set; }
    public DateTime? SubstituidaEm { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public int RevisaoPreenchimento { get; set; }
    public bool PodeEditarRascunho { get; set; }
    public Guid? PropostaOrigemPublicId { get; set; }
    public decimal? ValorParcela { get; set; }
    public decimal? IofPercentual { get; set; }
    public decimal? IofValor { get; set; }
    public DateOnly? DataPrimeiroVencimento { get; set; }
    public DateOnly? DataProximoVencimento { get; set; }
    public string? Observacao { get; set; }
    public string? ModuloNomeRegistrado { get; set; }
    public string? SubgrupoNomeRegistrado { get; set; }
    public IReadOnlyList<PropostaItemDto> Itens { get; set; } = [];
    public IReadOnlyList<PropostaCoberturaDto> Coberturas { get; set; } = [];
    public IReadOnlyList<PropostaBeneficiarioDto> Beneficiarios { get; set; } = [];
    public IReadOnlyList<PropostaHistoricoDto> Historico { get; set; } = [];
}

public sealed record PropostaHistoricoDto(DateTime DataAlteracao, string? Motivo, string? Observacao,
    Guid? PropostaAnteriorPublicId, int VersaoAnterior, Guid? PropostaNovaPublicId, int VersaoNova);

public sealed record PropostaItemDto(Guid PublicId, string? ProdutoNomeAtual, string? PlanoNomeAtual, string? PlanoNomeRegistrado, decimal? Valor, bool Ativo, bool PlanoContextual, string? Ramo, bool? PlanoPaga, bool? PlanoReajuste);
public sealed record PropostaCoberturaDto(Guid PublicId, Guid? ItemPublicId, string? NomeAtual, string? NomeRegistrado, decimal? PremioTitular, decimal? PremioConjuge, bool Ativo, string? NomeReduzidoRegistrado, string? BasicaRegistrada, bool? ReajusteRegistrado, bool PodeAjustarPremio = false, bool PremioAjustado = false, decimal? PremioTitularInicial = null, decimal? PremioConjugeInicial = null, string? JustificativaAjustePremio = null);
public sealed record PropostaBeneficiarioDto(string? Nome, string? Parentesco, decimal? PercentualParticipacao, Guid PublicId = default, bool PodeAlterar = false);
