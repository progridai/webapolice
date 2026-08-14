using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public sealed class ClienteRepository : IClienteRepository
{
    private readonly CadastroDbContext _dbContext;

    public ClienteRepository(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PessoaModel?> LocalizarPessoaPorDocumentoAsync(string documentoLimpo, CancellationToken cancellationToken)
    {
        return await _dbContext.Pessoas
            .FirstOrDefaultAsync(p => p.DocumentoPrincipalLimpo == documentoLimpo && p.DeletedAt == null, cancellationToken);
    }

    public async Task<PessoaModel?> LocalizarPessoaPorIdAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Pessoas
            .FirstOrDefaultAsync(p => p.Id == pessoaId && p.DeletedAt == null, cancellationToken);
    }

    public async Task<Cliente?> LocalizarClientePorPessoaIdAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes
            .FirstOrDefaultAsync(c => c.PessoaId == pessoaId && c.DeletedAt == null, cancellationToken);
    }

    public async Task<Cliente?> ObterParaEdicaoPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes
            .FirstOrDefaultAsync(c => c.PublicId == publicId && c.DeletedAt == null, cancellationToken);
    }

    public async Task<PessoaContatoModel?> ObterContatoPrincipalAsync(long pessoaId, string tipoContato, CancellationToken cancellationToken)
    {
        return await _dbContext.Contatos
            .Where(c => c.PessoaId == pessoaId && c.TipoContato == tipoContato && c.Principal && c.Ativo)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PessoaEnderecoModel?> ObterEnderecoPrincipalAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Enderecos
            .Where(e => e.PessoaId == pessoaId && e.Principal && e.Ativo)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void AdicionarPessoa(PessoaModel pessoa)
    {
        _dbContext.Pessoas.Add(pessoa);
    }

    public void AdicionarCliente(Cliente cliente)
    {
        _dbContext.Clientes.Add(cliente);
    }

    public void AdicionarContato(PessoaContatoModel contato)
    {
        _dbContext.Contatos.Add(contato);
    }

    public void AdicionarEndereco(PessoaEnderecoModel endereco)
    {
        _dbContext.Enderecos.Add(endereco);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ClienteStatusModel?> ObterStatusPorCodigoAsync(string codigo, CancellationToken cancellationToken)
    {
        return await _dbContext.Status.FirstOrDefaultAsync(s => s.Codigo.ToLower() == codigo.ToLower(), cancellationToken);
    }

    public async Task<bool> VerificarPessoaCompartilhadaAsync(long pessoaId, long? desconsiderarClienteId, CancellationToken cancellationToken)
    {
        bool existeEmOutroCliente = await _dbContext.Clientes
            .AnyAsync(c => c.PessoaId == pessoaId && c.DeletedAt == null && (!desconsiderarClienteId.HasValue || c.Id != desconsiderarClienteId.Value), cancellationToken);

        if (existeEmOutroCliente) return true;

        bool existeEstipulante = await _dbContext.Estipulantes.AnyAsync(e => e.PessoaId == pessoaId && e.DeletedAt == null, cancellationToken);
        if (existeEstipulante) return true;

        bool existeSubestipulante = await _dbContext.Subestipulantes.AnyAsync(s => s.PessoaId == pessoaId && s.DeletedAt == null, cancellationToken);
        if (existeSubestipulante) return true;

        bool existeCorretora = await _dbContext.Corretoras.AnyAsync(c => c.PessoaId == pessoaId && c.DeletedAt == null, cancellationToken);
        if (existeCorretora) return true;

        bool existeSeguradora = await _dbContext.Seguradoras.AnyAsync(s => s.PessoaId == pessoaId && s.DeletedAt == null, cancellationToken);
        if (existeSeguradora) return true;

        bool existeAgenciador = await _dbContext.Agenciadores.AnyAsync(a => a.PessoaId == pessoaId && a.DeletedAt == null, cancellationToken);
        return existeAgenciador;
    }
}
