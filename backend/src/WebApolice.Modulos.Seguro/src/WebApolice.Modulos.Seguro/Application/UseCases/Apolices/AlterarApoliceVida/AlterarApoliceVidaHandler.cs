using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApoliceVida;

/// <summary>
/// Handler para alterar dados editáveis de uma Vida na Apólice.
///
/// Regras de negócio:
/// 1. Apólice deve existir.
/// 2. Vida deve existir e pertencer à Apólice.
/// 3. Vida já encerrada (ativo=false) não pode ser editada.
/// 4. Campos imutáveis: ClienteId e ApoliceId. (O Subgrupo e Módulo podem ser alterados).
/// 5. DataFim >= DataInicio quando ambos informados.
/// 6. Vigência atualizada deve permanecer dentro da Apólice e do Módulo da Apólice.
/// 7. Registrar Histórico funcional da Apólice.
/// </summary>
public class AlterarApoliceVidaHandler
{
    private readonly IApoliceVinculosRepository _repository;

    public AlterarApoliceVidaHandler(IApoliceVinculosRepository dbContext)
    {
        _repository = dbContext;
    }

    public async Task Handle(AlterarApoliceVidaCommand request, CancellationToken cancellationToken)
    {
        // 1. Localizar Apólice
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, false, cancellationToken);
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // 2. Localizar Vida
        var vida = await _repository.ObterVidaAsync(request.ApoliceVidaPublicId, apolice.Id, false, cancellationToken);

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

        // 5. Avaliar mudança de Subgrupo e Módulo independentemente
        // Se null for enviado no Command, remove a associação. Se Value for enviado, altera/adiciona.
        long? novoSubgrupoId = null;
        long? novoModuloId = null;
        DateOnly? moduloDataInicio = null;
        DateOnly? moduloDataFim = null;

        if (request.ApoliceSubgrupoPublicId.HasValue)
        {
            var subgrupo = await _repository.ObterSubgrupoAsync(request.ApoliceSubgrupoPublicId.Value, cancellationToken);

            if (subgrupo == null) throw new ValidacaoException("Subgrupo não encontrado.");
            if (subgrupo.DeletedAt != null) throw new ValidacaoException("Subgrupo não encontrado.");
            if (subgrupo.ApoliceId != apolice.Id) throw new ValidacaoException("O Subgrupo informado pertence a outra Apólice.");
            if (!subgrupo.Ativo) throw new ValidacaoException("O Subgrupo está inativo. Não é possível vincular Vidas a um Subgrupo inativo.");
            
            novoSubgrupoId = subgrupo.Id;
        }

        if (request.ApoliceModuloPublicId.HasValue)
        {
            var modulo = await _repository.ObterModuloAsync(request.ApoliceModuloPublicId.Value, true, cancellationToken);

            if (modulo == null) throw new ValidacaoException("Módulo da Apólice não encontrado.");
            if (modulo.DeletedAt != null) throw new ValidacaoException("Módulo da Apólice não encontrado.");
            if (modulo.ApoliceId != apolice.Id) throw new ValidacaoException("O Módulo informado pertence a outra Apólice.");
            if (!modulo.Ativo) throw new ValidacaoException("O Módulo da Apólice está inativo. Não é possível vincular Vidas a um Módulo inativo.");
            
            novoModuloId = modulo.Id;
            moduloDataInicio = modulo.DataInicio;
            moduloDataFim = modulo.DataFim;
        }

        // 6. Validar vigência dentro da Apólice e do Módulo
        ValidarVigenciaDentroDoContextoPai(novoInicio, novoFim, apolice.DataInicioVigencia, apolice.DataFimVigencia, "Apólice");

        if (novoModuloId.HasValue)
        {
            ValidarVigenciaDentroDoContextoPai(novoInicio, novoFim, moduloDataInicio, moduloDataFim, "Módulo da Apólice");
        }

        // 7. Atualizar campos editáveis
        vida.ApoliceSubgrupoId = novoSubgrupoId;
        vida.ApoliceModuloId = novoModuloId;
        vida.DataInicioVigencia = request.DataInicioVigencia ?? vida.DataInicioVigencia;
        vida.DataFimVigencia = request.DataFimVigencia;
        vida.Observacao = request.Observacao ?? vida.Observacao;
        vida.UpdatedAt = DateTimeOffset.UtcNow;
        
        // Mantemos os IDs legados intocados caso já existissem (não os zeramos, conforme regra exigida)
        // vida.ApoliceSubestipulanteId permanece como está.

        // 8. Registrar Histórico funcional
        _repository.AdicionarHistorico(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Alteração Vida",
            Descricao = $"Dados da Vida (PublicId: {request.ApoliceVidaPublicId}) alterados na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }

    private static void ValidarVigenciaDentroDoContextoPai(
        DateOnly? dataInicio,
        DateOnly? dataFim,
        DateOnly? paiInicio,
        DateOnly? paiFim,
        string nomeContexto)
    {
        if (dataInicio.HasValue && paiInicio.HasValue && dataInicio < paiInicio)
        {
            throw new ValidacaoException(
                $"A data de início da Vida ({dataInicio}) não pode ser anterior à data de início de {nomeContexto} ({paiInicio}).");
        }

        if (dataFim.HasValue && paiFim.HasValue && dataFim > paiFim)
        {
            throw new ValidacaoException(
                $"A data de fim da Vida ({dataFim}) não pode ser posterior à data de fim de {nomeContexto} ({paiFim}).");
        }
    }
}
