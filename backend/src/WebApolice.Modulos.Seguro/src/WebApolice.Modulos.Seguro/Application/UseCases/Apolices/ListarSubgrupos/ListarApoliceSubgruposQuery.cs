using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubgrupos;

/// <summary>
/// Query para listar todos os Subgrupos de uma Apólice.
/// Subgrupo é uma divisão contextual da Apólice, identificada por nome.
/// </summary>
public sealed record ListarApoliceSubgruposQuery(
    Guid ApolicePublicId
);

/// <summary>
/// Resultado público de um Subgrupo da Apólice.
/// Não expõe PK interna (bigint). Identificação externa via SubgrupoPublicId.
/// </summary>
public sealed record ApoliceSubgrupoResult(
    Guid SubgrupoPublicId,
    string Nome,
    string? Observacao,
    bool Ativo
);
