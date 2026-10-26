using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarModuloApolice;

public class CriarModuloApoliceHandler : IRequestHandler<CriarModuloApoliceCommand, Guid>
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public CriarModuloApoliceHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task<Guid> Handle(CriarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar vigência
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser anterior à data de início.");
        }

        // 2. Localizar Apólice
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, false, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");
            
        // Validar limite de vigência da Apólice (regra de negócio: módulo deve estar contido na vigência da apólice, caso ela tenha datas)
        // Convertendo DateOnly? para DateOnly? se for necessário comparar com Apolice (Apolice tem DateOnly)
        // DateOnly? dataInicioApolice = apolice.DataInicioVigencia; // Isso não é anulável em ApoliceModel
        var dataInicioReqDate = request.DataInicio.HasValue ? request.DataInicio.Value : (DateOnly?)null;
        var dataFimReqDate = request.DataFim.HasValue ? request.DataFim.Value : (DateOnly?)null;
        
        if (dataInicioReqDate.HasValue && dataInicioReqDate < apolice.DataInicioVigencia)
            throw new ValidacaoException($"A data de início do Módulo não pode ser anterior à data de início de vigência da Apólice ({apolice.DataInicioVigencia}).");

        if (apolice.DataFimVigencia.HasValue && dataFimReqDate.HasValue && dataFimReqDate > apolice.DataFimVigencia)
            throw new ValidacaoException($"A data de fim do Módulo não pode ser posterior à data de fim de vigência da Apólice ({apolice.DataFimVigencia}).");

        // 3. Resolver Módulo Global via SQL (cross-module)
        var modulo = await _cadastro.ObterModuloAsync(request.ModuloPublicId, cancellationToken);

        if (modulo == null)
            throw new ValidacaoException("Módulo não encontrado no Cadastro Global.");

        if (!modulo.Ativo)
            throw new ValidacaoException("O Módulo Global está inativo. Não é possível vincular um Módulo inativo.");

        // 4. Verificar duplicidade (inclui inativos não deletados)
        var vinculoExistente = await _repository.ObterVinculoModuloAsync(apolice.Id, modulo.Id, cancellationToken);

        if (vinculoExistente != null)
        {
            if (vinculoExistente.Ativo)
                throw new ValidacaoException("Este Módulo já está vinculado ativamente a esta Apólice.");
            else
                throw new ValidacaoException($"Já existe um vínculo inativo do Módulo '{modulo.Nome}' com esta Apólice. A reativação silenciosa não é permitida.");
        }

        // 5. Criar Vínculo
        var novoVinculo = new ApoliceModuloModel
        {
            ApoliceId = apolice.Id,
            ModuloId = modulo.Id,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Observacao = request.Observacao,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        _repository.AdicionarModulo(novoVinculo);

        // 6. Registrar Histórico Funcional
        _repository.AdicionarHistorico(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Vínculo de Módulo",
            Descricao = $"Módulo '{modulo.Nome}' vinculado à Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return novoVinculo.PublicId;
    }
}
