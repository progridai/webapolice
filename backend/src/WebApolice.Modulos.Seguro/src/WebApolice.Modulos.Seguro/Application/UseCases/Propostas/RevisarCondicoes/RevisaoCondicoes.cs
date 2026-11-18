using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.RevisarCondicoes;

public sealed record OpcaoCondicoes(Guid PublicId, string Nome);
public sealed record OpcoesCondicoes(IReadOnlyList<OpcaoCondicoes> Modulos, IReadOnlyList<OpcaoCondicoes> Subgrupos,
    Guid? ModuloPublicId, Guid? SubgrupoPublicId);
public sealed record SelecaoCondicoes(int? RevisaoEsperada, Guid? ModuloPublicId, Guid? SubgrupoPublicId);
public sealed record AplicarCondicoesDados(int? RevisaoEsperada, Guid? ModuloPublicId, Guid? SubgrupoPublicId,
    string? Comparacao, string? TratamentoAjustes, string? Observacao);
public sealed record PlanoCondicoes(Guid? PublicId, string? Nome, string? Ramo, bool? Paga, bool? Reajuste);
public sealed record CoberturaCondicoes(Guid? PublicId, Guid? VinculoPublicId, string? Nome, string? NomeReduzido,
    string? Basica, bool? Reajuste, decimal? PremioTitular, decimal? PremioConjuge, bool PremioAjustado,
    decimal? PremioTitularInicial, decimal? PremioConjugeInicial, string? JustificativaAjustePremio);
public sealed record CondicoesRegistradas(Guid? ModuloPublicId, string? ModuloNomeRegistrado,
    Guid? SubgrupoPublicId, string? SubgrupoNomeRegistrado, PlanoCondicoes? Plano, IReadOnlyList<CoberturaCondicoes> Coberturas);
public sealed record ComparacaoCondicoes(string Comparacao, int RevisaoPreenchimento, CondicoesRegistradas Atual,
    CondicoesRegistradas Modulo, CondicoesRegistradas MantendoAjustes, bool AlteracaoModulo, bool AlteracaoMantendoAjustes);
public sealed record OpcoesCondicoesQuery(Guid PublicId) : IRequest<OpcoesCondicoes>;
public sealed record CompararCondicoesQuery(Guid PublicId, SelecaoCondicoes Dados) : IRequest<ComparacaoCondicoes>;
public sealed record AplicarCondicoesCommand(Guid PublicId, AplicarCondicoesDados Dados) : IRequest<GestaoAgendamentoResultado>;
internal sealed record AlvoRevisaoCondicoes(ApoliceModuloModel Modulo, ApoliceSubgrupoModel? Subgrupo,
    ApoliceModuloPlanoModel Plano, string Nome, ComparacaoCondicoes Comparacao);

internal static class RevisaoCondicoes
{
    internal static bool Permitida(Propostum p, ConfiguracaoPropostas config, DateOnly hoje) =>
        p.Status.VisivelOperacional && (p.PermitePreenchimento(config.StatusInicialCodigo) ||
        (!string.IsNullOrWhiteSpace(config.StatusConfirmadaCodigo) && p.Status.Codigo == config.StatusConfirmadaCodigo &&
        p.LegadoId == null && !p.Vigente && p.ConfirmadaEm != null && p.DataInicioVigencia > hoje &&
        p.DeletedAt == null && p.VisivelOperacional && !p.Status.Finalizador && !p.Status.PermiteMovimentacao &&
        p.CanceladaEm == null && p.DescartadaEm == null && p.SubstituidaEm == null && p.DataFimVigencia == null &&
        p.CancelamentoSolicitadoEm == null && p.DataCancelamentoVigencia == null && p.ApoliceId != null && p.ApoliceVidaId != null));

    internal static async Task ValidarAsync(Propostum p, int? revisao, ConfiguracaoPropostas config, TimeProvider relogio,
        IClientesPropostas clientes, CancellationToken ct)
    {
        if (!Permitida(p, config, VigenciaPropostas.Hoje(relogio)))
            throw new ValidacaoException("Revise somente pré propostas nativas ou propostas confirmadas com início ainda futuro.");
        if (revisao is null or < 0 || p.RevisaoPreenchimento != revisao)
            throw new ValidacaoException("A proposta foi alterada. Recarregue e compare novamente.");
        var identidade = await clientes.ResolverParaConfirmacaoAsync(p.ClienteId, p.ClienteVinculoId, ct);
        if (p.Apolice is not { Ativo: true, DeletedAt: null } a || p.ApoliceVida is not { Ativo: true, DeletedAt: null } v ||
            v.ApoliceId != a.Id || v.ClienteId != p.ClienteId || v.ClienteVinculoId != p.ClienteVinculoId ||
            identidade is null || identidade.PessoaId != p.PessoaId || identidade.ClienteId != p.ClienteId ||
            identidade.ClienteVinculoId != p.ClienteVinculoId || identidade.EstipulanteId != a.EstipulanteId ||
            p.EstipulanteId != a.EstipulanteId || p.SeguradoraId != a.SeguradoraId)
            throw new ValidacaoException("A Apólice, a Vida e a identidade devem permanecer ativas e compatíveis.");
        if (p.PropostaItems.Any(i => i.LegadoId != null) || p.PropostaCoberturas.Any(c => c.LegadoId != null))
            throw new ValidacaoException("Condições do legado não podem ser revisadas por esta operação.");
        if (p.PropostaItems.Count(i => i.Ativo) > 1)
            throw new ValidacaoException("Há mais de um Plano ativo registrado. Revise os registros antes de continuar.");
        if (p.PropostaCoberturas.Where(c => c.Ativo).GroupBy(c => c.CoberturaId).Any(g => g.Key == null || g.Count() != 1))
            throw new ValidacaoException("Há correspondências de coberturas ambíguas. Revise os registros antes de continuar.");
    }

    internal static CondicoesRegistradas Atual(Propostum p) => new(p.ApoliceModulo?.PublicId, p.ModuloNomeRegistrado,
        p.ApoliceSubgrupo?.PublicId, p.SubgrupoNomeRegistrado,
        p.PropostaItems.Where(i => i.Ativo).Select(i => new PlanoCondicoes(i.ApoliceModuloPlano?.PublicId,
            i.PlanoNomeRegistrado, i.Ramo, i.PlanoPaga, i.PlanoReajuste)).SingleOrDefault(),
        p.PropostaCoberturas.Where(c => c.Ativo).OrderBy(c => c.CoberturaId).Select(c => new CoberturaCondicoes(
            c.Cobertura?.PublicId, c.ApoliceModuloCobertura?.PublicId, c.CoberturaNomeRegistrado, c.CoberturaNomeReduzidoRegistrado,
            c.CoberturaBasicaRegistrada, c.CoberturaReajuste, c.PremioTitular, c.PremioConjuge, c.PremioAjustado,
            c.PremioTitularInicial, c.PremioConjugeInicial, c.JustificativaAjustePremio)).ToArray());

    internal static async Task<AlvoRevisaoCondicoes> CompararAsync(Propostum p, SelecaoCondicoes dados, IPropostasRepository repository,
        ICadastroSeguroQueries cadastro, CancellationToken ct)
    {
        if (dados.ModuloPublicId is null || dados.ModuloPublicId == Guid.Empty || dados.SubgrupoPublicId == Guid.Empty)
            throw new ValidacaoException("Escolha um Módulo e um Subgrupo válido ou Sem Subgrupo.");
        var modulo = await repository.ObterModuloRevisaoAsync(dados.ModuloPublicId.Value, ct);
        var subgrupo = dados.SubgrupoPublicId.HasValue ? await repository.ObterSubgrupoRevisaoAsync(dados.SubgrupoPublicId.Value, ct) : null;
        if (modulo is not { Ativo: true, DeletedAt: null } || modulo.ApoliceId != p.ApoliceId ||
            (dados.SubgrupoPublicId.HasValue && (subgrupo is not { Ativo: true, DeletedAt: null } || subgrupo.ApoliceId != p.ApoliceId)))
            throw new ValidacaoException("Escolha vínculos ativos da mesma Apólice.");
        var global = await cadastro.ObterModuloParaConfirmacaoAsync(modulo.ModuloId, ct);
        var plano = await repository.ObterPlanoModuloAsync(modulo.Id, ct);
        if (global is not { Ativo: true } || plano is not { Ativo: true, DeletedAt: null })
            throw new ValidacaoException("O Módulo deve possuir cadastro e Plano ativos.");
        var coberturas = plano.Coberturas.Where(c => c.Ativo && c.DeletedAt == null && c.Cobertura.Ativo).OrderBy(c => c.CoberturaId).ToList();
        if (coberturas.Count == 0 || coberturas.GroupBy(c => c.CoberturaId).Any(g => g.Count() != 1))
            throw new ValidacaoException("O Plano deve possuir coberturas ativas com correspondência única.");
        var anteriores = p.PropostaCoberturas.Where(c => c.Ativo && c.CoberturaId.HasValue).ToDictionary(c => c.CoberturaId!.Value);
        if (coberturas.Any(c => !ValidacaoCondicoesProposta.PremioValido(c.PremioTitular) || !ValidacaoCondicoesProposta.PremioValido(c.PremioConjuge)))
            throw new ValidacaoException("Revise os prêmios configurados no Módulo antes de continuar.");
        CondicoesRegistradas Destino(bool manter) => new(modulo.PublicId, global.Nome, subgrupo?.PublicId, subgrupo?.Nome,
            new(plano.PublicId, plano.Nome, plano.Ramo, plano.Paga, plano.Reajuste), coberturas.Select(c =>
            {
                anteriores.TryGetValue(c.CoberturaId, out var anterior);
                var ajuste = manter && anterior is { PremioAjustado: true };
                return new CoberturaCondicoes(c.Cobertura.PublicId, c.PublicId, c.Cobertura.Nome, c.Cobertura.NomeReduzido,
                    c.Cobertura.Basica, c.Cobertura.Reajuste, ajuste ? anterior!.PremioTitular : c.PremioTitular,
                    ajuste ? anterior!.PremioConjuge : c.PremioConjuge, ajuste,
                    ajuste ? anterior!.PremioTitularInicial : null, ajuste ? anterior!.PremioConjugeInicial : null,
                    ajuste ? anterior!.JustificativaAjustePremio : null);
            }).ToArray());
        var atual = Atual(p); var destino = Destino(false); var mantendo = Destino(true);
        var conteudo = JsonSerializer.Serialize(new { p.PublicId, p.RevisaoPreenchimento, p.CondicoesProprias,
            p.ConfirmadaEm, p.DataInicioVigencia, Atual = atual, Modulo = destino, Mantendo = mantendo,
            Vida = new { p.ApoliceVida!.PublicId, p.ApoliceVida.ClienteId, p.ApoliceVida.ClienteVinculoId } });
        var token = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(conteudo)));
        bool Diferente(CondicoesRegistradas proposta) => JsonSerializer.Serialize(atual) != JsonSerializer.Serialize(proposta);
        return new(modulo, subgrupo, plano, global.Nome,
            new(token, p.RevisaoPreenchimento, atual, destino, mantendo, Diferente(destino), Diferente(mantendo)));
    }
}

public sealed class OpcoesCondicoesHandler(IPropostasRepository repository, IClientesPropostas clientes, ICadastroSeguroQueries cadastro,
    IOptions<ConfiguracaoPropostas> config, TimeProvider relogio) : IRequestHandler<OpcoesCondicoesQuery, OpcoesCondicoes>
{
    public async Task<OpcoesCondicoes> Handle(OpcoesCondicoesQuery request, CancellationToken ct)
    {
        var p = await repository.ObterParaComparacaoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
        await RevisaoCondicoes.ValidarAsync(p, p.RevisaoPreenchimento, config.Value, relogio, clientes, ct);
        var modulos = await repository.ObterModulosRevisaoAsync(p.ApoliceId!.Value, ct);
        var opcoes = new List<OpcaoCondicoes>();
        foreach (var m in modulos)
        {
            var global = await cadastro.ObterModuloParaConfirmacaoAsync(m.ModuloId, ct);
            if (global is { Ativo: true }) opcoes.Add(new(m.PublicId, global.Nome));
        }
        var subgrupos = await repository.ObterSubgruposRevisaoAsync(p.ApoliceId.Value, ct);
        return new(opcoes.OrderBy(o => o.Nome).ThenBy(o => o.PublicId).ToArray(),
            subgrupos.Select(s => new OpcaoCondicoes(s.PublicId, s.Nome)).ToArray(), p.ApoliceModulo?.PublicId, p.ApoliceSubgrupo?.PublicId);
    }
}

public sealed class CompararCondicoesHandler(IPropostasRepository repository, IClientesPropostas clientes, ICadastroSeguroQueries cadastro,
    IOptions<ConfiguracaoPropostas> config, TimeProvider relogio) : IRequestHandler<CompararCondicoesQuery, ComparacaoCondicoes>
{
    public async Task<ComparacaoCondicoes> Handle(CompararCondicoesQuery request, CancellationToken ct)
    {
        var p = await repository.ObterParaComparacaoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
        await RevisaoCondicoes.ValidarAsync(p, request.Dados.RevisaoEsperada, config.Value, relogio, clientes, ct);
        return (await RevisaoCondicoes.CompararAsync(p, request.Dados, repository, cadastro, ct)).Comparacao;
    }
}

public sealed class AplicarCondicoesHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IClientesPropostas clientes, ICadastroSeguroQueries cadastro, IOptions<ConfiguracaoPropostas> config, TimeProvider relogio)
    : IRequestHandler<AplicarCondicoesCommand, GestaoAgendamentoResultado>
{
    public Task<GestaoAgendamentoResultado> Handle(AplicarCondicoesCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Revisar Condições da Proposta", async ct =>
        {
            var dados = request.Dados;
            if (dados.TratamentoAjustes is not ("MANTER" or "MODULO") || string.IsNullOrWhiteSpace(dados.Observacao) ||
                dados.Observacao.Length > 1000 || string.IsNullOrWhiteSpace(dados.Comparacao))
                throw new ValidacaoException("Compare as condições, escolha o tratamento dos ajustes e justifique em até 1.000 caracteres.");
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            await RevisaoCondicoes.ValidarAsync(p, dados.RevisaoEsperada, config.Value, relogio, clientes, ct);
            var alvo = await RevisaoCondicoes.CompararAsync(p, new(dados.RevisaoEsperada, dados.ModuloPublicId, dados.SubgrupoPublicId), repository, cadastro, ct);
            var comparacao = alvo.Comparacao;
            if (!string.Equals(comparacao.Comparacao, dados.Comparacao, StringComparison.Ordinal))
                throw new ValidacaoException("A proposta ou a configuração mudou desde a comparação. Compare novamente antes de aplicar.");
            var manter = dados.TratamentoAjustes == "MANTER";
            if (!(manter ? comparacao.AlteracaoMantendoAjustes : comparacao.AlteracaoModulo))
                throw new ValidacaoException("Não há diferenças para aplicar com a escolha informada.");
            var antes = comparacao.Atual;
            var agora = relogio.GetUtcNow().UtcDateTime;
            foreach (var c in p.PropostaCoberturas.Where(c => c.Ativo)) { c.Ativo = false; c.UpdatedAt = agora; }
            foreach (var i in p.PropostaItems.Where(i => i.Ativo)) { i.Ativo = false; i.UpdatedAt = agora; }
            // Encerrar condições no contexto anterior antes de trocar os vínculos, sem desligar triggers.
            await repository.SalvarAlteracoesAsync(ct);
            p.ApoliceModuloId = alvo.Modulo.Id; p.ApoliceSubgrupoId = alvo.Subgrupo?.Id;
            p.ApoliceModulo = alvo.Modulo; p.ApoliceSubgrupo = alvo.Subgrupo;
            p.ModuloNomeRegistrado = alvo.Nome; p.SubgrupoNomeRegistrado = alvo.Subgrupo?.Nome;
            p.CondicoesProprias = true;
            await repository.SalvarAlteracoesAsync(ct);
            var plano = alvo.Plano;
            var item = new PropostaItem { PublicId = Guid.NewGuid(), Proposta = p, ApoliceModuloPlanoId = plano.Id,
                PlanoNomeRegistrado = plano.Nome, Ramo = plano.Ramo, PlanoPaga = plano.Paga, PlanoReajuste = plano.Reajuste,
                Ativo = true, CreatedAt = agora, UpdatedAt = agora };
            p.PropostaItems.Add(item);
            var destino = manter ? comparacao.MantendoAjustes : comparacao.Modulo;
            foreach (var c in destino.Coberturas)
            {
                var vinculo = plano.Coberturas.Single(v => v.PublicId == c.VinculoPublicId);
                p.PropostaCoberturas.Add(new PropostaCobertura { PublicId = Guid.NewGuid(), Proposta = p, PropostaItem = item,
                    CoberturaId = vinculo.CoberturaId, ApoliceModuloCoberturaId = vinculo.Id, CoberturaNomeRegistrado = c.Nome,
                    CoberturaNomeReduzidoRegistrado = c.NomeReduzido, CoberturaBasicaRegistrada = c.Basica,
                    CoberturaReajuste = c.Reajuste, PremioTitular = c.PremioTitular, PremioConjuge = c.PremioConjuge,
                    PremioAjustado = c.PremioAjustado, PremioTitularInicial = c.PremioTitularInicial,
                    PremioConjugeInicial = c.PremioConjugeInicial, JustificativaAjustePremio = c.JustificativaAjustePremio,
                    Ativo = true, CreatedAt = agora, UpdatedAt = agora });
            }
            await repository.SalvarAlteracoesAsync(ct);
            if (p.ConfirmadaEm != null)
            {
                await VigenciaPropostas.ValidarContextoAsync(p, repository, clientes, cadastro, ct);
                VigenciaPropostas.ValidarDatas(p, p.DataInicioVigencia!.Value, VigenciaPropostas.Hoje(relogio), false);
                await VigenciaPropostas.ValidarOrigemAsync(p, p.DataInicioVigencia.Value, repository, config.Value, ct);
            }
            GestaoAgendamento.Registrar(p, "Revisão de Condições", dados.Observacao.Trim(), agora);
            await repository.SalvarAlteracoesAsync(ct);
            return (new GestaoAgendamentoResultado(p.RevisaoPreenchimento), p.PublicId.ToString(),
                (object?)new { Condicoes = antes, Comparacao = dados.Comparacao },
                (object?)new { Condicoes = destino, dados.TratamentoAjustes, p.CondicoesProprias, p.RevisaoPreenchimento });
        }, cancellationToken);
}
