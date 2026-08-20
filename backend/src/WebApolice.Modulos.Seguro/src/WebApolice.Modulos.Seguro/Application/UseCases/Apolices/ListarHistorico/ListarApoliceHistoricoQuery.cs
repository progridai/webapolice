using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico;

public sealed record ListarApoliceHistoricoQuery(
    Guid ApolicePublicId,
    int Pagina,
    int TamanhoPagina
);

public sealed record ApoliceHistoricoResult(
    string Acao,
    string? Descricao,
    Guid? UsuarioPublicId,
    DateTimeOffset DataAcao
);
