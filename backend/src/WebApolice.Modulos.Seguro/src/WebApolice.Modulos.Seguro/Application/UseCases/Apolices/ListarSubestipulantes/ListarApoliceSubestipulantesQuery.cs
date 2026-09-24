using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes;

public sealed record ListarApoliceSubestipulantesQuery(
    Guid ApolicePublicId
);

public sealed record ApoliceSubestipulanteResult(
    Guid SubestipulantePublicId,
    string Nome,
    string? Documento,
    string? Codigo,
    DateOnly? DataInicio,
    DateOnly? DataFim,
    bool Ativo
);


