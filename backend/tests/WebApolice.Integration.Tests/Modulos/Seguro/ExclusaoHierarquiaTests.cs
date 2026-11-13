using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Npgsql;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public sealed class ExclusaoHierarquiaTests(SeguroIntegrationTestFixture fixture) : IClassFixture<SeguroIntegrationTestFixture>
{
    public static TheoryData<TipoVinculoApolice> Tipos => new(Enum.GetValues<TipoVinculoApolice>());
    private sealed record Cenario(ApoliceModel Apolice, object Registro, Guid PublicId, ApoliceModuloModel? Modulo = null,
        ApoliceProdutoModel? Produto = null, ApolicePlanoModel? Plano = null);
    private ExclusaoVinculosApolice Exclusao(IRegistradorAuditoria? registrador = null) => new(fixture.DbContext,
        new OperacaoAuditada(fixture.Auditoria, registrador ?? new RegistradorAuditoria(fixture.Auditoria), Mock.Of<IContextoAuditoria>()));
    private async Task<T> Salvar<T>(T entity) where T : class
    {
        fixture.DbContext.Add(entity); await fixture.DbContext.SaveChangesAsync(); return entity;
    }
    private async Task<Cenario> Preparar(TipoVinculoApolice tipo)
    {
        var estipulante = new EstipulanteModel { PublicId = Guid.NewGuid(), Nome = "Empresa da Hierarquia", Ativo = true };
        fixture.Cadastro.Add(estipulante); await fixture.Cadastro.SaveChangesAsync();
        var a = await Salvar(new ApoliceModel { PublicId = Guid.NewGuid(), Nome = "Apólice da Hierarquia", EstipulanteId = estipulante.Id,
            SeguradoraId = 1, Ativo = true, DataInicioVigencia = new(2026, 1, 1) });
        object registro;
        ApoliceModuloModel? modulo = null; ApoliceProdutoModel? produto = null; ApolicePlanoModel? plano = null;
        if (tipo == TipoVinculoApolice.Ramo)
        {
            var ramo = await Salvar(new RamoModel { PublicId = Guid.NewGuid(), Codigo = Guid.NewGuid().ToString(), Nome = "Ramo", Ativo = true });
            registro = await Salvar(new ApoliceRamoModel { ApoliceId = a.Id, RamoId = ramo.Id, NumeroApolice = "aBc-009", Ativo = true });
        }
        else if (tipo == TipoVinculoApolice.Subgrupo)
            registro = await Salvar(new ApoliceSubgrupoModel { PublicId = Guid.NewGuid(), ApoliceId = a.Id, Nome = "Subgrupo", Ativo = true });
        else if (tipo == TipoVinculoApolice.Subestipulante)
            registro = await Salvar(new ApoliceSubestipulanteModel { ApoliceId = a.Id, SubestipulanteId = a.Id + 10000, Ativo = true });
        else if (tipo is TipoVinculoApolice.Modulo or TipoVinculoApolice.PlanoModulo or TipoVinculoApolice.CoberturaModulo)
        {
            var global = new ModuloModel { PublicId = Guid.NewGuid(), Nome = "Módulo", Ativo = true };
            fixture.Cadastro.Add(global); await fixture.Cadastro.SaveChangesAsync();
            modulo = await Salvar(new ApoliceModuloModel { PublicId = Guid.NewGuid(), ApoliceId = a.Id, ModuloId = global.Id, Ativo = true });
            registro = modulo;
            if (tipo != TipoVinculoApolice.Modulo)
            {
                var p = await Salvar(new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModuloId = modulo.Id, Nome = "Plano", Ativo = true });
                registro = p;
                if (tipo == TipoVinculoApolice.CoberturaModulo)
                {
                    var globalCobertura = await Salvar(new Cobertura { Nome = "Cobertura", Ativo = true });
                    registro = await Salvar(new ApoliceModuloCoberturaModel { PublicId = Guid.NewGuid(), ApoliceModuloPlanoId = p.Id,
                        CoberturaId = globalCobertura.Id, PremioTitular = 3.21m, PremioConjuge = 0m, Ativo = true });
                }
            }
        }
        else
        {
            var globalProduto = await Salvar(new Produto { Nome = "Produto", Ativo = true });
            produto = await Salvar(new ApoliceProdutoModel { ApoliceId = a.Id, ProdutoId = globalProduto.Id, Ativo = true });
            registro = produto;
            if (tipo != TipoVinculoApolice.ProdutoAnterior)
            {
                var globalPlano = await Salvar(new Plano { PublicId = Guid.NewGuid(), Nome = "Plano Anterior", Ativo = true });
                plano = await Salvar(new ApolicePlanoModel { ApoliceProdutoId = produto.Id, PlanoId = globalPlano.Id, Ativo = true });
                registro = plano;
                if (tipo == TipoVinculoApolice.CoberturaAnterior)
                {
                    var globalCobertura = await Salvar(new Cobertura { Nome = "Cobertura Anterior", Ativo = true });
                    registro = await Salvar(new ApoliceCoberturaModel { ApolicePlanoId = plano.Id, CoberturaId = globalCobertura.Id,
                        PremioTitularOverride = 7.43m, PremioConjugeOverride = 0m, Ativo = true });
                }
            }
        }
        return new(a, registro, (Guid)fixture.DbContext.Entry(registro).Property("PublicId").CurrentValue!, modulo, produto, plano);
    }
    private async Task Remover(Cenario c, TipoVinculoApolice tipo) =>
        await Exclusao().ExcluirAsync(c.Apolice.PublicId, tipo, c.PublicId, Guid.NewGuid(), default);
    private async Task<DateTimeOffset?> ExclusaoPersistida(Cenario c)
    {
        await using var leitura = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(
            new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(fixture.ConnectionString).UseSnakeCaseNamingConvention().Options);
        var valores = await leitura.Entry(c.Registro).GetDatabaseValuesAsync();
        Assert.NotNull(valores);
        return valores.GetValue<DateTimeOffset?>("DeletedAt");
    }

    [Theory, MemberData(nameof(Tipos))]
    public async Task ExcluiSemDependenciasMesmoInativoPreservaFisicoEAudita(TipoVinculoApolice tipo)
    {
        var c = await Preparar(tipo);
        Assert.NotEqual(Guid.Empty, c.PublicId);
        fixture.DbContext.Entry(c.Registro).Property("Ativo").CurrentValue = false;
        await fixture.DbContext.SaveChangesAsync();
        await Remover(c, tipo);
        Assert.NotNull(await ExclusaoPersistida(c));
        Assert.False((bool)fixture.DbContext.Entry(c.Registro).Property("Ativo").CurrentValue!);
        Assert.True(await fixture.Auditoria.RegistrosAuditoria.AnyAsync(a => a.RecursoId == c.PublicId.ToString() && a.Acao == "VINCULO_APOLICE_EXCLUIDO"));
        Assert.True(await fixture.DbContext.ApoliceHistoricos.AnyAsync(h => h.ApoliceId == c.Apolice.Id && h.Acao == "Exclusão de Vínculo"));
        await Assert.ThrowsAsync<ValidacaoException>(() => Remover(c, tipo));
        Assert.Null(c.Apolice.DeletedAt);
    }

    [Theory, MemberData(nameof(Tipos))]
    public async Task ImpedeOutraApoliceESemConcessaoDeIdentidadeGlobal(TipoVinculoApolice tipo)
    {
        var c = await Preparar(tipo); var outra = await Preparar(TipoVinculoApolice.Subgrupo);
        await Assert.ThrowsAsync<ValidacaoException>(() => Exclusao().ExcluirAsync(outra.Apolice.PublicId, tipo, c.PublicId, Guid.NewGuid(), default));
        Assert.Null(await ExclusaoPersistida(c));
    }

    [Theory, MemberData(nameof(Tipos))]
    public async Task FalhaAuditoriaReverteRegistroEHistorico(TipoVinculoApolice tipo)
    {
        var c = await Preparar(tipo);
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha da auditoria"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Exclusao(falha.Object).ExcluirAsync(c.Apolice.PublicId, tipo, c.PublicId, Guid.NewGuid(), default));
        Assert.Null(await ExclusaoPersistida(c));
        Assert.False(await fixture.DbContext.ApoliceHistoricos.AnyAsync(h => h.ApoliceId == c.Apolice.Id && h.Acao == "Exclusão de Vínculo"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HierarquiaDeModuloExigeExcluirCoberturasPlanoEModuloNessaOrdem(bool inativos)
    {
        var c = await Preparar(TipoVinculoApolice.CoberturaModulo);
        var cobertura = (ApoliceModuloCoberturaModel)c.Registro;
        var plano = await fixture.DbContext.ApoliceModuloPlanos.SingleAsync(p => p.Id == cobertura.ApoliceModuloPlanoId);
        if (inativos) { cobertura.Ativo = false; plano.Ativo = false; await fixture.DbContext.SaveChangesAsync(); }
        var p = c with { Registro = plano, PublicId = plano.PublicId };
        var m = c with { Registro = c.Modulo!, PublicId = c.Modulo!.PublicId };
        await Assert.ThrowsAsync<ValidacaoException>(() => Remover(p, TipoVinculoApolice.PlanoModulo));
        await Assert.ThrowsAsync<ValidacaoException>(() => Remover(m, TipoVinculoApolice.Modulo));
        await Remover(c, TipoVinculoApolice.CoberturaModulo);
        Assert.Null(await ExclusaoPersistida(p)); Assert.Null(await ExclusaoPersistida(m));
        await Remover(p, TipoVinculoApolice.PlanoModulo);
        // Um novo Plano pode ser cadastrado sem reutilizar o UUID removido.
        var novo = await Salvar(new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModuloId = m.Modulo!.Id, Nome = "Novo Plano" });
        await Assert.ThrowsAsync<ValidacaoException>(() => Remover(m, TipoVinculoApolice.Modulo));
        await Remover(c with { Registro = novo, PublicId = novo.PublicId }, TipoVinculoApolice.PlanoModulo);
        await Remover(m, TipoVinculoApolice.Modulo);
        Assert.True(await fixture.Cadastro.Modulos.AnyAsync(g => g.Id == m.Modulo.ModuloId));
        Assert.True(await fixture.DbContext.Coberturas.AnyAsync(g => g.Id == cobertura.CoberturaId));
    }

    [Fact]
    public async Task UniversoAnteriorTambemExigeExcluirFilhosSemApagarCadastrosGlobais()
    {
        var c = await Preparar(TipoVinculoApolice.CoberturaAnterior);
        var cobertura = (ApoliceCoberturaModel)c.Registro;
        var p = c with { Registro = c.Plano!, PublicId = c.Plano!.PublicId };
        var produto = c with { Registro = c.Produto!, PublicId = c.Produto!.PublicId };
        await Assert.ThrowsAsync<ValidacaoException>(() => Remover(p, TipoVinculoApolice.PlanoAnterior));
        await Assert.ThrowsAsync<ValidacaoException>(() => Remover(produto, TipoVinculoApolice.ProdutoAnterior));
        await Remover(c, TipoVinculoApolice.CoberturaAnterior);
        await Remover(p, TipoVinculoApolice.PlanoAnterior);
        await Remover(produto, TipoVinculoApolice.ProdutoAnterior);
        Assert.True(await fixture.DbContext.Produtos.AnyAsync(e => e.Id == c.Produto!.ProdutoId));
        Assert.True(await fixture.DbContext.Planos.AnyAsync(e => e.Id == c.Plano!.PlanoId));
        Assert.True(await fixture.DbContext.Coberturas.AnyAsync(e => e.Id == cobertura.CoberturaId));
    }

    [Theory, MemberData(nameof(Tipos))]
    public async Task PropostasHistoricasContinuamBloqueandoTodosOsNiveis(TipoVinculoApolice tipo)
    {
        var c = await Preparar(tipo);
        var pessoa = new PessoaModel(1, "Pessoa da Proposta", null, null, false, null, null, null);
        fixture.Cadastro.Add(pessoa); await fixture.Cadastro.SaveChangesAsync();
        var cliente = new Cliente(pessoa.Id, 1); fixture.Cadastro.Add(cliente); await fixture.Cadastro.SaveChangesAsync();
        var vinculo = new ClienteVinculoModel(cliente.Id, pessoa.Id, c.Apolice.EstipulanteId, "H-009");
        fixture.Cadastro.Add(vinculo); await fixture.Cadastro.SaveChangesAsync();
        var vida = await Salvar(new ApoliceVidaModel { PublicId = Guid.NewGuid(), ApoliceId = c.Apolice.Id, ClienteId = cliente.Id,
            ClienteVinculoId = vinculo.Id, Ativo = true, ApoliceModuloId = c.Modulo?.Id,
            ApoliceSubgrupoId = c.Registro is ApoliceSubgrupoModel s ? s.Id : null });
        var status = await Salvar(new PropostaStatus { Id = (short)((await fixture.DbContext.PropostaStatuses.MaxAsync(s => (short?)s.Id) ?? 0) + 1), Codigo = Guid.NewGuid().ToString("N")[..20], Nome = "Histórica" });
        var proposta = await Salvar(new Propostum { PublicId = Guid.NewGuid(), ApoliceId = c.Apolice.Id, ApoliceVidaId = vida.Id,
            PessoaId = pessoa.Id, ClienteId = cliente.Id, ClienteVinculoId = vinculo.Id, EstipulanteId = c.Apolice.EstipulanteId,
            StatusId = status.Id, SubestipulanteId = c.Registro is ApoliceSubestipulanteModel se ? se.SubestipulanteId : null });
        if (tipo is TipoVinculoApolice.ProdutoAnterior or TipoVinculoApolice.PlanoAnterior)
            await Salvar(new PropostaItem { PropostaId = proposta.Id, ProdutoId = c.Produto?.ProdutoId, PlanoId = c.Plano?.PlanoId });
        if (c.Registro is ApoliceCoberturaModel cov)
            await Salvar(new PropostaCobertura { PropostaId = proposta.Id, CoberturaId = cov.CoberturaId, LegadoId = (int)proposta.Id });
        proposta.DeletedAt = DateTime.UtcNow; vida.DeletedAt = DateTimeOffset.UtcNow; vida.Ativo = false;
        await fixture.DbContext.SaveChangesAsync();
        await Assert.ThrowsAsync<ValidacaoException>(() => Remover(c, tipo));
        Assert.Null(await ExclusaoPersistida(c));
    }

    [Fact]
    public async Task SnapshotAtualConfereComModeloEUuidDeRamoNaoSeConfundeComCadastro()
    {
        Assert.False(fixture.DbContext.Database.HasPendingModelChanges());
        var c = await Preparar(TipoVinculoApolice.Ramo);
        var ramo = (ApoliceRamoModel)c.Registro;
        var global = await fixture.DbContext.Ramos.SingleAsync(r => r.Id == ramo.RamoId);
        await Assert.ThrowsAsync<ValidacaoException>(() => Exclusao().ExcluirAsync(c.Apolice.PublicId, TipoVinculoApolice.Ramo, global.PublicId, Guid.NewGuid(), default));
        var queries = new WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries.ApolicesQueries(fixture.DbContext);
        var detalhe = await queries.ObterDetalhePorPublicIdAsync(c.Apolice.PublicId, default);
        var resultado = Assert.Single(detalhe!.Ramos);
        Assert.Equal(global.PublicId, resultado.PublicId); Assert.Equal(ramo.PublicId, resultado.VinculoPublicId);
        Assert.Equal(1, (await queries.ListarPaginadoAsync(1, 100, null, null, null, null, null, null, null, default)).Items.Single(a => a.PublicId == c.Apolice.PublicId).QuantidadeRamos);
        await Remover(c, TipoVinculoApolice.Ramo);
        Assert.Empty((await queries.ObterDetalhePorPublicIdAsync(c.Apolice.PublicId, default))!.Ramos);
        Assert.Equal(0, (await queries.ListarPaginadoAsync(1, 100, null, null, null, null, null, null, null, default)).Items.Single(a => a.PublicId == c.Apolice.PublicId).QuantidadeRamos);
        Assert.True(await fixture.DbContext.Ramos.AnyAsync(r => r.Id == global.Id));
    }

    [Fact]
    public async Task CriacaoConcorrenteDeFilhoEsperaExclusaoERejeitaPaiExcluido()
    {
        var c = await Preparar(TipoVinculoApolice.Modulo);
        var gravou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var registrador = new Mock<IRegistradorAuditoria>();
        registrador.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) =>
            {
                gravou.TrySetResult(); await liberar.Task.WaitAsync(ct);
                await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct);
            });
        var exclusao = Exclusao(registrador.Object).ExcluirAsync(c.Apolice.PublicId, TipoVinculoApolice.Modulo, c.PublicId, Guid.NewGuid(), default);
        await gravou.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await using var connection = new NpgsqlConnection(fixture.ConnectionString); await connection.OpenAsync();
        await using var outro = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(
            new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
        outro.Add(new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModuloId = c.Modulo!.Id, Nome = "Filho Concorrente" });
        var criacao = outro.SaveChangesAsync(); var bloqueada = false;
        try
        {
            var limite = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < limite && !criacao.IsCompleted)
            {
                await using var cmd = new NpgsqlCommand("SELECT cardinality(pg_blocking_pids(@pid)) > 0", fixture.Connection,
                    (NpgsqlTransaction)fixture.DbContext.Database.CurrentTransaction!.GetDbTransaction());
                cmd.Parameters.AddWithValue("pid", connection.ProcessID);
                bloqueada = (bool)(await cmd.ExecuteScalarAsync())!; if (bloqueada) break;
                await Task.Delay(10);
            }
        }
        finally { liberar.TrySetResult(); }
        await exclusao;
        var erro = await Assert.ThrowsAsync<DbUpdateException>(() => criacao);
        Assert.Equal("ck_hierarquia_vinculo_apolice", Assert.IsType<PostgresException>(erro.InnerException).ConstraintName);
        Assert.True(bloqueada);
        Assert.False(await fixture.DbContext.ApoliceModuloPlanos.AnyAsync(p => p.ApoliceModuloId == c.Modulo!.Id));
    }

    [Fact]
    public async Task FilhoCriadoAntesDoCommitBloqueiaExclusaoConcorrenteDoPai()
    {
        var c = await Preparar(TipoVinculoApolice.Modulo);
        await using var tx = await fixture.DbContext.Database.BeginTransactionAsync();
        await Salvar(new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModuloId = c.Modulo!.Id, Nome = "Filho Primeiro" });
        await using var connection = new NpgsqlConnection(fixture.ConnectionString); await connection.OpenAsync();
        await using var outro = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(
            new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
        await using var auditoria = new WebApolice.Auditoria.Infrastructure.AuditoriaDbContext(new DbContextOptionsBuilder<WebApolice.Auditoria.Infrastructure.AuditoriaDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
        var exclusao = new ExclusaoVinculosApolice(outro, new OperacaoAuditada(auditoria, new RegistradorAuditoria(auditoria), Mock.Of<IContextoAuditoria>()))
            .ExcluirAsync(c.Apolice.PublicId, TipoVinculoApolice.Modulo, c.PublicId, Guid.NewGuid(), default);
        var bloqueada = false; var limite = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < limite && !exclusao.IsCompleted)
        {
            await using var cmd = new NpgsqlCommand("SELECT cardinality(pg_blocking_pids(@pid)) > 0", fixture.Connection, (NpgsqlTransaction)tx.GetDbTransaction());
            cmd.Parameters.AddWithValue("pid", connection.ProcessID);
            bloqueada = (bool)(await cmd.ExecuteScalarAsync())!; if (bloqueada) break;
            await Task.Delay(10);
        }
        await tx.CommitAsync();
        var erro = await Assert.ThrowsAsync<ValidacaoException>(() => exclusao);
        Assert.Contains("plano", erro.Message); Assert.True(bloqueada);
        Assert.Null(await ExclusaoPersistida(c));
    }
}
