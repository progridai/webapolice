using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.SharedKernel.Application.Models;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class PropostasRepository(SeguroDbContext _dbContext, IClientesPropostas clientes) : IPropostasRepository
{
    public async Task<IReadOnlyList<ApoliceRamoModel>> ObterRamosSimulacaoAsync(long apoliceId, CancellationToken ct) =>
        await _dbContext.ApoliceRamos.AsNoTracking().Include(r => r.Ramo)
            .Where(r => r.ApoliceId == apoliceId && r.Ativo && r.DeletedAt == null && r.Ramo != null && r.Ramo.Ativo)
            .OrderBy(r => r.Ramo!.Nome).ThenBy(r => r.PublicId).ToListAsync(ct);
    public Task<int?> ObterRevisaoAtualAsync(Guid publicId, CancellationToken ct) =>
        _dbContext.Proposta.AsNoTracking().Where(p => p.PublicId == publicId && p.DeletedAt == null && p.VisivelOperacional && p.Status.VisivelOperacional)
            .Select(p => (int?)p.RevisaoPreenchimento).SingleOrDefaultAsync(ct);
    public async Task<Propostum?> ObterParaComparacaoAsync(Guid publicId, CancellationToken ct) =>
        await _dbContext.Proposta.AsNoTracking().Include(p => p.Status).Include(p => p.Apolice)
            .Include(p => p.ApoliceVida).Include(p => p.ApoliceModulo).Include(p => p.ApoliceSubgrupo)
            .Include(p => p.PropostaItems).ThenInclude(i => i.ApoliceModuloPlano)
            .Include(p => p.PropostaCoberturas).ThenInclude(c => c.Cobertura)
            .Include(p => p.PropostaCoberturas).ThenInclude(c => c.ApoliceModuloCobertura)
            .AsSplitQuery().SingleOrDefaultAsync(p => p.PublicId == publicId, ct);
    public async Task<IReadOnlyList<ApoliceModuloModel>> ObterModulosRevisaoAsync(long apoliceId, CancellationToken ct) =>
        await _dbContext.ApoliceModulos.AsNoTracking().Where(m => m.ApoliceId == apoliceId && m.Ativo && m.DeletedAt == null).OrderBy(m => m.Id).ToListAsync(ct);
    public async Task<IReadOnlyList<ApoliceSubgrupoModel>> ObterSubgruposRevisaoAsync(long apoliceId, CancellationToken ct) =>
        await _dbContext.ApoliceSubgrupos.AsNoTracking().Where(s => s.ApoliceId == apoliceId && s.Ativo && s.DeletedAt == null).OrderBy(s => s.Nome).ThenBy(s => s.Id).ToListAsync(ct);
    public async Task<ApoliceModuloModel?> ObterModuloRevisaoAsync(Guid publicId, CancellationToken ct)
    {
        if (_dbContext.Database.CurrentTransaction != null)
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM seguro.apolice_modulo WHERE public_id={publicId} FOR SHARE", ct);
        return await _dbContext.ApoliceModulos.SingleOrDefaultAsync(m => m.PublicId == publicId, ct);
    }
    public async Task<ApoliceSubgrupoModel?> ObterSubgrupoRevisaoAsync(Guid publicId, CancellationToken ct)
    {
        if (_dbContext.Database.CurrentTransaction != null)
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM seguro.apolice_subgrupo WHERE public_id={publicId} FOR SHARE", ct);
        return await _dbContext.ApoliceSubgrupos.SingleOrDefaultAsync(s => s.PublicId == publicId, ct);
    }
    public async Task<PropostaStatus?> ObterStatusAsync(string codigo, CancellationToken ct)
    {
        if (_dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction != null)
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM seguro.proposta_status WHERE codigo={codigo} FOR SHARE", ct);
        return await _dbContext.PropostaStatuses.SingleOrDefaultAsync(s => s.Codigo == codigo, ct);
    }
    public Task<ApoliceModel?> ObterApoliceAsync(Guid publicId, CancellationToken ct) => _dbContext.Apolices.AsNoTracking().FirstOrDefaultAsync(a => a.PublicId == publicId, ct);
    public Task<ApoliceVidaModel?> ObterVidaAsync(Guid publicId, CancellationToken ct) => _dbContext.ApoliceVidas.AsNoTracking()
        .Include(v => v.ApoliceModulo).Include(v => v.ApoliceSubgrupo).FirstOrDefaultAsync(v => v.PublicId == publicId, ct);
    public async Task BloquearApoliceAsync(Guid publicId, CancellationToken ct)
    {
        if (_dbContext.Database.IsRelational())
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM seguro.apolice WHERE public_id = {publicId} FOR UPDATE", ct);
    }
    public async Task<IReadOnlyList<Propostum>> ObterPropostasDaVidaAsync(long vidaId, CancellationToken ct) =>
        await _dbContext.Proposta.AsNoTracking().Include(p => p.Status).Where(p => p.ApoliceVidaId == vidaId).ToListAsync(ct);
    public async Task<ApoliceModuloPlanoModel?> ObterPlanoModuloAsync(long moduloId, CancellationToken ct)
    {
        if (_dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction != null)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM seguro.apolice_modulo_plano WHERE apolice_modulo_id={moduloId} ORDER BY id FOR SHARE", ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT c.id FROM seguro.apolice_modulo_cobertura c JOIN seguro.apolice_modulo_plano p ON p.id=c.apolice_modulo_plano_id WHERE p.apolice_modulo_id={moduloId} ORDER BY c.id FOR SHARE OF c", ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT c.id FROM seguro.cobertura c JOIN seguro.apolice_modulo_cobertura mc ON mc.cobertura_id=c.id JOIN seguro.apolice_modulo_plano p ON p.id=mc.apolice_modulo_plano_id WHERE p.apolice_modulo_id={moduloId} ORDER BY c.id FOR SHARE OF c", ct);
        }
        return await _dbContext.ApoliceModuloPlanos.AsNoTracking().Include(p => p.Coberturas).ThenInclude(c => c.Cobertura)
            .SingleOrDefaultAsync(p => p.ApoliceModuloId == moduloId && p.DeletedAt == null, ct);
    }
    public async Task<Propostum?> ObterParaPreenchimentoAsync(Guid publicId, CancellationToken ct)
    {
        var apolice = await _dbContext.Proposta.AsNoTracking().Where(p => p.PublicId == publicId)
            .Select(p => p.Apolice == null ? (Guid?)null : p.Apolice.PublicId).SingleOrDefaultAsync(ct);
        if (apolice.HasValue) await BloquearApoliceAsync(apolice.Value, ct);
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM seguro.proposta WHERE public_id = {publicId} FOR UPDATE", ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT v.id FROM seguro.apolice_vida v JOIN seguro.proposta p ON p.apolice_vida_id=v.id WHERE p.public_id={publicId} FOR SHARE OF v", ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT s.id FROM seguro.proposta_status s JOIN seguro.proposta p ON p.status_id=s.id WHERE p.public_id={publicId} FOR SHARE OF s", ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT m.id FROM seguro.apolice_modulo m JOIN seguro.proposta p ON p.apolice_modulo_id=m.id WHERE p.public_id={publicId} FOR SHARE OF m", ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT s.id FROM seguro.apolice_subgrupo s JOIN seguro.proposta p ON p.apolice_subgrupo_id=s.id WHERE p.public_id={publicId} FOR SHARE OF s", ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT c.id FROM seguro.cobertura c JOIN seguro.proposta_cobertura pc ON pc.cobertura_id=c.id JOIN seguro.proposta p ON p.id=pc.proposta_id WHERE p.public_id={publicId} ORDER BY c.id FOR SHARE OF c", ct);
        }
        return await _dbContext.Proposta.Include(p => p.Status).Include(p => p.PropostaBeneficiarios)
            .Include(p => p.Apolice).Include(p => p.ApoliceVida).ThenInclude(v => v!.ApoliceModulo)
            .Include(p => p.ApoliceVida).ThenInclude(v => v!.ApoliceSubgrupo)
            .Include(p => p.ApoliceModulo).Include(p => p.ApoliceSubgrupo)
            .Include(p => p.PropostaCoberturas).ThenInclude(c => c.Cobertura)
            .Include(p => p.PropostaCoberturas).ThenInclude(c => c.ApoliceModuloCobertura)
            .Include(p => p.PropostaItems).ThenInclude(i => i.ApoliceModuloPlano)
            .AsSplitQuery().SingleOrDefaultAsync(p => p.PublicId == publicId, ct);
    }
    public async Task<Propostum?> ObterOrigemParaVigenciaAsync(long id, CancellationToken ct)
    {
        if (_dbContext.Database.IsRelational())
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM seguro.proposta WHERE id = {id} FOR UPDATE", ct);
        return await _dbContext.Proposta.Include(p => p.Status).SingleOrDefaultAsync(p => p.Id == id, ct);
    }
    public async Task<IReadOnlyList<AgendamentoVigenciaProposta>> ListarAgendadasAsync(DateOnly ate, string statusConfirmada, long aposId, CancellationToken ct) =>
        await _dbContext.Proposta.AsNoTracking().Where(p => p.Id > aposId && p.DescartadaEm == null && p.CancelamentoSolicitadoEm == null && p.CanceladaEm == null && p.LegadoId == null && p.ConfirmadaEm != null &&
            p.DataInicioVigencia <= ate && p.SubstituidaEm == null && !p.Vigente && p.DeletedAt == null && p.VisivelOperacional && p.Status.Codigo == statusConfirmada)
            .OrderBy(p => p.Id).Take(100).Select(p => new AgendamentoVigenciaProposta(p.Id, p.PublicId, p.RevisaoPreenchimento)).ToListAsync(ct);
    public async Task<IReadOnlyList<AgendamentoVigenciaProposta>> ListarCancelamentosAgendadosAsync(DateOnly ate, string statusConfirmada, long aposId, CancellationToken ct) =>
        await _dbContext.Proposta.AsNoTracking().Where(p => p.Id > aposId && p.LegadoId == null && p.CancelamentoSolicitadoEm != null &&
            p.DataCancelamentoVigencia <= ate && p.CanceladaEm == null && p.DescartadaEm == null && p.Vigente &&
            p.DeletedAt == null && p.VisivelOperacional && p.Status.Codigo == statusConfirmada)
            .OrderBy(p => p.Id).Take(100).Select(p => new AgendamentoVigenciaProposta(p.Id, p.PublicId, p.RevisaoPreenchimento)).ToListAsync(ct);
    public Task<Propostum?> ObterAsync(Guid publicId, CancellationToken ct) => _dbContext.Proposta.FirstOrDefaultAsync(p => p.PublicId == publicId, ct);
    public Task<ApoliceProdutoModel?> ObterProdutoPermitidoAsync(long? apoliceId, long produtoId, CancellationToken ct) => _dbContext.ApoliceProdutos.FirstOrDefaultAsync(p => p.ApoliceId == apoliceId && p.ProdutoId == produtoId && p.DeletedAt == null, ct);
    public Task<ApolicePlanoModel?> ObterPlanoPermitidoAsync(long apoliceProdutoId, long planoId, CancellationToken ct) => _dbContext.ApolicePlanos.FirstOrDefaultAsync(p => p.ApoliceProdutoId == apoliceProdutoId && p.PlanoId == planoId && p.DeletedAt == null, ct);
    public void Adicionar(Propostum proposta) => _dbContext.Proposta.Add(proposta);
    public void AdicionarItem(PropostaItem item) => _dbContext.PropostaItems.Add(item);
    public async Task SalvarAlteracoesAsync(CancellationToken ct)
    {
        try { await _dbContext.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        { throw new ValidacaoException("A pré proposta foi alterada em outra tela. Recarregue a proposta antes de salvar."); }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { ConstraintName: "ux_proposta_nativa_vigente_vida" })
        { throw new ValidacaoException("Esta Vida já possui outra proposta nativa vigente. Recarregue as propostas antes de continuar."); }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { ConstraintName: "ck_proposta_vinculo" or "ck_proposta_contexto" } pg)
        { throw new WebApolice.SharedKernel.Application.Exceptions.ValidacaoException(pg.MessageText); }
    }
    public async Task<PagedResult<PropostaDto>> ListarAsync(ObterPropostasQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Proposta.AsNoTracking().Where(p => p.DeletedAt == null && p.VisivelOperacional);

        if (request.ApolicePublicId == Guid.Empty || request.ApoliceVidaPublicId == Guid.Empty)
            throw new ValidacaoException("Informe UUIDs válidos para Apólice e Vida.");
        if (request.ApoliceVidaPublicId.HasValue)
        {
            var vida = await _dbContext.ApoliceVidas.AsNoTracking().SingleOrDefaultAsync(v => v.PublicId == request.ApoliceVidaPublicId, cancellationToken);
            if (vida is null || (request.ApolicePublicId.HasValue &&
                !await _dbContext.Apolices.AnyAsync(a => a.Id == vida.ApoliceId && a.PublicId == request.ApolicePublicId, cancellationToken)))
                throw new ValidacaoException("Vida não encontrada no contexto da Apólice informada.");
            query = query.Where(p => p.ApoliceVidaId == vida.Id);
        }
        if (request.ApolicePublicId.HasValue)
            query = query.Where(p => p.Apolice != null && p.Apolice.PublicId == request.ApolicePublicId);

        if (!string.IsNullOrWhiteSpace(request.TermoBusca))
        {
            query = query.Where(p => p.Numero != null && p.Numero.Contains(request.TermoBusca));
        }

        var totalRegistros = await query.CountAsync(cancellationToken);

        var registros = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(p => new
            {
                p.ClienteId, p.ClienteVinculoId,
                Dto = new PropostaDto
                {
                    PublicId = p.PublicId, Numero = p.Numero,
                    IntegradaApolice = p.ApoliceId != null && p.ApoliceVidaId != null,
                    PremioLiquido = p.PremioLiquido, DataInclusao = p.DataInclusao,
                    Status = p.Status.Nome, StatusCodigo = p.Status.Codigo,
                    ApolicePublicId = p.Apolice == null ? null : p.Apolice.PublicId,
                    ApoliceVidaPublicId = p.ApoliceVida == null ? null : p.ApoliceVida.PublicId,
                    ApoliceNome = p.Apolice == null ? null : p.Apolice.Nome,
                    Vigente = p.Vigente, Versao = p.Versao, CreatedAt = p.CreatedAt
                }
            })
            .ToListAsync(cancellationToken);

        var referencias = await clientes.ConsultarReferenciasAsync(registros.Select(p => p.ClienteId).Distinct().ToArray(),
            registros.Select(p => p.ClienteVinculoId).Distinct().ToArray(), cancellationToken);
        foreach (var registro in registros) PreencherReferencias(registro.Dto, registro.ClienteId, registro.ClienteVinculoId, referencias);
        var propostas = registros.Select(p => p.Dto).ToList();
        return new PagedResult<PropostaDto> { Items = propostas, TotalCount = totalRegistros, Page = request.Pagina, PageSize = request.TamanhoPagina };
    }

    private static void PreencherReferencias(PropostaDto dto, long clienteId, long vinculoId, ReferenciasPropostas referencias)
    {
        var cliente = referencias.Clientes.SingleOrDefault(c => c.ClienteId == clienteId);
        var vinculo = referencias.Vinculos.SingleOrDefault(v => v.VinculoId == vinculoId && v.ClienteId == clienteId);
        dto.ClientePublicId = cliente?.ClientePublicId;
        dto.ClienteNome = cliente?.Nome;
        dto.ClienteVinculoPublicId = vinculo?.PublicId;
        dto.VinculoMatricula = vinculo?.Matricula;
    }

    public async Task<PropostaDetalheDto?> ObterDetalheAsync(Guid publicId, CancellationToken ct, string? codigoInicial = null)
    {
        var registro = await _dbContext.Proposta.AsNoTracking()
            .Where(p => p.PublicId == publicId && p.DeletedAt == null && p.VisivelOperacional)
            .Select(p => new
            {
                p.Id, p.ClienteId, p.ClienteVinculoId,
                Dto = new PropostaDetalheDto
                {
                    PodePrepararReinclusao = p.LegadoId == null && !p.Vigente && p.CanceladaEm != null && p.ConfirmadaEm != null &&
                        p.DataFimVigencia != null && p.DataCancelamentoVigencia == p.DataFimVigencia && p.CancelamentoSolicitadoEm != null &&
                        p.SubstituidaEm == null && p.DescartadaEm == null && p.Apolice != null && p.Apolice.Ativo && p.Apolice.DeletedAt == null,
                    Reinclusao = p.LegadoId == null && p.PropostaOrigem != null && p.PropostaOrigem.LegadoId == null &&
                        p.PropostaOrigem.CanceladaEm != null && p.PropostaOrigem.DataFimVigencia != null && p.PropostaOrigem.CancelamentoSolicitadoEm != null,
                    DataFimVigenciaOrigem = p.PropostaOrigem == null ? null : p.PropostaOrigem.DataFimVigencia,
                    MotivoPendenciaVigencia = p.MotivoPendenciaVigencia, PendenciaVigenciaEm = p.PendenciaVigenciaEm,
                    DataCancelamentoVigencia = p.DataCancelamentoVigencia, CancelamentoSolicitadoEm = p.CancelamentoSolicitadoEm, DescartadaEm = p.DescartadaEm,
                    PodeCancelarVigencia = p.LegadoId == null && p.Vigente && p.ConfirmadaEm != null && p.DataInicioVigencia != null &&
                        p.CanceladaEm == null && p.DescartadaEm == null && p.SubstituidaEm == null && p.DataFimVigencia == null &&
                        !p.Status.Finalizador && !p.Status.PermiteMovimentacao,
                    CanceladaEm = p.CanceladaEm,
                    ConfirmadaEm = p.ConfirmadaEm, SubstituidaEm = p.SubstituidaEm,
                    DataInicioVigencia = p.DataInicioVigencia, DataFimVigencia = p.DataFimVigencia,
                    PublicId = p.PublicId, Numero = p.Numero,
                    IntegradaApolice = p.ApoliceId != null && p.ApoliceVidaId != null,
                    PremioLiquido = p.PremioLiquido, DataInclusao = p.DataInclusao,
                    Status = p.Status.Nome, StatusCodigo = p.Status.Codigo,
                    ApolicePublicId = p.Apolice == null ? null : p.Apolice.PublicId,
                    ApoliceVidaPublicId = p.ApoliceVida == null ? null : p.ApoliceVida.PublicId,
                    ApoliceNome = p.Apolice == null ? null : p.Apolice.Nome,
                    PropostaOrigemPublicId = p.PropostaOrigem == null ? null : p.PropostaOrigem.PublicId,
                    Vigente = p.Vigente, Versao = p.Versao, CreatedAt = p.CreatedAt,
                    ValorParcela = p.ValorParcela, IofPercentual = p.IofPercentual, IofValor = p.IofValor,
                    DataPrimeiroVencimento = p.DataPrimeiroVencimento, DataProximoVencimento = p.DataProximoVencimento,
                    RevisaoPreenchimento = p.RevisaoPreenchimento,
                    PodeEfetivarVigencia = p.DescartadaEm == null && p.CancelamentoSolicitadoEm == null && p.CanceladaEm == null && p.LegadoId == null && p.ConfirmadaEm != null && p.DataInicioVigencia != null &&
                        !p.Vigente && p.SubstituidaEm == null && !p.Status.Finalizador && !p.Status.PermiteMovimentacao,
                    PodeEditarRascunho = p.DescartadaEm == null && p.CancelamentoSolicitadoEm == null && p.CanceladaEm == null && p.ConfirmadaEm == null && p.SubstituidaEm == null && codigoInicial != null && codigoInicial != "" && p.LegadoId == null &&
                        p.ApoliceId != null && p.ApoliceVidaId != null && !p.Vigente &&
                        p.Status.Codigo == codigoInicial && !p.Status.Finalizador && !p.Status.PermiteMovimentacao,
                    Observacao = p.Observacao, ModuloNomeRegistrado = p.ModuloNomeRegistrado, SubgrupoNomeRegistrado = p.SubgrupoNomeRegistrado,
                    CondicoesProprias = p.CondicoesProprias, IncluiConjuge = p.IncluiConjuge,
                    PodeSimularValorMensal = p.LegadoId == null && p.ApoliceId != null && p.ApoliceVidaId != null
                }
            }).SingleOrDefaultAsync(ct);
        if (registro is null) return null;
        var dto = registro.Dto;
        var referencias = await clientes.ConsultarReferenciasAsync([registro.ClienteId], [registro.ClienteVinculoId], ct);
        PreencherReferencias(dto, registro.ClienteId, registro.ClienteVinculoId, referencias);
        dto.Itens = await _dbContext.PropostaItems.AsNoTracking().Where(i => i.PropostaId == registro.Id).OrderBy(i => i.Id)
            .Select(i => new PropostaItemDto(i.PublicId, i.Produto == null ? null : i.Produto.Nome,
                i.Plano == null ? null : i.Plano.Nome, i.PlanoNomeRegistrado ?? i.PlanoNomeLegado, i.Valor, i.Ativo,
                i.ApoliceModuloPlanoId != null, i.Ramo, i.PlanoPaga, i.PlanoReajuste)).ToListAsync(ct);
        dto.Coberturas = await _dbContext.PropostaCoberturas.AsNoTracking().Where(c => c.PropostaId == registro.Id).OrderBy(c => c.Id)
            .Select(c => new PropostaCoberturaDto(c.PublicId, c.PropostaItem == null ? null : c.PropostaItem.PublicId,
                c.Cobertura == null ? null : c.Cobertura.Nome, c.CoberturaNomeRegistrado ?? c.CoberturaNomeLegado, c.PremioTitular, c.PremioConjuge, c.Ativo,
                c.CoberturaNomeReduzidoRegistrado, c.CoberturaBasicaRegistrada, c.CoberturaReajuste,
                c.LegadoId == null && c.Ativo && c.ApoliceModuloCoberturaId != null,
                c.PremioAjustado, c.PremioTitularInicial, c.PremioConjugeInicial, c.JustificativaAjustePremio)).ToListAsync(ct);
        dto.Beneficiarios = await _dbContext.PropostaBeneficiarios.AsNoTracking().Where(b => b.PropostaId == registro.Id && b.DeletedAt == null)
            .OrderBy(b => b.Ordem).ThenBy(b => b.Id)
            .Select(b => new PropostaBeneficiarioDto(b.Nome, b.ParentescoRegistrado ?? b.ParentescoOriginal, b.PercentualParticipacao,
                b.PublicId, b.LegadoId == null && b.PessoaId == null && b.CpfOriginal == null && b.CpfLimpo == null &&
                b.ParentescoOriginal == null && b.ParentescoNormalizado == null && b.NomeNormalizado == null)).ToListAsync(ct);
        dto.Historico = await _dbContext.PropostaHistoricos.AsNoTracking()
            .Where(h => h.PropostaAnteriorId == registro.Id || h.PropostaNovaId == registro.Id)
            .OrderBy(h => h.DataAlteracao).ThenBy(h => h.Id)
            .Select(h => new PropostaHistoricoDto(h.DataAlteracao, h.Motivo, h.Observacao,
                h.PropostaAnterior.DeletedAt == null && h.PropostaAnterior.VisivelOperacional ? h.PropostaAnterior.PublicId : null,
                h.PropostaAnterior.Versao,
                h.PropostaNova.DeletedAt == null && h.PropostaNova.VisivelOperacional ? h.PropostaNova.PublicId : null,
                h.PropostaNova.Versao)).ToListAsync(ct);
        return dto;
    }
}
