using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Api.Requests;

public sealed record CriarUsuarioRequest(
    string Username,
    string Nome,
    string Email,
    string SenhaTemporaria,
    bool Ativo,
    List<Guid> PerfilPublicIds
);

public sealed record AtualizarUsuarioRequest(
    string Nome,
    string Email,
    bool Ativo,
    List<Guid> PerfilPublicIds
);
