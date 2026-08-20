using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public class SubestipulanteRepository : ISubestipulanteRepository
{
    private readonly CadastroDbContext _dbContext;

    public SubestipulanteRepository(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubestipulanteModel?> LocalizarPorIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<SubestipulanteModel>()
            .Where(s => s.Id == id && s.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SubestipulanteModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<SubestipulanteModel>()
            .Where(s => s.PublicId == publicId && s.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CnpjJaExisteAsync(string cnpjLimpo, long? desconsiderarSubestipulanteId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cnpjLimpo)) return false;

        return await (from s in _dbContext.Set<SubestipulanteModel>()
                      join p in _dbContext.Pessoas on s.PessoaId equals p.Id
                      where p.DocumentoPrincipalLimpo == cnpjLimpo && s.DeletedAt == null && (!desconsiderarSubestipulanteId.HasValue || s.Id != desconsiderarSubestipulanteId.Value)
                      select s.Id).AnyAsync(cancellationToken);
    }

    public void Adicionar(SubestipulanteModel model)
    {
        _dbContext.Set<SubestipulanteModel>().Add(model);
    }

    public void Atualizar(SubestipulanteModel model)
    {
        _dbContext.Set<SubestipulanteModel>().Update(model);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
