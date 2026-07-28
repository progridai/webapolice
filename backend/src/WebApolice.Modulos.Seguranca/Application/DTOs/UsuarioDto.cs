using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record UsuarioListDto(
    Guid PublicId,
    string Username,
    string Nome,
    string Email,
    bool Ativo,
    DateTime? UltimoLoginEm,
    IReadOnlyList<string> Perfis
);

public sealed record UsuarioDetalheDto(
    Guid PublicId,
    string KeycloakSub,
    string Username,
    string Nome,
    string Email,
    bool Ativo,
    DateTime? UltimoLoginEm,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PerfilDto> PerfisAtribuidos,
    IReadOnlyList<PerfilDto> PerfisDisponiveis
);
