using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ObterModuloApolicePorPublicIdHandler : IRequestHandler<ObterModuloApolicePorPublicIdQuery, ModuloApoliceDto>
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public ObterModuloApolicePorPublicIdHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task<ModuloApoliceDto> Handle(ObterModuloApolicePorPublicIdQuery request, CancellationToken cancellationToken)
    {
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, false, cancellationToken);

        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        var vinculo = await _repository.ObterModuloAsync(request.ApoliceModuloPublicId, false, cancellationToken);

        if (vinculo == null)
            throw new ValidacaoException("Vínculo do Módulo não encontrado.");

        if (vinculo.ApoliceId != apolice.Id)
            throw new ValidacaoException("O Vínculo informado não pertence à Apólice especificada.");

        var moduloGlobal = await _cadastro.ObterModuloPorIdAsync(vinculo.ModuloId, cancellationToken);

        if (moduloGlobal == null)
            throw new ValidacaoException("Módulo Global associado não encontrado.");

        return new ModuloApoliceDto
        {
            PublicId = vinculo.PublicId,
            ModuloPublicId = moduloGlobal.PublicId,
            Nome = moduloGlobal.Nome,
            Descricao = moduloGlobal.Descricao,
            DataInicio = vinculo.DataInicio,
            DataFim = vinculo.DataFim,
            Observacao = vinculo.Observacao,
            Ativo = vinculo.Ativo
        };
    }
}
