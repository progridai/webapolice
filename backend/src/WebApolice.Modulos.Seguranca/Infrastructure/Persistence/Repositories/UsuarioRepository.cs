using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly SegurancaDbContext _context;

    public UsuarioRepository(SegurancaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DadosUsuarioPermissoes?> ObterDadosPermissoesPorKeycloakSubAsync(string keycloakSub, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakSub))
        {
            throw new ArgumentException("O keycloak_sub não pode ser vazio.", nameof(keycloakSub));
        }

        var projection = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.KeycloakSub == keycloakSub)
            .Select(u => new 
            {
                u.Ativo,
                AcessoTotal = u.Perfis.Where(up => up.Perfil.Ativo).Any(up => up.Perfil.AcessoTotal),
                OperadorSistema = u.Perfis.Where(up => up.Perfil.Ativo).Any(up => up.Perfil.Codigo == "ADMINISTRADOR" && up.Perfil.PerfilSistema),
                ModulosHabilitados = _context.Set<Domain.Modulo>()
                    .Where(m => m.Ativo && m.Habilitado)
                    .Select(m => m.Codigo)
                    .ToList(),
                RecursosHabilitados = _context.Set<Domain.Recurso>()
                    .Where(r => r.Ativo && r.Habilitado && r.Modulo.Ativo && r.Modulo.Habilitado)
                    .Select(r => r.Codigo)
                    .ToList(),
                Permissoes = u.Perfis
                    .Where(up => up.Perfil.Ativo)
                    .SelectMany(up => up.Perfil.Permissoes)
                    .Where(pp => pp.Permissao.Ativo && pp.Permissao.Recurso.Ativo && pp.Permissao.Recurso.Modulo.Ativo && pp.Permissao.Recurso.Modulo.Habilitado)
                    .Select(pp => pp.Permissao.Codigo)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (projection == null)
        {
            return null;
        }

        return new DadosUsuarioPermissoes(
            projection.Ativo,
            projection.AcessoTotal,
            projection.OperadorSistema,
            projection.ModulosHabilitados,
            projection.RecursosHabilitados,
            projection.Permissoes.Distinct().ToList()
        );
    }
}
