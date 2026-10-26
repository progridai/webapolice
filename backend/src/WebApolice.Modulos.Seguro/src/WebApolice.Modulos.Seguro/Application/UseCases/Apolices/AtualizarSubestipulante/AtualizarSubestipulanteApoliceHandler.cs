using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
using System.Linq;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarSubestipulante;

public class AtualizarSubestipulanteApoliceHandler : IRequestHandler<AtualizarSubestipulanteApoliceCommand>
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public AtualizarSubestipulanteApoliceHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task Handle(AtualizarSubestipulanteApoliceCommand request, CancellationToken cancellationToken)
    {
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser menor que a data de início.");
        }

        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, true, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        var subestipulanteId = (await _cadastro.ObterSubestipulanteAsync(request.SubestipulantePublicId, cancellationToken))?.Id ?? 0;

        if (subestipulanteId == 0)
        {
            throw new ValidacaoException("Subestipulante não encontrado.");
        }

        var vinculoExistente = await _repository.ObterVinculoSubestipulanteAsync(apolice.Id, subestipulanteId, cancellationToken);
        
        if (vinculoExistente == null)
        {
            throw new ValidacaoException("Vínculo não encontrado.");
        }

        if (!vinculoExistente.Ativo)
        {
            throw new ValidacaoException("Não é possível alterar um vínculo inativo.");
        }

        vinculoExistente.DataInicio = request.DataInicio;
        vinculoExistente.DataFim = request.DataFim;
        vinculoExistente.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.AtualizarSubestipulante(vinculoExistente);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }
}
