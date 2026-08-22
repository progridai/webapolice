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
    bool Ativo,
    List<ApoliceSubestipulanteModuloResult> Modulos
);

/// <summary>
/// Projeção pública do vínculo contextual Apólice → Subestipulante → Módulo.
/// Não expõe IDs internos (bigint). Identificação externa via ModuloPublicId.
/// </summary>
public sealed record ApoliceSubestipulanteModuloResult(
    Guid ModuloPublicId,
    string ModuloNome,
    string? ModuloDescricao,
    bool ModuloAtivoGlobal,
    bool VinculoAtivo,
    DateOnly? DataInicio,
    DateOnly? DataFim
);
