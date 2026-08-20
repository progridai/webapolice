using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas;

public sealed record ListarApoliceVidasQuery(
    Guid ApolicePublicId,
    int Pagina,
    int TamanhoPagina
);

public sealed record ApoliceVidaResult(
    Guid PublicId,
    long ClienteIdInternal, // Temporarily exposing internal ID due to lack of JOIN, or string
    string ClienteNome,
    string Cpf,
    long? SubestipulanteIdInternal,
    string? SubestipulanteNome,
    long? ModuloIdInternal,
    string? ModuloNome,
    DateOnly? DataInicioVigencia,
    DateOnly? DataFimVigencia,
    string Status,
    bool Ativo
);
