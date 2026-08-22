using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModulo;

/// <summary>
/// Handler para inativar o vínculo contextual de um Módulo.
///
/// Regras de negócio:
/// 1. Apólice, Subestipulante e vínculo pai devem existir.
/// 2. Vínculo Módulo deve existir e estar ativo.
/// 3. Não pode existir Vida ativa dependente (ApoliceSubestipulanteModuloId = vinculo.Id).
/// 4. Apenas o vínculo contextual é inativado — Módulo Global, Subestipulante, Apólice e Vidas permanecem intactos.
/// 5. Registrar Histórico funcional da Apólice.
///
/// Definição de Vida ativa: v.Ativo == true (padrão oficial vigente do domínio Seguro,
/// confirmado em InativarSubestipulanteApoliceHandler).
/// Sem cascata automática para Vidas.
/// </summary>
public class InativarModuloApoliceHandler : IRequestHandler<InativarModuloApoliceCommand>
{
    private readonly SeguroDbContext _dbContext;

    public InativarModuloApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(InativarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // 2. Resolver Subestipulante Global (cross-module via SQL parametrizado)
        var subestipulanteId = await _dbContext.Database
            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {request.SubestipulantePublicId} AND deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (subestipulanteId == 0)
            throw new ValidacaoException("Subestipulante não encontrado.");

        // 3. Localizar vínculo pai
        var vinculoPai = await _dbContext.ApoliceSubestipulantes
            .FirstOrDefaultAsync(s =>
                s.ApoliceId == apolice.Id &&
                s.SubestipulanteId == subestipulanteId &&
                s.DeletedAt == null, cancellationToken);

        if (vinculoPai == null)
            throw new ValidacaoException("Vínculo Apólice ↔ Subestipulante não encontrado.");

        // 4. Resolver Módulo Global
        var moduloId = await _dbContext.Database
            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.modulo WHERE public_id = {request.ModuloPublicId} AND deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (moduloId == 0)
            throw new ValidacaoException("Módulo não encontrado no Cadastro Global.");

        // 5. Localizar vínculo do Módulo com Vidas
        var vinculoModulo = await _dbContext.ApoliceSubestipulanteModulos
            .Include(m => m.Vidas)
            .FirstOrDefaultAsync(m =>
                m.ApoliceSubestipulanteId == vinculoPai.Id &&
                m.ModuloId == moduloId &&
                m.DeletedAt == null, cancellationToken);

        if (vinculoModulo == null)
            throw new ValidacaoException("Vínculo de Módulo não encontrado para este Subestipulante nesta Apólice.");

        if (!vinculoModulo.Ativo)
            throw new ValidacaoException("O vínculo de Módulo já está inativo.");

        // 6. Verificar Vidas ativas dependentes
        // Vida ativa: v.Ativo == true (definição oficial vigente)
        var possuiVidasAtivas = vinculoModulo.Vidas.Any(v => v.Ativo);
        if (possuiVidasAtivas)
        {
            throw new ValidacaoException(
                "Não é possível inativar este vínculo de Módulo porque existem Vidas ativas associadas a ele. " +
                "Inative ou transfira as Vidas antes de inativar o vínculo do Módulo.");
        }

        // 7. Obter nomes para histórico
        var nomeModulo = await _dbContext.Database
            .SqlQuery<string>($"SELECT nome AS \"Value\" FROM cadastro.modulo WHERE id = {moduloId}")
            .FirstOrDefaultAsync(cancellationToken) ?? $"ID {moduloId}";

        var nomeSubestipulante = await ObterNomeSubestipulanteAsync(subestipulanteId, cancellationToken);

        // 8. Inativar somente o vínculo contextual
        vinculoModulo.Ativo = false;
        vinculoModulo.UpdatedAt = DateTimeOffset.UtcNow;

        // 9. Registrar Histórico funcional
        _dbContext.ApoliceHistoricos.Add(new Infrastructure.Persistence.Models.ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Inativação Módulo",
            Descricao = $"Vínculo do Módulo '{nomeModulo}' com o Subestipulante '{nomeSubestipulante}' inativado na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
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
