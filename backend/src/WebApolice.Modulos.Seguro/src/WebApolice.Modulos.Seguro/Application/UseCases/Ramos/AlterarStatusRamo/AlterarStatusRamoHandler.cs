using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarStatusRamo;

public class AlterarStatusRamoHandler
{
    private readonly IRamosRepository _repository;

    public AlterarStatusRamoHandler(IRamosRepository context)
    {
        _repository = context;
    }

    public async Task Handle(AlterarStatusRamoCommand command, CancellationToken cancellationToken)
    {
        var model = await _repository.ObterAsync(command.PublicId, cancellationToken);

        if (model == null)
            throw new InvalidOperationException("Ramo não encontrado.");

        model.Ativo = command.Ativo;
        model.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Atualizar(model);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }
}
