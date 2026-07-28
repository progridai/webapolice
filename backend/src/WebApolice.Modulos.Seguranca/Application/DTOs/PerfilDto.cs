using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record PerfilDto(
    Guid PublicId,
    string Codigo,
    string Nome,
    string Descricao,
    bool Ativo,
    bool PerfilSistema,
    bool AcessoTotal
);

public sealed record PerfilDetalheDto(
    Guid PublicId,
    string Codigo,
    string Nome,
    string Descricao,
    bool Ativo,
    bool PerfilSistema,
    bool AcessoTotal,
    IReadOnlyList<Guid> PermissoesPublicIds
);
