using System.Globalization;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.SimularValorMensal;

public sealed record RamoSimulacao(Guid PublicId, string Nome, string Codigo, decimal? IofPercentual);
public sealed record OpcoesSimulacaoMensalQuery(Guid PublicId) : IRequest<IReadOnlyList<RamoSimulacao>>;
public sealed record SimulacaoMensalDados(int? RevisaoEsperada, Guid? RamoPublicId);
public sealed record SimularValorMensalQuery(Guid PublicId, SimulacaoMensalDados Dados) : IRequest<SimulacaoMensalResultado>;
public sealed record CoberturaSimulada(Guid PublicId, string? Nome, bool PremioAjustado,
    string PremioTitular, string PremioConjuge, string Liquido);
public sealed record SimulacaoMensalResultado(int RevisaoPreenchimento, bool IncluiConjuge, RamoSimulacao Ramo,
    DateTimeOffset CalculadaEm, IReadOnlyList<CoberturaSimulada> Coberturas,
    string LiquidoTitular, string LiquidoConjuge, string LiquidoTotal, string Iof, string TotalMensal);

public static class CalculoMensalProposta
{
    public const decimal Maximo = 9_999_999_999_999_999.99m;
    public static string Moeda(decimal valor) => valor.ToString("F2", CultureInfo.InvariantCulture);
    public static decimal Premio(decimal? valor)
    {
        if (valor is null || valor < 0 || valor > Maximo || decimal.Round(valor.Value, 2) != valor)
            throw new ValidacaoException("Informe os prêmios considerados, não negativos e com até duas casas decimais.");
        return valor.Value;
    }
    public static decimal Somar(decimal a, decimal b)
    {
        var soma = a + b;
        if (soma > Maximo) throw new ValidacaoException("O valor mensal excede o limite monetário permitido.");
        return soma;
    }
    public static decimal Imposto(decimal liquido, decimal? aliquota)
    {
        if (aliquota is null || aliquota < 0 || aliquota > 100 || decimal.Round(aliquota.Value, 4) != aliquota)
            throw new ValidacaoException("Informe no Ramo um IOF entre 0 e 100%, com até quatro casas decimais. Zero deve ser explícito.");
        return decimal.Round(liquido * aliquota.Value / 100m, 2, MidpointRounding.AwayFromZero);
    }
    internal static void Validar(Propostum p)
    {
        if (p.LegadoId != null || p.ApoliceId == null || p.ApoliceVidaId == null || p.DeletedAt != null ||
            !p.VisivelOperacional || !p.Status.VisivelOperacional)
            throw new ValidacaoException("Simule somente propostas nativas visíveis e vinculadas à Apólice e à Vida.");
    }
    internal static RamoSimulacao Ramo(ApoliceRamoModel r) => new(r.PublicId, r.Ramo!.Nome, r.Ramo.Codigo, r.IofPercentual);
}

public sealed class OpcoesSimulacaoMensalHandler(IPropostasRepository repository)
    : IRequestHandler<OpcoesSimulacaoMensalQuery, IReadOnlyList<RamoSimulacao>>
{
    public async Task<IReadOnlyList<RamoSimulacao>> Handle(OpcoesSimulacaoMensalQuery request, CancellationToken ct)
    {
        var p = await repository.ObterParaComparacaoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
        CalculoMensalProposta.Validar(p);
        return (await repository.ObterRamosSimulacaoAsync(p.ApoliceId!.Value, ct)).Select(CalculoMensalProposta.Ramo).ToArray();
    }
}

public sealed class SimularValorMensalHandler(IPropostasRepository repository, TimeProvider relogio)
    : IRequestHandler<SimularValorMensalQuery, SimulacaoMensalResultado>
{
    public async Task<SimulacaoMensalResultado> Handle(SimularValorMensalQuery request, CancellationToken ct)
    {
        var p = await repository.ObterParaComparacaoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
        CalculoMensalProposta.Validar(p);
        if (request.Dados.RevisaoEsperada is null or < 0 || p.RevisaoPreenchimento != request.Dados.RevisaoEsperada)
            throw new ValidacaoException("A proposta foi alterada. Recarregue antes de simular.");
        if (p.IncluiConjuge is null)
            throw new ValidacaoException("Informe na pré proposta se há contratação de cônjuge. Para propostas já confirmadas, prepare uma nova versão.");
        var ramo = (await repository.ObterRamosSimulacaoAsync(p.ApoliceId!.Value, ct))
            .SingleOrDefault(r => r.PublicId == request.Dados.RamoPublicId);
        if (ramo == null) throw new ValidacaoException("Escolha explicitamente um Ramo ativo vinculado à mesma Apólice.");
        var itens = p.PropostaItems.Where(i => i.Ativo).ToArray();
        var coberturas = p.PropostaCoberturas.Where(c => c.Ativo).OrderBy(c => c.PublicId).ToArray();
        if (itens.Length != 1 || itens[0].LegadoId != null || itens[0].ApoliceModuloPlanoId == null || coberturas.Length == 0 ||
            coberturas.Any(c => c.LegadoId != null || c.PropostaItemId != itens[0].Id || c.CoberturaId == null) ||
            coberturas.GroupBy(c => c.CoberturaId).Any(g => g.Count() != 1))
            throw new ValidacaoException("Revise o Plano e as coberturas ativas registradas antes de simular.");
        decimal titular = 0, conjuge = 0;
        var linhas = new List<CoberturaSimulada>();
        foreach (var c in coberturas)
        {
            var t = CalculoMensalProposta.Premio(c.PremioTitular);
            var s = p.IncluiConjuge.Value ? CalculoMensalProposta.Premio(c.PremioConjuge) : 0;
            titular = CalculoMensalProposta.Somar(titular, t);
            conjuge = CalculoMensalProposta.Somar(conjuge, s);
            linhas.Add(new(c.PublicId, c.CoberturaNomeRegistrado, c.PremioAjustado,
                CalculoMensalProposta.Moeda(t), CalculoMensalProposta.Moeda(s), CalculoMensalProposta.Moeda(CalculoMensalProposta.Somar(t, s))));
        }
        var liquido = CalculoMensalProposta.Somar(titular, conjuge);
        var iof = CalculoMensalProposta.Imposto(liquido, ramo.IofPercentual);
        var total = CalculoMensalProposta.Somar(liquido, iof);
        // A leitura das condições usa múltiplas consultas; recusar alterações concorrentes.
        var atual = (await repository.ObterRamosSimulacaoAsync(p.ApoliceId.Value, ct)).SingleOrDefault(r => r.PublicId == ramo.PublicId);
        if (await repository.ObterRevisaoAtualAsync(p.PublicId, ct) != p.RevisaoPreenchimento || atual == null ||
            CalculoMensalProposta.Ramo(atual) != CalculoMensalProposta.Ramo(ramo))
            throw new ValidacaoException("A proposta ou o Ramo mudou durante a simulação. Recarregue e tente novamente.");
        return new(p.RevisaoPreenchimento, p.IncluiConjuge.Value, CalculoMensalProposta.Ramo(ramo), relogio.GetUtcNow(), linhas,
            CalculoMensalProposta.Moeda(titular), CalculoMensalProposta.Moeda(conjuge), CalculoMensalProposta.Moeda(liquido),
            CalculoMensalProposta.Moeda(iof), CalculoMensalProposta.Moeda(total));
    }
}
