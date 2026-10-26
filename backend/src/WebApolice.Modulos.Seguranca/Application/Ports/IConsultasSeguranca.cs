using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IConsultasSeguranca
{
    Task<ListagemPaginadaDto<AuditoriaDto>> ListarAuditoriaAsync(
        int pagina,
        int tamanhoPagina,
        string? acao,
        string? entidade,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken);
    Task<AuditoriaDto?> ObterAuditoriaAsync(Guid publicId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CatalogoModuloDto>> ListarCatalogoAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CatalogoPermissaoDto>> ListarPermissoesAsync(CancellationToken cancellationToken);
    Task<List<ModuloDto>> ListarModulosAsync(CancellationToken cancellationToken);
    Task<ListagemPaginadaDto<PerfilDto>> ListarPerfisAsync(
        string? busca,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken);
    Task<PerfilDetalheDto?> ObterPerfilAsync(Guid publicId, CancellationToken cancellationToken);
    Task<ListagemPaginadaDto<UsuarioListDto>> ListarUsuariosAsync(
        string? busca,
        bool? ativo,
        Guid? perfilPublicId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken);
    Task<UsuarioDetalheDto?> ObterUsuarioAsync(Guid publicId, CancellationToken cancellationToken);
}
