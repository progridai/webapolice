using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ListarRamos;
using WebApolice.SharedKernel.Application.Models;
namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IRamosRepository
{
    Task<RamoModel?> ObterAsync(Guid publicId, CancellationToken ct);
    Task<RamoModel?> ObterParaLeituraAsync(Guid publicId, CancellationToken ct);
    Task<PagedResult<RamoDto>> ListarAsync(ListarRamosQuery query, CancellationToken cancellationToken);
    void Adicionar(RamoModel ramo);
    void Atualizar(RamoModel ramo);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
