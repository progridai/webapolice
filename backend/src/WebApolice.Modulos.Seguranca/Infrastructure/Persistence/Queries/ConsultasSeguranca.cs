using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Queries;

public sealed class ConsultasSeguranca(SegurancaDbContext _dbContext) : IConsultasSeguranca
{
    public async Task<ListagemPaginadaDto<AuditoriaDto>> ListarAuditoriaAsync(
        int pagina,
        int tamanhoPagina,
        string? acao,
        string? entidade,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditoriaPermissoes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(acao))
        {
            query = query.Where(a => a.Acao.Contains(acao));
        }

        if (!string.IsNullOrWhiteSpace(entidade))
        {
            query = query.Where(a => a.EntidadeTipo.Contains(entidade));
        }

        if (dataInicial.HasValue)
        {
            var start = dataInicial.Value.Date.ToUniversalTime();
            query = query.Where(a => a.CreatedAt >= start);
        }

        if (dataFinal.HasValue)
        {
            var end = dataFinal.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(a => a.CreatedAt <= end);
        }

        var totalItens = await query.CountAsync(cancellationToken);

        pagina = pagina > 0 ? pagina : 1;
        tamanhoPagina = tamanhoPagina > 0 ? tamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var auditorias = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        var itens = auditorias.Select(a => new AuditoriaDto(
            a.PublicId,
            a.Acao,
            a.EntidadeTipo,
            a.EntidadeId.ToString(),
            a.CreatedAt,
            a.DadosAnteriores,
            a.DadosNovos
        )).ToList();

        return new ListagemPaginadaDto<AuditoriaDto>(itens, pagina, tamanhoPagina, totalItens, totalPaginas);
    }
    public async Task<AuditoriaDto?> ObterAuditoriaAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var auditoria = await _dbContext.AuditoriaPermissoes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PublicId == publicId, cancellationToken);

        if (auditoria == null) return null;

        return new AuditoriaDto(
            auditoria.PublicId,
            auditoria.Acao,
            auditoria.EntidadeTipo,
            auditoria.EntidadeId.ToString(),
            auditoria.CreatedAt,
            auditoria.DadosAnteriores,
            auditoria.DadosNovos
        );
    }
    public async Task<IReadOnlyList<CatalogoModuloDto>> ListarCatalogoAsync(CancellationToken cancellationToken)
    {
        var modulos = await _dbContext.Modulos
            .Include(m => m.Recursos)
            .ThenInclude(r => r.Permissoes)
            .AsNoTracking()
            .OrderBy(m => m.Ordem)
            .ToListAsync(cancellationToken);

        return modulos.Select(m => new CatalogoModuloDto(
            m.PublicId,
            m.Codigo,
            m.Nome,
            m.Descricao,
            m.Icone,
            m.Recursos.OrderBy(r => r.Ordem).Select(r => new CatalogoRecursoDto(
                r.PublicId,
                r.Codigo,
                r.Nome,
                r.Descricao,
                r.RotaFrontend,
                r.Permissoes.OrderBy(p => p.Nome).Select(p => new CatalogoPermissaoDto(
                    p.PublicId,
                    p.Codigo,
                    p.Nome,
                    p.Descricao
                )).ToList()
            )).ToList()
        )).ToList();
    }
    public async Task<IReadOnlyList<CatalogoPermissaoDto>> ListarPermissoesAsync(CancellationToken cancellationToken)
    {
        var permissoes = await _dbContext.Permissoes
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        return permissoes.Select(p => new CatalogoPermissaoDto(
            p.PublicId,
            p.Codigo,
            p.Nome,
            p.Descricao
        )).ToList();
    }
    public async Task<List<ModuloDto>> ListarModulosAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Modulos
            .Include(m => m.Recursos)
            .OrderBy(m => m.Ordem)
            .Select(m => new ModuloDto(
                m.PublicId, m.Codigo, m.Nome, m.Descricao, m.Icone, m.Ordem, m.Ativo, m.Habilitado,
                m.Recursos.Select(r => new RecursoDto(r.PublicId, r.Codigo, r.Nome, r.Ativo, r.Habilitado)).ToList()
            ))
            .ToListAsync(cancellationToken);
    }
    public async Task<ListagemPaginadaDto<PerfilDto>> ListarPerfisAsync(
        string? busca,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Perfis.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            busca = busca.ToLower();
            query = query.Where(p => p.Nome.ToLower().Contains(busca) || p.Codigo.ToLower().Contains(busca));
        }

        if (ativo.HasValue)
        {
            query = query.Where(p => p.Ativo == ativo.Value);
        }

        var totalItens = await query.CountAsync(cancellationToken);

        pagina = pagina > 0 ? pagina : 1;
        tamanhoPagina = tamanhoPagina > 0 ? tamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var perfis = await query
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        var itens = perfis.Select(p => new PerfilDto(
            p.PublicId,
            p.Codigo,
            p.Nome,
            p.Descricao,
            p.Ativo,
            p.PerfilSistema,
            p.AcessoTotal
        )).ToList();

        return new ListagemPaginadaDto<PerfilDto>(itens, pagina, tamanhoPagina, totalItens, totalPaginas);
    }
    public async Task<PerfilDetalheDto?> ObterPerfilAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var perfil = await _dbContext.Perfis
            .Include(p => p.Permissoes)
            .ThenInclude(pp => pp.Permissao)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);

        if (perfil == null) return null;

        var permissoesIds = perfil.Permissoes.Select(pp => pp.Permissao.PublicId).ToList();

        return new PerfilDetalheDto(
            perfil.PublicId,
            perfil.Codigo,
            perfil.Nome,
            perfil.Descricao,
            perfil.Ativo,
            perfil.PerfilSistema,
            perfil.AcessoTotal,
            permissoesIds
        );
    }
    public async Task<ListagemPaginadaDto<UsuarioListDto>> ListarUsuariosAsync(
        string? busca,
        bool? ativo,
        Guid? perfilPublicId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Usuarios
            .Include(u => u.Perfis)
            .ThenInclude(up => up.Perfil)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            busca = busca.ToLower();
            query = query.Where(u => (u.Username != null && u.Username.ToLower().Contains(busca)) ||
                                     (u.Nome != null && u.Nome.ToLower().Contains(busca)) ||
                                     (u.Email != null && u.Email.ToLower().Contains(busca)));
        }

        if (ativo.HasValue)
        {
            query = query.Where(u => u.Ativo == ativo.Value);
        }

        if (perfilPublicId.HasValue)
        {
            query = query.Where(u => u.Perfis.Any(p => p.Perfil.PublicId == perfilPublicId.Value));
        }

        var totalItens = await query.CountAsync(cancellationToken);

        pagina = pagina > 0 ? pagina : 1;
        tamanhoPagina = tamanhoPagina > 0 ? tamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var usuarios = await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        var itens = usuarios.Select(u => new UsuarioListDto(
            u.PublicId,
            u.Username,
            u.Nome,
            u.Email,
            u.Ativo,
            u.UltimoLoginEm,
            u.Perfis.Select(p => p.Perfil.Nome).ToList()
        )).ToList();

        return new ListagemPaginadaDto<UsuarioListDto>(itens, pagina, tamanhoPagina, totalItens, totalPaginas);
    }
    public async Task<UsuarioDetalheDto?> ObterUsuarioAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Perfis)
            .ThenInclude(up => up.Perfil)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PublicId == publicId, cancellationToken);

        if (usuario == null) return null;

        var perfisAtribuidos = usuario.Perfis.Select(p => new PerfilDto(
            p.Perfil.PublicId,
            p.Perfil.Codigo,
            p.Perfil.Nome,
            p.Perfil.Descricao,
            p.Perfil.Ativo,
            p.Perfil.PerfilSistema,
            p.Perfil.AcessoTotal
        )).ToList();

        var perfisAtribuidosIds = usuario.Perfis.Select(p => p.PerfilId).ToList();

        var perfisDisponiveis = await _dbContext.Perfis
            .Where(p => !perfisAtribuidosIds.Contains(p.Id) && p.Ativo && p.Codigo != "ADMINISTRADOR")
            .AsNoTracking()
            .Select(p => new PerfilDto(
                p.PublicId,
                p.Codigo,
                p.Nome,
                p.Descricao,
                p.Ativo,
                p.PerfilSistema,
                p.AcessoTotal
            ))
            .ToListAsync(cancellationToken);

        return new UsuarioDetalheDto(
            usuario.PublicId,
            usuario.KeycloakSub,
            usuario.Username,
            usuario.Nome,
            usuario.Email,
            usuario.Ativo,
            usuario.UltimoLoginEm,
            usuario.CreatedAt,
            usuario.UpdatedAt,
            perfisAtribuidos,
            perfisDisponiveis
        );
    }
}
