using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Application.Services;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class PermissoesEfetivasServiceTests : IClassFixture<SegurancaIntegrationTestFixture>, IAsyncLifetime
{
    private readonly SegurancaDbContext _dbContext;
    private readonly PermissoesEfetivasService _service;
    
    // Para limpar no DisposeAsync
    private readonly List<string> _usuariosCriados = new();
    private readonly List<string> _modulosCriados = new();
    
    private readonly List<string> _perfisCriados = new();

    public PermissoesEfetivasServiceTests(SegurancaIntegrationTestFixture fixture)
    {
        _dbContext = fixture.DbContext;
        var repository = new UsuarioRepository(_dbContext);
        _service = new PermissoesEfetivasService(repository);
    }

    public Task InitializeAsync() 
    {
        _dbContext.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_usuariosCriados.Any())
        {
            var subs = string.Join("','", _usuariosCriados);
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.usuario_perfil WHERE usuario_id IN (SELECT id FROM seguranca.usuario WHERE keycloak_sub IN ('{subs}'))");
        }

        if (_perfisCriados.Any())
        {
            var perfis = string.Join("','", _perfisCriados);
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.perfil_permissao WHERE perfil_id IN (SELECT id FROM seguranca.perfil WHERE codigo IN ('{perfis}'))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.usuario_perfil WHERE perfil_id IN (SELECT id FROM seguranca.perfil WHERE codigo IN ('{perfis}'))");
        }

        if (_modulosCriados.Any()) 
        {
            var codigos = string.Join("','", _modulosCriados);
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.perfil_permissao WHERE permissao_id IN (SELECT id FROM seguranca.permissao WHERE recurso_id IN (SELECT id FROM seguranca.recurso WHERE modulo_id IN (SELECT id FROM seguranca.modulo WHERE codigo IN ('{codigos}'))))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.permissao WHERE recurso_id IN (SELECT id FROM seguranca.recurso WHERE modulo_id IN (SELECT id FROM seguranca.modulo WHERE codigo IN ('{codigos}')))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.recurso WHERE modulo_id IN (SELECT id FROM seguranca.modulo WHERE codigo IN ('{codigos}'))");
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.modulo WHERE codigo IN ('{codigos}')");
        }
        
        if (_perfisCriados.Any())
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.perfil WHERE codigo IN ('{string.Join("','", _perfisCriados)}')");
        }

        if (_usuariosCriados.Any())
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM seguranca.usuario WHERE keycloak_sub IN ('{string.Join("','", _usuariosCriados)}')");
        }
    }

    private async Task<Usuario> InserirUsuarioBaseAsync(string sub, bool ativo)
    {
        var u = new Usuario(sub, sub, sub, $"{sub}@teste.com", ativo);
        _dbContext.Usuarios.Add(u);
        await _dbContext.SaveChangesAsync();
        _usuariosCriados.Add(sub);
        return u;
    }

    private T Criar<T>(Action<T> action)
    {
        var entity = (T)Activator.CreateInstance(typeof(T), true)!;
        action(entity);
        return entity;
    }

    private async Task InserirPerfilCompletoAsync(
        Usuario usuario, string runId, bool acessoTotal, bool perfilAtivo, 
        bool permissaoAtiva, bool recursoAtivo, bool moduloAtivo)
    {
        var modulo = Criar<Modulo>(m => 
        {
            typeof(Modulo).GetProperty("Codigo")!.SetValue(m, $"MOD_{runId}");
            typeof(Modulo).GetProperty("Nome")!.SetValue(m, "MOD");
            typeof(Modulo).GetProperty("Ordem")!.SetValue(m, 1);
            typeof(Modulo).GetProperty("Ativo")!.SetValue(m, moduloAtivo);
        });
        
        var recurso = Criar<Recurso>(r => 
        {
            typeof(Recurso).GetProperty("ModuloId")!.SetValue(r, modulo.Id);
            typeof(Recurso).GetProperty("Modulo")!.SetValue(r, modulo);
            typeof(Recurso).GetProperty("Codigo")!.SetValue(r, $"REC_{runId}");
            typeof(Recurso).GetProperty("Nome")!.SetValue(r, "REC");
            typeof(Recurso).GetProperty("Ordem")!.SetValue(r, 1);
            typeof(Recurso).GetProperty("Ativo")!.SetValue(r, recursoAtivo);
        });

        var perm1 = Criar<Permissao>(p => 
        {
            typeof(Permissao).GetProperty("RecursoId")!.SetValue(p, recurso.Id);
            typeof(Permissao).GetProperty("Recurso")!.SetValue(p, recurso);
            typeof(Permissao).GetProperty("Codigo")!.SetValue(p, $"perm1_{runId}");
            typeof(Permissao).GetProperty("Nome")!.SetValue(p, "perm1");
            typeof(Permissao).GetProperty("Ativo")!.SetValue(p, permissaoAtiva);
        });

        var perm2 = Criar<Permissao>(p => 
        {
            typeof(Permissao).GetProperty("RecursoId")!.SetValue(p, recurso.Id);
            typeof(Permissao).GetProperty("Recurso")!.SetValue(p, recurso);
            typeof(Permissao).GetProperty("Codigo")!.SetValue(p, $"perm2_{runId}");
            typeof(Permissao).GetProperty("Nome")!.SetValue(p, "perm2");
            typeof(Permissao).GetProperty("Ativo")!.SetValue(p, permissaoAtiva);
        });
        
        _dbContext.Modulos.Add(modulo);
        _dbContext.Set<Recurso>().Add(recurso);
        _dbContext.Set<Permissao>().AddRange(perm1, perm2);
        _modulosCriados.Add(modulo.Codigo);

        var perfil = new Perfil($"PERF_{runId}", "PERF", "Desc", perfilAtivo, false, acessoTotal);
        _dbContext.Perfis.Add(perfil);
        _perfisCriados.Add(perfil.Codigo);

        await _dbContext.SaveChangesAsync();

        var perfilPerm1 = Criar<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao>(pp => 
        {
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PerfilId")!.SetValue(pp, perfil.Id);
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PermissaoId")!.SetValue(pp, perm1.Id);
        });
        var perfilPerm2 = Criar<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao>(pp => 
        {
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PerfilId")!.SetValue(pp, perfil.Id);
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PermissaoId")!.SetValue(pp, perm2.Id);
        });
        
        _dbContext.Set<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao>().AddRange(perfilPerm1, perfilPerm2);
        
        var usuarioPerfil = Criar<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil>(up => 
        {
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil).GetProperty("UsuarioId")!.SetValue(up, usuario.Id);
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil).GetProperty("PerfilId")!.SetValue(up, perfil.Id);
        });
        
        _dbContext.Set<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil>().Add(usuarioPerfil);
        
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Deve_Retornar_Falso_E_Sem_Permissoes_Quando_Usuario_Inexistente()
    {
        var result = await _service.CalcularPermissoesAsync("sub_inexistente_" + Guid.NewGuid());

        Assert.False(result.UsuarioEncontrado);
        Assert.False(result.UsuarioAtivo);
        Assert.False(result.AcessoTotal);
        Assert.Empty(result.Permissoes);
    }

    [Fact]
    public async Task Deve_Retornar_Falso_E_Sem_Permissoes_Quando_Usuario_Inativo()
    {
        var runId = Guid.NewGuid().ToString("N");
        var usuario = await InserirUsuarioBaseAsync("sub_inativo_" + runId, ativo: false);
        await InserirPerfilCompletoAsync(usuario, runId, acessoTotal: true, perfilAtivo: true, permissaoAtiva: true, recursoAtivo: true, moduloAtivo: true);

        var result = await _service.CalcularPermissoesAsync(usuario.KeycloakSub);

        Assert.True(result.UsuarioEncontrado);
        Assert.False(result.UsuarioAtivo);
        Assert.False(result.AcessoTotal);
        Assert.Empty(result.Permissoes);
    }

    [Fact]
    public async Task Deve_Retornar_Acesso_Total_Quando_Perfil_Tem_Acesso_Total()
    {
        var runId = Guid.NewGuid().ToString("N");
        var usuario = await InserirUsuarioBaseAsync("sub_admin_" + runId, ativo: true);
        await InserirPerfilCompletoAsync(usuario, runId, acessoTotal: true, perfilAtivo: true, permissaoAtiva: true, recursoAtivo: true, moduloAtivo: true);

        var result = await _service.CalcularPermissoesAsync(usuario.KeycloakSub);

        Assert.True(result.UsuarioEncontrado);
        Assert.True(result.UsuarioAtivo);
        Assert.True(result.AcessoTotal);
    }

    [Fact]
    public async Task Deve_Retornar_Permissoes_Para_Usuario_Ativo_Com_Perfil_Comum()
    {
        var runId = Guid.NewGuid().ToString("N");
        var usuario = await InserirUsuarioBaseAsync("sub_comum_" + runId, ativo: true);
        await InserirPerfilCompletoAsync(usuario, runId, acessoTotal: false, perfilAtivo: true, permissaoAtiva: true, recursoAtivo: true, moduloAtivo: true);

        var result = await _service.CalcularPermissoesAsync(usuario.KeycloakSub);

        Assert.True(result.UsuarioEncontrado);
        Assert.True(result.UsuarioAtivo);
        Assert.False(result.AcessoTotal);
        Assert.Contains($"perm1_{runId}", result.Permissoes);
        Assert.Contains($"perm2_{runId}", result.Permissoes);
        Assert.Equal(2, result.Permissoes.Count);
    }

    [Fact]
    public async Task Nao_Deve_Retornar_Permissoes_Quando_Modulo_Inativo()
    {
        var runId = Guid.NewGuid().ToString("N");
        var usuario = await InserirUsuarioBaseAsync("sub_mod_inativo_" + runId, ativo: true);
        await InserirPerfilCompletoAsync(usuario, runId, acessoTotal: false, perfilAtivo: true, permissaoAtiva: true, recursoAtivo: true, moduloAtivo: false);

        var result = await _service.CalcularPermissoesAsync(usuario.KeycloakSub);

        Assert.Empty(result.Permissoes);
    }

    [Fact]
    public async Task Nao_Deve_Retornar_Permissoes_Quando_Perfil_Inativo()
    {
        var runId = Guid.NewGuid().ToString("N");
        var usuario = await InserirUsuarioBaseAsync("sub_perf_inativo_" + runId, ativo: true);
        await InserirPerfilCompletoAsync(usuario, runId, acessoTotal: true, perfilAtivo: false, permissaoAtiva: true, recursoAtivo: true, moduloAtivo: true);

        var result = await _service.CalcularPermissoesAsync(usuario.KeycloakSub);

        Assert.False(result.AcessoTotal);
        Assert.Empty(result.Permissoes);
    }

    [Fact]
    public async Task Deve_Unir_Multiplos_Perfis_Sem_Duplicar_Permissoes()
    {
        var runId = Guid.NewGuid().ToString("N");
        var usuario = await InserirUsuarioBaseAsync("sub_multi_" + runId, ativo: true);

        var modulo = Criar<Modulo>(m => 
        {
            typeof(Modulo).GetProperty("Codigo")!.SetValue(m, $"MOD_{runId}");
            typeof(Modulo).GetProperty("Nome")!.SetValue(m, "MOD");
            typeof(Modulo).GetProperty("Ordem")!.SetValue(m, 1);
            typeof(Modulo).GetProperty("Ativo")!.SetValue(m, true);
        });
        
        var recurso = Criar<Recurso>(r => 
        {
            typeof(Recurso).GetProperty("ModuloId")!.SetValue(r, modulo.Id);
            typeof(Recurso).GetProperty("Modulo")!.SetValue(r, modulo);
            typeof(Recurso).GetProperty("Codigo")!.SetValue(r, $"REC_{runId}");
            typeof(Recurso).GetProperty("Nome")!.SetValue(r, "REC");
            typeof(Recurso).GetProperty("Ordem")!.SetValue(r, 1);
            typeof(Recurso).GetProperty("Ativo")!.SetValue(r, true);
        });

        var permUnica = Criar<Permissao>(p => 
        {
            typeof(Permissao).GetProperty("RecursoId")!.SetValue(p, recurso.Id);
            typeof(Permissao).GetProperty("Recurso")!.SetValue(p, recurso);
            typeof(Permissao).GetProperty("Codigo")!.SetValue(p, $"perm_unica_{runId}");
            typeof(Permissao).GetProperty("Nome")!.SetValue(p, "perm_unica");
            typeof(Permissao).GetProperty("Ativo")!.SetValue(p, true);
        });
        
        _dbContext.Modulos.Add(modulo);
        _dbContext.Set<Recurso>().Add(recurso);
        _dbContext.Set<Permissao>().Add(permUnica);
        _modulosCriados.Add(modulo.Codigo);

        var perfil1 = new Perfil($"P1_{runId}", "P1", "Desc", true, false, false);
        var perfil2 = new Perfil($"P2_{runId}", "P2", "Desc", true, false, false);
        _dbContext.Perfis.AddRange(perfil1, perfil2);
        _perfisCriados.Add(perfil1.Codigo);
        _perfisCriados.Add(perfil2.Codigo);

        await _dbContext.SaveChangesAsync();

        var perfilPerm1 = Criar<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao>(pp => 
        {
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PerfilId")!.SetValue(pp, perfil1.Id);
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PermissaoId")!.SetValue(pp, permUnica.Id);
        });
        var perfilPerm2 = Criar<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao>(pp => 
        {
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PerfilId")!.SetValue(pp, perfil2.Id);
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao).GetProperty("PermissaoId")!.SetValue(pp, permUnica.Id);
        });

        _dbContext.Set<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao>().AddRange(perfilPerm1, perfilPerm2);
        
        var up1 = Criar<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil>(up => 
        {
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil).GetProperty("UsuarioId")!.SetValue(up, usuario.Id);
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil).GetProperty("PerfilId")!.SetValue(up, perfil1.Id);
        });
        var up2 = Criar<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil>(up => 
        {
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil).GetProperty("UsuarioId")!.SetValue(up, usuario.Id);
            typeof(WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil).GetProperty("PerfilId")!.SetValue(up, perfil2.Id);
        });
        _dbContext.Set<WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil>().AddRange(up1, up2);
        
        await _dbContext.SaveChangesAsync();

        var result = await _service.CalcularPermissoesAsync(usuario.KeycloakSub);

        Assert.True(result.UsuarioEncontrado);
        Assert.True(result.UsuarioAtivo);
        Assert.False(result.AcessoTotal);
        Assert.Single(result.Permissoes);
        Assert.Contains($"perm_unica_{runId}", result.Permissoes);
    }
}
