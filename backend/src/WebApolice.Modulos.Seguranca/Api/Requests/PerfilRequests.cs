using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Api.Requests;

public sealed record CriarPerfilRequest(
    string Codigo,
    string Nome,
    string Descricao,
    bool Ativo,
    List<Guid> PermissaoPublicIds
);

public sealed record AtualizarPerfilRequest(
    string Nome,
    string Descricao,
    bool Ativo,
    List<Guid> PermissaoPublicIds
);
