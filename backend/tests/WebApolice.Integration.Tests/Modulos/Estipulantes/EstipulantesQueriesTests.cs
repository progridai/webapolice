using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Estipulantes;

public class EstipulantesQueriesTests
{
    [Fact]
    public async Task DeveConsultarSemErro()
    {
        var options = new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql("Host=localhost;Database=WebApolice;Username=postgres;Password=postgres")
            .Options;
            
        var dbContext = new CadastroDbContext(options);
        var queries = new EstipulantesQueries(dbContext);
        
        var estipulante = await dbContext.Estipulantes.FirstOrDefaultAsync();
        
        if (estipulante != null)
        {
            var result = await queries.ObterPorPublicIdAsync(estipulante.PublicId, CancellationToken.None);
            Assert.NotNull(result);
            
            var config = await queries.ObterConfiguracaoPorPublicIdAsync(estipulante.PublicId, CancellationToken.None);
            // Just verifying it doesn't throw
        }
    }
}
