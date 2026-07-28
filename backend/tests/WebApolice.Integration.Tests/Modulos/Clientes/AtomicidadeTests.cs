using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Repositories;
using WebApolice.Integration.Tests.Setup;
using Xunit;
using Moq;
using WebApolice.Auditoria.Contracts;

namespace WebApolice.Integration.Tests.Modulos.Clientes;

public class AtomicidadeTests : IClassFixture<ClientesIntegrationTestFixture>
{
    private readonly ClientesIntegrationTestFixture _fixture;

    public AtomicidadeTests(ClientesIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handler_FalhaAoPersistirEndereço_DeveFazerRollbackCompleto()
    {
        // Arrange
        var dbContext = _fixture.DbContext;
        var repository = new ClienteRepository(dbContext);
        var auditoriaMock = new Mock<IRegistradorAuditoria>();
        var handler = new CadastrarClienteHandler(repository, dbContext, auditoriaMock.Object);

        var enderecoRequest = new WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente.EnderecoCommand(
            "RESIDENCIAL",
            "00000000",
            "Rua Teste",
            "123",
            null,
            "Bairro Teste",
            9999999, // CidadeId Inexistente que vai violar a Foreign Key
            "RS",
            true
        );

        var contatoRequest = new WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente.ContatoCommand(
            "EMAIL", "falha_endereco@teste.com", true);

        var documento = "12345678909"; // Documento inédito
        var command = new CadastrarClienteCommand(1, "Fulano da Silva", documento, new DateOnly(1990, 1, 1), 1, null, false, null, new[] { contatoRequest }, new[] { enderecoRequest });

        // Act
        var act = () => handler.Handle(command, "user", CancellationToken.None);

        // Assert
        // A falha ocorrerá durante a persistência, o PostgreSQL lançará uma exceção relacionada à FK da cidade.
        // O EF Core envelopa isso em DbUpdateException.
        await act.Should().ThrowAsync<DbUpdateException>();

        // Verifica se a transação realmente não inseriu "sujeiras" no banco
        var dbContextVerify = _fixture.DbContext;
        
        var pessoaExiste = await dbContextVerify.Pessoas.AnyAsync(p => p.DocumentoPrincipalLimpo == "12345678909");
        pessoaExiste.Should().BeFalse("Pessoa não deveria ter sido salva devido ao rollback");

        var clienteExiste = await dbContextVerify.Clientes.AnyAsync(c => c.Observacao == "falha_endereco@teste.com");
        // Não temos como buscar pelo documento direto no cliente, vamos buscar pela pessoa se existisse
        
        var emailExiste = await dbContextVerify.Contatos.AnyAsync(c => c.Valor == "falha_endereco@teste.com");
        emailExiste.Should().BeFalse("Contato não deveria ter sido salvo devido ao rollback");

        var enderecoExiste = await dbContextVerify.Enderecos.AnyAsync(e => e.Cep == "00000000");
        enderecoExiste.Should().BeFalse("Endereço não deveria ter sido salvo devido ao rollback");
    }
}
