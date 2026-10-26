using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ListarModulosApoliceHandler : IRequestHandler<ListarModulosApoliceQuery, IEnumerable<ModuloApoliceDto>>
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public ListarModulosApoliceHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task<IEnumerable<ModuloApoliceDto>> Handle(ListarModulosApoliceQuery request, CancellationToken cancellationToken)
    {
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, false, cancellationToken);

        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        var vinculos = await _repository.ListarModulosAsync(apolice.Id, cancellationToken);

        if (!vinculos.Any())
            return new List<ModuloApoliceDto>();

        var modulosGlobais = await _cadastro.ObterModulosAsync(vinculos.Select(v => v.ModuloId).ToArray(), cancellationToken);

        var result = vinculos.Select(v => 
        {
            var mg = modulosGlobais.First(m => m.Id == v.ModuloId);
            return new ModuloApoliceDto
            {
                PublicId = v.PublicId,
                ModuloPublicId = mg.PublicId,
                Nome = mg.Nome,
                Descricao = mg.Descricao,
                DataInicio = v.DataInicio,
                DataFim = v.DataFim,
                Observacao = v.Observacao,
                Ativo = v.Ativo
            };
        }).ToList();

        return result;
    }
}
