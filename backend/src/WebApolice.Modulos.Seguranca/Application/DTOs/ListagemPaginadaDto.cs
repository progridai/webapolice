using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record ListagemPaginadaDto<T>(
    IReadOnlyList<T> Itens,
    int PaginaAtual,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas
);
