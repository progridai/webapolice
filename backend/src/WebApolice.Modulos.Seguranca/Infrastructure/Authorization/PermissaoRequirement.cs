using System;
using Microsoft.AspNetCore.Authorization;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Authorization;

public sealed class PermissaoRequirement : IAuthorizationRequirement
{
    public string CodigoPermissao { get; }

    public PermissaoRequirement(string codigoPermissao)
    {
        if (string.IsNullOrWhiteSpace(codigoPermissao))
        {
            throw new ArgumentException("O código da permissão não pode ser vazio.", nameof(codigoPermissao));
        }

        CodigoPermissao = codigoPermissao;
    }
}
