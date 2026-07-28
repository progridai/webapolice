using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;

public class ObterUsuarioUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ObterUsuarioUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UsuarioDetalheDto?> ExecuteAsync(Guid publicId, CancellationToken cancellationToken)
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
