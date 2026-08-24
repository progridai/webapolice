using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularModulo;

/// <summary>
/// Handler para vincular um Módulo Global a um Subestipulante no contexto de uma Apólice.
///
/// Regras de negócio:
/// 1. Apólice deve existir.
/// 2. Subestipulante Global deve existir e estar ativo (via cadastro.subestipulante).
/// 3. Vínculo Apólice ↔ Subestipulante deve existir e estar ativo.
/// 4. Módulo Global deve existir, não estar excluído e estar ativo (via cadastro.modulo).
/// 5. Não pode existir vínculo ativo — retorna conflito.
/// 6. Não pode existir vínculo inativo — retorna erro funcional (reativação não disponível).
/// 7. DataFim >= DataInicio quando ambos informados.
/// 8. Vigência do Módulo deve estar contida na vigência do vínculo pai.
///
/// Acesso cross-module: cadastro.modulo é consultado via SqlQuery parametrizado (padrão vigente).
/// Definição de Vida ativa: v.Ativo == true (padrão confirmado em InativarSubestipulanteApoliceHandler).
/// Reativação silenciosa: REMOVIDA — POST nunca reativa vínculo histórico.
/// </summary>
public class VincularModuloApoliceHandler : IRequestHandler<VincularModuloApoliceCommand, long>
{
    private readonly SeguroDbContext _dbContext;

    public VincularModuloApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> Handle(VincularModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar vigência do vínculo
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

        // 4. Localizar vínculo ApoliceSubestipulante ativo
        var vinculoPai = await _dbContext.ApoliceSubestipulantes
            .FirstOrDefaultAsync(s =>
                s.ApoliceId == apolice.Id &&
                s.SubestipulanteId == subestipulanteId &&
                s.DeletedAt == null, cancellationToken);

        if (vinculoPai == null)
            throw new ValidacaoException("Vínculo Apólice ↔ Subestipulante não encontrado.");

        if (!vinculoPai.Ativo)
            throw new ValidacaoException("O vínculo Apólice ↔ Subestipulante está inativo. Não é possível vincular Módulos a um vínculo pai inativo.");

        // 5. Validar vigência do Módulo dentro da vigência do vínculo pai
        ValidarVigenciaDentroDoVinculoPai(request.DataInicio, request.DataFim, vinculoPai);

        // 6. Resolver Módulo Global (cross-module via SQL parametrizado — padrão vigente)
        var modulo = await _dbContext.Database
            .SqlQuery<ModuloGlobalDto>($"SELECT id, nome, descricao, ativo FROM cadastro.modulo WHERE public_id = {request.ModuloPublicId} AND deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (modulo == null)
            throw new ValidacaoException("Módulo não encontrado no Cadastro Global.");

        if (!modulo.Ativo)
            throw new ValidacaoException("O Módulo Global está inativo. Apenas Módulos ativos podem ser vinculados.");

        // 7. Verificar duplicidade — consultar sem filtrar por ativo (índice cobre ativos e inativos com deleted_at IS NULL)
        var vinculoExistente = await _dbContext.ApoliceSubestipulanteModulos
            .FirstOrDefaultAsync(m =>
                m.ApoliceSubestipulanteId == vinculoPai.Id &&
                m.ModuloId == modulo.Id &&
                m.DeletedAt == null, cancellationToken);

        if (vinculoExistente != null && vinculoExistente.Ativo)
            throw new ValidacaoException("Este Módulo já está vinculado ativamente a este Subestipulante nesta Apólice.");

        if (vinculoExistente != null && !vinculoExistente.Ativo)
            throw new ValidacaoException(
                $"Já existe um vínculo histórico inativo do Módulo '{modulo.Nome}' com este Subestipulante nesta Apólice. " +
                "A reativação de vínculos não está disponível nesta etapa.");

        // 8. Criar vínculo
        var novoVinculo = new ApoliceSubestipulanteModuloModel
        {
            ApoliceSubestipulanteId = vinculoPai.Id,
            ModuloId = modulo.Id,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.ApoliceSubestipulanteModulos.Add(novoVinculo);

        // 9. Registrar Histórico funcional da Apólice
        // Recuperar nome do subestipulante para a mensagem
        var nomeSubestipulante = await ObterNomeSubestipulanteAsync(subestipulanteId, cancellationToken);

        _dbContext.ApoliceHistoricos.Add(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Vínculo Módulo",
            Descricao = $"Módulo '{modulo.Nome}' vinculado ao Subestipulante '{nomeSubestipulante}' na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return novoVinculo.Id;
    }

    private static void ValidarVigenciaDentroDoVinculoPai(
        DateOnly? dataInicio,
        DateOnly? dataFim,
        ApoliceSubestipulanteModel vinculoPai)
    {
        // Módulo não pode iniciar antes do vínculo pai
        if (dataInicio.HasValue && vinculoPai.DataInicio.HasValue && dataInicio < vinculoPai.DataInicio)
        {
            throw new ValidacaoException(
                $"A data de início do Módulo ({dataInicio}) não pode ser anterior à data de início do vínculo pai ({vinculoPai.DataInicio}).");
        }

        // Módulo não pode terminar depois do vínculo pai
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

/// <summary>DTO interno para leitura cross-module do Cadastro Global de Módulos.</summary>
internal class ModuloGlobalDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
