using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class PerfisUseCaseTests : IClassFixture<SegurancaIntegrationTestFixture>, IAsyncLifetime
{
    private readonly SegurancaIntegrationTestFixture _fixture;
    private readonly SegurancaDbContext _dbContext;
    private readonly Mock<IContextoUsuarioAutenticado> _contextoMock;
    private readonly AtualizarPerfilUseCase _sutAtualizar;

    private readonly List<Guid> _perfisCriados = new();

    public PerfisUseCaseTests(SegurancaIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        _contextoMock = new Mock<IContextoUsuarioAutenticado>();
        
        _sutAtualizar = new AtualizarPerfilUseCase(_dbContext, _contextoMock.Object);
    }

    public Task InitializeAsync() 
    {
        _dbContext.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_perfisCriados.Any())
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.perfil_permissao WHERE perfil_id IN (SELECT id FROM seguranca.perfil WHERE public_id = ANY(ARRAY['{string.Join("','", _perfisCriados)}']::uuid[]))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.perfil WHERE public_id = ANY(ARRAY['{string.Join("','", _perfisCriados)}']::uuid[])");
        }
    }

    [Fact]
    public async Task NaoDevePermitirAlterarPerfilAdministrador()
    {
        var runId = Guid.NewGuid();
        
        // Simula o perfil ADMINISTRADOR que seria carregado da base (na verdade ele já existe da carga inicial)
        var perfilAdmin = await _dbContext.Perfis.FirstOrDefaultAsync(p => p.Codigo == "ADMINISTRADOR");
        
        bool wasCreated = false;
        if (perfilAdmin == null)
        {
            perfilAdmin = new Perfil("ADMINISTRADOR", "Admin", "Admin", true, true, true);
            _dbContext.Perfis.Add(perfilAdmin);
            await _dbContext.SaveChangesAsync();
            wasCreated = true;
        }

        await _dbContext.Entry(perfilAdmin).ReloadAsync();
        
        if (wasCreated)
        {
            _perfisCriados.Add(perfilAdmin.PublicId);
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _sutAtualizar.ExecuteAsync(perfilAdmin.PublicId, "Novo Nome", "Nova desc", true, new List<Guid>(), CancellationToken.None));
            
        Assert.Equal("Não é permitido alterar o perfil ADMINISTRADOR. Ele é um perfil de sistema com acesso total.", ex.Message);
    }
}
