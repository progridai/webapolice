using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;

public class ApolicesQueries : IApolicesQueries
{
    private readonly SeguroDbContext _dbContext;

    public ApolicesQueries(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // A ser implementado na Fase 2 para consultas DTOs otimizadas (AsNoTracking)
}
