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

public class AtualizarUsuarioUseCaseTests : IClassFixture<SegurancaIntegrationTestFixture>, IAsyncLifetime
{
    private readonly SegurancaIntegrationTestFixture _fixture;
    private readonly SegurancaDbContext _dbContext;
    private readonly Mock<IKeycloakUsuariosAdminClient> _keycloakClientMock;
    private readonly Mock<IContextoUsuarioAutenticado> _contextoMock;
    private readonly Mock<ILogger<AtualizarUsuarioUseCase>> _loggerMock;
    private readonly AtualizarUsuarioUseCase _sut;

    private readonly List<Guid> _usuariosCriados = new();

    public AtualizarUsuarioUseCaseTests(SegurancaIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        _keycloakClientMock = new Mock<IKeycloakUsuariosAdminClient>();
        _contextoMock = new Mock<IContextoUsuarioAutenticado>();
        _loggerMock = new Mock<ILogger<AtualizarUsuarioUseCase>>();

        _sut = new AtualizarUsuarioUseCase(_dbContext, _keycloakClientMock.Object, _contextoMock.Object, _loggerMock.Object);
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
    }

    [Fact]
    public async Task DeveAtualizarUsuarioComSucesso()
    {
        var runId = Guid.NewGuid();
        var keycloakSub = Guid.NewGuid().ToString();
        var usuario = new Usuario(keycloakSub, $"user_{runId}", "Nome Antigo", $"user_{runId}@exemplo.local", true);
        
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Entry(usuario).ReloadAsync(); // Fetch DB generated PublicId
        _usuariosCriados.Add(usuario.PublicId);

        _keycloakClientMock.Setup(x => x.ObterUsuarioPorSubAsync(keycloakSub, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeycloakUsuarioRecord(keycloakSub, usuario.Username, usuario.Email, "Nome", "Antigo", true));
            
        await _sut.ExecuteAsync(usuario.PublicId, "Nome Novo", $"novo_{runId}@exemplo.local", false, new List<Guid>(), CancellationToken.None);

        var atualizado = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.PublicId == usuario.PublicId);
        Assert.Equal("Nome Novo", atualizado!.Nome);
        Assert.Equal($"novo_{runId}@exemplo.local", atualizado.Email);
        Assert.False(atualizado.Ativo);

        _keycloakClientMock.Verify(x => x.AtualizarUsuarioAsync(keycloakSub, $"novo_{runId}@exemplo.local", "Nome Novo", false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveRestaurarKeycloakSeFalharBanco()
    {
        var runId = Guid.NewGuid();
        var keycloakSub1 = Guid.NewGuid().ToString();
        var keycloakSub2 = Guid.NewGuid().ToString();
        
        var usuario1 = new Usuario(keycloakSub1, $"user1_{runId}", "Nome Um", $"user1_{runId}@exemplo.local", true);
        var usuario2 = new Usuario(keycloakSub2, $"user2_{runId}", "Nome Dois", $"user2_{runId}@exemplo.local", true);
        
        _dbContext.Usuarios.AddRange(usuario1, usuario2);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Entry(usuario1).ReloadAsync();
        await _dbContext.Entry(usuario2).ReloadAsync();
        _usuariosCriados.Add(usuario1.PublicId);
        _usuariosCriados.Add(usuario2.PublicId);

        _keycloakClientMock.Setup(x => x.ObterUsuarioPorSubAsync(keycloakSub1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeycloakUsuarioRecord(keycloakSub1, usuario1.Username, usuario1.Email, "Nome", "Um", true));
            
        // Força constraint ao tentar atualizar com nome maior que 150 caracteres
        var nomeInvalido = new string('A', 200);
        await Assert.ThrowsAsync<DbUpdateException>(() => 
            _sut.ExecuteAsync(usuario1.PublicId, nomeInvalido, usuario1.Email, true, new List<Guid>(), CancellationToken.None));

        // Deve ter chamado a atualização inicialmente com o nome longo
        _keycloakClientMock.Verify(x => x.AtualizarUsuarioAsync(keycloakSub1, usuario1.Email, nomeInvalido, true, It.IsAny<CancellationToken>()), Times.Once);

        // Deve ter chamado a compensação restaurando os dados originais (no Keycloak)
        _keycloakClientMock.Verify(x => x.AtualizarUsuarioAsync(keycloakSub1, usuario1.Email, "Nome Um", true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
