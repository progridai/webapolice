using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Shared.Infrastructure.Security;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class PermissaoPolicyProviderTests
{
    private readonly PermissaoPolicyProvider _provider;

    public PermissaoPolicyProviderTests()
    {
        var options = Options.Create(new AuthorizationOptions
        {
            // Simular as opções padrão que existem no sistema
        });
        
        options.Value.AddPolicy(PoliticasAutorizacao.GestaoClientes, policy => 
            policy.RequireRole(PerfisAcesso.Gestor));

        _provider = new PermissaoPolicyProvider(options);
    }

    [Fact]
    public async Task Policy_Com_Prefixo_Permissao_Cria_Requirement_Correto()
    {
        var policy = await _provider.GetPolicyAsync("Permissao:clientes.visualizar");

        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, r => r is PermissaoRequirement req && req.CodigoPermissao == "clientes.visualizar");
    }

    [Fact]
    public async Task Policy_Comum_E_Delegada_Ao_Fallback()
    {
        var policy = await _provider.GetPolicyAsync(PoliticasAutorizacao.GestaoClientes);

        Assert.NotNull(policy);
        // Não deve ter PermissaoRequirement, pois caiu no fallback que tem RequireRole
        Assert.DoesNotContain(policy.Requirements, r => r is PermissaoRequirement);
    }

    [Fact]
    public async Task Policy_Vazia_Apos_Prefixo_Nao_Gera_Autorizacao_Incorreta()
    {
        var policy = await _provider.GetPolicyAsync("Permissao:");

        // O provider deve rejeitar a criação e delegar ao fallback (que não achará a policy "Permissao:" e retornará null)
        Assert.Null(policy);
    }

    [Fact]
    public async Task Policy_Com_Espacos_Vazios_Nao_Gera_Autorizacao_Incorreta()
    {
        var policy = await _provider.GetPolicyAsync("Permissao:   ");

        Assert.Null(policy);
    }
}
