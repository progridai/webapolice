using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface IClienteRepository
{
    Task<PessoaModel?> LocalizarPessoaPorDocumentoAsync(string documentoLimpo, CancellationToken cancellationToken);
    Task<PessoaModel?> LocalizarPessoaPorIdAsync(long pessoaId, CancellationToken cancellationToken);
    Task<Cliente?> LocalizarClientePorPessoaIdAsync(long pessoaId, CancellationToken cancellationToken);
    Task<Cliente?> ObterParaEdicaoPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<PessoaContatoModel?> ObterContatoPrincipalAsync(long pessoaId, string tipoContato, CancellationToken cancellationToken);
    Task<PessoaEnderecoModel?> ObterEnderecoPrincipalAsync(long pessoaId, CancellationToken cancellationToken);
    
    void AdicionarPessoa(PessoaModel pessoa);
    void AdicionarCliente(Cliente cliente);
    void AdicionarContato(PessoaContatoModel contato);
    void AdicionarEndereco(PessoaEnderecoModel endereco);
    
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);

    Task<ClienteStatusModel?> ObterStatusPorCodigoAsync(string codigo, CancellationToken cancellationToken);
    Task<bool> VerificarPessoaCompartilhadaAsync(long pessoaId, long? desconsiderarClienteId, CancellationToken cancellationToken);
}
