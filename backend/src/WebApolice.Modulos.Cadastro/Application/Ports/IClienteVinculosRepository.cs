using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Cadastro.Application.UseCases.Vinculos;
using WebApolice.SharedKernel.Application.Models;
namespace WebApolice.Modulos.Cadastro.Application.Ports;

public sealed record ClienteVinculoConsulta(long Id, ClienteVinculoResult Dados);

public interface IClienteVinculosRepository
{
    Task<Cliente?> ObterClienteAsync(Guid id, bool bloquear, CancellationToken ct);
    Task<ClienteVinculoModel?> ObterAsync(Guid clienteId, Guid id, CancellationToken ct);
    Task<EstipulanteModel?> ObterEstipulanteAsync(Guid id, CancellationToken ct);
    Task<bool> DuplicadoAsync(long clienteId, long estipulanteId, string matricula, long? excetoId, CancellationToken ct);
    Task<PagedResult<ClienteVinculoConsulta>> ListarAsync(Guid clienteId, int pagina, int tamanho, CancellationToken ct);
    Task<PagedResult<EstipulanteVinculoOpcao>> ListarEstipulantesAsync(string? busca, int pagina, int tamanho, CancellationToken ct);
    void Adicionar(ClienteVinculoModel vinculo);
    Task SalvarAsync(CancellationToken ct);
}
