using WebApolice.Modulos.Cadastro.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApoliceVida;

/// <summary>
/// Handler para incluir um Cliente (Vida) em uma Apólice.
///
/// Regras de negócio:
/// 1. Apólice deve existir (deleted_at IS NULL).
/// 2. Cliente deve existir no Cadastro Global (deleted_at IS NULL).
/// 3. Se Subgrupo: vínculo deve existir na mesma apólice e estar ativo.
/// 4. Se Módulo: vínculo deve existir na mesma apólice e estar ativo.
/// 5. Vigência da Vida deve estar contida na vigência da Apólice e do Módulo (quando aplicável).
/// 6. DataFim >= DataInicio quando ambos informados.
/// 7. Registrar Histórico funcional da Apólice.
/// </summary>
public class CriarApoliceVidaHandler
{
    private readonly IApoliceVinculosRepository _repository;
    private readonly ICadastroSeguroQueries _cadastro;

    public CriarApoliceVidaHandler(IApoliceVinculosRepository dbContext, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _cadastro = cadastro;
    }

    public async Task<Guid> Handle(CriarApoliceVidaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar datas
        if (request.DataFimVigencia.HasValue && request.DataInicioVigencia.HasValue
            && request.DataFimVigencia < request.DataInicioVigencia)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser anterior à data de início.");
        }

        // 2. Localizar Apólice
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, false, cancellationToken);
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // 3. Resolver Cliente
        var clienteDto = await _cadastro.ObterClienteAsync(request.ClientePublicId, cancellationToken);

        if (clienteDto == null)
            throw new ValidacaoException("Cliente não encontrado no Cadastro Global.");

        long? apoliceSubgrupoId = null;
        long? apoliceModuloId = null;
        DateOnly? moduloDataInicio = null;
        DateOnly? moduloDataFim = null;
        string infoSubgrupo = "Nenhum";
        string infoModulo = "Nenhum";

        // 4. Resolver Subgrupo
        if (request.ApoliceSubgrupoPublicId.HasValue)
        {
            var subgrupo = await _repository.ObterSubgrupoAsync(request.ApoliceSubgrupoPublicId.Value, cancellationToken);

            if (subgrupo == null)
                throw new ValidacaoException("Subgrupo não encontrado.");
            
            if (subgrupo.DeletedAt != null)
                throw new ValidacaoException("Subgrupo não encontrado."); // Soft-deleted

            if (subgrupo.ApoliceId != apolice.Id)
                throw new ValidacaoException("O Subgrupo informado pertence a outra Apólice.");

            if (!subgrupo.Ativo)
                throw new ValidacaoException("O Subgrupo está inativo. Não é possível vincular Vidas a um Subgrupo inativo.");

            apoliceSubgrupoId = subgrupo.Id;
            infoSubgrupo = subgrupo.Nome;
        }

        // 5. Resolver Módulo
        if (request.ApoliceModuloPublicId.HasValue)
        {
            var modulo = await _repository.ObterModuloAsync(request.ApoliceModuloPublicId.Value, true, cancellationToken);

            if (modulo == null)
                throw new ValidacaoException("Módulo da Apólice não encontrado.");

            if (modulo.DeletedAt != null)
                throw new ValidacaoException("Módulo da Apólice não encontrado."); // Soft-deleted

            if (modulo.ApoliceId != apolice.Id)
                throw new ValidacaoException("O Módulo informado pertence a outra Apólice.");

            if (!modulo.Ativo)
                throw new ValidacaoException("O Módulo da Apólice está inativo. Não é possível vincular Vidas a um Módulo inativo.");

            apoliceModuloId = modulo.Id;
            moduloDataInicio = modulo.DataInicio;
            moduloDataFim = modulo.DataFim;
            
            // Busca nome do módulo cross-module
            var nomeModulo = (await _cadastro.ObterModuloPorIdAsync(modulo.ModuloId, cancellationToken))?.Nome;
            infoModulo = nomeModulo ?? "Desconhecido";
        }

        // 6. Validar vigência da Vida dentro da Apólice e do Módulo
        ValidarVigenciaDentroDoContextoPai(request.DataInicioVigencia, request.DataFimVigencia, apolice.DataInicioVigencia, apolice.DataFimVigencia, "Apólice");

        if (apoliceModuloId.HasValue)
        {
            ValidarVigenciaDentroDoContextoPai(request.DataInicioVigencia, request.DataFimVigencia, moduloDataInicio, moduloDataFim, "Módulo da Apólice");
        }

        // 7. Criar ApoliceVida
        var vidaPublicId = Guid.NewGuid();
        var novaVida = new ApoliceVidaModel
        {
            PublicId = vidaPublicId,
            ApoliceId = apolice.Id,
            ClienteId = clienteDto.Id,
            ApoliceSubgrupoId = apoliceSubgrupoId,
            ApoliceModuloId = apoliceModuloId,
            DataInicioVigencia = request.DataInicioVigencia,
            DataFimVigencia = request.DataFimVigencia,
            Status = "ativa",
            Ativo = true,
            Observacao = request.Observacao,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _repository.AdicionarVida(novaVida);

        // 8. Registrar Histórico funcional
        _repository.AdicionarHistorico(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Inclusão Vida",
            Descricao = $"Cliente '{clienteDto.Nome}' incluído como Vida na Apólice. Subgrupo: {infoSubgrupo}, Módulo: {infoModulo}.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return vidaPublicId;
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
