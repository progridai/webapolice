namespace WebApolice.Modulos.Seguro.Application.Ports;

public enum TipoVinculoApolice
{
    Ramo, Subgrupo, Subestipulante, Modulo, PlanoModulo, CoberturaModulo,
    ProdutoAnterior, PlanoAnterior, CoberturaAnterior
}

public interface IExclusaoVinculosApolice
{
    Task ExcluirAsync(Guid apolicePublicId, TipoVinculoApolice tipo, Guid vinculoPublicId, Guid usuarioPublicId, CancellationToken ct);
}
