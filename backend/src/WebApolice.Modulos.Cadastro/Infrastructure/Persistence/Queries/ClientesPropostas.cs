using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Contracts;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

public sealed class ClientesPropostas(CadastroDbContext db) : IClientesPropostas
{
    public Task<IdentidadeClienteProposta?> ResolverAsync(long clienteId, long vinculoId, CancellationToken cancellationToken) =>
        (from cliente in db.Clientes
         join pessoa in db.Pessoas on cliente.PessoaId equals pessoa.Id
         join vinculo in db.Vinculos on cliente.Id equals vinculo.ClienteId
         where cliente.Id == clienteId && cliente.DeletedAt == null && pessoa.DeletedAt == null &&
               vinculo.Id == vinculoId && vinculo.PessoaId == pessoa.Id && vinculo.Ativo
         select new IdentidadeClienteProposta(cliente.Id, pessoa.Id, vinculo.Id)).SingleOrDefaultAsync(cancellationToken);
}
