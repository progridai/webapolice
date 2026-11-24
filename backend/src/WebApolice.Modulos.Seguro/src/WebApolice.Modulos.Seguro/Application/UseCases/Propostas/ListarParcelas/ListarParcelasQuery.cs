using System.Globalization;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarParcelas;

public sealed record ParcelaHistoricoDto(int Posicao, Guid? PublicId, Guid PropostaPublicId, string? PropostaNumero,
    int Versao, int? Parcela, int? CompetenciaInt, int? Ano, int? Mes, DateOnly? DataVencimento,
    DateOnly? DataLancamento, DateOnly? DataPagamento, string Classificacao, bool Legado,
    string? PremioLiquido, string? Iof, string? PremioTotal, string? ValorPago, string? Observacao);
public sealed record ListarParcelasQuery(Guid PublicId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<PagedResult<ParcelaHistoricoDto>>;

public sealed class ListarParcelasHandler(IPropostasRepository repository) : IRequestHandler<ListarParcelasQuery, PagedResult<ParcelaHistoricoDto>>
{
    public async Task<PagedResult<ParcelaHistoricoDto>> Handle(ListarParcelasQuery request, CancellationToken ct)
    {
        if (request.PublicId == Guid.Empty || request.Pagina < 1 || request.TamanhoPagina is < 1 or > 100 ||
            (long)(request.Pagina - 1) * request.TamanhoPagina > int.MaxValue)
            throw new ValidacaoException("Informe uma proposta válida, página positiva e tamanho de 1 a 100.");
        var pagina = await repository.ListarParcelasAsync(request.PublicId, request.Pagina, request.TamanhoPagina, ct)
            ?? throw new ValidacaoException("Proposta não encontrada ou indisponível para consulta.");
        static string? Moeda(decimal? valor) => valor?.ToString("F2", CultureInfo.InvariantCulture);
        return new()
        {
            Page = pagina.Page, PageSize = pagina.PageSize, TotalCount = pagina.TotalCount,
            Items = pagina.Items.Select((m, i) => new ParcelaHistoricoDto((pagina.Page - 1) * pagina.PageSize + i + 1,
                m.PublicId, m.Proposta!.PublicId, m.Proposta.Numero, m.Proposta.Versao, m.Parcela, m.CompetenciaInt,
                m.Ano, m.Mes, m.DataVencimento, m.DataLancamento, m.DataPagamento, m.Classificacao, m.LegadoId != null,
                Moeda(m.PremioLiquido), Moeda(m.Iof), Moeda(m.PremioTotal), Moeda(m.ValorPago), m.Observacao)).ToArray()
        };
    }
}
