using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace WebApolice.Modulos.Cadastro.Contracts;

public sealed record ClienteSeguro(long Id, Guid PublicId, string Nome, string? Documento);
public sealed record ModuloSeguro(long Id, Guid PublicId, string Nome, string? Descricao, bool Ativo);
public sealed record SubestipulanteSeguro(long Id, bool Ativo);

/// <summary>Consultas p?blicas dos dados de Cadastro necess?rios aos v?nculos de Seguro.</summary>
public interface ICadastroSeguroQueries
{
    Task<ClienteSeguro?> ObterClienteAsync(Guid publicId, CancellationToken ct);
    Task<ModuloSeguro?> ObterModuloAsync(Guid publicId, CancellationToken ct);
    Task<ModuloSeguro?> ObterModuloPorIdAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<ModuloSeguro>> ObterModulosAsync(IReadOnlyCollection<long> ids, CancellationToken ct);
    Task<SubestipulanteSeguro?> ObterSubestipulanteAsync(Guid publicId, CancellationToken ct);
}
