using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ReincluirProposta;

internal static class ReinclusaoPropostas
{
    internal static bool SemImpedimentos(Propostum origem, IReadOnlyList<Propostum> propostas, ConfiguracaoPropostas config, long? tentativaId = null) =>
        origem.CanceladaComVigencia(config.StatusCanceladaCodigo) &&
        !propostas.Any(p => p.Id != tentativaId && (p.Vigente ||
            (p.Versao > origem.Versao && !p.TentativaEncerradaSemVigencia(config.StatusCanceladaCodigo, config.StatusDescartadaCodigo))));

    internal static void CopiarCondicoes(Propostum origem, Propostum nova, ApoliceModuloPlanoModel? plano)
    {
        if (origem.ApoliceModuloId != nova.ApoliceModuloId || origem.ApoliceSubgrupoId != nova.ApoliceSubgrupoId ||
            origem.PropostaItems.Count(i => i.Ativo) != 1 || plano == null)
            throw new ValidacaoException("As condições anteriores divergem do contexto atual. Escolha as condições do Módulo para preparar a reinclusão.");
        var anterior = origem.PropostaItems.Single(i => i.Ativo);
        var coberturas = origem.PropostaCoberturas.Where(c => c.Ativo).ToList();
        if (anterior.LegadoId != null || anterior.ApoliceModuloPlanoId != plano.Id || coberturas.Count == 0 ||
            coberturas.Any(c => c.LegadoId != null || c.PropostaItemId != anterior.Id ||
                !plano.Coberturas.Any(atual => atual.Id == c.ApoliceModuloCoberturaId && atual.CoberturaId == c.CoberturaId && atual.Ativo && atual.DeletedAt == null && atual.Cobertura.Ativo)))
            throw new ValidacaoException("O Plano ou as coberturas anteriores estão indisponíveis. Escolha as condições do Módulo ou revise a configuração.");
        nova.SubgrupoNomeRegistrado = origem.SubgrupoNomeRegistrado; nova.ModuloNomeRegistrado = origem.ModuloNomeRegistrado;
        var item = new PropostaItem
        {
            PublicId = Guid.NewGuid(), Proposta = nova, ApoliceModuloPlanoId = anterior.ApoliceModuloPlanoId,
            PlanoNomeRegistrado = anterior.PlanoNomeRegistrado, PlanoPaga = anterior.PlanoPaga, PlanoReajuste = anterior.PlanoReajuste,
            Ramo = anterior.Ramo, Valor = anterior.Valor, Ativo = true, CreatedAt = nova.CreatedAt, UpdatedAt = nova.CreatedAt
        };
        nova.PropostaItems.Add(item);
        foreach (var c in coberturas)
            nova.PropostaCoberturas.Add(new PropostaCobertura
            {
                PublicId = Guid.NewGuid(), Proposta = nova, PropostaItem = item, CoberturaId = c.CoberturaId,
                ApoliceModuloCoberturaId = c.ApoliceModuloCoberturaId, CoberturaNomeRegistrado = c.CoberturaNomeRegistrado,
                CoberturaNomeReduzidoRegistrado = c.CoberturaNomeReduzidoRegistrado, CoberturaBasicaRegistrada = c.CoberturaBasicaRegistrada,
                CoberturaReajuste = c.CoberturaReajuste, PremioTitular = c.PremioTitular, PremioConjuge = c.PremioConjuge,
                PremioAjustado = c.PremioAjustado, PremioTitularInicial = c.PremioTitularInicial, PremioConjugeInicial = c.PremioConjugeInicial,
                JustificativaAjustePremio = c.JustificativaAjustePremio, Ativo = true, CreatedAt = nova.CreatedAt, UpdatedAt = nova.CreatedAt
            });
    }

    internal static void CopiarBeneficiarios(Propostum origem, Propostum nova)
    {
        foreach (var b in origem.PropostaBeneficiarios.Where(b => b.DeletedAt == null && b.PermitePreenchimento()))
            nova.PropostaBeneficiarios.Add(new PropostaBeneficiario
            {
                PublicId = Guid.NewGuid(), Proposta = nova, Nome = b.Nome, ParentescoRegistrado = b.ParentescoRegistrado,
                PercentualParticipacao = b.PercentualParticipacao, Ordem = b.Ordem,
                CreatedAt = nova.CreatedAt, UpdatedAt = nova.CreatedAt
            });
    }
}
