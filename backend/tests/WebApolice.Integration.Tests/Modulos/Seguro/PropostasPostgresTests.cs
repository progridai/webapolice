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

public partial class PropostasPostgresTests(SeguroIntegrationTestFixture fixture) : IClassFixture<SeguroIntegrationTestFixture>
{
    [Fact]
    public async Task GravaPropostasNativasEItensComUuidPersistidoSemLegadoInventado()
    {
        var (command, codigo) = await PrepararAsync();
        var handler = CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria));
        var primeira = await handler.Handle(command, default);
        await Assert.ThrowsAsync<WebApolice.SharedKernel.Application.Exceptions.ValidacaoException>(() => handler.Handle(command, default));
        var (outroCommand, outroCodigo) = await PrepararAsync();
        var segunda = await CriarHandler(outroCodigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(outroCommand, default);
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
        var itemHandler = new AdicionarPropostaItemHandler(new PropostasRepository(db, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)));
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
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), new ClientesPropostas(fixture.Cadastro),
        Options.Create(new ConfiguracaoPropostas { StatusInicialCodigo = codigo, StatusCanceladaCodigo = "CANCELADA", StatusDescartadaCodigo = "DESCARTADA" }), Operacao(registrador), SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext));
    private async Task<(CriarPropostaCommand, string)> PrepararAsync()
    {
        fixture.DbContext.ChangeTracker.Clear();
        // Dados exclusivamente artificiais; não promover propostas reais por esta fixture.
        fixture.Cadastro.Pessoas.Add(new PessoaModel(1, "Pessoa Extra", null, null, false, null, null, null));
        var pessoa = new PessoaModel(1, "Pessoa da Fixture", null, null, false, null, null, null);
        fixture.Cadastro.Pessoas.Add(pessoa); await fixture.Cadastro.SaveChangesAsync();
        var cliente = new Cliente(pessoa.Id, 1); fixture.Cadastro.Clientes.Add(cliente); await fixture.Cadastro.SaveChangesAsync();
        if (!await fixture.Cadastro.Estipulantes.AnyAsync(e => e.Id == 1))
        {
            fixture.Cadastro.Estipulantes.Add(new() { Id = 1, PublicId = Guid.NewGuid(), Nome = "Estipulante Propostas", Ativo = true });
            await fixture.Cadastro.SaveChangesAsync();
        }
        var vinculo = new ClienteVinculoModel(cliente.Id, pessoa.Id, 1, "MAT-Proposta"); fixture.Cadastro.Vinculos.Add(vinculo);
        fixture.Cadastro.Entry(vinculo).Property(v => v.ClienteId).CurrentValue = cliente.Id;
        fixture.Cadastro.Entry(vinculo).Property(v => v.PessoaId).CurrentValue = pessoa.Id;
        fixture.Cadastro.Entry(vinculo).Property(v => v.Ativo).CurrentValue = true;
        fixture.Cadastro.Entry(vinculo).Property(v => v.CreatedAt).CurrentValue = DateTime.UtcNow;
        await fixture.Cadastro.SaveChangesAsync();
        var apolice = new ApoliceModel { PublicId = Guid.NewGuid(), Nome = "Apólice da Fixture", EstipulanteId = 1, SeguradoraId = 1, DataInicioVigencia = new DateOnly(2026, 1, 1), Ativo = true };
        var vida = new ApoliceVidaModel { PublicId = Guid.NewGuid(), Apolice = apolice, ClienteId = cliente.Id, ClienteVinculoId = vinculo.Id, Ativo = true };
        var status = new PropostaStatus { Id = (short)Math.Max(7, (await fixture.DbContext.PropostaStatuses.MaxAsync(s => (short?)s.Id) ?? 0) + 1), Codigo = "FIXTURE_" + Guid.NewGuid().ToString("N")[..20], Nome = "Status da Fixture", VisivelOperacional = true };
        fixture.DbContext.AddRange(apolice, vida, status); await fixture.DbContext.SaveChangesAsync();
        return (new CriarPropostaCommand { ClienteId = cliente.Id, ApoliceId = apolice.PublicId, ApoliceVidaId = vida.PublicId }, status.Codigo);
    }

    private async Task<(CriarPropostaCommand Command, string Codigo, ApoliceModuloPlanoModel Plano)> PrepararModuloAsync()
    {
        var (command, codigo) = await PrepararAsync();
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == command.ApoliceVidaId);
        var global = new ModuloModel { PublicId = Guid.NewGuid(), Nome = "MÓDULO DA ADESÃO", Ativo = true };
        fixture.Cadastro.Add(global); await fixture.Cadastro.SaveChangesAsync();
        var modulo = new ApoliceModuloModel { PublicId = Guid.NewGuid(), ApoliceId = vida.ApoliceId, ModuloId = global.Id, Ativo = true };
        var subgrupo = new ApoliceSubgrupoModel { PublicId = Guid.NewGuid(), ApoliceId = vida.ApoliceId, Nome = "SUBGRUPO DA ADESÃO", Ativo = true };
        var plano = new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModulo = modulo, Nome = "PLANO DE PROTEÇÃO", Paga = true, Reajuste = false, Ativo = true };
        var cobertura = new Cobertura { PublicId = Guid.NewGuid(), Nome = "PROTEÇÃO DA FAMÍLIA", NomeReduzido = "FAMÍLIA", Basica = "S", Reajuste = true, Ativo = true };
        plano.Coberturas.Add(new() { PublicId = Guid.NewGuid(), Plano = plano, Cobertura = cobertura, PremioTitular = 12.34m, PremioConjuge = 0m, Ativo = true });
        plano.Coberturas.Add(new() { PublicId = Guid.NewGuid(), Plano = plano, Cobertura = new Cobertura { PublicId = Guid.NewGuid(), Nome = "Inativa", Ativo = true }, PremioTitular = 99m, PremioConjuge = 99m, Ativo = false });
        fixture.DbContext.AddRange(plano, subgrupo); await fixture.DbContext.SaveChangesAsync();
        vida.ApoliceModuloId = modulo.Id; vida.ApoliceSubgrupoId = subgrupo.Id;
        await fixture.DbContext.SaveChangesAsync();
        return (command, codigo, plano);
    }

    [Fact]
    public async Task RascunhoCopiaCondicoesContextuaisSemCalcularFinanceiroESemReescreverDepois()
    {
        var (command, codigo, plano) = await PrepararModuloAsync();
        var id = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(command, default);
        await using var leitura = LeituraNovaConexao();
        var proposta = await leitura.Proposta.Include(p => p.PropostaItems).Include(p => p.PropostaCoberturas).SingleAsync(p => p.PublicId == id);
        Assert.False(proposta.Vigente); Assert.Equal(1, proposta.Versao);
        Assert.Equal("Módulo da Adesão", proposta.ModuloNomeRegistrado);
        Assert.Equal("Subgrupo da Adesão", proposta.SubgrupoNomeRegistrado);
        var item = Assert.Single(proposta.PropostaItems);
        Assert.Equal(plano.Id, item.ApoliceModuloPlanoId); Assert.Null(item.PlanoId); Assert.Null(item.ProdutoId);
        Assert.Equal("Plano de Proteção", item.PlanoNomeRegistrado);
        var cobertura = Assert.Single(proposta.PropostaCoberturas);
        Assert.Null(cobertura.LegadoId); Assert.NotEqual(Guid.Empty, cobertura.PublicId);
        Assert.Equal("Proteção da Família", cobertura.CoberturaNomeRegistrado);
        Assert.Equal("S", cobertura.CoberturaBasicaRegistrada);
        Assert.Equal(12.34m, cobertura.PremioTitular); Assert.Equal(0m, cobertura.PremioConjuge);
        Assert.Null(proposta.PremioLiquido); Assert.Null(proposta.ValorParcela); Assert.Null(proposta.IofValor); Assert.Null(item.Valor);
        Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.PropostaId == proposta.Id));
        plano.Nome = "Plano Alterado";
        var atual = plano.Coberturas.Single(c => c.Ativo); atual.PremioTitular = 55m; atual.Cobertura.Nome = "Cadastro Alterado";
        await fixture.DbContext.SaveChangesAsync();
        var detalhe = await new PropostasRepository(leitura, new ClientesPropostas(fixture.Cadastro)).ObterDetalheAsync(id, default);
        Assert.Equal("Plano de Proteção", Assert.Single(detalhe!.Itens).PlanoNomeRegistrado);
        Assert.Equal("Proteção da Família", Assert.Single(detalhe.Coberturas).NomeRegistrado);
        Assert.Equal("Cadastro Alterado", detalhe.Coberturas.Single().NomeAtual);
        Assert.Equal(12.34m, detalhe.Coberturas.Single().PremioTitular);
        Assert.Equal(command.ClienteId, proposta.ClienteId);
        await Assert.ThrowsAsync<WebApolice.SharedKernel.Application.Exceptions.ValidacaoException>(() =>
            new AdicionarPropostaItemHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)))
                .Handle(new() { PropostaId = id, ProdutoId = 1, PlanoId = 1 }, default));
    }

    [Fact]
    public async Task NovaVersaoExigeOrigemDaMesmaVidaEPreservaAnteriorVigente()
    {
        var (command, codigo) = await PrepararAsync();
        var handler = CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria));
        var primeira = await handler.Handle(command, default);
        var anterior = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == primeira);
        var confirmadoFixture = new PropostaStatus { Id = (short)(anterior.StatusId + 1), Codigo = "CONFIRMADO_FIXTURE_" + Guid.NewGuid().ToString("N")[..8], Nome = "Confirmado da Fixture", VisivelOperacional = true };
        fixture.DbContext.Add(confirmadoFixture); await fixture.DbContext.SaveChangesAsync();
        anterior.StatusId = confirmadoFixture.Id; anterior.Vigente = true; await fixture.DbContext.SaveChangesAsync();
        await Assert.ThrowsAsync<WebApolice.SharedKernel.Application.Exceptions.ValidacaoException>(() => handler.Handle(command, default));
        command.PropostaOrigemPublicId = Guid.NewGuid();
        await Assert.ThrowsAsync<WebApolice.SharedKernel.Application.Exceptions.ValidacaoException>(() => handler.Handle(command, default));
        command.PropostaOrigemPublicId = primeira;
        var nova = await handler.Handle(command, default);
        await using var leitura = LeituraNovaConexao();
        var versao = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.Equal(2, versao.Versao); Assert.Equal(anterior.Id, versao.PropostaOrigemId); Assert.False(versao.Vigente);
        Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == primeira)).Vigente);
        Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.PropostaId == versao.Id));
        await Assert.ThrowsAsync<WebApolice.SharedKernel.Application.Exceptions.ValidacaoException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task FalhaDeAuditoriaReverteTambemItensECoberturasContextuais()
    {
        var (command, codigo, _) = await PrepararModuloAsync();
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha da fixture"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => CriarHandler(codigo, falha.Object).Handle(command, default));
        await using var leitura = LeituraNovaConexao();
        Assert.False(await leitura.Proposta.AnyAsync(p => p.ApoliceVida!.PublicId == command.ApoliceVidaId));
        Assert.False(await leitura.PropostaItems.AnyAsync(i => i.Proposta.ApoliceVida!.PublicId == command.ApoliceVidaId));
        Assert.False(await leitura.PropostaCoberturas.AnyAsync(c => c.Proposta.ApoliceVida!.PublicId == command.ApoliceVidaId));
    }

    [Fact]
    public async Task CriacoesConcorrentesDaMesmaVidaGeramSomenteUmRascunho()
    {
        var (command, codigo) = await PrepararAsync();
        async Task<bool> Criar()
        {
            await using var connection = new Npgsql.NpgsqlConnection(fixture.ConnectionString); await connection.OpenAsync();
            await using var db = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var cadastro = new WebApolice.Modulos.Cadastro.Infrastructure.Persistence.CadastroDbContext(new DbContextOptionsBuilder<WebApolice.Modulos.Cadastro.Infrastructure.Persistence.CadastroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var auditoria = new AuditoriaDbContext(new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            var clientes = new ClientesPropostas(cadastro);
            var handler = new CriarPropostaHandler(new PropostasRepository(db, clientes), clientes, Options.Create(new ConfiguracaoPropostas { StatusInicialCodigo = codigo, StatusCanceladaCodigo = "CANCELADA", StatusDescartadaCodigo = "DESCARTADA" }),
                new OperacaoPropostas(db, new OperacaoAuditada(auditoria, new RegistradorAuditoria(auditoria), Mock.Of<IContextoAuditoria>())),
                new CadastroSeguroQueries(cadastro, new WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries.ClienteVinculosUso(db)));
            try { await handler.Handle(command, default); return true; }
            catch (WebApolice.SharedKernel.Application.Exceptions.ValidacaoException) { return false; }
        }
        var resultados = await Task.WhenAll(Criar(), Criar());
        Assert.Single(resultados, r => r); Assert.Single(resultados, r => !r);
        await using var leitura = LeituraNovaConexao();
        Assert.Equal(1, await leitura.Proposta.CountAsync(p => p.ApoliceVida!.PublicId == command.ApoliceVidaId));
    }

    [Fact]
    public async Task SnapshotBloqueiaExclusaoMesmoDepoisDeTrocarContextoDaVida()
    {
        var (command, codigo, plano) = await PrepararModuloAsync();
        var id = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(command, default);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == command.ApoliceVidaId);
        vida.ApoliceModuloId = null; vida.ApoliceSubgrupoId = null;
        await fixture.DbContext.SaveChangesAsync();
        var cobertura = plano.Coberturas.Single(c => c.Ativo);
        await using var escrita = LeituraNovaConexao();
        var ex = await Assert.ThrowsAsync<Npgsql.PostgresException>(() => escrita.Database.ExecuteSqlInterpolatedAsync($"UPDATE seguro.apolice_modulo_cobertura SET deleted_at=now() WHERE id={cobertura.Id}"));
        Assert.Equal("ck_proposta_contexto", ex.ConstraintName);
        var exclusao = new ExclusaoVinculosApolice(fixture.DbContext, new OperacaoAuditada(fixture.Auditoria, new RegistradorAuditoria(fixture.Auditoria), Mock.Of<IContextoAuditoria>()));
        await Assert.ThrowsAsync<WebApolice.SharedKernel.Application.Exceptions.ValidacaoException>(() => exclusao.ExcluirAsync(command.ApoliceId,
            WebApolice.Modulos.Seguro.Application.Ports.TipoVinculoApolice.CoberturaModulo, cobertura.PublicId, Guid.NewGuid(), default));
        Assert.True(await escrita.Proposta.AnyAsync(p => p.PublicId == id));
        Assert.Null((await escrita.ApoliceModuloCoberturas.SingleAsync(c => c.Id == cobertura.Id)).DeletedAt);
    }

    [Fact]
    public async Task BancoRecusaItemContextualDeOutroModuloSemAfetarPropostas()
    {
        var (command, codigo, _) = await PrepararModuloAsync();
        var id = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(command, default);
        var (_, _, outroPlano) = await PrepararModuloAsync();
        await using var escrita = LeituraNovaConexao();
        var propostaId = await escrita.Proposta.Where(p => p.PublicId == id).Select(p => p.Id).SingleAsync();
        escrita.PropostaItems.Add(new PropostaItem { PropostaId = propostaId, ApoliceModuloPlanoId = outroPlano.Id, PlanoNomeRegistrado = "Incompatível" });
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => escrita.SaveChangesAsync());
        Assert.Equal("ck_proposta_contexto", Assert.IsType<Npgsql.PostgresException>(ex.InnerException).ConstraintName);
        await using var leitura = LeituraNovaConexao();
        Assert.Equal(1, await leitura.PropostaItems.CountAsync(i => i.PropostaId == propostaId));
    }
}
