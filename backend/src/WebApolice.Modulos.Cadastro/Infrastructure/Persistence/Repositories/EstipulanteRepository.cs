using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public class EstipulanteRepository : IEstipulanteRepository
{
    private readonly CadastroDbContext _dbContext;

    public EstipulanteRepository(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PessoaModel?> LocalizarPessoaPorDocumentoAsync(string documentoLimpo, CancellationToken cancellationToken)
    {
        return await _dbContext.Pessoas
            .Where(p => p.DocumentoPrincipalLimpo == documentoLimpo && p.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PessoaModel?> LocalizarPessoaPorIdAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Pessoas
            .Where(p => p.Id == pessoaId && p.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EstipulanteModel?> LocalizarEstipulantePorPessoaIdAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Estipulantes
            .Where(e => e.PessoaId == pessoaId && e.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EstipulanteModel?> ObterParaEdicaoPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Estipulantes
            .Where(e => e.PublicId == publicId && e.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> VerificarPessoaCompartilhadaAsync(long pessoaId, long? desconsiderarEstipulanteId, CancellationToken cancellationToken)
    {
        bool existeEmOutroEstipulante = await _dbContext.Estipulantes
            .AnyAsync(e => e.PessoaId == pessoaId && e.DeletedAt == null && (!desconsiderarEstipulanteId.HasValue || e.Id != desconsiderarEstipulanteId.Value), cancellationToken);

        if (existeEmOutroEstipulante) return true;

        bool existeCliente = await _dbContext.Clientes.AnyAsync(c => c.PessoaId == pessoaId && c.DeletedAt == null, cancellationToken);
        if (existeCliente) return true;

        bool existeSubestipulante = await _dbContext.Subestipulantes.AnyAsync(s => s.PessoaId == pessoaId && s.DeletedAt == null, cancellationToken);
        if (existeSubestipulante) return true;

        bool existeCorretora = await _dbContext.Corretoras.AnyAsync(c => c.PessoaId == pessoaId && c.DeletedAt == null, cancellationToken);
        if (existeCorretora) return true;

        bool existeAgenciador = await _dbContext.Agenciadores.AnyAsync(a => a.PessoaId == pessoaId && a.DeletedAt == null, cancellationToken);
        return existeAgenciador;
    }

    public async Task<PessoaContatoModel?> ObterContatoPrincipalAsync(long pessoaId, string tipoContato, CancellationToken cancellationToken)
    {
        return await _dbContext.Contatos
            .Where(c => c.PessoaId == pessoaId && c.TipoContato == tipoContato && c.Principal && c.Ativo)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<System.Collections.Generic.IReadOnlyList<PessoaContatoModel>> ObterContatosAtivosAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Contatos
            .Where(c => c.PessoaId == pessoaId && c.Ativo)
            .ToListAsync(cancellationToken);
    }

    public async Task<System.Collections.Generic.IReadOnlyList<PessoaContatoInstitucionalModel>> ObterContatosInstitucionaisAtivosAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.ContatosInstitucionais
            .Where(c => c.PessoaId == pessoaId && c.Ativo)
            .ToListAsync(cancellationToken);
    }

    public async Task<PessoaEnderecoModel?> ObterEnderecoPrincipalAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Enderecos
            .Where(e => e.PessoaId == pessoaId && e.Principal && e.Ativo)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EstipulanteConfiguracaoModel?> ObterConfiguracaoPorEstipulanteIdAsync(long estipulanteId, CancellationToken cancellationToken)
    {
        return await _dbContext.EstipulanteConfiguracoes
            .Where(c => c.EstipulanteId == estipulanteId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> GrupoExisteAsync(long grupoId, CancellationToken cancellationToken)
    {
        return await _dbContext.Grupos.AnyAsync(g => g.Id == grupoId, cancellationToken);
    }

    public async Task<long?> ObterSeguradoraIdPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var seguradora = await _dbContext.Seguradoras
            .Where(s => s.PublicId == publicId)
            .FirstOrDefaultAsync(cancellationToken);
            
        return seguradora?.Id;
    }

    public async Task<bool> CidadeExisteAsync(long cidadeId, CancellationToken cancellationToken)
    {
        return await _dbContext.Cidades.AnyAsync(c => c.Id == cidadeId, cancellationToken);
    }

    public void AdicionarPessoa(PessoaModel pessoa)
    {
        _dbContext.Pessoas.Add(pessoa);
    }

    public void AdicionarEstipulante(EstipulanteModel estipulante)
    {
        _dbContext.Estipulantes.Add(estipulante);
    }

    public void AdicionarEndereco(PessoaEnderecoModel endereco)
    {
        _dbContext.Enderecos.Add(endereco);
    }

    public void AdicionarContato(PessoaContatoModel contato)
    {
        _dbContext.Contatos.Add(contato);
    }

    public void AdicionarContatoInstitucional(PessoaContatoInstitucionalModel contato)
    {
        _dbContext.ContatosInstitucionais.Add(contato);
    }

    public void AdicionarConfiguracao(EstipulanteConfiguracaoModel configuracao)
    {
        _dbContext.EstipulanteConfiguracoes.Add(configuracao);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}
