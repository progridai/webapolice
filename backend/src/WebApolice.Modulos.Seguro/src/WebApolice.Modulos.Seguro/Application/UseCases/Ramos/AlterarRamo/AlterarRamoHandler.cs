using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarRamo;

public class AlterarRamoHandler
{
    private readonly IRamosRepository _repository;

    public AlterarRamoHandler(IRamosRepository context)
    {
        _repository = context;
    }

    public async Task Handle(AlterarRamoCommand command, CancellationToken cancellationToken)
    {
        var model = await _repository.ObterAsync(command.PublicId, cancellationToken);

        if (model == null)
            throw new InvalidOperationException("Ramo não encontrado.");

        model.Nome = command.Nome;
        model.Descricao = command.Descricao;
        model.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Atualizar(model);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }
}
