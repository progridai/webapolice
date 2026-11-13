namespace WebApolice.Modulos.Cadastro.Contracts;

public interface IClienteVinculosUso
{
    Task<IReadOnlyDictionary<long, IReadOnlyList<ApoliceVinculoCliente>>> ListarApolicesAsync(
        long clienteId, IReadOnlyCollection<long> vinculoIds, CancellationToken ct);
    Task<bool> EstaEmUsoAsync(long vinculoId, CancellationToken ct);
    Task<IReadOnlyCollection<long>> ListarEmUsoAsync(IReadOnlyCollection<long> vinculoIds, CancellationToken ct);
}

public sealed record ApoliceVinculoCliente(Guid PublicId, string? Nome,
    IReadOnlyList<string> Numeros, bool Ativa, bool ParticipacaoAtiva);
