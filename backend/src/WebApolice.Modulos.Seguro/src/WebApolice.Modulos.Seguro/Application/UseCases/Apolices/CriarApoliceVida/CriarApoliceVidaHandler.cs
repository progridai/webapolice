using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApoliceVida;

/// <summary>
/// Handler para incluir um Cliente (Vida) em uma Apólice.
///
/// Regras de negócio:
/// 1. Módulo não pode ser informado sem Subestipulante.
/// 2. Apólice deve existir (deleted_at IS NULL).
/// 3. Cliente deve existir no Cadastro Global (deleted_at IS NULL).
/// 4. Se Subestipulante: vínculo Apólice ↔ Subestipulante deve existir e estar ativo.
/// 5. Se Módulo: vínculo Apólice ↔ Subestipulante ↔ Módulo deve existir e estar ativo.
/// 6. Vigência da Vida deve estar contida na vigência do contexto pai (quando aplicável).
/// 7. DataFim >= DataInicio quando ambos informados.
/// 8. Múltiplas participações do mesmo cliente na mesma apólice/contexto são permitidas (sem constraint de unicidade).
/// 9. Registrar Histórico funcional da Apólice.
///
/// Cross-module: cadastro.cliente, core.pessoa, cadastro.subestipulante, cadastro.modulo
/// consultados via SqlQuery parametrizado (padrão vigente).
/// </summary>
public class CriarApoliceVidaHandler
{
    private readonly SeguroDbContext _dbContext;

    public CriarApoliceVidaHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CriarApoliceVidaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar coerência do contexto
        if (request.ModuloPublicId.HasValue && !request.SubestipulantePublicId.HasValue)
        {
            throw new ValidacaoException("Não é possível vincular um Módulo sem informar o Subestipulante. O Módulo exige o contexto Apólice ↔ Subestipulante ↔ Módulo.");
        }

        // 2. Validar datas
        if (request.DataFimVigencia.HasValue && request.DataInicioVigencia.HasValue
            && request.DataFimVigencia < request.DataInicioVigencia)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser anterior à data de início.");
        }

        // 3. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // 4. Resolver Cliente (cross-module via SQL parametrizado — padrão vigente)
        var clienteDto = await _dbContext.Database
            .SqlQuery<ClienteGlobalDto>(
                $"SELECT c.id, c.public_id, p.nome, p.documento_principal AS documento FROM cadastro.cliente c INNER JOIN core.pessoa p ON c.pessoa_id = p.id WHERE c.public_id = {request.ClientePublicId} AND c.deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (clienteDto == null)
            throw new ValidacaoException("Cliente não encontrado no Cadastro Global.");

        long? apoliceSubestipulanteId = null;
        long? apoliceSubestipulanteModuloId = null;
        DateOnly? dataPaiInicio = null;
        DateOnly? dataPaiFim = null;

        // 5. Resolver Subestipulante (Contexto B e C)
        if (request.SubestipulantePublicId.HasValue)
        {
            var subestipulanteId = await _dbContext.Database
                .SqlQuery<long>(
                    $"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {request.SubestipulantePublicId.Value} AND deleted_at IS NULL")
                .FirstOrDefaultAsync(cancellationToken);

            if (subestipulanteId == 0)
                throw new ValidacaoException("Subestipulante não encontrado no Cadastro Global.");

            var vinculoPai = await _dbContext.ApoliceSubestipulantes
                .FirstOrDefaultAsync(s =>
                    s.ApoliceId == apolice.Id &&
                    s.SubestipulanteId == subestipulanteId &&
                    s.DeletedAt == null, cancellationToken);

            if (vinculoPai == null)
                throw new ValidacaoException("Vínculo Apólice ↔ Subestipulante não encontrado.");

            if (!vinculoPai.Ativo)
                throw new ValidacaoException("O vínculo Apólice ↔ Subestipulante está inativo. Não é possível incluir Vidas em um vínculo inativo.");

            apoliceSubestipulanteId = vinculoPai.Id;
            dataPaiInicio = vinculoPai.DataInicio;
            dataPaiFim = vinculoPai.DataFim;

            // 6. Resolver Módulo (Contexto C)
            if (request.ModuloPublicId.HasValue)
            {
                var moduloId = await _dbContext.Database
                    .SqlQuery<long>(
                        $"SELECT id AS \"Value\" FROM cadastro.modulo WHERE public_id = {request.ModuloPublicId.Value} AND deleted_at IS NULL")
                    .FirstOrDefaultAsync(cancellationToken);

                if (moduloId == 0)
                    throw new ValidacaoException("Módulo não encontrado no Cadastro Global.");

                var vinculoModulo = await _dbContext.ApoliceSubestipulanteModulos
                    .FirstOrDefaultAsync(m =>
                        m.ApoliceSubestipulanteId == vinculoPai.Id &&
                        m.ModuloId == moduloId &&
                        m.DeletedAt == null, cancellationToken);

                if (vinculoModulo == null)
                    throw new ValidacaoException("Módulo não encontrado para este Subestipulante nesta Apólice.");

                if (!vinculoModulo.Ativo)
                    throw new ValidacaoException("O vínculo do Módulo com o Subestipulante está inativo. Não é possível incluir Vidas em um vínculo de Módulo inativo.");

                apoliceSubestipulanteModuloId = vinculoModulo.Id;
                dataPaiInicio = vinculoModulo.DataInicio ?? dataPaiInicio;
                dataPaiFim = vinculoModulo.DataFim ?? dataPaiFim;
            }

            // 7. Validar vigência dentro do contexto pai
            ValidarVigenciaDentroDoContextoPai(request.DataInicioVigencia, request.DataFimVigencia, dataPaiInicio, dataPaiFim);
        }

        // 8. Determinar contexto textual
        var contexto = apoliceSubestipulanteModuloId.HasValue ? "modulo"
            : apoliceSubestipulanteId.HasValue ? "subestipulante"
            : "direto";

        // 9. Criar ApoliceVida
        var vidaPublicId = Guid.NewGuid();
        var novaVida = new ApoliceVidaModel
        {
            PublicId = vidaPublicId,
            ApoliceId = apolice.Id,
            ClienteId = clienteDto.Id,
            ApoliceSubestipulanteId = apoliceSubestipulanteId,
            ApoliceSubestipulanteModuloId = apoliceSubestipulanteModuloId,
            DataInicioVigencia = request.DataInicioVigencia,
            DataFimVigencia = request.DataFimVigencia,
            Status = "ativa",
            Ativo = true,
            Observacao = request.Observacao,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.ApoliceVidas.Add(novaVida);

        // 10. Registrar Histórico funcional
        _dbContext.ApoliceHistoricos.Add(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Inclusão Vida",
            Descricao = $"Cliente '{clienteDto.Nome}' incluído como Vida na Apólice. Contexto: {contexto}.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return vidaPublicId;
    }

    private static void ValidarVigenciaDentroDoContextoPai(
        DateOnly? dataInicio,
        DateOnly? dataFim,
        DateOnly? paiInicio,
        DateOnly? paiFim)
    {
        if (dataInicio.HasValue && paiInicio.HasValue && dataInicio < paiInicio)
        {
            throw new ValidacaoException(
                $"A data de início da Vida ({dataInicio}) não pode ser anterior à data de início do contexto pai ({paiInicio}).");
        }

        if (dataFim.HasValue && paiFim.HasValue && dataFim > paiFim)
        {
            throw new ValidacaoException(
                $"A data de fim da Vida ({dataFim}) não pode ser posterior à data de fim do contexto pai ({paiFim}).");
        }
    }
}

/// <summary>DTO interno para leitura cross-module do Cadastro Global de Clientes.</summary>
internal class ClienteGlobalDto
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Documento { get; set; }
}
