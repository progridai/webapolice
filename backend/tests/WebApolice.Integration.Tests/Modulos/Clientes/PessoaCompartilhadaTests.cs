using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;
using WebApolice.Integration.Tests.Setup;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Clientes;

public class PessoaCompartilhadaTests : IClassFixture<ClientesIntegrationTestFixture>
{
    private readonly ClientesIntegrationTestFixture _fixture;

    public PessoaCompartilhadaTests(ClientesIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<long> SetupPessoaAsync()
    {
        var id = DateTime.Now.Ticks;
        var pessoa = new PessoaModel(
            tipoPessoa: 1,
            nome: "Pessoa Teste",
            documentoPrincipal: id.ToString(),
            documentoPrincipalLimpo: id.ToString(),
            documentoValido: true,
            dataNascimento: new DateTime(1990, 1, 1),
            sexo: 1,
            observacao: null
        );
        typeof(PessoaModel).GetProperty("Id")?.SetValue(pessoa, id);
        
        _fixture.DbContext.Pessoas.Add(pessoa);
        await _fixture.DbContext.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task VerificarPessoaCompartilhadaAsync_VinculoNaoExcluido_DeveBloquear()
    {
        var pessoaId = await SetupPessoaAsync();
        
        var corretora = new CorretoraModel();
        typeof(CorretoraModel).GetProperty("Id")?.SetValue(corretora, DateTime.Now.Ticks);
        typeof(CorretoraModel).GetProperty("PessoaId")?.SetValue(corretora, pessoaId);
        
        _fixture.DbContext.Corretoras.Add(corretora);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new ClienteRepository(_fixture.DbContext);
        var result = await repo.VerificarPessoaCompartilhadaAsync(pessoaId, null, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerificarPessoaCompartilhadaAsync_VinculoExcluido_NaoDeveBloquear()
    {
        var pessoaId = await SetupPessoaAsync();
        
        var corretora = new CorretoraModel();
        typeof(CorretoraModel).GetProperty("Id")?.SetValue(corretora, DateTime.Now.Ticks);
        typeof(CorretoraModel).GetProperty("PessoaId")?.SetValue(corretora, pessoaId);
        typeof(CorretoraModel).GetProperty("DeletedAt")?.SetValue(corretora, DateTime.UtcNow); // ExcluÃ­do logicamente
        
        _fixture.DbContext.Corretoras.Add(corretora);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new ClienteRepository(_fixture.DbContext);
        var result = await repo.VerificarPessoaCompartilhadaAsync(pessoaId, null, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerificarPessoaCompartilhadaAsync_OutroClienteExcluido_NaoDeveBloquear()
    {
        var pessoaId = await SetupPessoaAsync();
        
        var cliente = new Cliente(pessoaId, 1);
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, DateTime.Now.Ticks);
        typeof(Cliente).GetProperty("DeletedAt")?.SetValue(cliente, DateTime.UtcNow);
        
        _fixture.DbContext.Clientes.Add(cliente);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new ClienteRepository(_fixture.DbContext);
        var result = await repo.VerificarPessoaCompartilhadaAsync(pessoaId, null, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerificarPessoaCompartilhadaAsync_OutroClienteNaoExcluido_DeveBloquear()
    {
        var pessoaId = await SetupPessoaAsync();
        
        var cliente = new Cliente(pessoaId, 1);
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, DateTime.Now.Ticks);
        
        _fixture.DbContext.Clientes.Add(cliente);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new ClienteRepository(_fixture.DbContext);
        var result = await repo.VerificarPessoaCompartilhadaAsync(pessoaId, null, CancellationToken.None); // ClienteId null simula "criar novo cliente"

        result.Should().BeTrue();
    }
}
