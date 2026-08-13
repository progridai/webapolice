using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record PermissoesEfetivasUsuario(
    bool UsuarioEncontrado,
    bool UsuarioAtivo,
    bool AcessoTotal,
    bool OperadorSistema,
    IReadOnlyCollection<string> ModulosHabilitados,
    IReadOnlyCollection<string> RecursosHabilitados,
    IReadOnlyCollection<string> Permissoes);
