using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.SharedKernel.Application.Models;
namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.Vinculos;

public sealed record VinculoVidaOpcao(Guid PublicId, string Matricula, string? Estipulante, bool RequerEstipulante = false);
public sealed class ListarVinculosVidaHandler(IApoliceVinculosRepository repository, ICadastroSeguroQueries cadastro)
{
    public async Task<PagedResult<VinculoVidaOpcao>> Handle(Guid apoliceId, Guid clienteId, int pagina, int tamanho, CancellationToken ct)
    {
        if (pagina < 1 || tamanho < 1 || tamanho > 100 || (long)(pagina - 1) * tamanho > int.MaxValue)
            throw new ValidacaoException("Paginação inválida.");
        var apolice = await repository.ObterApoliceAsync(apoliceId, false, ct) ?? throw new ValidacaoException("Apólice não encontrada.");
        var result = await cadastro.ListarVinculosAsync(clienteId, apolice.EstipulanteId, pagina, tamanho, ct);
        return new()
        {
            Items = result.Items.Select(v => new VinculoVidaOpcao(v.PublicId, v.Matricula, v.Estipulante, v.EstipulanteId == null)).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}
