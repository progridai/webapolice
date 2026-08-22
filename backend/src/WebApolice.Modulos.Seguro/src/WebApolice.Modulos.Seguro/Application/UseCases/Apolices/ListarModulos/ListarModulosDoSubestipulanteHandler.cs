using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ListarModulosDoSubestipulanteHandler
{
    private readonly IApolicesQueries _queries;

    public ListarModulosDoSubestipulanteHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<List<ModuloDoSubestipulanteResult>> Handle(
        ListarModulosDoSubestipulanteQuery request,
        CancellationToken cancellationToken)
    {
        return await _queries.ListarModulosDoSubestipulanteAsync(
            request.ApolicePublicId,
            request.SubestipulantePublicId,
            cancellationToken);
    }
}
