using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterSubgrupo;

/// <summary>
/// Query para obter um Subgrupo específico da Apólice por publicId.
/// Valida que o Subgrupo realmente pertence à Apólice indicada (isolamento).
/// </summary>
public sealed record ObterApoliceSubgrupoPorPublicIdQuery(
    Guid ApolicePublicId,
    Guid SubgrupoPublicId
);
