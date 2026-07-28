using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class CriarUsuarioUseCaseTests : IClassFixture<SegurancaIntegrationTestFixture>, IAsyncLifetime
{
    private readonly SegurancaIntegrationTestFixture _fixture;
    private readonly SegurancaDbContext _dbContext;
    private readonly Mock<IKeycloakUsuariosAdminClient> _keycloakClientMock;
    private readonly Mock<IContextoUsuarioAutenticado> _contextoMock;
    private readonly Mock<ILogger<CriarUsuarioUseCase>> _loggerMock;
    private readonly CriarUsuarioUseCase _sut;

    private readonly List<Guid> _usuariosCriados = new();
    private readonly List<Guid> _perfisCriados = new();

    public CriarUsuarioUseCaseTests(SegurancaIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        _keycloakClientMock = new Mock<IKeycloakUsuariosAdminClient>();
        _contextoMock = new Mock<IContextoUsuarioAutenticado>();
        _loggerMock = new Mock<ILogger<CriarUsuarioUseCase>>();

        _sut = new CriarUsuarioUseCase(_dbContext, _keycloakClientMock.Object, _contextoMock.Object, _loggerMock.Object);
    }

    public Task InitializeAsync() 
    {
        _dbContext.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_usuariosCriados.Any())
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.auditoria_permissao WHERE usuario_afetado_id IN (SELECT id FROM seguranca.usuario WHERE public_id = ANY(ARRAY['{string.Join("','", _usuariosCriados)}']::uuid[]))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.usuario_perfil WHERE usuario_id IN (SELECT id FROM seguranca.usuario WHERE public_id = ANY(ARRAY['{string.Join("','", _usuariosCriados)}']::uuid[]))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.usuario WHERE public_id = ANY(ARRAY['{string.Join("','", _usuariosCriados)}']::uuid[])");
        }
        if (_perfisCriados.Any())
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.perfil_permissao WHERE perfil_id IN (SELECT id FROM seguranca.perfil WHERE public_id = ANY(ARRAY['{string.Join("','", _perfisCriados)}']::uuid[]))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.perfil WHERE public_id = ANY(ARRAY['{string.Join("','", _perfisCriados)}']::uuid[])");
        }
    }

    [Fact]
    public async Task DeveCriarUsuarioComSucesso()
    {
        var runId = Guid.NewGuid();
        var username = $"teste_{runId}";
        var email = $"teste_{runId}@exemplo.local";
        var sub = Guid.NewGuid().ToString();

        _keycloakClientMock.Setup(x => x.ExisteUsernameAsync(username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _keycloakClientMock.Setup(x => x.ExisteEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _keycloakClientMock.Setup(x => x.CriarUsuarioAsync(username, email, "Nome Teste", true, It.IsAny<CancellationToken>())).ReturnsAsync(sub);
        
        var publicId = await _sut.ExecuteAsync(username, "Nome Teste", email, "senha", true, new List<Guid>(), CancellationToken.None);
        
        _usuariosCriados.Add(publicId);

        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.PublicId == publicId);
        Assert.NotNull(usuario);
        Assert.Equal(username, usuario.Username);
        
        _keycloakClientMock.Verify(x => x.DefinirSenhaTemporariaAsync(sub, "senha", It.IsAny<CancellationToken>()), Times.Once);
        _keycloakClientMock.Verify(x => x.RemoverUsuarioAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeveCompensarSeFalharSenhaTemporaria()
    {
        var runId = Guid.NewGuid();
        var username = $"teste_senha_{runId}";
        var email = $"teste_{runId}@exemplo.local";
        var sub = Guid.NewGuid().ToString();

        _keycloakClientMock.Setup(x => x.ExisteUsernameAsync(username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _keycloakClientMock.Setup(x => x.ExisteEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _keycloakClientMock.Setup(x => x.CriarUsuarioAsync(username, email, "Nome Teste", true, It.IsAny<CancellationToken>())).ReturnsAsync(sub);
        
        _keycloakClientMock.Setup(x => x.DefinirSenhaTemporariaAsync(sub, "senha", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Falha senha"));

        await Assert.ThrowsAsync<Exception>(() => _sut.ExecuteAsync(username, "Nome Teste", email, "senha", true, new List<Guid>(), CancellationToken.None));
        
        _keycloakClientMock.Verify(x => x.RemoverUsuarioAsync(sub, It.IsAny<CancellationToken>()), Times.Once);
        var existe = await _dbContext.Usuarios.AnyAsync(u => u.Username == username);
        Assert.False(existe);
    }

    [Fact]
    public async Task DeveCompensarEGerarLogCriticalSeCompensacaoSenhaFalhar()
    {
        var runId = Guid.NewGuid();
        var username = $"teste_senha2_{runId}";
        var email = $"teste_{runId}@exemplo.local";
        var sub = Guid.NewGuid().ToString();

        _keycloakClientMock.Setup(x => x.ExisteUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _keycloakClientMock.Setup(x => x.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _keycloakClientMock.Setup(x => x.CriarUsuarioAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(sub);
        
        _keycloakClientMock.Setup(x => x.DefinirSenhaTemporariaAsync(sub, "senha", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Falha senha original"));
            
        _keycloakClientMock.Setup(x => x.RemoverUsuarioAsync(sub, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Falha compensacao"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.ExecuteAsync(username, "Nome Teste", email, "senha", true, new List<Guid>(), CancellationToken.None));
        Assert.Equal("Falha senha original", ex.Message);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
