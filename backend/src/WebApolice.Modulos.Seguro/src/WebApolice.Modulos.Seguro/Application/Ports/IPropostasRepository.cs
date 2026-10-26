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
    Task<PropostaStatus?> ObterStatusAsync(string codigo, CancellationToken ct);
    Task<ApoliceModel?> ObterApoliceAsync(Guid publicId, CancellationToken ct);
    Task<ApoliceVidaModel?> ObterVidaAsync(Guid publicId, CancellationToken ct);
    Task<Propostum?> ObterAsync(Guid publicId, CancellationToken ct);
    Task<ApoliceProdutoModel?> ObterProdutoPermitidoAsync(long? apoliceId, long produtoId, CancellationToken ct);
    Task<ApolicePlanoModel?> ObterPlanoPermitidoAsync(long apoliceProdutoId, long planoId, CancellationToken ct);
    Task<PagedResult<PropostaDto>> ListarAsync(ObterPropostasQuery request, CancellationToken cancellationToken);
    void Adicionar(Propostum proposta);
    void AdicionarItem(PropostaItem item);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
