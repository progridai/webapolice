using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarSubgrupo;
public sealed class CriarSubgrupoApoliceHandler(ISubgruposCadastro cadastro)
{
    public Task<Guid> Handle(CriarSubgrupoApoliceCommand request,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(request.Nome)) throw new ValidacaoException("O nome do Subgrupo é obrigatório.");
        if(request.Nome.Trim().Length>200) throw new ValidacaoException("O nome do Subgrupo não pode exceder 200 caracteres.");
        if(request.ConvenioCobrancaPublicId is null || request.ConvenioCobrancaPublicId==Guid.Empty) throw new ValidacaoException("O Convênio de Cobrança é obrigatório para novos Subgrupos.");
        return cadastro.SalvarAsync(request.ApolicePublicId,null,request.Nome,request.Observacao,request.ConvenioCobrancaPublicId,cancellationToken);
    }
}
