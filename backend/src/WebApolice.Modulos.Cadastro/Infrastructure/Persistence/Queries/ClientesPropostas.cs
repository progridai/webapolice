using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Contracts;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

public sealed class ClientesPropostas(CadastroDbContext db) : IClientesPropostas
{
    public Task<IdentidadeClienteProposta?> ResolverParaConsultaAsync(Guid clientePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        (from cliente in db.Clientes.AsNoTracking()
         join pessoa in db.Pessoas on cliente.PessoaId equals pessoa.Id
         join vinculo in db.Vinculos on cliente.Id equals vinculo.ClienteId
         where cliente.PublicId == clientePublicId && vinculo.PublicId == vinculoPublicId &&
               cliente.DeletedAt == null && pessoa.DeletedAt == null && vinculo.PessoaId == pessoa.Id
         select new IdentidadeClienteProposta(cliente.Id, pessoa.Id, vinculo.Id, vinculo.EstipulanteId)).SingleOrDefaultAsync(ct);

    public async Task<IdentidadeClienteProposta?> ResolverParaConfirmacaoAsync(long clienteId, long vinculoId, CancellationToken ct)
    {
        if (db.Database.IsRelational())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT v.id FROM cadastro.cliente_vinculo v JOIN cadastro.cliente c ON c.id=v.cliente_id JOIN core.pessoa p ON p.id=c.pessoa_id WHERE v.id={vinculoId} AND c.id={clienteId} FOR SHARE OF v,c,p", ct);
        return await ResolverAsync(clienteId, vinculoId, ct);
    }
    public async Task<ReferenciasPropostas> ConsultarReferenciasAsync(IReadOnlyCollection<long> clientes, IReadOnlyCollection<long> vinculos, CancellationToken cancellationToken)
    {
        var referencias = await (from cliente in db.Clientes.AsNoTracking()
            join pessoa in db.Pessoas on cliente.PessoaId equals pessoa.Id
            where clientes.Contains(cliente.Id) && cliente.DeletedAt == null && pessoa.DeletedAt == null
            select new ReferenciaClienteProposta(cliente.Id, cliente.PublicId, pessoa.Nome)).ToListAsync(cancellationToken);
        var ids = referencias.Select(c => c.ClienteId).ToArray();
        var matriculas = await db.Vinculos.AsNoTracking()
            .Where(v => vinculos.Contains(v.Id) && ids.Contains(v.ClienteId))
            .Select(v => new ReferenciaVinculoProposta(v.Id, v.ClienteId, v.PublicId, v.Matricula)).ToListAsync(cancellationToken);
        return new(referencias, matriculas);
    }

    public Task<IdentidadeClienteProposta?> ResolverAsync(long clienteId, long vinculoId, CancellationToken cancellationToken) =>
        (from cliente in db.Clientes
         join pessoa in db.Pessoas on cliente.PessoaId equals pessoa.Id
         join vinculo in db.Vinculos on cliente.Id equals vinculo.ClienteId
         where cliente.Id == clienteId && cliente.DeletedAt == null && pessoa.DeletedAt == null &&
               vinculo.Id == vinculoId && vinculo.PessoaId == pessoa.Id && vinculo.Ativo
         select new IdentidadeClienteProposta(cliente.Id, pessoa.Id, vinculo.Id, vinculo.EstipulanteId)).SingleOrDefaultAsync(cancellationToken);
}
