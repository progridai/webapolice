using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarModulo;

/// <summary>
/// Handler para atualizar a vigência contextual de um vínculo Módulo.
///
/// Regras de negócio:
/// 1. Apólice, Subestipulante e vínculo pai devem existir.
/// 2. Vínculo Módulo deve existir e estar ativo.
/// 3. ModuloPublicId é read-only — não pode ser trocado.
/// 4. DataFim >= DataInicio quando ambos informados.
/// 5. Vigência resultante deve estar contida na do vínculo pai.
/// 6. Nova vigência não pode deixar Vidas ativas fora do período.
///
/// Definição de Vida ativa: v.Ativo == true (padrão vigente do domínio Seguro).
/// </summary>
public class AtualizarModuloApoliceHandler : IRequestHandler<AtualizarModuloApoliceCommand>
{
    private readonly SeguroDbContext _dbContext;

    public AtualizarModuloApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AtualizarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar vigência
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência do Módulo não pode ser anterior à data de início.");
        }

        // 2. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // 3. Resolver Subestipulante Global (cross-module via SQL parametrizado — padrão vigente)
        var subestipulanteId = await _dbContext.Database
            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {request.SubestipulantePublicId} AND deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (subestipulanteId == 0)
            throw new ValidacaoException("Subestipulante não encontrado.");

        // 4. Localizar vínculo pai ativo
        var vinculoPai = await _dbContext.ApoliceSubestipulantes
            .FirstOrDefaultAsync(s =>
                s.ApoliceId == apolice.Id &&
                s.SubestipulanteId == subestipulanteId &&
                s.DeletedAt == null, cancellationToken);

        if (vinculoPai == null)
            throw new ValidacaoException("Vínculo Apólice ↔ Subestipulante não encontrado.");

        if (!vinculoPai.Ativo)
            throw new ValidacaoException("O vínculo Apólice ↔ Subestipulante está inativo.");

        // 5. Validar vigência dentro do vínculo pai
        ValidarVigenciaDentroDoVinculoPai(request.DataInicio, request.DataFim, vinculoPai);

        // 6. Resolver Módulo Global para obter o Id interno
        var moduloId = await _dbContext.Database
            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.modulo WHERE public_id = {request.ModuloPublicId} AND deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (moduloId == 0)
            throw new ValidacaoException("Módulo não encontrado no Cadastro Global.");

        // 7. Localizar vínculo do Módulo
        var vinculoModulo = await _dbContext.ApoliceSubestipulanteModulos
            .Include(m => m.Vidas)
            .FirstOrDefaultAsync(m =>
                m.ApoliceSubestipulanteId == vinculoPai.Id &&
                m.ModuloId == moduloId &&
                m.DeletedAt == null, cancellationToken);

        if (vinculoModulo == null)
            throw new ValidacaoException("Vínculo de Módulo não encontrado para este Subestipulante nesta Apólice.");

        if (!vinculoModulo.Ativo)
            throw new ValidacaoException("Não é possível alterar um vínculo de Módulo inativo.");

        // 8. Verificar conflito com Vidas ativas dependentes
        // Vida ativa: v.Ativo == true (definição oficial vigente)
        if (request.DataInicio.HasValue || request.DataFim.HasValue)
        {
            var vidasAtivas = vinculoModulo.Vidas.Where(v => v.Ativo).ToList();

            foreach (var vida in vidasAtivas)
            {
                // Se a nova DataInicio for posterior ao fim da vigência da Vida
                if (request.DataInicio.HasValue && vida.DataFimVigencia.HasValue &&
                    request.DataInicio > vida.DataFimVigencia)
                {
                    throw new ValidacaoException(
                        "A nova data de início do Módulo deixaria Vidas ativas fora do período de vigência. " +
                        "Ajuste ou inative as Vidas antes de alterar a vigência do Módulo.");
                }

                // Se a nova DataFim for anterior ao início da vigência da Vida
                if (request.DataFim.HasValue && vida.DataInicioVigencia.HasValue &&
                    request.DataFim < vida.DataInicioVigencia)
                {
                    throw new ValidacaoException(
                        "A nova data de fim do Módulo deixaria Vidas ativas fora do período de vigência. " +
                        "Ajuste ou inative as Vidas antes de alterar a vigência do Módulo.");
                }
            }
        }

        // 9. Obter nome do módulo para histórico
        var nomeModulo = await _dbContext.Database
            .SqlQuery<string>($"SELECT nome AS \"Value\" FROM cadastro.modulo WHERE id = {moduloId}")
            .FirstOrDefaultAsync(cancellationToken) ?? $"ID {moduloId}";

        var nomeSubestipulante = await ObterNomeSubestipulanteAsync(subestipulanteId, cancellationToken);

        // 10. Aplicar alteração
        vinculoModulo.DataInicio = request.DataInicio;
        vinculoModulo.DataFim = request.DataFim;
        vinculoModulo.UpdatedAt = DateTimeOffset.UtcNow;

        // 11. Registrar Histórico funcional
        _dbContext.ApoliceHistoricos.Add(new Infrastructure.Persistence.Models.ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Alteração Vigência Módulo",
            Descricao = $"Vigência do Módulo '{nomeModulo}' com Subestipulante '{nomeSubestipulante}' alterada para {request.DataInicio?.ToString("dd/MM/yyyy") ?? "sem início"} - {request.DataFim?.ToString("dd/MM/yyyy") ?? "sem fim"}.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidarVigenciaDentroDoVinculoPai(
        DateOnly? dataInicio,
        DateOnly? dataFim,
        Infrastructure.Persistence.Models.ApoliceSubestipulanteModel vinculoPai)
    {
        if (dataInicio.HasValue && vinculoPai.DataInicio.HasValue && dataInicio < vinculoPai.DataInicio)
        {
            throw new ValidacaoException(
                $"A data de início do Módulo ({dataInicio}) não pode ser anterior à data de início do vínculo pai ({vinculoPai.DataInicio}).");
        }

        if (dataFim.HasValue && vinculoPai.DataFim.HasValue && dataFim > vinculoPai.DataFim)
        {
            throw new ValidacaoException(
                $"A data de fim do Módulo ({dataFim}) não pode ser posterior à data de fim do vínculo pai ({vinculoPai.DataFim}).");
        }
    }

    private async Task<string> ObterNomeSubestipulanteAsync(long subestipulanteId, CancellationToken cancellationToken)
    {
        try
        {
            var nome = await _dbContext.Database
                .SqlQuery<string>($"SELECT p.nome AS \"Value\" FROM cadastro.subestipulante s INNER JOIN core.pessoa p ON s.pessoa_id = p.id WHERE s.id = {subestipulanteId}")
                .FirstOrDefaultAsync(cancellationToken);
            return nome ?? $"ID {subestipulanteId}";
        }
        catch
        {
            return $"ID {subestipulanteId}";
        }
    }
}
