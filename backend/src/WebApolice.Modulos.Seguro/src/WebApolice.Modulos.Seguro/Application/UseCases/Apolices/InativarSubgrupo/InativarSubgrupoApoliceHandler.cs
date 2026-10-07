using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarSubgrupo;
public sealed class InativarSubgrupoApoliceHandler(ISubgruposCadastro cadastro)
{
    public Task Handle(InativarSubgrupoApoliceCommand request,CancellationToken cancellationToken)
    {
        return cadastro.InativarAsync(request.ApolicePublicId,request.SubgrupoPublicId,cancellationToken);
    }
}
