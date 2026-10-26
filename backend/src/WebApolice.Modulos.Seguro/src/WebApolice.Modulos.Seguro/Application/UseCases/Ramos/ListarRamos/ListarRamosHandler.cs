using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ListarRamos;

public class ListarRamosHandler
{
    private readonly IRamosRepository _repository;

    public ListarRamosHandler(IRamosRepository context)
    {
        _repository = context;
    }

    public Task<PagedResult<RamoDto>> Handle(ListarRamosQuery query, CancellationToken cancellationToken)
        => _repository.ListarAsync(query, cancellationToken);
}
