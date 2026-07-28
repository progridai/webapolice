using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.Services;
using WebApolice.Modulos.Seguranca.Domain;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class ProvisionamentoUsuarioServiceTests
{
    private readonly Mock<IContextoUsuarioAutenticado> _contextoMock;
    private readonly Mock<IUsuarioProvisionamentoRepository> _repositoryMock;
    private readonly Mock<ILogger<ProvisionamentoUsuarioService>> _loggerMock;
    private readonly ProvisionamentoUsuarioService _service;

    public ProvisionamentoUsuarioServiceTests()
    {
        _contextoMock = new Mock<IContextoUsuarioAutenticado>();
        _repositoryMock = new Mock<IUsuarioProvisionamentoRepository>();
        _loggerMock = new Mock<ILogger<ProvisionamentoUsuarioService>>();

        _service = new ProvisionamentoUsuarioService(
            _contextoMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Nao_Autenticado_Nao_Deve_Consultar_Repository()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(false);

        await _service.ProvisionarAsync(CancellationToken.None);

        _repositoryMock.Verify(r => r.ObterPorKeycloakSubParaAtualizacaoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Autenticado_Sem_Sub_Nao_Deve_Consultar_Repository()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns((string?)null);

        await _service.ProvisionarAsync(CancellationToken.None);

        _repositoryMock.Verify(r => r.ObterPorKeycloakSubParaAtualizacaoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Primeiro_Acesso_Deve_Criar_Usuario_Com_Ativo_True()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");
        _contextoMock.Setup(c => c.Username).Returns("testuser");
        _contextoMock.Setup(c => c.Nome).Returns("Test User");
        _contextoMock.Setup(c => c.Email).Returns("test@test.com");

        _repositoryMock.Setup(r => r.ObterPorKeycloakSubParaAtualizacaoAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        await _service.ProvisionarAsync(CancellationToken.None);

        _repositoryMock.Verify(r => r.AdicionarAsync(It.Is<Usuario>(u => 
            u.KeycloakSub == "123" && 
            u.Username == "testuser" && 
            u.Nome == "Test User" && 
            u.Email == "test@test.com" &&
            u.Ativo == true &&
            u.Perfis.Count == 0
        ), It.IsAny<CancellationToken>()), Times.Once);
        
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Usuario_Existente_Atualiza_Dados_Se_Diferente()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");
        _contextoMock.Setup(c => c.Username).Returns("newuser");
        _contextoMock.Setup(c => c.Nome).Returns("New Name");
        _contextoMock.Setup(c => c.Email).Returns("new@test.com");

        var usuarioExistente = Usuario.Criar("123", "olduser", "Old Name", "old@test.com");

        _repositoryMock.Setup(r => r.ObterPorKeycloakSubParaAtualizacaoAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        await _service.ProvisionarAsync(CancellationToken.None);

        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal("newuser", usuarioExistente.Username);
        Assert.Equal("New Name", usuarioExistente.Nome);
        Assert.Equal("new@test.com", usuarioExistente.Email);
    }

    [Fact]
    public async Task Usuario_Existente_Com_Valores_Iguais_Nao_Salva_Novamente()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");
        _contextoMock.Setup(c => c.Username).Returns("olduser");
        _contextoMock.Setup(c => c.Nome).Returns("Old Name");
        _contextoMock.Setup(c => c.Email).Returns("old@test.com");

        var usuarioExistente = Usuario.Criar("123", "olduser", "Old Name", "old@test.com");

        _repositoryMock.Setup(r => r.ObterPorKeycloakSubParaAtualizacaoAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        await _service.ProvisionarAsync(CancellationToken.None);

        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Claims_Ausentes_Nao_Apagam_Valores_Existentes()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");
        // JWT não retornou email ou name ou username
        _contextoMock.Setup(c => c.Username).Returns((string?)null);
        _contextoMock.Setup(c => c.Nome).Returns((string?)null);
        _contextoMock.Setup(c => c.Email).Returns((string?)null);

        var usuarioExistente = Usuario.Criar("123", "olduser", "Old Name", "old@test.com");

        _repositoryMock.Setup(r => r.ObterPorKeycloakSubParaAtualizacaoAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        await _service.ProvisionarAsync(CancellationToken.None);

        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);

        Assert.Equal("olduser", usuarioExistente.Username);
        Assert.Equal("Old Name", usuarioExistente.Nome);
        Assert.Equal("old@test.com", usuarioExistente.Email);
    }

    [Fact]
    public async Task Concorrencia_DbUpdateException_Violacao_KeycloakSub_Deve_Ser_Tratada()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");
        _contextoMock.Setup(c => c.Username).Returns("newuser");

        var usuarioRecemCriado = Usuario.Criar("123", "olduser", null, null);

        // Primeira vez não acha, segunda acha (foi inserido concorrentemente)
        _repositoryMock.SetupSequence(r => r.ObterPorKeycloakSubParaAtualizacaoAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null)
            .ReturnsAsync(usuarioRecemCriado);

        // Usar reflection para criar a PostgresException
        var pgExType = typeof(Npgsql.PostgresException);
        var pgEx = (Npgsql.PostgresException)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(pgExType);
        
        foreach (var field in pgExType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
        {
            if (field.Name.Contains("SqlState", System.StringComparison.OrdinalIgnoreCase))
                field.SetValue(pgEx, "23505");
            if (field.Name.Contains("ConstraintName", System.StringComparison.OrdinalIgnoreCase))
                field.SetValue(pgEx, "ix_usuario_keycloak_sub");
        }

        var dbEx = new Microsoft.EntityFrameworkCore.DbUpdateException("Erro", pgEx);

        _repositoryMock.SetupSequence(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Throws(dbEx) // Falha no primeiro SalvarAlteracoesAsync
            .Returns(Task.CompletedTask); // Passa no segundo (após atualização)

        await _service.ProvisionarAsync(CancellationToken.None);

        _repositoryMock.Verify(r => r.LimparRastreamento(), Times.Once);
        Assert.Equal("newuser", usuarioRecemCriado.Username); // Confirmamos que ele tentou atualizar o concorrente
    }

    [Fact]
    public async Task Outra_DbUpdateException_Deve_Ser_Relancada()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");

        _repositoryMock.Setup(r => r.ObterPorKeycloakSubParaAtualizacaoAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var dbEx = new Microsoft.EntityFrameworkCore.DbUpdateException("Outro Erro Genérico");

        _repositoryMock.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbEx);

        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(() => 
            _service.ProvisionarAsync(CancellationToken.None));

        _repositoryMock.Verify(r => r.LimparRastreamento(), Times.Never);
    }
}
