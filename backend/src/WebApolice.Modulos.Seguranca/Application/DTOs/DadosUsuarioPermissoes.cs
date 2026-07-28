using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record DadosUsuarioPermissoes(
    bool Ativo,
    bool AcessoTotal,
    bool OperadorSistema,
    IReadOnlyCollection<string> ModulosHabilitados,
    IReadOnlyCollection<string> Permissoes
);
