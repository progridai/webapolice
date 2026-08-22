using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public sealed record ListarModulosDoSubestipulanteQuery(
    Guid ApolicePublicId,
    Guid SubestipulantePublicId
);

/// <summary>
/// Projeção pública de um vínculo contextual Módulo.
/// Não expõe IDs internos (bigint). Identificação via ModuloPublicId.
/// </summary>
public sealed record ModuloDoSubestipulanteResult(
    Guid ModuloPublicId,
    string ModuloNome,
    string? ModuloDescricao,
    bool ModuloAtivoGlobal,
    bool VinculoAtivo,
    DateOnly? DataInicio,
    DateOnly? DataFim
);
