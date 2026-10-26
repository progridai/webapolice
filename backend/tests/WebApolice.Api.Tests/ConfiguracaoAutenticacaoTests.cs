using WebApolice.Api.Autenticacao;

namespace WebApolice.Api.Tests;

public class ConfiguracaoAutenticacaoTests
{
    [Theory]
    [InlineData("Production", "https://idp.example/realms/test", "", true)]
    [InlineData("Staging", "https://idp.example/realms/test", "api", false)]
    [InlineData("Production", "http://127.0.0.1/realms/test", "api", true)]
    [InlineData("Development", "http://idp.example/realms/test", "api", false)]
    [InlineData("Testing", "invalid", "api", false)]
    [InlineData("Development", "http://127.0.0.1/realms/test", "", false)]
    public void ConfiguracaoInseguraImpedeInicializacao(string ambiente, string authority, string audience, bool https)
    {
        var config = new ConfiguracaoAutenticacao { Authority = authority, Audience = audience, RequireHttpsMetadata = https };
        Assert.Throws<InvalidOperationException>(() => config.Validar(ambiente));
    }

    [Theory]
    [InlineData("Production", "https://idp.example/realms/test", true)]
    [InlineData("Development", "http://127.0.0.1/realms/test", false)]
    [InlineData("Testing", ApiTestFactory.IsserDeTeste, false)]
    public void ConfiguracaoValidaPermiteInicializacao(string ambiente, string authority, bool https)
    {
        new ConfiguracaoAutenticacao { Authority = authority, Audience = "api", RequireHttpsMetadata = https }.Validar(ambiente);
    }
}
