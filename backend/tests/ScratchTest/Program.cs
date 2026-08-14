using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;

namespace ScratchTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ClientesDbContext>();
            optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!", x => x.MigrationsHistoryTable("__EFMigrationsHistory", "cadastro"))
                          .UseSnakeCaseNamingConvention();

            using var _dbContext = new ClientesDbContext(optionsBuilder.Options);

            try
            {
                var client = await _dbContext.Clientes.FirstOrDefaultAsync();
                if (client != null)
                {
                    Console.WriteLine($"Found client: {client.PublicId}");
                    
                    var baseInfo = await (
                        from c in _dbContext.Clientes.AsNoTracking()
                        join p in _dbContext.Pessoas.AsNoTracking() on c.PessoaId equals p.Id
                        join s in _dbContext.Status.AsNoTracking() on c.StatusId equals s.Id
                        where c.PublicId == client.PublicId && c.DeletedAt == null && p.DeletedAt == null
                        select new { c, p, s }
                    ).FirstOrDefaultAsync();

                    Console.WriteLine($"Detail obtained successfully for: {baseInfo?.p?.Nome}");
                }
                else
                {
                    Console.WriteLine("No clients found in the database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION CAUGHT:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
