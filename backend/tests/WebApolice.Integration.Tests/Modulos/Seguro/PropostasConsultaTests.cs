using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class PropostasConsultaTests(SeguroIntegrationTestFixture fixture) : IClassFixture<SeguroIntegrationTestFixture>
{
    private PropostasRepository Repository => new(fixture.DbContext, new ClientesPropostas(fixture.Cadastro));

    [Fact]
    public async Task FiltraPorVidaEApoliceSemMisturarParticipacoesDoMesmoCliente()
    {
        var (apolice, vida, proposta) = await PrepararAsync();
        var outraVida = new ApoliceVidaModel { PublicId = Guid.NewGuid(), Apolice = apolice, ClienteId = vida.ClienteId, ClienteVinculoId = vida.ClienteVinculoId, Ativo = true };
        fixture.DbContext.Add(outraVida);
        await fixture.DbContext.SaveChangesAsync();
        var outra = NovaProposta(apolice, outraVida, proposta.StatusId, proposta.PessoaId);
        fixture.DbContext.Add(outra);
        await fixture.DbContext.SaveChangesAsync();
        outraVida.Ativo = false;
        await fixture.DbContext.SaveChangesAsync();
        var result = await Repository.ListarAsync(new() { ApolicePublicId = apolice.PublicId, ApoliceVidaPublicId = vida.PublicId }, default);
        Assert.Equal(proposta.PublicId, Assert.Single(result.Items).PublicId);
        var encerrada = await Repository.ListarAsync(new() { ApoliceVidaPublicId = outraVida.PublicId }, default);
        Assert.Equal(outra.PublicId, Assert.Single(encerrada.Items).PublicId);
    }

    [Fact]
    public async Task RecusaCombinacaoDeOutraApoliceEUuidsVazios()
    {
        var (_, vida, _) = await PrepararAsync();
        await Assert.ThrowsAsync<ValidacaoException>(() => Repository.ListarAsync(new() { ApolicePublicId = Guid.NewGuid(), ApoliceVidaPublicId = vida.PublicId }, default));
        await Assert.ThrowsAsync<ValidacaoException>(() => Repository.ListarAsync(new() { ApoliceVidaPublicId = Guid.NewGuid() }, default));
        await Assert.ThrowsAsync<ValidacaoException>(() => Repository.ListarAsync(new() { ApolicePublicId = Guid.Empty }, default));
    }

    [Fact]
    public async Task ListaEDetalheOcultamExcluidasEInvisiveisMasPreservamNaoVigentes()
    {
        var (apolice, vida, proposta) = await PrepararAsync();
        var excluida = NovaProposta(apolice, vida, proposta.StatusId, proposta.PessoaId); excluida.DeletedAt = DateTime.UtcNow;
        var invisivel = NovaProposta(apolice, vida, proposta.StatusId, proposta.PessoaId); invisivel.VisivelOperacional = false;
        fixture.DbContext.AddRange(excluida, invisivel);
        await fixture.DbContext.SaveChangesAsync();
        var resultado = await Repository.ListarAsync(new() { ApoliceVidaPublicId = vida.PublicId }, default);
        Assert.Equal(proposta.PublicId, Assert.Single(resultado.Items).PublicId);
        Assert.False(resultado.Items.Single().Vigente);
        Assert.Null(await Repository.ObterDetalheAsync(excluida.PublicId, default));
        Assert.Null(await Repository.ObterDetalheAsync(invisivel.PublicId, default));
        Assert.Null(await Repository.ObterDetalheAsync(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task PaginacaoTemOrdemEstavelEBuscaNaoConfundeNumeroComNome()
    {
        var (apolice, vida, proposta) = await PrepararAsync();
        proposta.Numero = "aBc-X001";
        var segunda = NovaProposta(apolice, vida, proposta.StatusId, proposta.PessoaId); segunda.Numero = "aBc-X002";
        segunda.CreatedAt = proposta.CreatedAt;
        fixture.DbContext.Add(segunda); await fixture.DbContext.SaveChangesAsync();
        var primeiraPagina = await Repository.ListarAsync(new() { ApoliceVidaPublicId = vida.PublicId, TamanhoPagina = 1 }, default);
        var segundaPagina = await Repository.ListarAsync(new() { ApoliceVidaPublicId = vida.PublicId, TamanhoPagina = 1, Pagina = 2 }, default);
        Assert.Equal(2, primeiraPagina.TotalCount);
        Assert.Equal(segunda.PublicId, Assert.Single(primeiraPagina.Items).PublicId);
        Assert.Equal(proposta.PublicId, Assert.Single(segundaPagina.Items).PublicId);
        var busca = await Repository.ListarAsync(new() { TermoBusca = "aBc-X001", ApoliceVidaPublicId = vida.PublicId }, default);
        Assert.Equal("aBc-X001", Assert.Single(busca.Items).Numero);
    }

    [Fact]
    public async Task DetalheUsaValoresRegistradosSemHerdarConfiguracaoAtualENaoRetornaDocumentos()
    {
        var (_, _, proposta) = await PrepararAsync();
        var cobertura = new Cobertura { PublicId = Guid.NewGuid(), Nome = "Cadastro Atual", Ativo = true };
        var plano = new Plano { PublicId = Guid.NewGuid(), Nome = "Plano Atual", Ativo = true };
        var item = new PropostaItem { Proposta = proposta, Plano = plano, PlanoNomeLegado = "EVIDÊNCIA ORIGINAL", Valor = 12.34m };
        fixture.DbContext.AddRange(cobertura, item);
        await fixture.DbContext.SaveChangesAsync();
        fixture.DbContext.Add(new PropostaCobertura { Proposta = proposta, PropostaItem = item, Cobertura = cobertura, LegadoId = 9821, PremioTitular = 10.23m, PremioConjuge = 4.56m, CoberturaNomeLegado = "NOME ORIGINAL", Ativo = false });
        fixture.DbContext.Add(new PropostaBeneficiario { Proposta = proposta, LegadoId = 9821, Nome = "Maria da Silva", CpfOriginal = "12345678901", ParentescoOriginal = "FILHA", PercentualParticipacao = 100m });
        fixture.DbContext.Add(new PropostaBeneficiario { Proposta = proposta, LegadoId = 9822, Nome = "Excluído", DeletedAt = DateTime.UtcNow });
        await fixture.DbContext.SaveChangesAsync();
        var dto = await Repository.ObterDetalheAsync(proposta.PublicId, default);
        Assert.NotNull(dto);
        Assert.Equal(12.34m, Assert.Single(dto.Itens).Valor);
        Assert.Equal("EVIDÊNCIA ORIGINAL", dto.Itens.Single().PlanoNomeRegistrado);
        Assert.Equal("Plano Atual", dto.Itens.Single().PlanoNomeAtual);
        Assert.Equal(10.23m, Assert.Single(dto.Coberturas).PremioTitular);
        Assert.Equal(item.PublicId, dto.Coberturas.Single().ItemPublicId);
        Assert.False(dto.Coberturas.Single().Ativo);
        Assert.Equal("FILHA", Assert.Single(dto.Beneficiarios).Parentesco);
        var json = System.Text.Json.JsonSerializer.Serialize(dto);
        Assert.DoesNotContain("12345678901", json);
        Assert.DoesNotContain("ClienteId", json);
        Assert.DoesNotContain("LegadoId", json);
        Assert.DoesNotContain("BancoAgencia", json);
    }

    [Fact]
    public async Task HistoricoPreservaEventosENaoOfereceLinkDePropostaOculta()
    {
        var (apolice, vida, anterior) = await PrepararAsync();
        var nova = NovaProposta(apolice, vida, anterior.StatusId, anterior.PessoaId);
        nova.Versao = 2; nova.PropostaOrigem = anterior;
        fixture.DbContext.Add(nova); await fixture.DbContext.SaveChangesAsync();
        fixture.DbContext.Add(new PropostaHistorico { PropostaAnterior = anterior, PropostaNova = nova,
            Motivo = "PREPARAÇÃO DE NOVA VERSÃO", Observacao = "AÇÃO DA FAMÍLIA", DataAlteracao = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
        await fixture.DbContext.SaveChangesAsync();
        var detalhe = await Repository.ObterDetalheAsync(anterior.PublicId, default);
        var evento = Assert.Single(detalhe!.Historico);
        Assert.Equal(anterior.PublicId, evento.PropostaAnteriorPublicId); Assert.Equal(nova.PublicId, evento.PropostaNovaPublicId);
        Assert.Equal(2, evento.VersaoNova); Assert.Equal("Preparação de Nova Versão", evento.Motivo);
        Assert.Equal("Ação da Família", evento.Observacao);
        nova.VisivelOperacional = false; await fixture.DbContext.SaveChangesAsync();
        detalhe = await Repository.ObterDetalheAsync(anterior.PublicId, default);
        Assert.Null(Assert.Single(detalhe!.Historico).PropostaNovaPublicId);
        Assert.Null(await Repository.ObterDetalheAsync(nova.PublicId, default));
    }

    [Fact]
    public async Task CancelamentoChegaAoPostgres()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Repository.ListarAsync(new(), cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Repository.ObterDetalheAsync(Guid.NewGuid(), cancellation.Token));
    }

    private static Propostum NovaProposta(ApoliceModel apolice, ApoliceVidaModel vida, short status, long pessoa = 0) => new()
    {
        PublicId = Guid.NewGuid(), Apolice = apolice, ApoliceVida = vida, StatusId = status,
        ClienteId = vida.ClienteId, ClienteVinculoId = vida.ClienteVinculoId!.Value, PessoaId = pessoa, EstipulanteId = apolice.EstipulanteId,
        VisivelOperacional = true, Vigente = false, Versao = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    private async Task<(ApoliceModel, ApoliceVidaModel, Propostum)> PrepararAsync()
    {
        fixture.DbContext.ChangeTracker.Clear();
        var pessoa = new PessoaModel(1, "Cliente da Consulta", null, null, false, null, null, null);
        fixture.Cadastro.Pessoas.Add(pessoa); await fixture.Cadastro.SaveChangesAsync();
        var cliente = new Cliente(pessoa.Id, 1); fixture.Cadastro.Clientes.Add(cliente); await fixture.Cadastro.SaveChangesAsync();
        if (!await fixture.Cadastro.Estipulantes.AnyAsync(e => e.Id == 1))
        {
            fixture.Cadastro.Estipulantes.Add(new() { Id = 1, PublicId = Guid.NewGuid(), Nome = "Estipulante da Consulta", Ativo = true });
            await fixture.Cadastro.SaveChangesAsync();
        }
        var vinculo = new ClienteVinculoModel(cliente.Id, pessoa.Id, 1, "aBc-Consulta");
        fixture.Cadastro.Vinculos.Add(vinculo); await fixture.Cadastro.SaveChangesAsync();
        var status = new PropostaStatus { Id = (short)(100 + await fixture.DbContext.PropostaStatuses.CountAsync()), Codigo = "CONSULTA_" + Guid.NewGuid().ToString("N")[..10], Nome = "Situação de Teste" };
        var apolice = new ApoliceModel { PublicId = Guid.NewGuid(), Nome = "Apólice da Consulta", EstipulanteId = 1, SeguradoraId = 1, Ativo = true, DataInicioVigencia = new(2026, 1, 1) };
        var vida = new ApoliceVidaModel { PublicId = Guid.NewGuid(), Apolice = apolice, ClienteId = cliente.Id, ClienteVinculoId = vinculo.Id, Ativo = true };
        var proposta = NovaProposta(apolice, vida, status.Id);
        proposta.PessoaId = pessoa.Id;
        fixture.DbContext.AddRange(status, proposta);
        await fixture.DbContext.SaveChangesAsync();
        return (apolice, vida, proposta);
    }
}
