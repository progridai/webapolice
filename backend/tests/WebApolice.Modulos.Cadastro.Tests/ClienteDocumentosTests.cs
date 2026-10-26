using Moq;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Seguranca.Contracts;

namespace WebApolice.Modulos.Cadastro.Tests;

public class ClienteDocumentosTests
{
    [Fact]
    public async Task CadastroInvalidoNaoIniciaOperacaoDePersistencia()
    {
        var operacao = new Mock<IOperacaoCadastroAuditada>(MockBehavior.Strict);
        var handler = new CadastrarClienteHandler(Mock.Of<IClienteRepository>(), operacao.Object, Mock.Of<IDisponibilidadeCatalogo>());
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => handler.Handle(new CadastrarClienteCommand(
            1, "Nome", "11111111111", new DateOnly(1990, 1, 1), null, null, false, null, [], []), "sub", default));
        operacao.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("111.111.111-11", "11111111111", false)]
    [InlineData("111.111.111-11", "52998224725", true)]
    public async Task AtualizacaoPreservaLegadoInalteradoEValidaSubstituicao(string original, string enviado, bool valido)
    {
        var pessoa = new PessoaModel(1, "Nome Original", original, "11111111111", false, null, null, null) { Id = 10 };
        var cliente = new Cliente(pessoa.Id, 1);
        var repository = Preparar(cliente, pessoa);
        var handler = new AlterarClienteHandler(repository.Object, new OperacaoSemPersistencia(), Mock.Of<IDisponibilidadeCatalogo>());
        await handler.Handle(new AlterarClienteCommand(cliente.PublicId, "Nome", enviado,
            new DateOnly(1990, 1, 1), null, null, false, null, [], []), "sub", default);
        Assert.Equal(valido, pessoa.DocumentoValido);
        Assert.Equal(valido ? enviado : original, pessoa.DocumentoPrincipal);
        repository.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubstituicaoPorDocumentoInvalidoNaoSalva()
    {
        var pessoa = new PessoaModel(1, "Nome", "52998224725", "52998224725", true, null, null, null) { Id = 10 };
        var cliente = new Cliente(pessoa.Id, 1);
        var repository = Preparar(cliente, pessoa);
        var handler = new AlterarClienteHandler(repository.Object, new OperacaoSemPersistencia(), Mock.Of<IDisponibilidadeCatalogo>());
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => handler.Handle(new AlterarClienteCommand(cliente.PublicId,
            "Nome", "11111111111", new DateOnly(1990, 1, 1), null, null, false, null, [], []), "sub", default));
        repository.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal("52998224725", pessoa.DocumentoPrincipal);
    }

    private static Mock<IClienteRepository> Preparar(Cliente cliente, PessoaModel pessoa)
    {
        var repository = new Mock<IClienteRepository>();
        repository.Setup(r => r.ObterParaEdicaoPorPublicIdAsync(cliente.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        repository.Setup(r => r.LocalizarPessoaPorIdAsync(pessoa.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pessoa);
        repository.Setup(r => r.ObterContatosAtivosAsync(pessoa.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(r => r.ObterEnderecosAtivosAsync(pessoa.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        return repository;
    }

    // Testa somente o handler; transações físicas são exercitadas em ClienteAuditadoPostgresTests.
    private sealed class OperacaoSemPersistencia : IOperacaoCadastroAuditada
    {
        public async Task<T> ExecutarAsync<T>(string modulo, string recurso, string acao,
            Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
            CancellationToken cancellationToken) => (await executar(cancellationToken)).Resultado;
    }
}
