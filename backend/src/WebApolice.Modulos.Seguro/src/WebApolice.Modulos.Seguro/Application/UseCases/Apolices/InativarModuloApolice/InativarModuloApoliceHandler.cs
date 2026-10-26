using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModuloApolice;

public class InativarModuloApoliceHandler : IRequestHandler<InativarModuloApoliceCommand, Unit>
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public InativarModuloApoliceHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task<Unit> Handle(InativarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Localizar Apólice
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, false, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");
            
        // 2. Localizar Vínculo e garantir isolamento
        var vinculo = await _repository.ObterModuloAsync(request.ApoliceModuloPublicId, false, cancellationToken);

        if (vinculo == null)
            throw new ValidacaoException("Vínculo do Módulo não encontrado.");

        if (vinculo.ApoliceId != apolice.Id)
            throw new ValidacaoException("O Vínculo informado não pertence à Apólice especificada.");

        if (!vinculo.Ativo)
            throw new ValidacaoException("O Vínculo do Módulo já está inativo.");

        // 3. Inativar Vínculo
        vinculo.Ativo = false;
        vinculo.UpdatedAt = DateTimeOffset.UtcNow;

        // 4. Histórico
        // Buscar nome do Módulo
        var moduloNome = (await _cadastro.ObterModuloPorIdAsync(vinculo.ModuloId, cancellationToken))?.Nome;

        _repository.AdicionarHistorico(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Inativação de Vínculo de Módulo",
            Descricao = $"Vínculo do Módulo '{moduloNome}' inativado na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return Unit.Value;
    }
}
