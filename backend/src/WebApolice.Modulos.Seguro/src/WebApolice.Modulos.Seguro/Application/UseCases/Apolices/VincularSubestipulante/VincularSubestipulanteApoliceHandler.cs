using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;
using System.Linq;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularSubestipulante;

public class VincularSubestipulanteApoliceHandler : IRequestHandler<VincularSubestipulanteApoliceCommand, long>
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public VincularSubestipulanteApoliceHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task<long> Handle(VincularSubestipulanteApoliceCommand request, CancellationToken cancellationToken)
    {
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser menor que a data de início.");
        }

        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, true, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        var subestipulante = await _cadastro.ObterSubestipulanteAsync(request.SubestipulantePublicId, cancellationToken);
        if (subestipulante == null || !subestipulante.Ativo)
            throw new ValidacaoException("Subestipulante não encontrado ou está inativo.");
        var subestipulanteId = subestipulante.Id;

        var vinculoExistente = await _repository.ObterVinculoSubestipulanteAsync(apolice.Id, subestipulanteId, cancellationToken);
        
        if (vinculoExistente != null && vinculoExistente.Ativo)
        {
            throw new ValidacaoException("O Subestipulante já está vinculado ativamente nesta Apólice.");
        }

        if (vinculoExistente != null && !vinculoExistente.Ativo)
        {
            throw new ValidacaoException("Existe um vínculo inativo com este Subestipulante. A reativação de vínculos ainda não está implementada/aprovada.");
        }

        var vinculo = new ApoliceSubestipulanteModel
        {
            ApoliceId = apolice.Id,
            SubestipulanteId = subestipulanteId,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _repository.AdicionarSubestipulante(vinculo);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return vinculo.Id;
    }
}
