using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Domain.Modelos;
namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IApoliceVinculosRepository
{
    Task<ApoliceModel?> ObterApoliceAsync(Guid publicId, bool incluirExcluidas, CancellationToken ct);
    Task<bool> ApoliceExisteAsync(long id, CancellationToken ct);
    Task<ApoliceVidaModel?> ObterVidaAsync(Guid publicId, long? apoliceId, bool incluirExcluidas, CancellationToken ct);
    Task<ApoliceSubgrupoModel?> ObterSubgrupoAsync(Guid publicId, CancellationToken ct);
    Task<ApoliceModuloModel?> ObterModuloAsync(Guid publicId, bool incluirExcluidos, CancellationToken ct);
    Task<ApoliceModuloModel?> ObterVinculoModuloAsync(long apoliceId, long moduloId, CancellationToken ct);
    Task<List<ApoliceModuloModel>> ListarModulosAsync(long apoliceId, CancellationToken ct);
    Task<RamoModel?> ObterRamoAsync(Guid publicId, CancellationToken ct);
    Task<ApoliceRamoModel?> ObterVinculoRamoAsync(long apoliceId, long ramoId, CancellationToken ct);
    Task<ApoliceSubestipulanteModel?> ObterVinculoSubestipulanteAsync(long apoliceId, long subestipulanteId, CancellationToken ct);
    void AdicionarVida(ApoliceVidaModel vida);
    void AdicionarModulo(ApoliceModuloModel modulo);
    void AdicionarRamo(ApoliceRamoModel ramo);
    void AdicionarSubestipulante(ApoliceSubestipulanteModel subestipulante);
    void AdicionarHistorico(ApoliceHistoricoModel historico);
    void AtualizarRamo(ApoliceRamoModel ramo);
    void AtualizarSubestipulante(ApoliceSubestipulanteModel subestipulante);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
