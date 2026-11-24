using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.Financeiro;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ReincluirProposta;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas;

internal static class VigenciaPropostas
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    internal static DateOnly DataLocal(DateTime utc) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, Fuso));
    internal static DateOnly Hoje(TimeProvider relogio) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(relogio.GetUtcNow(), Fuso).DateTime);

    internal static async Task ValidarContextoAsync(Propostum p, IPropostasRepository repository,
        IClientesPropostas clientes, ICadastroSeguroQueries cadastro, CancellationToken ct)
    {
        if (p.Apolice is null || p.ApoliceVida is null) throw new ValidacaoException("Proposta sem contexto completo de Apólice e Vida.");
        var identidade = await clientes.ResolverParaConfirmacaoAsync(p.ClienteId, p.ClienteVinculoId, ct);
        var plano = p.ApoliceModuloId.HasValue ? await repository.ObterPlanoModuloAsync(p.ApoliceModuloId.Value, ct) : null;
        ValidacaoCondicoesProposta.ValidarCondicoes(p, p.Apolice, p.ApoliceVida, identidade, plano);
        var modulo = await cadastro.ObterModuloParaConfirmacaoAsync((p.CondicoesProprias ? p.ApoliceModulo : p.ApoliceVida.ApoliceModulo)!.ModuloId, ct);
        if (modulo is not { Ativo: true }) throw new ValidacaoException("O cadastro do Módulo está indisponível para confirmar ou ativar a proposta.");
    }

    internal static void ValidarDatas(Propostum p, DateOnly inicio, DateOnly hoje, bool efetivar)
    {
        var apolice = p.Apolice!; var vida = p.ApoliceVida!;
        bool Dentro(DateOnly data) => data >= apolice.DataInicioVigencia &&
            (!apolice.DataFimVigencia.HasValue || data <= apolice.DataFimVigencia) &&
            (!vida.DataInicioVigencia.HasValue || data >= vida.DataInicioVigencia) &&
            (!vida.DataFimVigencia.HasValue || data <= vida.DataFimVigencia);
        if (!Dentro(inicio)) throw new ValidacaoException("A data de início deve estar dentro dos períodos da Apólice e da Vida.");
        if (efetivar && (inicio > hoje || !Dentro(hoje)))
            throw new ValidacaoException("A vigência só pode ser ativada após o início e enquanto os períodos da Apólice e da Vida estiverem válidos.");
    }

    internal static async Task<Propostum?> ValidarOrigemAsync(Propostum p, DateOnly inicio, IPropostasRepository repository, ConfiguracaoPropostas config, CancellationToken ct)
    {
        var propostas = await repository.ObterPropostasDaVidaAsync(p.ApoliceVidaId!.Value, ct);
        var vigentes = propostas.Where(v => v.Vigente && v.DeletedAt == null).ToList();
        if (!p.PropostaOrigemId.HasValue)
        {
            if (vigentes.Count != 0) throw new ValidacaoException("Há outra proposta vigente nesta Vida. Revise a origem da versão antes de continuar.");
            return null;
        }
        var origem = await repository.ObterOrigemParaVigenciaAsync(p.PropostaOrigemId.Value, ct);
        if (origem is not null && origem.CanceladaComVigencia(config.StatusCanceladaCodigo))
        {
            if (origem.ApoliceVidaId != p.ApoliceVidaId || origem.ApoliceId != p.ApoliceId || origem.ClienteId != p.ClienteId ||
                origem.ClienteVinculoId != p.ClienteVinculoId || origem.PessoaId != p.PessoaId || p.Versao <= origem.Versao ||
                !ReinclusaoPropostas.SemImpedimentos(origem, propostas, config, p.Id))
                throw new ValidacaoException("A origem da reinclusão deve permanecer cancelada, sem outra vigente ou tentativa posterior em aberto.");
            if (inicio < origem.DataFimVigencia)
                throw new ValidacaoException("A reinclusão não pode começar antes do primeiro dia sem cobertura da proposta cancelada.");
            return origem;
        }
        if (vigentes.Count != 1 || origem is null || vigentes[0].Id != origem.Id || !origem.Vigente || origem.DeletedAt != null ||
            !origem.VisivelOperacional || origem.Status.Finalizador || origem.CancelamentoSolicitadoEm != null || origem.CanceladaEm != null || origem.SubstituidaEm != null ||
            origem.ApoliceVidaId != p.ApoliceVidaId || origem.ApoliceId != p.ApoliceId || origem.ClienteId != p.ClienteId ||
            origem.ClienteVinculoId != p.ClienteVinculoId || origem.PessoaId != p.PessoaId || p.Versao <= origem.Versao)
            throw new ValidacaoException("A proposta de origem deixou de ser a única vigente da mesma Vida. Revise as versões antes de continuar.");
        if (origem.DataInicioVigencia.HasValue && inicio < origem.DataInicioVigencia)
            throw new ValidacaoException("O início da nova versão não pode anteceder o início registrado da proposta anterior.");
        return origem;
    }

    internal static async Task EfetivarAsync(Propostum p, Propostum? origem, DateTime agora, IPropostasRepository repository, CancellationToken ct, IParcelasPropostasRepository? financeiro = null)
    {
        if (financeiro != null) await ParcelasNativas.PrimeiraAsync(p, financeiro, agora, ct);
        if (origem is not null && origem.CanceladaEm != null && origem.DataFimVigencia != null)
            Historico(p, origem, "Efetivação de Reinclusão", agora);
        else if (origem is not null)
        {
            origem.Vigente = false; origem.SubstituidaEm = agora; origem.DataFimVigencia = p.DataInicioVigencia;
            origem.UpdatedAt = agora; origem.RevisaoPreenchimento = checked(origem.RevisaoPreenchimento + 1);
            Historico(p, origem, "Substituição de Proposta", agora);
            // Liberar a unicidade antes de ativar a sucessora, sempre na mesma transação auditada.
            await repository.SalvarAlteracoesAsync(ct);
        }
        p.Vigente = true; p.MotivoPendenciaVigencia = null; p.PendenciaVigenciaEm = null;
    }

    internal static void Historico(Propostum p, Propostum origem, string motivo, DateTime agora) =>
        p.PropostaHistoricoPropostaNovas.Add(new PropostaHistorico { PropostaAnteriorId = origem.Id, PropostaNova = p,
            Motivo = motivo, DataAlteracao = agora, CreatedAt = agora });

    internal static object Retrato(Propostum p, Propostum? origem = null) => new
    {
        p.PublicId, p.StatusId, p.DescartadaEm, p.CancelamentoSolicitadoEm, p.DataCancelamentoVigencia, p.CanceladaEm, p.RevisaoPreenchimento, p.Vigente, p.ConfirmadaEm, p.SubstituidaEm,
        p.PremioLiquido, p.IofPercentual, p.IofValor, p.ValorParcela, p.DataPrimeiroVencimento,
        p.DataInicioVigencia, p.DataFimVigencia, p.MotivoPendenciaVigencia, p.PendenciaVigenciaEm,
        ParcelasNativas = p.PropostaMovimentos.Where(m => m.LegadoId == null && m.Classificacao == "PARCELA_MENSAL")
            .Select(m => new { m.PublicId, m.Parcela, m.CompetenciaInt, m.DataVencimento, m.PremioLiquido, m.Iof, m.PremioTotal }).ToArray(),
        Origem = origem is null ? null : new { origem.PublicId, origem.Vigente, origem.RevisaoPreenchimento, origem.SubstituidaEm, origem.DataFimVigencia }
    };
}
