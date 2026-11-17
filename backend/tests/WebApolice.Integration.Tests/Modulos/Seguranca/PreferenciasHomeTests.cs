using Microsoft.EntityFrameworkCore;
using Moq;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.UseCases.Me;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Domain.Exceptions;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;
using WebApolice.SharedKernel.Text;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public sealed class PreferenciasHomeUseCaseTests
{
    [Theory]
    [InlineData(false, "sub")]
    [InlineData(true, null)]
    [InlineData(true, " ")]
    public async Task SemIdentidadeNaoConsultaNemGravaPreferencias(bool autenticado, string? sub)
    {
        var contexto = new Mock<IContextoUsuarioAutenticado>();
        contexto.SetupGet(c => c.EstaAutenticado).Returns(autenticado);
        contexto.SetupGet(c => c.KeycloakSub).Returns(sub);
        var repository = new Mock<IPreferenciasHomeRepository>(MockBehavior.Strict);
        var useCase = new PreferenciasHomeUseCase(contexto.Object, repository.Object);
        Assert.Null(await useCase.ObterAsync(default));
        Assert.Null(await useCase.SalvarAsync([], default));
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UsuarioAusenteOuInativoNaoGrava()
    {
        var contexto = new Mock<IContextoUsuarioAutenticado>();
        contexto.SetupGet(c => c.EstaAutenticado).Returns(true);
        contexto.SetupGet(c => c.KeycloakSub).Returns("sub-atual");
        var repository = new Mock<IPreferenciasHomeRepository>();
        repository.Setup(r => r.ObterUsuarioAtivoAsync("sub-atual", It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);
        var useCase = new PreferenciasHomeUseCase(contexto.Object, repository.Object);
        Assert.Null(await useCase.ObterAsync(default));
        Assert.Null(await useCase.SalvarAsync(["/clientes"], default));
        repository.Verify(r => r.SalvarAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void RotasSaoTecnicasENaoRecebemTitleCase()
    {
        Assert.False(PadronizacaoTexto.CampoHumano("AtalhosHome"));
        var usuario = Usuario.Criar("sub", "usuario", "Maria Silva", "maria@local.test");
        Assert.Null(usuario.AtalhosHome);
        string[] selecao = ["/seguranca/usuarios", "/convenios-cobranca"];
        usuario.DefinirAtalhosHome(selecao);
        selecao[0] = "/clientes";
        Assert.Equal(["/seguranca/usuarios", "/convenios-cobranca"], Assert.IsType<string[]>(usuario.AtalhosHome));
        usuario.DefinirAtalhosHome([]);
        Assert.Empty(usuario.AtalhosHome!);
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/clientes/novo")]
    [InlineData("https://exemplo.test")]
    [InlineData("/CLIENTES")]
    [InlineData("/clientes?usuario=outro")]
    [InlineData("")]
    public void RejeitaDestinoDesconhecidoSemAlterarSelecaoAnterior(string destino)
    {
        var usuario = Usuario.Criar("sub", "usuario", "Maria Silva", "maria@local.test");
        usuario.DefinirAtalhosHome(["/clientes"]);
        Assert.Throws<UsuarioInvalidoException>(() => usuario.DefinirAtalhosHome([destino]));
        Assert.Equal(["/clientes"], Assert.IsType<string[]>(usuario.AtalhosHome));
        Assert.Throws<UsuarioInvalidoException>(() => usuario.DefinirAtalhosHome(["/clientes", "/clientes"]));
        Assert.Throws<UsuarioInvalidoException>(() => usuario.DefinirAtalhosHome(null));
        Assert.Throws<UsuarioInvalidoException>(() => usuario.DefinirAtalhosHome(Enumerable.Repeat("/clientes", 19).ToArray()));
    }
}

public sealed class PreferenciasHomePersistenceTests(SegurancaIntegrationTestFixture fixture) : IClassFixture<SegurancaIntegrationTestFixture>
{
    [Fact]
    public async Task MigrationPersistePorContaPreservaIdentidadeEDistingueNullDeVazio()
    {
        var db = fixture.DbContext;
        db.ChangeTracker.Clear();
        await using var transaction = await db.Database.BeginTransactionAsync();
        Assert.Contains("20261117160039_AdicionarAtalhosHomeUsuario", await db.Database.GetAppliedMigrationsAsync());
        var subA = Guid.NewGuid().ToString();
        var subB = Guid.NewGuid().ToString();
        var subInativo = Guid.NewGuid().ToString();
        var usuarioA = Usuario.Criar(subA, "usuario.a", "Maria Silva", "maria@local.test");
        var usuarioB = Usuario.Criar(subB, "usuario.b", "João Silva", "joao@local.test");
        var inativo = new Usuario(subInativo, "inativo", "Inativo", "inativo@local.test", false);
        db.Usuarios.AddRange(usuarioA, usuarioB, inativo);
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE seguranca.usuario SET nome = 'EVIDÊNCIA ORIGINAL' WHERE keycloak_sub = {subA}");
        db.ChangeTracker.Clear();
        var repository = new PreferenciasHomeRepository(db);
        var contexto = new Mock<IContextoUsuarioAutenticado>();
        contexto.SetupGet(c => c.EstaAutenticado).Returns(true);
        contexto.SetupGet(c => c.KeycloakSub).Returns(subA);
        var useCase = new PreferenciasHomeUseCase(contexto.Object, repository);
        Assert.Null((await useCase.ObterAsync(default))!.AtalhosHome);
        await useCase.SalvarAsync(["/clientes", "/apolices"], default);
        db.ChangeTracker.Clear();
        Assert.Equal(["/clientes", "/apolices"], Assert.IsType<string[]>((await useCase.ObterAsync(default))!.AtalhosHome));
        Assert.Equal("EVIDÊNCIA ORIGINAL", (await repository.ObterUsuarioAtivoAsync(subA, default))!.Nome);
        Assert.Null((await repository.ObterUsuarioAtivoAsync(subB, default))!.AtalhosHome);
        contexto.SetupGet(c => c.KeycloakSub).Returns(subB);
        await useCase.SalvarAsync([], default);
        db.ChangeTracker.Clear();
        Assert.Empty((await useCase.ObterAsync(default))!.AtalhosHome!);
        Assert.Equal(["/clientes", "/apolices"], Assert.IsType<string[]>((await repository.ObterUsuarioAtivoAsync(subA, default))!.AtalhosHome));
        Assert.Null(await repository.ObterUsuarioAtivoAsync(subInativo, default));
        Assert.False(await db.UsuariosPerfis.AnyAsync(up => up.UsuarioId == usuarioA.Id || up.UsuarioId == usuarioB.Id));
        await transaction.RollbackAsync();
        db.ChangeTracker.Clear();
    }
}
