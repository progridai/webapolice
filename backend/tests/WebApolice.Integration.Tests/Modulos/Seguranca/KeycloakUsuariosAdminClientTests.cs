using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Infrastructure.Keycloak;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class KeycloakUsuariosAdminClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly KeycloakUsuariosAdminClient _client;
    private readonly KeycloakAdminOptions _options;
    private readonly Mock<ILogger<KeycloakUsuariosAdminClient>> _loggerMock;

    public KeycloakUsuariosAdminClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://auth.bravida.com.br")
        };

        _options = new KeycloakAdminOptions
        {
            BaseUrl = "https://auth.bravida.com.br",
            Realm = "WebApolice",
            ClientId = "admin-cli",
            ClientSecret = "secret"
        };

        var optionsMock = new Mock<IOptions<KeycloakAdminOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_options);

        _loggerMock = new Mock<ILogger<KeycloakUsuariosAdminClient>>();

        _client = new KeycloakUsuariosAdminClient(httpClient, optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObterUsuarioPorSubAsync_DeveRetornarUsuario_QuandoEncontrado()
    {
        // Arrange
        var tokenResponse = new { access_token = "fake-token", expires_in = 3600 };
        var usuarioResponse = new { id = "123", username = "user1", email = "test@test.com", firstName = "User", lastName = "One", enabled = true };

        SetupHttpResponse("https://auth.bravida.com.br/realms/WebApolice/protocol/openid-connect/token", HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));
        SetupHttpResponse("https://auth.bravida.com.br/admin/realms/WebApolice/users/123", HttpStatusCode.OK, JsonSerializer.Serialize(usuarioResponse));

        // Act
        var result = await _client.ObterUsuarioPorSubAsync("123", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("user1");
        result.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task CriarUsuarioAsync_DeveRetornarId_QuandoSucesso()
    {
        // Arrange
        var tokenResponse = new { access_token = "fake-token", expires_in = 3600 };
        SetupHttpResponse("https://auth.bravida.com.br/realms/WebApolice/protocol/openid-connect/token", HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        var responseMsg = new HttpResponseMessage(HttpStatusCode.Created);
        responseMsg.Headers.Location = new Uri("https://auth.bravida.com.br/admin/realms/WebApolice/users/12345-67890");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().Contains("users")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMsg);

        // Act
        var id = await _client.CriarUsuarioAsync("newuser", "new@test.com", "New User", true, CancellationToken.None);

        // Assert
        id.Should().Be("12345-67890");
    }

    [Fact]
    public async Task CriarUsuarioAsync_DevePreservarDetalheDoKeycloak_QuandoDadosForemRecusados()
    {
        // Arrange
        var tokenResponse = new { access_token = "fake-token", expires_in = 3600 };
        SetupHttpResponse("https://auth.bravida.com.br/realms/WebApolice/protocol/openid-connect/token", HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));
        SetupHttpResponse(
            "https://auth.bravida.com.br/admin/realms/WebApolice/users",
            HttpStatusCode.BadRequest,
            "{\"errorMessage\":\"error-user-attribute-required\",\"field\":\"department\"}");

        // Act
        var action = () => _client.CriarUsuarioAsync(
            "newuser",
            "new@test.com",
            "New User",
            true,
            CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<KeycloakUsuarioAdminException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.Message.Should().Be(
            "O provedor de identidade exige um atributo obrigatório que não foi informado.");
        exception.Which.ProviderResponse.Should().Contain("department");
    }

    [Fact]
    public async Task CriarUsuarioAsync_DeveTraduzirErroDeCaractereInvalidoNoUsername()
    {
        // Arrange
        var tokenResponse = new { access_token = "fake-token", expires_in = 3600 };
        SetupHttpResponse("https://auth.bravida.com.br/realms/WebApolice/protocol/openid-connect/token", HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));
        SetupHttpResponse(
            "https://auth.bravida.com.br/admin/realms/WebApolice/users",
            HttpStatusCode.BadRequest,
            "{\"errorMessage\":\"error-username-invalid-character\",\"field\":\"username\"}");

        // Act
        var action = () => _client.CriarUsuarioAsync(
            "invalid username",
            "new@test.com",
            "New User",
            true,
            CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<KeycloakUsuarioAdminException>();
        exception.Which.Message.Should().Be(
            "Username inválido. Use apenas letras, números, ponto, hífen ou sublinhado, sem espaços.");
    }

    [Fact]
    public async Task ExisteUsernameAsync_DeveRetornarVerdadeiro_QuandoUsuarioExiste()
    {
        var tokenResponse = new { access_token = "fake-token", expires_in = 3600 };
        SetupHttpResponse("https://auth.bravida.com.br/realms/WebApolice/protocol/openid-connect/token", HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));
        
        var usersResponse = new[] { new { id = "123" } };
        SetupHttpResponse("https://auth.bravida.com.br/admin/realms/WebApolice/users?username=testuser&exact=true", HttpStatusCode.OK, JsonSerializer.Serialize(usersResponse));

        var exists = await _client.ExisteUsernameAsync("testuser", CancellationToken.None);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExisteEmailAsync_DeveRetornarFalso_QuandoEmailNaoExiste()
    {
        var tokenResponse = new { access_token = "fake-token", expires_in = 3600 };
        SetupHttpResponse("https://auth.bravida.com.br/realms/WebApolice/protocol/openid-connect/token", HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));
        
        SetupHttpResponse("https://auth.bravida.com.br/admin/realms/WebApolice/users?email=notfound@test.com&exact=true", HttpStatusCode.OK, "[]");

        var exists = await _client.ExisteEmailAsync("notfound@test.com", CancellationToken.None);
        exists.Should().BeFalse();
    }

    [Theory]
    [InlineData("{\"errorMessage\":\"internal-fixture-marker\"}")]
    [InlineData("{\"error\":\"internal-fixture-marker\"}")]
    [InlineData("internal-fixture-marker")]
    [InlineData("[]")]
    [InlineData("null")]
    public async Task ErroDesconhecidoDoProvedorNaoVazaMensagemInterna(string body)
    {
        SetupHttpResponse("https://auth.bravida.com.br/realms/WebApolice/protocol/openid-connect/token", HttpStatusCode.OK,
            JsonSerializer.Serialize(new { access_token = "fake-token", expires_in = 3600 }));
        SetupHttpResponse("https://auth.bravida.com.br/admin/realms/WebApolice/users", HttpStatusCode.BadRequest, body);
        var erro = await Assert.ThrowsAsync<KeycloakUsuarioAdminException>(() => _client.CriarUsuarioAsync("fixture", "fixture@example.com", "Fixture", true, default));
        Assert.DoesNotContain("internal-fixture-marker", erro.Message);
        Assert.Equal(body, erro.ProviderResponse);
    }

    private void SetupHttpResponse(string url, HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}
