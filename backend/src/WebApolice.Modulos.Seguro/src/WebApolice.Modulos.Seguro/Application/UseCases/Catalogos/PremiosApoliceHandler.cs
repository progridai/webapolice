using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;

public sealed class PremiosApoliceHandler(IPremiosApolice premios)
{
    public Task AlterarAsync(Guid apoliceId, Guid vinculoId, PremiosCoberturaDados dados, CancellationToken ct)
    {
        CatalogosSeguroHandler.ValidarPremios(dados, obrigatorios: false);
        return premios.AlterarAsync(apoliceId, vinculoId, dados, ct);
    }
}
