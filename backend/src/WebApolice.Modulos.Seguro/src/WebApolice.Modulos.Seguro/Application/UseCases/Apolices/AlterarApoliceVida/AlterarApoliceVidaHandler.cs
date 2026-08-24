using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApoliceVida;

/// <summary>
/// Handler para alterar dados editáveis de uma Vida na Apólice.
///
/// Regras de negócio:
/// 1. Apólice deve existir.
/// 2. Vida deve existir e pertencer à Apólice.
/// 3. Vida já encerrada (ativo=false) não pode ser editada.
/// 4. Campos imutáveis: ClienteId, ApoliceId, ApoliceSubestipulanteId, ApoliceSubestipulanteModuloId.
/// 5. DataFim >= DataInicio quando ambos informados.
/// 6. Vigência atualizada deve permanecer dentro do contexto pai.
/// 7. Registrar Histórico funcional da Apólice.
/// </summary>
public class AlterarApoliceVidaHandler
{
    private readonly SeguroDbContext _dbContext;

    public AlterarApoliceVidaHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AlterarApoliceVidaCommand request, CancellationToken cancellationToken)
    {
        // 1. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // 2. Localizar Vida
        var vida = await _dbContext.ApoliceVidas
            .FirstOrDefaultAsync(v =>
                v.PublicId == request.ApoliceVidaPublicId &&
                v.ApoliceId == apolice.Id &&
                v.DeletedAt == null, cancellationToken);

        if (vida == null)
            throw new ValidacaoException("Vida não encontrada nesta Apólice.");

        // 3. Vida encerrada não pode ser editada
        if (!vida.Ativo)
            throw new ValidacaoException("Esta participação já está encerrada e não pode ser editada. Crie uma nova participação se necessário.");

        // 4. Validar datas
        var novoInicio = request.DataInicioVigencia ?? vida.DataInicioVigencia;
        var novoFim = request.DataFimVigencia;

        // DataFim=null significa "sem data fim" (participação em aberto)
        if (novoFim.HasValue && novoInicio.HasValue && novoFim < novoInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser anterior à data de início.");
        }

        // 5. Validar vigência dentro do contexto pai (quando há vínculo pai)
        if (vida.ApoliceSubestipulanteId.HasValue)
        {
            var vinculoPai = await _dbContext.ApoliceSubestipulantes
                .FirstOrDefaultAsync(s => s.Id == vida.ApoliceSubestipulanteId.Value, cancellationToken);

            DateOnly? paiInicio = vinculoPai?.DataInicio;
            DateOnly? paiFim = vinculoPai?.DataFim;

            if (vida.ApoliceSubestipulanteModuloId.HasValue)
            {
                var vinculoModulo = await _dbContext.ApoliceSubestipulanteModulos
                    .FirstOrDefaultAsync(m => m.Id == vida.ApoliceSubestipulanteModuloId.Value, cancellationToken);
                paiInicio = vinculoModulo?.DataInicio ?? paiInicio;
                paiFim = vinculoModulo?.DataFim ?? paiFim;
            }

            if (novoInicio.HasValue && paiInicio.HasValue && novoInicio < paiInicio)
                throw new ValidacaoException($"A data de início da Vida ({novoInicio}) não pode ser anterior à data de início do contexto pai ({paiInicio}).");

            if (novoFim.HasValue && paiFim.HasValue && novoFim > paiFim)
                throw new ValidacaoException($"A data de fim da Vida ({novoFim}) não pode ser posterior à data de fim do contexto pai ({paiFim}).");
        }

        // 6. Atualizar campos editáveis
        vida.DataInicioVigencia = request.DataInicioVigencia ?? vida.DataInicioVigencia;
        vida.DataFimVigencia = request.DataFimVigencia;
        vida.Observacao = request.Observacao ?? vida.Observacao;
        vida.UpdatedAt = DateTimeOffset.UtcNow;

        // 7. Registrar Histórico funcional
        _dbContext.ApoliceHistoricos.Add(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Alteração Vida",
            Descricao = $"Dados da Vida (PublicId: {request.ApoliceVidaPublicId}) alterados na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
