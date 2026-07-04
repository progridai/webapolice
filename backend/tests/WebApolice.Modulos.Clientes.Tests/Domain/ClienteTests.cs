using System;
using FluentAssertions;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using Xunit;

namespace WebApolice.Modulos.Clientes.Tests.Domain;

public class ClienteTests
{
    // =========================================================================
    // NOME TESTS
    // =========================================================================
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Criar_ComNomeInvalido_DeveLancarClienteInvalidoException(string? nome)
    {
        var act = () => new Cliente(nome!, "01821765419", null, null, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*Nome*");
    }

    [Fact]
    public void Criar_ComNomeCurto_DeveLancarClienteInvalidoException()
    {
        var act = () => new Cliente("A", "01821765419", null, null, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*mínimo 2*");
    }

    [Fact]
    public void Criar_ComNomeEspacosExternos_DeveNormalizarESalvar()
    {
        var cliente = new Cliente("   João da Silva   ", "01821765419", null, null, null, null);
        cliente.Nome.Should().Be("João da Silva");
    }

    [Fact]
    public void Criar_ComNomeMultiplosEspacosInternos_DeveNormalizarESalvar()
    {
        var cliente = new Cliente("João    da    Silva", "01821765419", null, null, null, null);
        cliente.Nome.Should().Be("João da Silva");
    }

    [Fact]
    public void Criar_ComNomeTamanhoAcimaDoLimite_DeveLancarClienteInvalidoException()
    {
        var nomeLongo = new string('A', 151);
        var act = () => new Cliente(nomeLongo, "01821765419", null, null, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*exceder 150*");
    }

    [Fact]
    public void Criar_ComNomeComAcentosECapitalizacao_DevePreservar()
    {
        var cliente = new Cliente("João da Conceição SÁ", "01821765419", null, null, null, null);
        cliente.Nome.Should().Be("João da Conceição SÁ");
    }

    // =========================================================================
    // CPF TESTS
    // =========================================================================
    [Fact]
    public void Criar_ComCpfValidoFormatado_DeveNormalizarESalvarApenasDigitos()
    {
        var cliente = new Cliente("Fulano", "018.217.654-19", null, null, null, null);
        cliente.Cpf.Should().Be("01821765419");
    }

    [Fact]
    public void Criar_ComCpfValidoSemFormatacao_DeveSalvarApenasDigitos()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.Cpf.Should().Be("01821765419");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Criar_ComCpfComQuantidadeInvalida_DeveLancarClienteInvalidoException(string cpf)
    {
        var act = () => new Cliente("Fulano", cpf, null, null, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*11 dígitos*");
    }

    [Fact]
    public void Criar_ComCpfDigitoVerificadorInvalido_DeveLancarClienteInvalidoException()
    {
        var act = () => new Cliente("Fulano", "01821765418", null, null, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("CPF inválido.");
    }

    [Fact]
    public void Criar_ComCpfSequenciaRepetida_DeveLancarClienteInvalidoException()
    {
        var act = () => new Cliente("Fulano", "11111111111", null, null, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("CPF inválido.");
    }

    [Fact]
    public void Criar_CpfInvalido_ExcecaoNaoDeveConterCpfCompleto()
    {
        var cpfInvalido = "01821765418";
        var act = () => new Cliente("Fulano", cpfInvalido, null, null, null, null);
        var ex = act.Should().Throw<ClienteInvalidoException>().And;
        ex.Message.Should().NotContain(cpfInvalido);
    }

    // =========================================================================
    // DATA NASCIMENTO TESTS
    // =========================================================================
    [Fact]
    public void Criar_ComDataNascimentoValida_DeveInstanciarCorretamente()
    {
        var data = new DateOnly(1995, 5, 20);
        var cliente = new Cliente("Fulano", "01821765419", data, null, null, null);
        cliente.DataNascimento.Should().Be(data);
    }

    [Fact]
    public void Criar_ComDataNascimentoNula_DeveInstanciarCorretamente()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.DataNascimento.Should().BeNull();
    }

    [Fact]
    public void Criar_ComDataNascimentoFutura_DeveLancarClienteInvalidoException()
    {
        var futura = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var act = () => new Cliente("Fulano", "01821765419", futura, null, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*futuro*");
    }

    // =========================================================================
    // EMAIL TESTS
    // =========================================================================
    [Fact]
    public void Criar_ComEmailValido_DeveSalvar()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, "teste@webapolice.com.br", null, null);
        cliente.Email.Should().Be("teste@webapolice.com.br");
    }

    [Fact]
    public void Criar_ComEmailNulo_DeveSalvar()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.Email.Should().BeNull();
    }

    [Fact]
    public void Criar_ComEmailEspacosExternos_DeveNormalizar()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, "  teste@webapolice.com  ", null, null);
        cliente.Email.Should().Be("teste@webapolice.com");
    }

    [Theory]
    [InlineData("emailsemarroba")]
    [InlineData("email@")]
    [InlineData("@dominio")]
    [InlineData("email com espacos@dominio.com")]
    public void Criar_ComEmailInvalido_DeveLancarClienteInvalidoException(string email)
    {
        var act = () => new Cliente("Fulano", "01821765419", null, email, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*Email*");
    }

    [Fact]
    public void Criar_ComEmailAcimaLimite_DeveLancarClienteInvalidoException()
    {
        var emailLongo = new string('a', 250) + "@t.com";
        var act = () => new Cliente("Fulano", "01821765419", null, emailLongo, null, null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*254*");
    }

    // =========================================================================
    // TELEFONE TESTS
    // =========================================================================
    [Fact]
    public void Criar_ComTelefoneValido_DeveSalvar()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, "(51) 99999-8888", null);
        cliente.Telefone.Should().Be("(51) 99999-8888");
    }

    [Fact]
    public void Criar_ComTelefoneNulo_DeveSalvar()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.Telefone.Should().BeNull();
    }

    [Fact]
    public void Criar_ComTelefoneEspacosExternos_DeveNormalizar()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, "  51999998888  ", null);
        cliente.Telefone.Should().Be("51999998888");
    }

    [Fact]
    public void Criar_ComTelefoneQuantidadeInvalida_DeveLancarClienteInvalidoException()
    {
        var act = () => new Cliente("Fulano", "01821765419", null, null, "123", null);
        act.Should().Throw<ClienteInvalidoException>().WithMessage("*mínimo 8*");
    }

    // =========================================================================
    // DATAS TECNICAS TESTS
    // =========================================================================
    [Fact]
    public void Criar_DeveDefinirDatasTecnicasEmUtc()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.DataCadastroUtc.Kind.Should().Be(DateTimeKind.Utc);
        cliente.DataAtualizacaoUtc.Kind.Should().Be(DateTimeKind.Utc);
        cliente.DataCadastroUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cliente.DataAtualizacaoUtc.Should().Be(cliente.DataCadastroUtc);
    }

    [Fact]
    public void Alterar_DevePreservarDataCadastroEModificarDataAtualizacao()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        var dataCadastro = cliente.DataCadastroUtc;

        // Simular pequeno delay
        System.Threading.Thread.Sleep(10);

        cliente.Alterar("Ciclano", null, null, null);
        cliente.DataCadastroUtc.Should().Be(dataCadastro);
        cliente.DataAtualizacaoUtc.Should().BeAfter(dataCadastro);
    }

    // =========================================================================
    // CODIGO LEGADO TESTS
    // =========================================================================
    [Fact]
    public void Criar_ComCodigoLegadoNulo_DeveInstanciarCorretamente()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.CodigoLegado.Should().BeNull();
    }

    [Fact]
    public void Criar_ComCodigoLegadoValido_DeveInstanciarCorretamente()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, 12345L);
        cliente.CodigoLegado.Should().Be(12345L);
    }

    // =========================================================================
    // STATUS TESTS
    // =========================================================================
    [Fact]
    public void Criar_DeveDefinirStatusInicialComoAtivo()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.Status.Should().Be(StatusCliente.Ativo);
    }

    [Fact]
    public void Inativar_Ativo_DeveMudarStatusParaInativoEModificarAtualizacao()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        var atualizacaoAnterior = cliente.DataAtualizacaoUtc;

        System.Threading.Thread.Sleep(10);
        cliente.Inativar();

        cliente.Status.Should().Be(StatusCliente.Inativo);
        cliente.DataAtualizacaoUtc.Should().BeAfter(atualizacaoAnterior);
    }

    [Fact]
    public void Inativar_Idempotente_NaoDeveModificarDataAtualizacao()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.Inativar();
        var atualizacaoAnterior = cliente.DataAtualizacaoUtc;

        System.Threading.Thread.Sleep(10);
        cliente.Inativar(); // Idempotente

        cliente.Status.Should().Be(StatusCliente.Inativo);
        cliente.DataAtualizacaoUtc.Should().Be(atualizacaoAnterior);
    }

    [Fact]
    public void Ativar_Inativo_DeveMudarStatusParaAtivoEModificarAtualizacao()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        cliente.Inativar();
        var atualizacaoAnterior = cliente.DataAtualizacaoUtc;

        System.Threading.Thread.Sleep(10);
        cliente.Ativar();

        cliente.Status.Should().Be(StatusCliente.Ativo);
        cliente.DataAtualizacaoUtc.Should().BeAfter(atualizacaoAnterior);
    }

    [Fact]
    public void Ativar_Idempotente_NaoDeveModificarDataAtualizacao()
    {
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        var atualizacaoAnterior = cliente.DataAtualizacaoUtc;

        System.Threading.Thread.Sleep(10);
        cliente.Ativar(); // Idempotente

        cliente.Status.Should().Be(StatusCliente.Ativo);
        cliente.DataAtualizacaoUtc.Should().Be(atualizacaoAnterior);
    }
}
