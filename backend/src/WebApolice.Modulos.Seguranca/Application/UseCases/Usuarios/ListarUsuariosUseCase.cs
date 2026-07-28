using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;

public class ListarUsuariosUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ListarUsuariosUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListagemPaginadaDto<UsuarioListDto>> ExecuteAsync(
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
            query = query.Where(u => u.Username.ToLower().Contains(busca) || 
                                     u.Nome.ToLower().Contains(busca) || 
                                     u.Email.ToLower().Contains(busca));
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
}
