using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ObterRamo;

public class ObterRamoHandler
{
    private readonly IRamosRepository _repository;

    public ObterRamoHandler(IRamosRepository context)
    {
        _repository = context;
    }

    public async Task<RamoDto?> Handle(ObterRamoQuery query, CancellationToken cancellationToken)
    {
        var entity = await _repository.ObterParaLeituraAsync(query.PublicId, cancellationToken);

        return entity?.ToDto();
    }
}
