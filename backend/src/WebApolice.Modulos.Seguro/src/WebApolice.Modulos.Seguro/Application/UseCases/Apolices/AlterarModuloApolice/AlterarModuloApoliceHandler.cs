using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarModuloApolice;

public class AlterarModuloApoliceHandler : IRequestHandler<AlterarModuloApoliceCommand, Unit>
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public AlterarModuloApoliceHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task<Unit> Handle(AlterarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar vigência local
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser anterior à data de início.");
        }

        // 2. Localizar Apólice
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, false, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");
            
        var dataInicioReqDate = request.DataInicio.HasValue ? request.DataInicio.Value : (DateOnly?)null;
        var dataFimReqDate = request.DataFim.HasValue ? request.DataFim.Value : (DateOnly?)null;
        
        if (dataInicioReqDate.HasValue && dataInicioReqDate < apolice.DataInicioVigencia)
            throw new ValidacaoException($"A data de início do Módulo não pode ser anterior à data de início de vigência da Apólice ({apolice.DataInicioVigencia}).");

        if (apolice.DataFimVigencia.HasValue && dataFimReqDate.HasValue && dataFimReqDate > apolice.DataFimVigencia)
            throw new ValidacaoException($"A data de fim do Módulo não pode ser posterior à data de fim de vigência da Apólice ({apolice.DataFimVigencia}).");

        // 3. Localizar Vínculo e garantir isolamento
        var vinculo = await _repository.ObterModuloAsync(request.ApoliceModuloPublicId, false, cancellationToken);

        if (vinculo == null)
            throw new ValidacaoException("Vínculo do Módulo não encontrado.");

        if (vinculo.ApoliceId != apolice.Id)
            throw new ValidacaoException("O Vínculo informado não pertence à Apólice especificada.");

        if (!vinculo.Ativo)
            throw new ValidacaoException("O Vínculo do Módulo está inativo e não pode ser alterado.");

        // 4. Alterar Vínculo
        vinculo.DataInicio = request.DataInicio;
        vinculo.DataFim = request.DataFim;
        vinculo.Observacao = request.Observacao;
        vinculo.UpdatedAt = DateTimeOffset.UtcNow;

        // 5. Histórico
        // Buscar nome do Módulo para compor o histórico
        var moduloNome = (await _cadastro.ObterModuloPorIdAsync(vinculo.ModuloId, cancellationToken))?.Nome;

        _repository.AdicionarHistorico(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Alteração de Vínculo de Módulo",
            Descricao = $"Vínculo do Módulo '{moduloNome}' alterado na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return Unit.Value;
    }
}
