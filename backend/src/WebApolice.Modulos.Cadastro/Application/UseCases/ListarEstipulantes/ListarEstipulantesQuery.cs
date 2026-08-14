using System.Collections.Generic;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarEstipulantes;

public sealed record ListarEstipulantesQuery(
    int Pagina,
    int TamanhoPagina,
    string? Nome,
    string? Cnpj);

public sealed record ListagemPaginadaResult<T>(
    IEnumerable<T> Itens,
    int PaginaAtual,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);
