using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguranca.Contracts;
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
        // Falha controlada do banco após salvar Pessoa/Cliente. Não presume FKs ausentes do modelo parcial.
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE OR REPLACE FUNCTION core.falha_endereco_teste() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'falha-controlada-endereco'; END $$;
            CREATE TRIGGER falha_endereco_teste BEFORE INSERT ON core.pessoa_endereco
            FOR EACH ROW WHEN (NEW.cep = '00000000') EXECUTE FUNCTION core.falha_endereco_teste();
            """);
        var repository = new ClienteRepository(dbContext);
        var auditoriaMock = new Mock<IRegistradorAuditoria>();
        await using var auditoriaDb = new AuditoriaDbContext(new DbContextOptionsBuilder<AuditoriaDbContext>()
            .UseNpgsql(dbContext.Database.GetDbConnection()).UseSnakeCaseNamingConvention().Options);
        var operacao = new OperacaoCadastroAuditada(dbContext,
            new OperacaoAuditada(auditoriaDb, auditoriaMock.Object, Mock.Of<IContextoAuditoria>()));
        var handler = new CadastrarClienteHandler(repository, operacao, Mock.Of<IDisponibilidadeCatalogo>());


        var enderecoRequest = new WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente.EnderecoCommand(
            "RESIDENCIAL",
            "00000000",
            "Rua Teste",
            "123",
            null,
            "Bairro Teste",
            9999999, // Falha é injetada pelo trigger exclusivo de testes.
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
