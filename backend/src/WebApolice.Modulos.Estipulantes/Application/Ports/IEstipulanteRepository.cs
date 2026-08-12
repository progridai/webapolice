using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Estipulantes.Application.Ports;

public interface IEstipulanteRepository
{
    Task<PessoaModel?> LocalizarPessoaPorDocumentoAsync(string documentoLimpo, CancellationToken cancellationToken);
    Task<PessoaModel?> LocalizarPessoaPorIdAsync(long pessoaId, CancellationToken cancellationToken);
    Task<EstipulanteModel?> LocalizarEstipulantePorPessoaIdAsync(long pessoaId, CancellationToken cancellationToken);
    Task<EstipulanteModel?> ObterParaEdicaoPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    
    Task<bool> VerificarPessoaCompartilhadaAsync(long pessoaId, long? desconsiderarEstipulanteId, CancellationToken cancellationToken);
    
    Task<PessoaContatoModel?> ObterContatoPrincipalAsync(long pessoaId, string tipoContato, CancellationToken cancellationToken);
    Task<System.Collections.Generic.IReadOnlyList<PessoaContatoModel>> ObterContatosAtivosAsync(long pessoaId, CancellationToken cancellationToken);
    Task<PessoaEnderecoModel?> ObterEnderecoPrincipalAsync(long pessoaId, CancellationToken cancellationToken);
    Task<EstipulanteConfiguracaoModel?> ObterConfiguracaoPorEstipulanteIdAsync(long estipulanteId, CancellationToken cancellationToken);
    
    Task<bool> GrupoExisteAsync(long grupoId, CancellationToken cancellationToken);
    Task<long?> ObterSeguradoraIdPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<bool> CidadeExisteAsync(long cidadeId, CancellationToken cancellationToken);

    void AdicionarPessoa(PessoaModel pessoa);
    void AdicionarEstipulante(EstipulanteModel estipulante);
    void AdicionarEndereco(PessoaEnderecoModel endereco);
    void AdicionarContato(PessoaContatoModel contato);
    void AdicionarConfiguracao(EstipulanteConfiguracaoModel configuracao);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
