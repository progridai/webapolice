using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;

public sealed record ListarClientesQuery(
    int Pagina,
    int TamanhoPagina,
    string? Nome,
    string? Documento,
    short? StatusId,
    string? OrdenarPor,
    string? Direcao
);

public sealed record ListagemPaginadaResult<T>(
    IReadOnlyList<T> Itens,
    int PaginaAtual,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas
);

public sealed record ClienteListagemItemResult(
    Guid Id,
    string Nome,
    string DocumentoMascarado,
    string Status,
    DateTime DataCadastroUtc
);
