using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.PreencherRascunho;

public sealed record BeneficiarioPreenchimento(Guid? PublicId, string? Nome, string? ParentescoRegistrado, decimal? PercentualParticipacao);
public sealed record CoberturaPreenchimento(Guid PublicId, decimal? PremioTitular, decimal? PremioConjuge, string? JustificativaAjustePremio);
public sealed record PreencherRascunhoDados(int? RevisaoEsperada, string? Observacao,
    IReadOnlyList<BeneficiarioPreenchimento>? Beneficiarios, IReadOnlyList<CoberturaPreenchimento>? Coberturas, bool? IncluiConjuge = null);
public sealed record PreencherRascunhoCommand(Guid PublicId, PreencherRascunhoDados Dados) : IRequest<int>;

public sealed class PreencherRascunhoHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IOptions<ConfiguracaoPropostas> configuracao) : IRequestHandler<PreencherRascunhoCommand, int>
{
    public Task<int> Handle(PreencherRascunhoCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Preencher Pré Proposta", async ct =>
        {
            var dados = request.Dados;
            if (request.PublicId == Guid.Empty || dados.RevisaoEsperada is null or < 0 ||
                dados.Beneficiarios is null || dados.Coberturas is null ||
                dados.Beneficiarios.Any(b => b is null) || dados.Coberturas.Any(c => c is null))
                throw new ValidacaoException("Informe a revisão, os beneficiários e as coberturas da pré proposta.");
            var proposta = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct)
                ?? throw new ValidacaoException("Proposta não encontrada.");
            if (!proposta.PermitePreenchimento(configuracao.Value.StatusInicialCodigo))
                throw new ValidacaoException("Somente propostas nativas em pré proposta sem vigência podem ser preenchidas.");
            if (proposta.RevisaoPreenchimento != dados.RevisaoEsperada)
                throw new ValidacaoException("A pré proposta foi alterada em outra tela. Recarregue a proposta antes de salvar.");

            var beneficiarios = proposta.PropostaBeneficiarios.Where(b => b.DeletedAt == null).ToList();
            var editaveis = beneficiarios.Where(b => b.PermitePreenchimento()).ToDictionary(b => b.PublicId);
            var ids = dados.Beneficiarios.Where(b => b.PublicId.HasValue).Select(b => b.PublicId!.Value).ToList();
            if (ids.Contains(Guid.Empty) || ids.Distinct().Count() != ids.Count || ids.Any(id => !editaveis.ContainsKey(id)))
                throw new ValidacaoException("Há beneficiário repetido, protegido ou que não pertence à pré proposta.");
            foreach (var b in dados.Beneficiarios)
                ValidacaoBeneficiariosProposta.ValidarIndicacao(b.Nome, b.ParentescoRegistrado, b.PercentualParticipacao);
            var total = dados.Beneficiarios.Sum(b => b.PercentualParticipacao!.Value) +
                beneficiarios.Where(b => !b.PermitePreenchimento()).Sum(b => b.PercentualParticipacao ?? 0);
            ValidacaoBeneficiariosProposta.ValidarTotal(total);

            var coberturas = proposta.PropostaCoberturas.Where(c => c.LegadoId == null && c.Ativo && c.ApoliceModuloCoberturaId != null).ToDictionary(c => c.PublicId);
            if (dados.Coberturas.Count != coberturas.Count || dados.Coberturas.Select(c => c.PublicId).Distinct().Count() != dados.Coberturas.Count ||
                dados.Coberturas.Any(c => !coberturas.ContainsKey(c.PublicId)))
                throw new ValidacaoException("Informe uma vez cada cobertura editável da pré proposta.");
            var antes = Retrato(proposta);
            var agora = DateTime.UtcNow;
            foreach (var c in dados.Coberturas)
            {
                if (!ValidacaoCondicoesProposta.PremioValido(c.PremioTitular) || !ValidacaoCondicoesProposta.PremioValido(c.PremioConjuge))
                    throw new ValidacaoException("Prêmios devem ser não negativos e ter até duas casas decimais.");
                var atual = coberturas[c.PublicId];
                if (atual.PremioTitular == c.PremioTitular && atual.PremioConjuge == c.PremioConjuge) continue;
                if (string.IsNullOrWhiteSpace(c.JustificativaAjustePremio) || c.JustificativaAjustePremio.Trim().Length > 1000)
                    throw new ValidacaoException("Justifique cada ajuste de prêmio (até 1000 caracteres).");
                if (!atual.PremioAjustado)
                {
                    atual.PremioTitularInicial = atual.PremioTitular;
                    atual.PremioConjugeInicial = atual.PremioConjuge;
                }
                atual.PremioAjustado = true;
                atual.PremioTitular = c.PremioTitular;
                atual.PremioConjuge = c.PremioConjuge;
                atual.JustificativaAjustePremio = c.JustificativaAjustePremio.Trim();
                atual.UpdatedAt = agora;
            }
            foreach (var removido in editaveis.Values.Where(b => !ids.Contains(b.PublicId)))
            { removido.DeletedAt = agora; removido.UpdatedAt = agora; }
            foreach (var b in dados.Beneficiarios)
            {
                var atual = b.PublicId.HasValue ? editaveis[b.PublicId.Value] : new PropostaBeneficiario { CreatedAt = agora };
                if (!b.PublicId.HasValue) proposta.PropostaBeneficiarios.Add(atual);
                atual.Nome = b.Nome!.Trim();
                atual.ParentescoRegistrado = b.ParentescoRegistrado!.Trim();
                atual.PercentualParticipacao = b.PercentualParticipacao;
                atual.UpdatedAt = agora;
            }
            proposta.Observacao = string.IsNullOrWhiteSpace(dados.Observacao) ? null : dados.Observacao.Trim();
            if (dados.IncluiConjuge.HasValue) proposta.IncluiConjuge = dados.IncluiConjuge;
            proposta.RevisaoPreenchimento = checked(proposta.RevisaoPreenchimento + 1);
            proposta.UpdatedAt = agora;
            await repository.SalvarAlteracoesAsync(ct);
            return (proposta.RevisaoPreenchimento, proposta.PublicId.ToString(), antes, Retrato(proposta));
        }, cancellationToken);

    // Minimização: nomes, parentescos e conteúdo livre da observação não são duplicados na auditoria.
    private static object Retrato(Propostum p) => new
    {
        p.PublicId, p.RevisaoPreenchimento, p.IncluiConjuge, PossuiObservacao = !string.IsNullOrEmpty(p.Observacao),
        Beneficiarios = p.PropostaBeneficiarios.Where(b => b.DeletedAt == null).Select(b => new { b.PublicId, b.PercentualParticipacao }).ToArray(),
        Coberturas = p.PropostaCoberturas.Select(c => new { c.PublicId, c.PremioTitular, c.PremioConjuge,
            c.PremioAjustado, c.JustificativaAjustePremio }).ToArray()
    };
}
