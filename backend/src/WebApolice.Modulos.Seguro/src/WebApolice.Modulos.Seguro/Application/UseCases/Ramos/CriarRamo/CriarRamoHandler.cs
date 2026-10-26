using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.CriarRamo;

public class CriarRamoHandler
{
    private readonly IRamosRepository _repository;

    public CriarRamoHandler(IRamosRepository context)
    {
        _repository = context;
    }

    public async Task<RamoDto> Handle(CriarRamoCommand command, CancellationToken cancellationToken)
    {
        var model = new RamoModel
        {
            PublicId = Guid.NewGuid(),
            Codigo = command.Codigo,
            Nome = command.Nome,
            Descricao = command.Descricao,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _repository.Adicionar(model);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return model.ToDto();
    }
}
