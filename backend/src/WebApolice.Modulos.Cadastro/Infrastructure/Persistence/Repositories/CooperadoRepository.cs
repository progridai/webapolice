using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public class CooperadoRepository : ICooperadoRepository
{
    private readonly CadastroDbContext _dbContext;

    public CooperadoRepository(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PessoaModel?> LocalizarPessoaPorCpfAsync(string cpfLimpo, CancellationToken cancellationToken)
    {
        return await _dbContext.Pessoas
            .FirstOrDefaultAsync(p => p.DocumentoPrincipalLimpo == cpfLimpo && p.DeletedAt == null, cancellationToken);
    }

    public async Task<PessoaModel?> LocalizarPessoaPorIdAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Pessoas
            .FirstOrDefaultAsync(p => p.Id == pessoaId && p.DeletedAt == null, cancellationToken);
    }

    public async Task<Agenciador?> ObterPorIdAsync(long agenciadorId, CancellationToken cancellationToken)
    {
        return await _dbContext.Agenciadores
            .FirstOrDefaultAsync(a => a.Id == agenciadorId && a.DeletedAt == null, cancellationToken);
    }

    public async Task<Agenciador?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Agenciadores
            .FirstOrDefaultAsync(a => a.PublicId == publicId && a.DeletedAt == null, cancellationToken);
    }

    public async Task<Agenciador?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken)
    {
        return await _dbContext.Agenciadores
            .FirstOrDefaultAsync(a => a.Codigo == codigo && a.DeletedAt == null, cancellationToken);
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

    public async Task<PessoaDocumentoModel?> ObterDocumentoPrincipalAsync(long pessoaId, string tipoDocumento, CancellationToken cancellationToken)
    {
        return await _dbContext.Documentos
            .Where(d => d.PessoaId == pessoaId && d.TipoDocumento == tipoDocumento && d.Principal)
            .OrderByDescending(d => d.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExisteCooperadoComPessoaIdAsync(long pessoaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Agenciadores
            .AnyAsync(a => a.PessoaId == pessoaId && a.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> CoordenadorAtivoExisteAsync(long coordenadorId, CancellationToken cancellationToken)
    {
        return await _dbContext.Agenciadores
            .AnyAsync(a => a.Id == coordenadorId && a.Tipo == TipoAgenciador.Coordenador && !a.Desativado && a.DeletedAt == null, cancellationToken);
    }

    public void AdicionarPessoa(PessoaModel pessoa) => _dbContext.Pessoas.Add(pessoa);
    public void AdicionarAgenciador(Agenciador agenciador) => _dbContext.Agenciadores.Add(agenciador);
    public void AdicionarContato(PessoaContatoModel contato) => _dbContext.Contatos.Add(contato);
    public void AdicionarEndereco(PessoaEnderecoModel endereco) => _dbContext.Enderecos.Add(endereco);
    public void AdicionarDocumento(PessoaDocumentoModel documento) => _dbContext.Documentos.Add(documento);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
