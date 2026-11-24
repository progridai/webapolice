using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.SharedKernel.Application.Models;
namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IPropostasRepository
{
    Task<PagedResult<PropostaMovimento>?> ListarParcelasAsync(Guid publicId, int pagina, int tamanhoPagina, CancellationToken ct);
    Task<PropostaStatus?> ObterStatusAsync(string codigo, CancellationToken ct);
    Task<ApoliceModel?> ObterApoliceAsync(Guid publicId, CancellationToken ct);
    Task<ApoliceVidaModel?> ObterVidaAsync(Guid publicId, CancellationToken ct);
    Task<Propostum?> ObterParaPreenchimentoAsync(Guid publicId, CancellationToken ct);
    Task<Propostum?> ObterParaComparacaoAsync(Guid publicId, CancellationToken ct);
    Task<IReadOnlyList<ApoliceRamoModel>> ObterRamosSimulacaoAsync(long apoliceId, CancellationToken ct);
    Task<int?> ObterRevisaoAtualAsync(Guid publicId, CancellationToken ct);
    Task<IReadOnlyList<ApoliceModuloModel>> ObterModulosRevisaoAsync(long apoliceId, CancellationToken ct);
    Task<IReadOnlyList<ApoliceSubgrupoModel>> ObterSubgruposRevisaoAsync(long apoliceId, CancellationToken ct);
    Task<ApoliceModuloModel?> ObterModuloRevisaoAsync(Guid publicId, CancellationToken ct);
    Task<ApoliceSubgrupoModel?> ObterSubgrupoRevisaoAsync(Guid publicId, CancellationToken ct);
    Task<Propostum?> ObterOrigemParaVigenciaAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<AgendamentoVigenciaProposta>> ListarAgendadasAsync(DateOnly ate, string statusConfirmada, long aposId, CancellationToken ct);
    Task<IReadOnlyList<AgendamentoVigenciaProposta>> ListarCancelamentosAgendadosAsync(DateOnly ate, string statusConfirmada, long aposId, CancellationToken ct);
    Task<Propostum?> ObterAsync(Guid publicId, CancellationToken ct);
    Task BloquearApoliceAsync(Guid publicId, CancellationToken ct);
    Task<IReadOnlyList<Propostum>> ObterPropostasDaVidaAsync(long vidaId, CancellationToken ct);
    Task<ApoliceModuloPlanoModel?> ObterPlanoModuloAsync(long moduloId, CancellationToken ct);
    Task<ApoliceProdutoModel?> ObterProdutoPermitidoAsync(long? apoliceId, long produtoId, CancellationToken ct);
    Task<ApolicePlanoModel?> ObterPlanoPermitidoAsync(long apoliceProdutoId, long planoId, CancellationToken ct);
    Task<PagedResult<PropostaDto>> ListarAsync(ObterPropostasQuery request, CancellationToken cancellationToken);
    Task<WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta.PropostaDetalheDto?> ObterDetalheAsync(Guid publicId, CancellationToken ct, string? codigoInicial = null);
    void Adicionar(Propostum proposta);
    void AdicionarItem(PropostaItem item);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}

public sealed record AgendamentoVigenciaProposta(long Posicao, Guid PublicId, int RevisaoPreenchimento);
