using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas;

internal static class ValidacaoCondicoesProposta
{
    internal static void ValidarCondicoes(Propostum proposta, ApoliceModel apolice, ApoliceVidaModel vida,
        IdentidadeClienteProposta? identidade, ApoliceModuloPlanoModel? plano)
    {
        if (!apolice.Ativo || apolice.DeletedAt != null || !vida.Ativo || vida.DeletedAt != null)
            throw new ValidacaoException("A Apólice e a Vida devem estar ativas para confirmar a proposta.");
        if (proposta.ApoliceId != apolice.Id || proposta.ApoliceVidaId != vida.Id || vida.ApoliceId != apolice.Id ||
            vida.ClienteId != proposta.ClienteId || vida.ClienteVinculoId != proposta.ClienteVinculoId ||
            identidade is null || identidade.ClienteId != proposta.ClienteId || identidade.PessoaId != proposta.PessoaId ||
            identidade.ClienteVinculoId != proposta.ClienteVinculoId || identidade.EstipulanteId != apolice.EstipulanteId ||
            proposta.EstipulanteId != apolice.EstipulanteId || proposta.SeguradoraId != apolice.SeguradoraId)
            throw new ValidacaoException("A identidade ou a hierarquia da proposta diverge do contexto atual. Revise a participação antes de confirmar.");
        if (proposta.ApoliceModuloId == null || proposta.ApoliceModuloId != vida.ApoliceModuloId ||
            proposta.ApoliceSubgrupoId != vida.ApoliceSubgrupoId ||
            vida.ApoliceModulo is not { Ativo: true, DeletedAt: null } modulo || modulo.ApoliceId != apolice.Id ||
            (vida.ApoliceSubgrupoId.HasValue && (vida.ApoliceSubgrupo is not { Ativo: true, DeletedAt: null } subgrupo || subgrupo.ApoliceId != apolice.Id)))
            throw new ValidacaoException("O Módulo ou Subgrupo registrado está indisponível ou diverge da Vida. As condições do rascunho precisam ser revistas.");

        var itens = proposta.PropostaItems.Where(i => i.Ativo).ToList();
        if (plano is not { Ativo: true, DeletedAt: null } || plano.ApoliceModuloId != modulo.Id ||
            itens.Count != 1 || itens[0].LegadoId != null || itens[0].ApoliceModuloPlanoId != plano.Id)
            throw new ValidacaoException("A confirmação exige um Plano ativo do Módulo, registrado no rascunho.");
        var coberturas = proposta.PropostaCoberturas.Where(c => c.Ativo).ToList();
        if (coberturas.Count == 0 || coberturas.Any(c => c.LegadoId != null || c.PropostaItemId != itens[0].Id ||
            !plano.Coberturas.Any(atual => atual.Id == c.ApoliceModuloCoberturaId && atual.CoberturaId == c.CoberturaId &&
                atual.Ativo && atual.DeletedAt == null && atual.Cobertura.Ativo)))
            throw new ValidacaoException("A confirmação exige coberturas ativas e compatíveis com o Plano registrado.");
        foreach (var c in coberturas)
        {
            if (!PremioValido(c.PremioTitular) || !PremioValido(c.PremioConjuge) ||
                (c.PremioAjustado && (string.IsNullOrWhiteSpace(c.JustificativaAjustePremio) || c.JustificativaAjustePremio.Length > 1000)))
                throw new ValidacaoException("Revise os prêmios registrados e as justificativas dos ajustes antes de confirmar.");
        }
        var beneficiarios = proposta.PropostaBeneficiarios.Where(b => b.DeletedAt == null).ToList();
        foreach (var b in beneficiarios)
            ValidacaoBeneficiariosProposta.ValidarIndicacao(b.Nome, b.ParentescoRegistrado ?? b.ParentescoOriginal, b.PercentualParticipacao);
        ValidacaoBeneficiariosProposta.ValidarTotal(beneficiarios.Sum(b => b.PercentualParticipacao ?? 0), exigirCem: true);
    }

    internal static bool PremioValido(decimal? valor) => valor is null ||
        (valor >= 0 && valor <= 9999999999999999.99m && decimal.Round(valor.Value, 2) == valor.Value);
}
