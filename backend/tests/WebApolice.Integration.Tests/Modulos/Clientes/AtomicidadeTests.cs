using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;
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
    public async Task Handler_FalhaAoPersistirEndereco_DeveFazerRollbackCompleto()
    {
        // Arrange
        var dbContext = _fixture.DbContext;
        var repository = new ClienteRepository(dbContext);
        var auditoriaMock = new Mock<IRegistradorAuditoria>();
        var segurancaDbContext = new WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext(
            new DbContextOptionsBuilder<WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext>()
                .UseNpgsql(dbContext.Database.GetConnectionString())
                .Options);
        var handler = new CadastrarClienteHandler(repository, dbContext, auditoriaMock.Object, segurancaDbContext);

        var enderecoRequest = new WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente.EnderecoCommand(
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

        var contatoRequest = new WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente.ContatoCommand(
            "EMAIL", "falha_endereco@teste.com", true);

        var documento = "12345678909"; // Documento inÃ©dito
        var command = new CadastrarClienteCommand(1, "Fulano da Silva", documento, new DateOnly(1990, 1, 1), 1, null, false, null, new[] { contatoRequest }, new[] { enderecoRequest });

        // Act
        var act = () => handler.Handle(command, "user", CancellationToken.None);

        // Assert
        // A falha ocorrerÃ¡ durante a persistÃªncia, o PostgreSQL lanÃ§arÃ¡ uma exceÃ§Ã£o relacionada Ã  FK da cidade.
        // O EF Core envelopa isso em DbUpdateException.
        await act.Should().ThrowAsync<DbUpdateException>();

        // Verifica se a transaÃ§Ã£o realmente nÃ£o inseriu "sujeiras" no banco
        var dbContextVerify = _fixture.DbContext;
        
        var pessoaExiste = await dbContextVerify.Pessoas.AnyAsync(p => p.DocumentoPrincipalLimpo == "12345678909");
        pessoaExiste.Should().BeFalse("Pessoa nÃ£o deveria ter sido salva devido ao rollback");

        var clienteExiste = await dbContextVerify.Clientes.AnyAsync(c => c.Observacao == "falha_endereco@teste.com");
        // NÃ£o temos como buscar pelo documento direto no cliente, vamos buscar pela pessoa se existisse
        
        var emailExiste = await dbContextVerify.Contatos.AnyAsync(c => c.Valor == "falha_endereco@teste.com");
        emailExiste.Should().BeFalse("Contato nÃ£o deveria ter sido salvo devido ao rollback");

        var enderecoExiste = await dbContextVerify.Enderecos.AnyAsync(e => e.Cep == "00000000");
        enderecoExiste.Should().BeFalse("EndereÃ§o nÃ£o deveria ter sido salvo devido ao rollback");
    }
}
