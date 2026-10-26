using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;

public record RecursoDto(Guid PublicId, string Codigo, string Nome, bool Ativo, bool Habilitado);
public record ModuloDto(Guid PublicId, string Codigo, string Nome, string? Descricao, string? Icone, int Ordem, bool Ativo, bool Habilitado, List<RecursoDto> Recursos);

public class ListarModulosUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ListarModulosUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<List<ModuloDto>> ExecuteAsync(CancellationToken cancellationToken)
        => _consultas.ListarModulosAsync(cancellationToken);
}
