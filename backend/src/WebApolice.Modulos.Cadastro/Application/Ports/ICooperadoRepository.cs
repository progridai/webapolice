using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ICooperadoRepository
{
    Task<PessoaModel?> LocalizarPessoaPorCpfAsync(string cpfLimpo, CancellationToken cancellationToken);
    Task<PessoaModel?> LocalizarPessoaPorIdAsync(long pessoaId, CancellationToken cancellationToken);
    Task<Agenciador?> ObterPorIdAsync(long agenciadorId, CancellationToken cancellationToken);
    Task<Agenciador?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Agenciador?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken);
    
    Task<PessoaContatoModel?> ObterContatoPrincipalAsync(long pessoaId, string tipoContato, CancellationToken cancellationToken);
    Task<PessoaEnderecoModel?> ObterEnderecoPrincipalAsync(long pessoaId, CancellationToken cancellationToken);
    Task<PessoaDocumentoModel?> ObterDocumentoPrincipalAsync(long pessoaId, string tipoDocumento, CancellationToken cancellationToken);
    
    Task<bool> ExisteCooperadoComPessoaIdAsync(long pessoaId, CancellationToken cancellationToken);
    Task<bool> CoordenadorAtivoExisteAsync(long coordenadorId, CancellationToken cancellationToken);

    void AdicionarPessoa(PessoaModel pessoa);
    void AdicionarAgenciador(Agenciador agenciador);
    void AdicionarContato(PessoaContatoModel contato);
    void AdicionarEndereco(PessoaEnderecoModel endereco);
    void AdicionarDocumento(PessoaDocumentoModel documento);
    
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
