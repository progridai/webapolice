using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.AdicionarItem;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public class PropostasPostgresTests(SeguroIntegrationTestFixture fixture) : IClassFixture<SeguroIntegrationTestFixture>
{
    [Fact]
    public async Task GravaPropostasNativasEItensComUuidPersistidoSemLegadoInventado()
    {
        var (command, codigo) = await PrepararAsync();
        var handler = CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria));
        var primeira = await handler.Handle(command, default);
        var segunda = await handler.Handle(command, default);
        Assert.NotEqual(primeira, segunda);
        var db = fixture.DbContext;
        var produto = new Produto { Nome = "Produto da Fixture", Ativo = true };
        var plano = new Plano { PublicId = Guid.NewGuid(), Nome = "Plano da Fixture", Ativo = true };
        db.AddRange(produto, plano);
        await db.SaveChangesAsync();
        var proposta = await db.Proposta.SingleAsync(p => p.PublicId == primeira);
        var permitido = new ApoliceProdutoModel { ApoliceId = proposta.ApoliceId!.Value, ProdutoId = produto.Id };
        db.Add(permitido); await db.SaveChangesAsync();
        db.Add(new ApolicePlanoModel { ApoliceProdutoId = permitido.Id, PlanoId = plano.Id }); await db.SaveChangesAsync();
        var itemHandler = new AdicionarPropostaItemHandler(new PropostasRepository(db), Operacao(new RegistradorAuditoria(fixture.Auditoria)));
        var item1 = await itemHandler.Handle(new AdicionarPropostaItemCommand { PropostaId = primeira, ProdutoId = produto.Id, PlanoId = plano.Id }, default);
        var item2 = await itemHandler.Handle(new AdicionarPropostaItemCommand { PropostaId = primeira, ProdutoId = produto.Id, PlanoId = plano.Id }, default);
        Assert.NotEqual(item1, item2);
        await using var leitura = LeituraNovaConexao();
        var propostas = await leitura.Proposta.AsNoTracking().Where(p => p.PublicId == primeira || p.PublicId == segunda).ToListAsync();
        Assert.Equal(2, propostas.Count); Assert.All(propostas, p => Assert.Null(p.LegadoId));
        Assert.All(propostas, p => Assert.NotEqual(p.ClienteId, p.PessoaId));
        var itens = await leitura.PropostaItems.AsNoTracking().Where(i => i.PropostaId == proposta.Id).ToListAsync();
        Assert.Equal(new[] { item1, item2 }.Order(), itens.Select(i => i.PublicId).Order());
        Assert.All(itens, i => Assert.Null(i.LegadoId));
        Assert.Equal(3, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == primeira.ToString()));
    }

    [Fact]
    public async Task FalhaDaAuditoriaRevertePropostaNaConexaoFisica()
    {
        var (command, codigo) = await PrepararAsync();
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha artificial da auditoria"));
        var handler = CriarHandler(codigo, falha.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        await using var leitura = LeituraNovaConexao();
        Assert.False(await leitura.Proposta.AnyAsync(p => p.ApoliceVida!.PublicId == command.ApoliceVidaId));
        falha.Verify(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private SeguroDbContext LeituraNovaConexao() => new(new DbContextOptionsBuilder<SeguroDbContext>()
        .UseNpgsql(fixture.ConnectionString).UseSnakeCaseNamingConvention().Options);
    private OperacaoPropostas Operacao(IRegistradorAuditoria registrador)
    {
        var contexto = new Mock<IContextoAuditoria>(); contexto.Setup(c => c.ObterUsuarioIdExterno()).Returns("fixture-proposta");
        return new(fixture.DbContext, new OperacaoAuditada(fixture.Auditoria, registrador, contexto.Object));
    }
    private CriarPropostaHandler CriarHandler(string codigo, IRegistradorAuditoria registrador) => new(
        new PropostasRepository(fixture.DbContext), new ClientesPropostas(fixture.Cadastro),
        Options.Create(new ConfiguracaoPropostas { StatusInicialCodigo = codigo }), Operacao(registrador));
    private async Task<(CriarPropostaCommand, string)> PrepararAsync()
    {
        // Dados exclusivamente artificiais; o código do status real do produto continua pendente.
        fixture.Cadastro.Pessoas.Add(new PessoaModel(1, "Pessoa Extra", null, null, false, null, null, null));
        var pessoa = new PessoaModel(1, "Pessoa da Fixture", null, null, false, null, null, null);
        fixture.Cadastro.Pessoas.Add(pessoa); await fixture.Cadastro.SaveChangesAsync();
        var cliente = new Cliente(pessoa.Id, 1); fixture.Cadastro.Clientes.Add(cliente); await fixture.Cadastro.SaveChangesAsync();
        var vinculo = new ClienteVinculoModel(); fixture.Cadastro.Vinculos.Add(vinculo);
        fixture.Cadastro.Entry(vinculo).Property(v => v.ClienteId).CurrentValue = cliente.Id;
        fixture.Cadastro.Entry(vinculo).Property(v => v.PessoaId).CurrentValue = pessoa.Id;
        fixture.Cadastro.Entry(vinculo).Property(v => v.Ativo).CurrentValue = true;
        fixture.Cadastro.Entry(vinculo).Property(v => v.CreatedAt).CurrentValue = DateTime.UtcNow;
        await fixture.Cadastro.SaveChangesAsync();
        var apolice = new ApoliceModel { PublicId = Guid.NewGuid(), Nome = "Apólice da Fixture", EstipulanteId = 1, SeguradoraId = 1, DataInicioVigencia = new DateOnly(2026, 1, 1), Ativo = true };
        var vida = new ApoliceVidaModel { PublicId = Guid.NewGuid(), Apolice = apolice, ClienteId = cliente.Id, ClienteVinculoId = vinculo.Id, Ativo = true };
        var status = new PropostaStatus { Id = (short)Math.Max(7, (await fixture.DbContext.PropostaStatuses.MaxAsync(s => (short?)s.Id) ?? 0) + 1), Codigo = "FIXTURE_" + Guid.NewGuid().ToString("N")[..20], Nome = "Status da Fixture" };
        fixture.DbContext.AddRange(apolice, vida, status); await fixture.DbContext.SaveChangesAsync();
        return (new CriarPropostaCommand { ClienteId = cliente.Id, ApoliceId = apolice.PublicId, ApoliceVidaId = vida.PublicId }, status.Codigo);
    }
}
