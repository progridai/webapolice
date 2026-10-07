using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarSubgrupo;
public sealed class AlterarSubgrupoApoliceHandler(ISubgruposCadastro cadastro)
{
    public Task Handle(AlterarSubgrupoApoliceCommand request,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(request.Nome)) throw new ValidacaoException("O nome do Subgrupo é obrigatório.");
        if(request.Nome.Trim().Length>200) throw new ValidacaoException("O nome do Subgrupo não pode exceder 200 caracteres.");
        return cadastro.SalvarAsync(request.ApolicePublicId,request.SubgrupoPublicId,request.Nome,request.Observacao,request.ConvenioCobrancaPublicId,cancellationToken);
    }
}
