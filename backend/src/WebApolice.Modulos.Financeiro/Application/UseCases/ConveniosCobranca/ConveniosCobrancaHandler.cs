using WebApolice.Modulos.Financeiro.Application.Ports;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Financeiro.Application.UseCases.ConveniosCobranca;
public sealed class ConveniosCobrancaHandler(IConveniosCobrancaCadastro cadastro)
{
    public Task<ConveniosPagina> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct) =>
        cadastro.ListarAsync(Math.Max(1, pagina), Math.Clamp(tamanhoPagina, 1, 100), busca?.Trim(), ativo, ct);
    public Task<ConvenioCobrancaDto?> ConsultarAsync(Guid id, CancellationToken ct) => cadastro.ConsultarAsync(id, ct);
    public Task<IReadOnlyList<BancoOpcao>> BancosAsync(CancellationToken ct) => cadastro.BancosAsync(ct);
    public Task AlterarStatusAsync(Guid id, bool ativo, CancellationToken ct) => cadastro.AlterarStatusAsync(id, ativo, ct);
    public Task<ConvenioCobrancaDto> SalvarAsync(Guid? id, ConvenioCobrancaDados dados, CancellationToken ct)
    {
        Validar(dados);
        return cadastro.SalvarAsync(id, dados, ct);
    }
    public static void Validar(ConvenioCobrancaDados dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome)) throw new ValidacaoException("O nome do Convênio é obrigatório.");
        if (dados.BancoCodigo?.Trim().Length > 3) throw new ValidacaoException("BancoCodigo excede 3 caracteres.");
        if (dados.Nome?.Trim().Length > 150) throw new ValidacaoException("Nome excede 150 caracteres.");
        if (dados.Agencia?.Trim().Length > 30) throw new ValidacaoException("Agencia excede 30 caracteres.");
        if (dados.ContaCorrente?.Trim().Length > 30) throw new ValidacaoException("ContaCorrente excede 30 caracteres.");
        if (dados.NomeEmpresa?.Trim().Length > 150) throw new ValidacaoException("NomeEmpresa excede 150 caracteres.");
        if (dados.CodigoEmpresa?.Trim().Length > 80) throw new ValidacaoException("CodigoEmpresa excede 80 caracteres.");
        if (dados.NomeInicialArquivo?.Trim().Length > 80) throw new ValidacaoException("NomeInicialArquivo excede 80 caracteres.");
        if (dados.ExtensaoArquivo?.Trim().Length > 10) throw new ValidacaoException("ExtensaoArquivo excede 10 caracteres.");
        if (dados.InscricaoEstadual?.Trim().Length > 40) throw new ValidacaoException("InscricaoEstadual excede 40 caracteres.");
        if (dados.EstEndereco?.Trim().Length > 150) throw new ValidacaoException("EstEndereco excede 150 caracteres.");
        if (dados.EstNumero?.Trim().Length > 100) throw new ValidacaoException("EstNumero excede 100 caracteres.");
        if (dados.EstBairro?.Trim().Length > 100) throw new ValidacaoException("EstBairro excede 100 caracteres.");
        if (dados.EstComplemento?.Trim().Length > 100) throw new ValidacaoException("EstComplemento excede 100 caracteres.");
        if (dados.EstCep?.Trim().Length > 20) throw new ValidacaoException("EstCep excede 20 caracteres.");
        if (dados.EstCidade?.Trim().Length > 100) throw new ValidacaoException("EstCidade excede 100 caracteres.");
        if (dados.EstUf?.Trim().Length > 2) throw new ValidacaoException("EstUf excede 2 caracteres.");
        if (dados.EstNome?.Trim().Length > 120) throw new ValidacaoException("EstNome excede 120 caracteres.");
        if (dados.NumeroArquivo < 0) throw new ValidacaoException("Número de arquivo não pode ser negativo.");
    }
}
