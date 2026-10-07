using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Financeiro.Application.UseCases.ConveniosCobranca;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.Modulos.Financeiro.Infrastructure;
using WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarSubgrupo;
using WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.PlanoModulo;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

// Run through scripts/test-catalogos-isolado.ps1; never accepts the application database.
public sealed class CatalogosWorkflowTests : IAsyncLifetime
{
    private NpgsqlConnection _connection = null!;
    private FinanceiroDbContext _financeiro = null!;
    private SeguroDbContext _seguro = null!;
    private AuditoriaDbContext _audit = null!;
    private SegurancaDbContext _seguranca = null!;
    private OperacaoAuditada _operacao = null!;
    private ConveniosCobrancaRepository _convenios = null!;
    private CatalogosSeguroRepository _catalogos = null!;

    public async Task InitializeAsync()
    {
        var value = Environment.GetEnvironmentVariable("WEBAPOLICE_CATALOGOS_TEST_CONNECTION")
            ?? throw new InvalidOperationException("Execute scripts/test-catalogos-isolado.ps1.");
        var builder = new NpgsqlConnectionStringBuilder(value);
        if (!System.Text.RegularExpressions.Regex.IsMatch(builder.Database!, "^webapolice_catalogos_[a-f0-9]{32}$"))
            throw new InvalidOperationException("Testes exigem uma base temporária isolada.");
        _connection = new(value);
        await _connection.OpenAsync();
        _financeiro = new(new DbContextOptionsBuilder<FinanceiroDbContext>().UseNpgsql(_connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "financeiro")).UseSnakeCaseNamingConvention().Options);
        _seguro = new(new DbContextOptionsBuilder<SeguroDbContext>().UseNpgsql(_connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "seguro")).UseSnakeCaseNamingConvention().Options);
        _audit = new(new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(_connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria")).UseSnakeCaseNamingConvention().Options);
        _seguranca = new(new DbContextOptionsBuilder<SegurancaDbContext>().UseNpgsql(_connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "seguranca")).UseSnakeCaseNamingConvention().Options);
        Assert.Same(_connection, _financeiro.Database.GetDbConnection());
        Assert.Same(_connection, _seguro.Database.GetDbConnection());
        Assert.Same(_connection, _seguranca.Database.GetDbConnection());
        await _financeiro.Database.MigrateAsync();
        Guid? legadoPublicId = null;
        if ((await _seguro.Database.GetAppliedMigrationsAsync()).Contains("20261007010053_PlanoECoberturasExclusivosModuloApolice") &&
            (await _seguro.Database.GetPendingMigrationsAsync()).Contains("20261007012211_CompartilharCoberturaNoPlanoModulo"))
        {
            var apoliceLegada = await CriarApolice(); var moduloLegado = await CriarModulo(apoliceLegada);
            var planoLegado = new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModuloId = moduloLegado.Id, Nome = "Plano da migration", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
            _seguro.Add(planoLegado); await _seguro.SaveChangesAsync();
            legadoPublicId = Guid.NewGuid();
            await _seguro.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO seguro.apolice_modulo_cobertura(public_id,apolice_modulo_plano_id,nome,nome_reduzido,basica,reajuste,premio_titular,premio_conjuge,ativo) VALUES ({legadoPublicId.Value},{planoLegado.Id},'Cobertura migrada','CM','Básica',true,31.42,0,false)");
        }
        await _seguro.Database.MigrateAsync();
        if (legadoPublicId is Guid legado)
        {
            var migrada = await _seguro.ApoliceModuloCoberturas.Include(e => e.Cobertura).SingleAsync(e => e.PublicId == legado);
            Assert.Equal("Cobertura migrada", migrada.Cobertura.Nome); Assert.Equal("CM", migrada.Cobertura.NomeReduzido);
            Assert.Equal("Básica", migrada.Cobertura.Basica); Assert.True(migrada.Cobertura.Reajuste);
            Assert.Equal(31.42m, migrada.PremioTitular); Assert.Equal(0m, migrada.PremioConjuge); Assert.False(migrada.Ativo);
            Assert.NotEqual(Guid.Empty, migrada.Cobertura.PublicId); Assert.NotEqual(migrada.PublicId, migrada.Cobertura.PublicId);
            _seguro.ChangeTracker.Clear();
        }
        await _seguranca.Database.MigrateAsync();
        var context = new Mock<IContextoAuditoria>();
        context.Setup(e => e.ObterUsuarioIdExterno()).Returns("catalogos-test-user");
        _operacao = new(_audit, new RegistradorAuditoria(_audit), context.Object);
        _convenios = new(_financeiro, _operacao);
        _catalogos = new(_seguro, _operacao);
    }

    private Task<ConvenioCobrancaDto> CriarConvenio() => new ConveniosCobrancaHandler(_convenios).SalvarAsync(null,
        new() { Nome = "Convênio " + Guid.NewGuid(), Agencia = "001", ContaCorrente = "123-4", ComunicaVindi = false, NumeroArquivo = 3, EstUf = "RS" }, default);

    private async Task<ApoliceModel> CriarApolice()
    {
        var e = new ApoliceModel
        {
            PublicId = Guid.NewGuid(),
            Nome = "Apólice Teste",
            EstipulanteId = 1,
            SeguradoraId = 1,
            Ativo = true,
            Status = "ativa",
            DataInicioVigencia = new DateOnly(2026, 1, 1),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _seguro.Add(e); await _seguro.SaveChangesAsync(); return e;
    }

    [CatalogosFact]
    public async Task Convenio_CadastroCompleto_StatusEConsultaPublica()
    {
        await _financeiro.Database.ExecuteSqlRawAsync("INSERT INTO core.banco(codigo,nome,created_at) VALUES ('987','Banco de teste',now()) ON CONFLICT DO NOTHING");
        Assert.Contains(await _convenios.BancosAsync(default), b => b.Codigo == "987");
        var dados = new ConvenioCobrancaDados
        {
            Nome = "Completo " + Guid.NewGuid(),
            BancoCodigo = "987",
            Agencia = "001",
            ContaCorrente = "123-4",
            NomeEmpresa = "Empresa Teste",
            CodigoEmpresa = "empresa",
            NumeroArquivo = 3,
            NomeInicialArquivo = "REM",
            ExtensaoArquivo = "txt",
            LayoutArquivo = 1,
            LocalRemessaArquivo = "remessa",
            LocalRetornoArquivo = "retorno",
            ComunicaVindi = false,
            Observacao = "Teste",
            InscricaoEstadual = "123",
            EstEndereco = "Rua Teste",
            EstNumero = "100",
            EstBairro = "Centro",
            EstComplemento = "Sala",
            EstCep = "90000000",
            EstCidade = "Teste",
            EstUf = "RS",
            EstNome = "Estabelecimento"
        };
        var dto = await new ConveniosCobrancaHandler(_convenios).SalvarAsync(null, dados, default);
        Assert.NotEqual(Guid.Empty, dto.PublicId);
        var read = await _convenios.ConsultarAsync(dto.PublicId, default);
        Assert.Equal("123-4", read!.ContaCorrente); Assert.False(read.ComunicaVindi); Assert.Equal("RS", read.EstUf);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(dados), System.Text.Json.JsonSerializer.Serialize<ConvenioCobrancaDados>(read));
        Assert.Equal("987", (await _convenios.ListarAsync(1, 20, dados.Nome, true, default)).Items.Single().BancoCodigo);
        await Assert.ThrowsAsync<ValidacaoException>(() => _convenios.SalvarAsync(null, new() { Nome = "Banco inválido", BancoCodigo = "xxx" }, default));
        read.Nome = "Alterado " + dto.PublicId;
        await _convenios.SalvarAsync(dto.PublicId, read, default);
        await _convenios.AlterarStatusAsync(dto.PublicId, false, default);
        Assert.DoesNotContain(await _convenios.OpcoesAsync(default), e => e.PublicId == dto.PublicId);
        await _convenios.AlterarStatusAsync(dto.PublicId, true, default);
        Assert.Contains(await _convenios.OpcoesAsync(default), e => e.PublicId == dto.PublicId);
        Assert.True(await _audit.RegistrosAuditoria.AnyAsync(e => e.RecursoId == dto.PublicId.ToString() && e.UsuarioIdExterno == "catalogos-test-user"));
    }

    [CatalogosFact]
    public async Task Subgrupos_CompartilhamConvenio_MasPreservamIsolamentoDaApolice()
    {
        var convenio = await CriarConvenio(); var a = await CriarApolice(); var b = await CriarApolice();
        var repo = new SubgruposCadastroRepository(_seguro, _convenios, _operacao);
        var handler = new CriarSubgrupoApoliceHandler(repo);
        var primeiro = await handler.Handle(new() { ApolicePublicId = a.PublicId, Nome = "Matriz", ConvenioCobrancaPublicId = convenio.PublicId }, default);
        await handler.Handle(new() { ApolicePublicId = a.PublicId, Nome = "Filial", ConvenioCobrancaPublicId = convenio.PublicId }, default);
        var rows = await new ApolicesQueries(_seguro, _convenios).ListarSubgruposAsync(a.PublicId, default);
        Assert.Equal(2, rows.Count); Assert.All(rows, e => Assert.Equal(convenio.PublicId, e.ConvenioCobrancaPublicId));
        await Assert.ThrowsAsync<ValidacaoException>(() => repo.SalvarAsync(b.PublicId, primeiro, "Inválido", null, convenio.PublicId, default));
        Assert.Null(await new ApolicesQueries(_seguro, _convenios).ObterSubgrupoPorPublicIdAsync(b.PublicId, primeiro, default));
    }

    [CatalogosFact]
    public async Task Subgrupo_ExigeConvenioNovo_PreservaLegadoEReferenciaInativa()
    {
        var a = await CriarApolice(); var repo = new SubgruposCadastroRepository(_seguro, _convenios, _operacao);
        var handler = new CriarSubgrupoApoliceHandler(repo);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(new() { ApolicePublicId = a.PublicId, Nome = "Sem convênio" }, default));
        var legacy = new ApoliceSubgrupoModel { PublicId = Guid.NewGuid(), ApoliceId = a.Id, Nome = "Anterior", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        _seguro.Add(legacy); await _seguro.SaveChangesAsync();
        await repo.SalvarAsync(a.PublicId, legacy.PublicId, "Legado editado", null, null, default);
        var c = await CriarConvenio();
        await repo.SalvarAsync(a.PublicId, legacy.PublicId, "Vinculado", null, c.PublicId, default);
        await _convenios.AlterarStatusAsync(c.PublicId, false, default);
        await repo.SalvarAsync(a.PublicId, legacy.PublicId, "Preservado", null, c.PublicId, default);
        await Assert.ThrowsAsync<ValidacaoException>(() => repo.SalvarAsync(a.PublicId, legacy.PublicId, "Removido", null, null, default));
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(new() { ApolicePublicId = a.PublicId, Nome = "Inativo", ConvenioCobrancaPublicId = c.PublicId }, default));
    }

    [CatalogosFact]
    public async Task PlanoCobertura_CompartilhamentoUnicidadeEInativacaoDoVinculo()
    {
        var handler = new CatalogosSeguroHandler(_catalogos);
        var c = await handler.SalvarCoberturaAsync(null, new() { Nome = "Cobertura " + Guid.NewGuid(), Basica = "Básica", Reajuste = false }, default);
        var a = await handler.SalvarPlanoAsync(null, new() { Nome = "Plano A " + Guid.NewGuid() }, default);
        var b = await handler.SalvarPlanoAsync(null, new() { Nome = "Plano B " + Guid.NewGuid() }, default);
        await handler.SalvarVinculoAsync(a.PublicId, c.PublicId, new() { PremioTitular = 12.34m, PremioConjuge = 5.67m }, default);
        await handler.SalvarVinculoAsync(a.PublicId, c.PublicId, new() { PremioTitular = 12.34m, PremioConjuge = 5.67m }, default);
        await handler.SalvarVinculoAsync(b.PublicId, c.PublicId, new() { PremioTitular = 20m, PremioConjuge = 0m }, default);
        Assert.Single(await handler.CoberturasPlanoAsync(a.PublicId, default));
        Assert.Single(await handler.CoberturasPlanoAsync(b.PublicId, default));
        await handler.VincularAsync(a.PublicId, c.PublicId, false, default);
        Assert.False((await handler.CoberturasPlanoAsync(a.PublicId, default)).Single().Ativo);
        Assert.True((await handler.ObterCoberturaAsync(c.PublicId, default))!.Ativo);
        await handler.StatusAsync(c.PublicId, false, true, default);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.VincularAsync(a.PublicId, c.PublicId, true, default));
        Assert.False((await handler.CoberturasPlanoAsync(b.PublicId, default)).Single().CoberturaAtiva);
        await handler.StatusAsync(c.PublicId, true, true, default);
        await handler.VincularAsync(a.PublicId, c.PublicId, true, default);
    }

    [CatalogosFact]
    public async Task PremiosPlano_PersistemCentavosEValidamValoresSemAlterarVinculos()
    {
        var handler = new CatalogosSeguroHandler(_catalogos);
        var c = await handler.SalvarCoberturaAsync(null, new() { Nome = "Preço " + Guid.NewGuid() }, default);
        var p = await handler.SalvarPlanoAsync(null, new() { Nome = "Preço " + Guid.NewGuid() }, default);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarVinculoAsync(p.PublicId, c.PublicId, new(), default));
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarVinculoAsync(p.PublicId, c.PublicId, new() { PremioTitular = -1m, PremioConjuge = 0m }, default));
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarVinculoAsync(p.PublicId, c.PublicId, new() { PremioTitular = 1.234m, PremioConjuge = 0m }, default));
        await handler.SalvarVinculoAsync(p.PublicId, c.PublicId, new() { PremioTitular = 1234.56m, PremioConjuge = 0m }, default);
        var link = Assert.Single(await handler.CoberturasPlanoAsync(p.PublicId, default));
        Assert.Equal(1234.56m, link.PremioTitular); Assert.Equal(0m, link.PremioConjuge);
        await handler.VincularAsync(p.PublicId, c.PublicId, false, default);
        await handler.SalvarVinculoAsync(p.PublicId, c.PublicId, new() { PremioTitular = 10.01m, PremioConjuge = 2.02m, Ativo = false }, default);
        link = Assert.Single(await handler.CoberturasPlanoAsync(p.PublicId, default));
        Assert.False(link.Ativo); Assert.Equal(10.01m, link.PremioTitular);
        await handler.VincularAsync(p.PublicId, c.PublicId, true, default);
        link = Assert.Single(await handler.CoberturasPlanoAsync(p.PublicId, default));
        Assert.True(link.Ativo); Assert.Equal(2.02m, link.PremioConjuge);
        var audits = await _audit.RegistrosAuditoria.Where(e => e.RecursoId == p.PublicId.ToString() && e.Recurso == "plano_cobertura").ToListAsync();
        Assert.Contains(audits, e => e.DadosPosteriores!.RootElement.GetProperty("PremioTitular").GetDecimal() == 1234.56m);
    }

    [CatalogosFact]
    public async Task PremiosApolice_PadraoAjustesIndependentesZeroIsolamentoERollback()
    {
        var handler = new CatalogosSeguroHandler(_catalogos);
        var c = await handler.SalvarCoberturaAsync(null, new() { Nome = "Apólice " + Guid.NewGuid() }, default);
        var p = await handler.SalvarPlanoAsync(null, new() { Nome = "Apólice " + Guid.NewGuid() }, default);
        await handler.SalvarVinculoAsync(p.PublicId, c.PublicId, new() { PremioTitular = 10.20m, PremioConjuge = 3.40m }, default);
        var plano = await _seguro.Planos.SingleAsync(e => e.PublicId == p.PublicId);
        var cobertura = await _seguro.Coberturas.SingleAsync(e => e.PublicId == c.PublicId);
        var produto = new Produto { Nome = "Teste", PlanoId = plano.Id, Ativo = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _seguro.Add(produto); await _seguro.SaveChangesAsync();
        var a = await CriarApolice(); var b = await CriarApolice();
        async Task<ApoliceCoberturaModel> CriarContrato(ApoliceModel apolice)
        {
            var context = new ApoliceCoberturaModel
            {
                PublicId = Guid.NewGuid(),
                CoberturaId = cobertura.Id,
                Ativo = true,
                PremioOverride = 99m,
                ApolicePlano = new()
                {
                    PlanoId = plano.Id,
                    Ativo = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    ApoliceProduto = new() { ApoliceId = apolice.Id, ProdutoId = produto.Id, Ativo = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
                },
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _seguro.Add(context); await _seguro.SaveChangesAsync(); return context;
        }
        var ca = await CriarContrato(a); await CriarContrato(b);
        var query = new ApolicesQueries(_seguro, _convenios);
        async Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ApoliceCoberturaResult> Ler(Guid id) =>
            (await query.ObterUniversoPermitidoAsync(id, default))!.Produtos.Single().Planos.Single().Coberturas.Single();
        var read = await Ler(a.PublicId); Assert.Equal(ca.PublicId, read.PublicId); Assert.Equal(10.20m, read.PremioTitularEfetivo); Assert.Equal(3.40m, read.PremioConjugeEfetivo); Assert.Equal(99m, read.PremioOverride);
        var overrides = new PremiosApoliceHandler(new PremiosApoliceRepository(_seguro, _operacao));
        await overrides.AlterarAsync(a.PublicId, ca.PublicId, new() { PremioTitular = 20m, PremioConjuge = 0m }, default);
        read = await Ler(a.PublicId); Assert.Equal(20m, read.PremioTitularEfetivo); Assert.Equal(0m, read.PremioConjugeEfetivo);
        Assert.Equal(10.20m, (await Ler(b.PublicId)).PremioTitularEfetivo);
        await handler.SalvarVinculoAsync(p.PublicId, c.PublicId, new() { PremioTitular = 30m, PremioConjuge = 4m }, default);
        Assert.Equal(30m, (await Ler(b.PublicId)).PremioTitularEfetivo); Assert.Equal(20m, (await Ler(a.PublicId)).PremioTitularEfetivo);
        await overrides.AlterarAsync(a.PublicId, ca.PublicId, new() { PremioConjuge = 2m }, default);
        read = await Ler(a.PublicId); Assert.Equal(30m, read.PremioTitularEfetivo); Assert.Equal(2m, read.PremioConjugeEfetivo);
        await Assert.ThrowsAsync<ValidacaoException>(() => overrides.AlterarAsync(b.PublicId, ca.PublicId, new() { PremioTitular = 200m }, default));
        var failed = new Mock<IRegistradorAuditoria>();
        failed.Setup(e => e.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha simulada"));
        var failing = new PremiosApoliceRepository(_seguro, new(_audit, failed.Object, new Mock<IContextoAuditoria>().Object));
        await Assert.ThrowsAsync<InvalidOperationException>(() => failing.AlterarAsync(a.PublicId, ca.PublicId, new() { PremioTitular = 500m }, default));
        failed.Verify(e => e.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()), Times.Once);
        read = await Ler(a.PublicId); Assert.Equal(30m, read.PremioTitularEfetivo); Assert.Equal(2m, read.PremioConjugeEfetivo);
        await overrides.AlterarAsync(a.PublicId, ca.PublicId, new(), default);
        Assert.Equal(4m, (await Ler(a.PublicId)).PremioConjugeEfetivo);
    }

    private async Task<ApoliceModuloModel> CriarModulo(ApoliceModel apolice, long moduloId = 1)
    {
        var e = new ApoliceModuloModel { PublicId = Guid.NewGuid(), ApoliceId = apolice.Id, ModuloId = moduloId,
            Ativo = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        _seguro.Add(e); await _seguro.SaveChangesAsync(); return e;
    }

    [CatalogosFact]
    public async Task Modulo_PlanoUnicoECoberturaCompartilhadaComPremiosIndependentes()
    {
        var a = await CriarApolice(); var b = await CriarApolice();
        var ma = await CriarModulo(a); var mb = await CriarModulo(b); var ma2 = await CriarModulo(a, 2);
        var handler = new PlanoModuloApoliceHandler(new PlanoModuloApoliceRepository(_seguro, _operacao));
        var baseCobertura = await new CatalogosSeguroHandler(_catalogos).SalvarCoberturaAsync(null, new() { Nome = "Morte", NomeReduzido = "M", Basica = "Básica", Reajuste = true }, default);
        var globalPlanos = await _seguro.Planos.CountAsync(); var globalCoberturas = await _seguro.Coberturas.CountAsync();
        Assert.Null((await handler.ObterAsync(a.PublicId, ma.PublicId, default)).Plano);
        var p = await handler.SalvarPlanoAsync(a.PublicId, ma.PublicId, new() { Nome = " Plano A ", Ramo = "Vida", Paga = true, Reajuste = false }, default);
        var pb = await handler.SalvarPlanoAsync(b.PublicId, mb.PublicId, new() { Nome = "Plano A" }, default);
        Assert.NotEqual(p.PublicId, pb.PublicId);
        var c = await handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, null, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 12.34m, PremioConjuge = 0m }, default);
        var cb = await handler.SalvarCoberturaAsync(b.PublicId, mb.PublicId, null, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 30m, PremioConjuge = 5m }, default);
        Assert.NotEqual(c.PublicId, cb.PublicId); Assert.Equal(c.CoberturaPublicId, cb.CoberturaPublicId);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, null, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 5m, PremioConjuge = 0m }, default));
        Assert.Equal(30m, (await handler.ObterAsync(b.PublicId, mb.PublicId, default)).Plano!.Coberturas.Single().PremioTitular);
        await new CatalogosSeguroHandler(_catalogos).SalvarCoberturaAsync(baseCobertura.PublicId, new() { Nome = "Morte atualizada", NomeReduzido = "MA" }, default);
        Assert.Equal("Morte atualizada", (await handler.ObterAsync(a.PublicId, ma.PublicId, default)).Plano!.Coberturas.Single().Nome);
        Assert.Equal("Morte atualizada", (await handler.ObterAsync(b.PublicId, mb.PublicId, default)).Plano!.Coberturas.Single().Nome);
        var atualizado = await handler.SalvarPlanoAsync(a.PublicId, ma.PublicId, new() { Nome = "Plano atualizado", Paga = false }, default);
        Assert.Equal(p.PublicId, atualizado.PublicId); Assert.Single(atualizado.Coberturas);
        Assert.Equal(12.34m, atualizado.Coberturas.Single().PremioTitular); Assert.Equal(0m, atualizado.Coberturas.Single().PremioConjuge);
        Assert.Equal("Plano A", (await handler.ObterAsync(b.PublicId, mb.PublicId, default)).Plano!.Nome);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.ObterAsync(a.PublicId, mb.PublicId, default));
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarPlanoAsync(b.PublicId, ma.PublicId, new() { Nome = "Invasão" }, default));
        await handler.SalvarPlanoAsync(a.PublicId, ma2.PublicId, new() { Nome = "Segundo Módulo" }, default);
        var outroModulo = await handler.SalvarCoberturaAsync(a.PublicId, ma2.PublicId, null, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 99m, PremioConjuge = 3m }, default);
        Assert.Equal(c.CoberturaPublicId, outroModulo.CoberturaPublicId); Assert.NotEqual(c.PublicId, outroModulo.PublicId);
        Assert.Equal(12.34m, (await handler.ObterAsync(a.PublicId, ma.PublicId, default)).Plano!.Coberturas.Single().PremioTitular);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, ma2.PublicId, c.PublicId, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 1m, PremioConjuge = 1m }, default));
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(b.PublicId, mb.PublicId, c.PublicId, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 1m, PremioConjuge = 1m }, default));
        var dbPlano = await _seguro.ApoliceModuloPlanos.SingleAsync(e => e.PublicId == p.PublicId);
        _seguro.Add(new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModuloId = dbPlano.ApoliceModuloId, Nome = "Duplicado", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await Assert.ThrowsAsync<DbUpdateException>(() => _seguro.SaveChangesAsync()); _seguro.ChangeTracker.Clear();
        Assert.Equal(globalPlanos, await _seguro.Planos.CountAsync()); Assert.Equal(globalCoberturas, await _seguro.Coberturas.CountAsync());
        Assert.Contains(await _audit.RegistrosAuditoria.Where(e => e.RecursoId == c.PublicId.ToString()).ToListAsync(), e => e.DadosPosteriores!.RootElement.GetProperty("Dados").GetProperty("PremioTitular").GetDecimal() == 12.34m);
    }

    [CatalogosFact]
    public async Task Modulo_ValidacaoPremiosStatusEConsultaHistorica()
    {
        var a = await CriarApolice(); var m = await CriarModulo(a);
        var handler = new PlanoModuloApoliceHandler(new PlanoModuloApoliceRepository(_seguro, _operacao));
        var baseCobertura = await new CatalogosSeguroHandler(_catalogos).SalvarCoberturaAsync(null, new() { Nome = "Cobertura" }, default);
        CoberturaModuloDados dados = new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 1.25m, PremioConjuge = 0m };
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, null, dados, default));
        await handler.SalvarPlanoAsync(a.PublicId, m.PublicId, new() { Nome = "Plano" }, default);
        foreach (var valor in new decimal?[] { null, -1m, 1.234m, 10000000000000000m })
        {
            dados.PremioTitular = valor;
            await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, null, dados, default));
        }
        dados.PremioTitular = 1.25m;
        var c = await handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, null, dados, default);
        dados.Ativo = false;
        await handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, c.PublicId, dados, default);
        Assert.False((await handler.ObterAsync(a.PublicId, m.PublicId, default)).Plano!.Coberturas.Single().Ativo);
        await _catalogos.StatusAsync(baseCobertura.PublicId, false, true, default);
        Assert.DoesNotContain((await handler.OpcoesCoberturasAsync(a.PublicId, m.PublicId, 1, 100, default)).Items, e => e.PublicId == baseCobertura.PublicId);
        dados.Ativo = true;
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, c.PublicId, dados, default));
        await _catalogos.StatusAsync(baseCobertura.PublicId, true, true, default);
        await handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, c.PublicId, dados, default);
        await handler.SalvarPlanoAsync(a.PublicId, m.PublicId, new() { Nome = "Plano", Ativo = false }, default);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, c.PublicId, dados, default));
        await handler.SalvarPlanoAsync(a.PublicId, m.PublicId, new() { Nome = "Plano" }, default);
        var modulo = await _seguro.ApoliceModulos.SingleAsync(e => e.PublicId == m.PublicId);
        modulo.Ativo = false; await _seguro.SaveChangesAsync();
        Assert.False((await handler.ObterAsync(a.PublicId, m.PublicId, default)).PodeAlterar);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarPlanoAsync(a.PublicId, m.PublicId, new() { Nome = "Inativo" }, default));
        modulo = await _seguro.ApoliceModulos.SingleAsync(e => e.PublicId == m.PublicId); modulo.Ativo = true;
        var apolice = await _seguro.Apolices.SingleAsync(e => e.PublicId == a.PublicId); apolice.Ativo = false; await _seguro.SaveChangesAsync();
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, c.PublicId, dados, default));
        Assert.False((await handler.ObterAsync(a.PublicId, m.PublicId, default)).PodeAlterar);
        modulo = await _seguro.ApoliceModulos.SingleAsync(e => e.PublicId == m.PublicId); modulo.DeletedAt = DateTimeOffset.UtcNow; await _seguro.SaveChangesAsync();
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.ObterAsync(a.PublicId, m.PublicId, default));
    }

    [CatalogosFact]
    public async Task Modulo_VinculoNaoSubstituiCadastroENaoDuplicaMesmoInativo()
    {
        var a = await CriarApolice(); var b = await CriarApolice();
        var ma = await CriarModulo(a); var mb = await CriarModulo(b);
        var handler = new PlanoModuloApoliceHandler(new PlanoModuloApoliceRepository(_seguro, _operacao));
        await handler.SalvarPlanoAsync(a.PublicId, ma.PublicId, new() { Nome = "Plano A" }, default);
        await handler.SalvarPlanoAsync(b.PublicId, mb.PublicId, new() { Nome = "Plano B" }, default);
        var baseA = await _catalogos.SalvarCoberturaAsync(null, new() { Nome = "Compartilhada A" }, default);
        var baseB = await _catalogos.SalvarCoberturaAsync(null, new() { Nome = "Compartilhada B" }, default);
        var dados = new CoberturaModuloDados { CoberturaPublicId = baseA.PublicId, PremioTitular = 10m, PremioConjuge = 0m };
        var vinculo = await handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, null, dados, default);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, vinculo.PublicId,
            new() { CoberturaPublicId = baseB.PublicId, PremioTitular = 99m, PremioConjuge = 1m }, default));
        await _catalogos.StatusAsync(baseA.PublicId, false, true, default);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(b.PublicId, mb.PublicId, null, dados, default));
        dados.PremioTitular = 20m;
        await handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, vinculo.PublicId, dados, default);
        var leitura = (await handler.ObterAsync(a.PublicId, ma.PublicId, default)).Plano!.Coberturas.Single();
        Assert.True(leitura.Ativo); Assert.False(leitura.CoberturaAtiva); Assert.Equal(20m, leitura.PremioTitular);
        var opcoes = await handler.OpcoesCoberturasAsync(a.PublicId, ma.PublicId, 1, 100, default);
        Assert.DoesNotContain(opcoes.Items, e => e.PublicId == baseA.PublicId);
        Assert.Contains(opcoes.Items, e => e.PublicId == baseB.PublicId);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.OpcoesCoberturasAsync(b.PublicId, ma.PublicId, 1, 100, default));
        dados.Ativo = false;
        await handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, vinculo.PublicId, dados, default);
        await _catalogos.StatusAsync(baseA.PublicId, true, true, default);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, null, dados, default));
        var row = await _seguro.ApoliceModuloCoberturas.SingleAsync(e => e.PublicId == vinculo.PublicId);
        _seguro.Add(new ApoliceModuloCoberturaModel { PublicId = Guid.NewGuid(), ApoliceModuloPlanoId = row.ApoliceModuloPlanoId,
            CoberturaId = row.CoberturaId, PremioTitular = 1m, PremioConjuge = 0m, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await Assert.ThrowsAsync<DbUpdateException>(() => _seguro.SaveChangesAsync()); _seguro.ChangeTracker.Clear();
        dados.Ativo = true;
        await handler.SalvarCoberturaAsync(a.PublicId, ma.PublicId, vinculo.PublicId, dados, default);
        Assert.Equal(vinculo.PublicId, (await handler.ObterAsync(a.PublicId, ma.PublicId, default)).Plano!.Coberturas.Single().PublicId);
    }

    [CatalogosFact]
    public async Task Modulo_FalhaAuditoriaRevertePlanoECobertura()
    {
        var a = await CriarApolice(); var m = await CriarModulo(a);
        var baseCobertura = await new CatalogosSeguroHandler(_catalogos).SalvarCoberturaAsync(null, new() { Nome = "Cobertura" }, default);
        var failed = new Mock<IRegistradorAuditoria>();
        failed.Setup(e => e.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha simulada"));
        var failing = new PlanoModuloApoliceHandler(new PlanoModuloApoliceRepository(_seguro, new(_audit, failed.Object, new Mock<IContextoAuditoria>().Object)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => failing.SalvarPlanoAsync(a.PublicId, m.PublicId, new() { Nome = "Rollback" }, default));
        var handler = new PlanoModuloApoliceHandler(new PlanoModuloApoliceRepository(_seguro, _operacao));
        Assert.Null((await handler.ObterAsync(a.PublicId, m.PublicId, default)).Plano);
        await handler.SalvarPlanoAsync(a.PublicId, m.PublicId, new() { Nome = "Plano" }, default);
        await Assert.ThrowsAsync<InvalidOperationException>(() => failing.SalvarCoberturaAsync(a.PublicId, m.PublicId, null, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 1m, PremioConjuge = 0m }, default));
        Assert.Empty((await handler.ObterAsync(a.PublicId, m.PublicId, default)).Plano!.Coberturas);
        var c = await handler.SalvarCoberturaAsync(a.PublicId, m.PublicId, null, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 1m, PremioConjuge = 0m }, default);
        await Assert.ThrowsAsync<InvalidOperationException>(() => failing.SalvarCoberturaAsync(a.PublicId, m.PublicId, c.PublicId, new() { CoberturaPublicId = baseCobertura.PublicId, PremioTitular = 100m, PremioConjuge = 0m }, default));
        Assert.Equal(1m, (await handler.ObterAsync(a.PublicId, m.PublicId, default)).Plano!.Coberturas.Single().PremioTitular);
        await Assert.ThrowsAsync<InvalidOperationException>(() => failing.SalvarPlanoAsync(a.PublicId, m.PublicId, new() { Nome = "Alterado" }, default));
        Assert.Equal("Plano", (await handler.ObterAsync(a.PublicId, m.PublicId, default)).Plano!.Nome);
    }

    [CatalogosFact]
    public async Task AuditoriaFalha_ReverteCadastroInteiro()
    {
        var failed = new Mock<IRegistradorAuditoria>();
        failed.Setup(e => e.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha simulada"));
        var operation = new OperacaoAuditada(_audit, failed.Object, new Mock<IContextoAuditoria>().Object);
        var name = "Rollback " + Guid.NewGuid();
        var repo = new ConveniosCobrancaRepository(_financeiro, operation);
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.SalvarAsync(null, new() { Nome = name }, default));
        failed.Verify(e => e.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(await _financeiro.ConvenioCobrancas.AnyAsync(e => e.Nome == name));
    }

    [CatalogosTheory]
    [InlineData(false, false, true, true, false)]
    [InlineData(true, false, true, true, true)]
    [InlineData(false, true, true, true, true)]
    [InlineData(true, true, false, true, false)]
    [InlineData(true, true, true, false, false)]
    public async Task Permissoes_AcessoTotalNaoIgnoraModuloOuRecursoDesabilitado(bool permit, bool total, bool moduleOn, bool resourceOn, bool expected)
    {
        var resource = await _seguranca.Recursos.Include(e => e.Modulo).SingleAsync(e => e.Codigo == "CONVENIOS_COBRANCA");
        var oldModule = resource.Modulo.Habilitado; var oldResource = resource.Habilitado;
        if (moduleOn) resource.Modulo.Habilitar(); else resource.Modulo.Desabilitar(); if (resourceOn) resource.Habilitar(); else resource.Desabilitar(); await _seguranca.SaveChangesAsync();
        try
        {
            var user = new Mock<IContextoUsuarioAutenticado>(); user.SetupGet(e => e.EstaAutenticado).Returns(true); user.SetupGet(e => e.KeycloakSub).Returns("test");
            var permissions = new Mock<IPermissoesEfetivasService>();
            permissions.Setup(e => e.CalcularPermissoesAsync("test", It.IsAny<CancellationToken>())).ReturnsAsync(new PermissoesEfetivasUsuario(true, true, total, false, ["CADASTRO"], ["CONVENIOS_COBRANCA"], permit ? ["convenios_cobranca.visualizar"] : []));
            var request = new DefaultHttpContext(); request.Response.Body = new MemoryStream();
            var handler = new PermissaoAuthorizationHandler(user.Object, permissions.Object, new HttpContextAccessor { HttpContext = request }, _seguranca);
            var context = new AuthorizationHandlerContext([new PermissaoRequirement("convenios_cobranca.visualizar")], new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "test")], "test")), null);
            await handler.HandleAsync(context); Assert.Equal(expected, context.HasSucceeded);
        }
        finally { if (oldModule) resource.Modulo.Habilitar(); else resource.Modulo.Desabilitar(); if (oldResource) resource.Habilitar(); else resource.Desabilitar(); await _seguranca.SaveChangesAsync(); }
    }

    public async Task DisposeAsync()
    {
        await _financeiro.DisposeAsync(); await _seguro.DisposeAsync(); await _audit.DisposeAsync(); await _seguranca.DisposeAsync(); await _connection.DisposeAsync();
    }
}

public sealed class CatalogosFactAttribute : FactAttribute
{
    public CatalogosFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("WEBAPOLICE_CATALOGOS_TEST_CONNECTION") is null)
            Skip = "Execute scripts/test-catalogos-isolado.ps1 para preparar a base isolada.";
    }
}
public sealed class CatalogosTheoryAttribute : TheoryAttribute
{
    public CatalogosTheoryAttribute()
    {
        if (Environment.GetEnvironmentVariable("WEBAPOLICE_CATALOGOS_TEST_CONNECTION") is null)
            Skip = "Execute scripts/test-catalogos-isolado.ps1 para preparar a base isolada.";
    }
}
