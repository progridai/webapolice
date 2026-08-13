using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Modulos.Clientes.Tests.Application;

public class CadastrarClienteHandlerTests
{
    [Fact(Skip = "Implementado pelo usuário, teste obsoleto")]
    public void Handle_Sempre_LancaNotSupportedException_DevidoARefatoracao()
    {
    }

    [Fact]
    public async Task Handle_DeveRejeitar_SeREInformado_ERecursoNaoHabilitado()
    {
        // Arrange
        var repositoryMock = new Mock<IClienteRepository>();
        var dbContextMock = new Mock<ClientesDbContext>(new DbContextOptionsBuilder<ClientesDbContext>().Options);
        var auditoriaMock = new Mock<IRegistradorAuditoria>();
        
        var segurancaOptions = new DbContextOptionsBuilder<WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var segurancaDbContext = new WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext(segurancaOptions);

        var handler = new CadastrarClienteHandler(repositoryMock.Object, dbContextMock.Object, auditoriaMock.Object, segurancaDbContext);

        var command = new CadastrarClienteCommand(
            TipoPessoa: 1,
            Nome: "Teste",
            Documento: "12345678909",
            DataNascimento: new DateOnly(1990, 1, 1),
            Sexo: 1,
            Observacao: null,
            Falecido: false,
            DataObito: null,
            Contatos: Array.Empty<ContatoCommand>(),
            Enderecos: Array.Empty<EnderecoCommand>(),
            Re: "RE123"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ClienteInvalidoException>(() =>
            handler.Handle(command, "usuario", CancellationToken.None));
        
        exception.Message.Should().Be("O campo RE não está habilitado no sistema.");
    }
}
