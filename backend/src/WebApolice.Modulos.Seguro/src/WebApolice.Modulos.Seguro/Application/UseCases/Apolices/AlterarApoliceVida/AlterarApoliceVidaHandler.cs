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
/// 4. Campos imutáveis: ClienteId e ApoliceId. (O Contexto - Subestipulante e Módulo - pode ser alterado).
/// 5. DataFim >= DataInicio quando ambos informados.
/// 6. Vigência atualizada deve permanecer dentro do contexto pai atualizado.
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

        // 5. Avaliar mudança de contexto
        long? novoSubestipulanteId = vida.ApoliceSubestipulanteId;
        long? novoModuloId = vida.ApoliceSubestipulanteModuloId;

        if (!string.IsNullOrWhiteSpace(request.Contexto))
        {
            if (request.Contexto == "direto")
            {
                novoSubestipulanteId = null;
                novoModuloId = null;
            }
            else if (request.Contexto == "subestipulante" || request.Contexto == "modulo")
            {
                if (request.SubestipulantePublicId.HasValue)
                {
                    var subestipulanteId = await _dbContext.Database
                        .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {request.SubestipulantePublicId.Value} AND deleted_at IS NULL")
                        .FirstOrDefaultAsync(cancellationToken);

                    if (subestipulanteId == 0) throw new ValidacaoException("Subestipulante não encontrado no Cadastro Global.");

                    var sub = await _dbContext.ApoliceSubestipulantes
                        .FirstOrDefaultAsync(s => s.SubestipulanteId == subestipulanteId && s.ApoliceId == apolice.Id && s.DeletedAt == null, cancellationToken);
                    if (sub == null || !sub.Ativo) throw new ValidacaoException("Subestipulante não encontrado ou inativo nesta Apólice.");
                    novoSubestipulanteId = sub.Id;
                }
                
                if (request.Contexto == "modulo")
                {
                    if (request.ModuloPublicId.HasValue)
                    {
                        var moduloId = await _dbContext.Database
                            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.modulo WHERE public_id = {request.ModuloPublicId.Value} AND deleted_at IS NULL")
                            .FirstOrDefaultAsync(cancellationToken);

                        if (moduloId == 0) throw new ValidacaoException("Módulo não encontrado no Cadastro Global.");

                        var mod = await _dbContext.ApoliceSubestipulanteModulos
                            .FirstOrDefaultAsync(m => m.ModuloId == moduloId && m.ApoliceSubestipulanteId == novoSubestipulanteId && m.DeletedAt == null, cancellationToken);
                        if (mod == null || !mod.Ativo) throw new ValidacaoException("Módulo não encontrado ou inativo no Subestipulante informado.");
                        novoModuloId = mod.Id;
                    }
                }
                else
                {
                    novoModuloId = null;
                }
            }
        }

        // 6. Validar vigência dentro do contexto pai (quando há vínculo pai)
        if (novoSubestipulanteId.HasValue)
        {
            var vinculoPai = await _dbContext.ApoliceSubestipulantes
                .FirstOrDefaultAsync(s => s.Id == novoSubestipulanteId.Value, cancellationToken);

            DateOnly? paiInicio = vinculoPai?.DataInicio;
            DateOnly? paiFim = vinculoPai?.DataFim;

            if (novoModuloId.HasValue)
            {
                var vinculoModulo = await _dbContext.ApoliceSubestipulanteModulos
                    .FirstOrDefaultAsync(m => m.Id == novoModuloId.Value, cancellationToken);
                paiInicio = vinculoModulo?.DataInicio ?? paiInicio;
                paiFim = vinculoModulo?.DataFim ?? paiFim;
            }

            if (novoInicio.HasValue && paiInicio.HasValue && novoInicio < paiInicio)
                throw new ValidacaoException($"A data de início da Vida ({novoInicio}) não pode ser anterior à data de início do contexto pai ({paiInicio}).");

            if (novoFim.HasValue && paiFim.HasValue && novoFim > paiFim)
                throw new ValidacaoException($"A data de fim da Vida ({novoFim}) não pode ser posterior à data de fim do contexto pai ({paiFim}).");
        }

        // 7. Atualizar campos editáveis
        vida.ApoliceSubestipulanteId = novoSubestipulanteId;
        vida.ApoliceSubestipulanteModuloId = novoModuloId;
        vida.DataInicioVigencia = request.DataInicioVigencia ?? vida.DataInicioVigencia;
        vida.DataFimVigencia = request.DataFimVigencia;
        vida.Observacao = request.Observacao ?? vida.Observacao;
        vida.UpdatedAt = DateTimeOffset.UtcNow;

        // 8. Registrar Histórico funcional
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
