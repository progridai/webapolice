using WebApolice.Modulos.Seguranca.Api.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class CatalogoNegacaoPadraoTests
{
    public static IEnumerable<object[]> Matriz =>
        from flags in Enumerable.Range(0, 32)
        from total in new[] { false, true }
        select new object[] { flags, total };

    [Theory]
    [MemberData(nameof(Matriz))]
    public async Task EstadosDoCatalogoValemTambemParaAcessoTotal(int flags, bool acessoTotal)
    {
        await using var db = new CatalogoMemoriaDbContext(new DbContextOptionsBuilder<SegurancaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var modulo = Criar<Modulo>("APOLICES");
        var recurso = Criar<Recurso>("APOLICES");
        var permissao = Criar<Permissao>("apolices.visualizar");
        Set(modulo, "Ativo", (flags & 1) != 0);
        Set(modulo, "Habilitado", (flags & 2) != 0);
        Set(recurso, "Ativo", (flags & 4) != 0);
        Set(recurso, "Habilitado", (flags & 8) != 0);
        Set(permissao, "Ativo", (flags & 16) != 0);
        Set(recurso, "Modulo", modulo);
        Set(permissao, "Recurso", recurso);
        db.Add(permissao);
        await db.SaveChangesAsync();
        var handler = CriarHandler(db, acessoTotal, true);
        var requirement = new PermissaoRequirement("apolices.visualizar");
        var context = new AuthorizationHandlerContext([requirement], new ClaimsPrincipal(), null);
        await handler.HandleAsync(context);
        Assert.Equal(flags == 31, context.HasSucceeded);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PermissaoInexistenteNaoAutoriza(bool acessoTotal)
    {
        await using var db = new CatalogoMemoriaDbContext(new DbContextOptionsBuilder<SegurancaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var requirement = new PermissaoRequirement("inexistente.visualizar");
        var context = new AuthorizationHandlerContext([requirement], new ClaimsPrincipal(), null);
        await CriarHandler(db, acessoTotal, true).HandleAsync(context);
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    private static PermissaoAuthorizationHandler CriarHandler(SegurancaDbContext db, bool acessoTotal, bool ativo)
    {
        var usuario = new Mock<IContextoUsuarioAutenticado>();
        usuario.SetupGet(u => u.EstaAutenticado).Returns(true);
        usuario.SetupGet(u => u.KeycloakSub).Returns("usuario-teste");
        var permissoes = new Mock<IPermissoesEfetivasService>();
        permissoes.Setup(p => p.CalcularPermissoesAsync("usuario-teste", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissoesEfetivasUsuario(true, ativo, acessoTotal, false, [], [], ["apolices.visualizar"]));
        return new PermissaoAuthorizationHandler(usuario.Object, permissoes.Object, new HttpContextAccessor(), new DisponibilidadeCatalogo(db));
    }

    private static T Criar<T>(string codigo)
    {
        var value = (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
        Set(value!, "Codigo", codigo);
        Set(value!, "Nome", "Teste");
        Set(value!, "PublicId", Guid.NewGuid());
        return value;
    }

    private static void Set(object value, string property, object data) => value.GetType().GetProperty(property)!.SetValue(value, data);

    // Adaptação exclusiva ao provider em memória: mantém os dois campos JSON, serializados como texto.
    // Esta matriz testa a regra do catálogo; não comprova SQL, migrations ou atomicidade PostgreSQL.
    private sealed class CatalogoMemoriaDbContext(DbContextOptions<SegurancaDbContext> options) : SegurancaDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var auditoria = modelBuilder.Entity<WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao>();
            auditoria.Property(a => a.DadosAnteriores).HasConversion(
                value => value == null ? null : value.RootElement.GetRawText(),
                value => value == null ? null : System.Text.Json.JsonDocument.Parse(value, new System.Text.Json.JsonDocumentOptions()));
            auditoria.Property(a => a.DadosNovos).HasConversion(
                value => value == null ? null : value.RootElement.GetRawText(),
                value => value == null ? null : System.Text.Json.JsonDocument.Parse(value, new System.Text.Json.JsonDocumentOptions()));
        }
    }
}
