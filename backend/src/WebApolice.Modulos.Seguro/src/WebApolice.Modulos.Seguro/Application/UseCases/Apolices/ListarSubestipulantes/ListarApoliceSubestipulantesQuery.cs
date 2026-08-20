using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes;

public sealed record ListarApoliceSubestipulantesQuery(
    Guid ApolicePublicId
);

public sealed record ApoliceSubestipulanteResult(
    long SubestipulanteIdInternal, // ID real do subestipulante (temporário sem JOIN)
    DateOnly? DataInicio,
    DateOnly? DataFim,
    bool Ativo,
    List<ApoliceSubestipulanteModuloResult> Modulos
);

public sealed record ApoliceSubestipulanteModuloResult(
    long ModuloIdInternal, // ID real do módulo
    DateOnly? DataInicio,
    DateOnly? DataFim,
    bool Ativo
);
