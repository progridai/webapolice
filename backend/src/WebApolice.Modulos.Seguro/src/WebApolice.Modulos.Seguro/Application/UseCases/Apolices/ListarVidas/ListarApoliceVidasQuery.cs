using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas;

public sealed record ListarApoliceVidasQuery(
    Guid ApolicePublicId,
    int Pagina,
    int TamanhoPagina,
    string? BuscaCliente = null,
    string? Status = null,
    Guid? SubestipulantePublicId = null,
    Guid? ModuloPublicId = null,
    DateOnly? VigenciaDataReferencia = null
);

public sealed record ApoliceVidaResult(
    Guid ApoliceVidaPublicId,
    Guid ClientePublicId,
    string ClienteNome,
    string ClienteDocumentoMascarado,
    string Contexto,             // "direto" | "subestipulante" | "modulo"
    Guid? SubestipulantePublicId,
    string? SubestipulanteNome,
    Guid? ModuloPublicId,
    string? ModuloNome,
    DateOnly? DataInicioVigencia,
    DateOnly? DataFimVigencia,
    string Status,
    bool Ativo,
    string? Observacao
);
