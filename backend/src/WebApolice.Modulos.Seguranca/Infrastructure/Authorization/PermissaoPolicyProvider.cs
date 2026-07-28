using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguranca.Application.Authorization;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Authorization;

public sealed class PermissaoPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissaoPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissoesSeguranca.PrefixoPolicy, StringComparison.OrdinalIgnoreCase))
        {
            var codigoPermissao = policyName.Substring(PermissoesSeguranca.PrefixoPolicy.Length);

            if (string.IsNullOrWhiteSpace(codigoPermissao))
            {
                // Rejeita a criação de policy válida com código de permissão vazio. 
                // Se retornarmos null, o fallback lida com isso (provavelmente falhando o acesso caso não encontre).
                return Task.FromResult<AuthorizationPolicy?>(null);
            }

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissaoRequirement(codigoPermissao))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
