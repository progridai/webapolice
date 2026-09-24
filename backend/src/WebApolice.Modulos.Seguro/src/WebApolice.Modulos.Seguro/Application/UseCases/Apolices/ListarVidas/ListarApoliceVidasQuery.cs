using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas;

public sealed record ListarApoliceVidasQuery(
    Guid ApolicePublicId,
    int Pagina,
    int TamanhoPagina,
    string? BuscaCliente = null,
    string? Status = null,
    Guid? ApoliceSubgrupoPublicId = null,
    Guid? ApoliceModuloPublicId = null,
    DateOnly? VigenciaDataReferencia = null
);

public sealed record ApoliceVidaResult(
    Guid ApoliceVidaPublicId,
    Guid ClientePublicId,
    string ClienteNome,
    string ClienteDocumentoMascarado,
    Guid? ApoliceSubgrupoPublicId,
    string? SubgrupoNome,
    Guid? ApoliceModuloPublicId,
    string? ModuloNome,
    DateOnly? DataInicioVigencia,
    DateOnly? DataFimVigencia,
    string Status,
    bool Ativo,
    string? Observacao
);
