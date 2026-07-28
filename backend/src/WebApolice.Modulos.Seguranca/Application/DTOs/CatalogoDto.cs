using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record CatalogoModuloDto(
    Guid PublicId,
    string Codigo,
    string Nome,
    string Descricao,
    string Icone,
    IReadOnlyList<CatalogoRecursoDto> Recursos
);

public sealed record CatalogoRecursoDto(
    Guid PublicId,
    string Codigo,
    string Nome,
    string Descricao,
    string RotaFrontend,
    IReadOnlyList<CatalogoPermissaoDto> Permissoes
);

public sealed record CatalogoPermissaoDto(
    Guid PublicId,
    string Codigo,
    string Nome,
    string Descricao
);
