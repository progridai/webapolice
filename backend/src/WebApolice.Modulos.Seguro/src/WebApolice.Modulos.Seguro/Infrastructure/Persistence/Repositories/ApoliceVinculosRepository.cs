using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class ApoliceVinculosRepository(SeguroDbContext db) : IApoliceVinculosRepository
{
    public Task<ApoliceModel?> ObterApoliceAsync(Guid publicId, bool incluirExcluidas, CancellationToken ct) => db.Apolices.FirstOrDefaultAsync(a => a.PublicId == publicId && (incluirExcluidas || a.DeletedAt == null), ct);
    public Task<bool> ApoliceExisteAsync(long id, CancellationToken ct) => db.Apolices.AnyAsync(a => a.Id == id, ct);
    public Task<ApoliceVidaModel?> ObterVidaAsync(Guid publicId, long? apoliceId, bool incluirExcluidas, CancellationToken ct) => db.ApoliceVidas.FirstOrDefaultAsync(v => v.PublicId == publicId && (!apoliceId.HasValue || v.ApoliceId == apoliceId) && (incluirExcluidas || v.DeletedAt == null), ct);
    public Task<ApoliceSubgrupoModel?> ObterSubgrupoAsync(Guid publicId, CancellationToken ct) => db.ApoliceSubgrupos.FirstOrDefaultAsync(s => s.PublicId == publicId, ct);
    public Task<ApoliceModuloModel?> ObterModuloAsync(Guid publicId, bool incluirExcluidos, CancellationToken ct) => db.ApoliceModulos.FirstOrDefaultAsync(m => m.PublicId == publicId && (incluirExcluidos || m.DeletedAt == null), ct);
    public Task<ApoliceModuloModel?> ObterVinculoModuloAsync(long apoliceId, long moduloId, CancellationToken ct) => db.ApoliceModulos.FirstOrDefaultAsync(m => m.ApoliceId == apoliceId && m.ModuloId == moduloId && m.DeletedAt == null, ct);
    public Task<List<ApoliceModuloModel>> ListarModulosAsync(long apoliceId, CancellationToken ct) => db.ApoliceModulos.AsNoTracking().Where(m => m.ApoliceId == apoliceId && m.DeletedAt == null).ToListAsync(ct);
    public Task<RamoModel?> ObterRamoAsync(Guid publicId, CancellationToken ct) => db.Ramos.FirstOrDefaultAsync(r => r.PublicId == publicId, ct);
    public Task<ApoliceRamoModel?> ObterVinculoRamoAsync(long apoliceId, long ramoId, CancellationToken ct) => db.ApoliceRamos.FirstOrDefaultAsync(r => r.ApoliceId == apoliceId && r.RamoId == ramoId, ct);
    public Task<ApoliceSubestipulanteModel?> ObterVinculoSubestipulanteAsync(long apoliceId, long subestipulanteId, CancellationToken ct) => db.ApoliceSubestipulantes.FirstOrDefaultAsync(s => s.ApoliceId == apoliceId && s.SubestipulanteId == subestipulanteId, ct);
    public void AdicionarVida(ApoliceVidaModel vida) => db.ApoliceVidas.Add(vida);
    public void AdicionarModulo(ApoliceModuloModel modulo) => db.ApoliceModulos.Add(modulo);
    public void AdicionarRamo(ApoliceRamoModel ramo) => db.ApoliceRamos.Add(ramo);
    public void AdicionarSubestipulante(ApoliceSubestipulanteModel subestipulante) => db.ApoliceSubestipulantes.Add(subestipulante);
    public void AdicionarHistorico(ApoliceHistoricoModel historico) => db.ApoliceHistoricos.Add(historico);
    public void AtualizarRamo(ApoliceRamoModel ramo) => db.ApoliceRamos.Update(ramo);
    public void AtualizarSubestipulante(ApoliceSubestipulanteModel subestipulante) => db.ApoliceSubestipulantes.Update(subestipulante);
    public async Task SalvarAlteracoesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
