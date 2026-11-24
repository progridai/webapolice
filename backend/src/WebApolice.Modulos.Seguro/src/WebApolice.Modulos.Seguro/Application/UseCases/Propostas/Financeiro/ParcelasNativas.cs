using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.Financeiro;

internal static class ParcelasNativas
{
    internal const string Classificacao = "PARCELA_MENSAL";
    internal static bool Nativa(PropostaMovimento m) => m.LegadoId == null && m.Classificacao == Classificacao && m.PublicId != null;
    internal static decimal Aberto(PropostaMovimento m) => Math.Max(0, (m.PremioTotal ?? 0) - (m.ValorPago ?? 0));
    internal static void ValidarPlano(PropostaFinanceiro f)
    {
        if (f.Proposta.DataInicioVigencia != f.InicioConferido || f.CondicoesHash != FinanceiroConferencia.Condicoes(f.Proposta, f.InicioConferido))
            throw new ValidacaoException("As condições ou o início mudaram. Confira o financeiro novamente antes da ativação.");
    }
    internal static async Task ValidarPrimeiraAsync(Propostum p, IParcelasPropostasRepository repo, CancellationToken ct)
    {
        var f = await repo.ObterPlanoAsync(p.Id, ct);
        if (f == null) return; // Confirmações anteriores à implantação não recebem cobrança presumida.
        f.Proposta = p;
        ValidarPlano(f);
        var cabecalhos = await repo.ObterCompetenciasAsync(p.ApoliceId!.Value, ct);
        if (cabecalhos.Any(c => c.Competencia >= f.CompetenciaInicial && c.FechadaEm != null))
            throw new ValidacaoException("A competência inicial já foi fechada. Confira outra competência antes de ativar.");
    }
    internal static async Task PrimeiraAsync(Propostum p, IParcelasPropostasRepository repo, DateTime agora, CancellationToken ct)
    {
        await ValidarPrimeiraAsync(p, repo, ct);
        var f = await repo.ObterPlanoAsync(p.Id, ct);
        if (f == null) return;
        var movimentos = await repo.ObterMovimentosAsync(p.ApoliceId!.Value, ct);
        if (movimentos.Any(m => Nativa(m) && m.ApoliceVidaId == p.ApoliceVidaId && m.CompetenciaInt == f.CompetenciaInicial)) return;
        var header = (await repo.ObterCompetenciasAsync(p.ApoliceId.Value, ct)).SingleOrDefault(c => c.Competencia == f.CompetenciaInicial);
        if (header == null) { header = new() { ApoliceId = p.ApoliceId.Value, Competencia = f.CompetenciaInicial }; repo.Adicionar(header); }
        repo.Adicionar(Criar(f, header, movimentos, agora));
        header.Revisao = checked(header.Revisao + 1);
    }
    internal static PropostaMovimento Criar(PropostaFinanceiro f, ParcelaCompetencia header, IReadOnlyList<PropostaMovimento> historico, DateTime agora)
    {
        var p = f.Proposta;
        var maior = historico.Where(m => m.Proposta?.ApoliceVidaId == p.ApoliceVidaId || m.ApoliceVidaId == p.ApoliceVidaId)
            .Select(m => m.Parcela ?? 0).DefaultIfEmpty(0).Max();
        if (maior == int.MaxValue) throw new ValidacaoException("A sequência de parcelas atingiu o limite permitido.");
        var vencimento = CalendarioParcelas.Vencimento(header.Competencia, f.CompetenciaReferencia, f.VencimentoReferencia);
        return new() { PublicId = Guid.NewGuid(), Proposta = p, PropostaId = p.Id, ApoliceVidaId = p.ApoliceVidaId,
            PropostaFinanceiro = f, ParcelaCompetencia = header, Classificacao = Classificacao,
            PessoaId = p.PessoaId, ClienteId = p.ClienteId, ClienteVinculoId = p.ClienteVinculoId, EstipulanteId = p.EstipulanteId,
            CompetenciaInt = header.Competencia, Ano = header.Competencia / 100, Mes = header.Competencia % 100,
            Parcela = maior + 1, DataVencimento = vencimento, DataLancamento = VigenciaPropostas.DataLocal(agora),
            PremioLiquido = f.PremioLiquido, Iof = f.IofValor, PremioTotal = f.TotalMensal, ValorPago = 0,
            CreatedAt = agora, UpdatedAt = agora };
    }
}
