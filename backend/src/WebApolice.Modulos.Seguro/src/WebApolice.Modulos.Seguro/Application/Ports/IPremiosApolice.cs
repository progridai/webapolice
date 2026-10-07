namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IPremiosApolice
{
    Task AlterarAsync(Guid apolicePublicId, Guid vinculoCoberturaPublicId, PremiosCoberturaDados dados, CancellationToken ct);
}
