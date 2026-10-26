using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApolice.Modulos.Seguranca.Domain.Exceptions;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class UsuarioProvisionamentoRepositoryTests
{
    [Theory]
    [InlineData("23505", "ix_usuario_keycloak_sub", true)]
    [InlineData("23505", "outra_constraint", false)]
    [InlineData("23503", "ix_usuario_keycloak_sub", false)]
    public async Task TraduzSomenteConcorrenciaDoSub(string sqlState, string constraint, bool traduzir)
    {
        var original = new DbUpdateException("Falha artificial", new PostgresException("fixture", "ERROR", "ERROR", sqlState, constraintName: constraint));
        await using var db = new ContextoComFalha(original);
        var repository = new UsuarioProvisionamentoRepository(db);
        if (traduzir)
        {
            var erro = await Assert.ThrowsAsync<ConcorrenciaProvisionamentoException>(() => repository.SalvarAlteracoesAsync(default));
            Assert.Same(original, erro.InnerException);
        }
        else
        {
            var erro = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SalvarAlteracoesAsync(default));
            Assert.Same(original, erro);
        }
    }

    private sealed class ContextoComFalha(Exception erro) : SegurancaDbContext(new DbContextOptionsBuilder<SegurancaDbContext>().Options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromException<int>(erro);
    }
}
