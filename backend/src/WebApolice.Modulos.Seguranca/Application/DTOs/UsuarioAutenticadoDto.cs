using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record UsuarioAutenticadoDto(
    bool UsuarioEncontrado,
    bool UsuarioAtivo,
    bool AcessoTotal,
    bool OperadorSistema,
    IReadOnlyList<string> ModulosHabilitados,
    IReadOnlyList<string> Permissoes
);
