namespace WebApolice.Modulos.Cadastro.Contracts;

public sealed record IdentidadeClienteProposta(long ClienteId, long PessoaId, long ClienteVinculoId, long? EstipulanteId = null);
public sealed record ReferenciaClienteProposta(long ClienteId, Guid ClientePublicId, string Nome);
public sealed record ReferenciaVinculoProposta(long VinculoId, long ClienteId, Guid PublicId, string? Matricula);
public sealed record ReferenciasPropostas(IReadOnlyList<ReferenciaClienteProposta> Clientes, IReadOnlyList<ReferenciaVinculoProposta> Vinculos);

public interface IClientesPropostas
{
    Task<IdentidadeClienteProposta?> ResolverAsync(long clienteId, long vinculoId, CancellationToken cancellationToken);
    Task<IdentidadeClienteProposta?> ResolverParaConfirmacaoAsync(long clienteId, long vinculoId, CancellationToken cancellationToken);
    Task<ReferenciasPropostas> ConsultarReferenciasAsync(IReadOnlyCollection<long> clientes, IReadOnlyCollection<long> vinculos, CancellationToken cancellationToken);
}
