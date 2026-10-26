namespace WebApolice.Modulos.Cadastro.Contracts;

public sealed record IdentidadeClienteProposta(long ClienteId, long PessoaId, long ClienteVinculoId);

public interface IClientesPropostas
{
    Task<IdentidadeClienteProposta?> ResolverAsync(long clienteId, long vinculoId, CancellationToken cancellationToken);
}
