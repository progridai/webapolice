using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Npgsql;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Cadastro.Application.UseCases.Vinculos;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.Vinculos;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ExcluirApoliceVida;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApoliceVida;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApoliceVida;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public class ClienteVinculosVidaTests(SeguroIntegrationTestFixture fixture) : IClassFixture<SeguroIntegrationTestFixture>
{
    private CadastroSeguroQueries Cadastro => new(fixture.Cadastro, new ClienteVinculosUso(fixture.DbContext));
    private ApoliceVinculosRepository Vidas => new(fixture.DbContext);
    private OperacaoAuditada Operacao(IRegistradorAuditoria? registrador = null) => new(fixture.Auditoria,
        registrador ?? new RegistradorAuditoria(fixture.Auditoria), Mock.Of<IContextoAuditoria>());
    private ClienteVinculosHandler Vinculos(IRegistradorAuditoria? registrador = null) => new(
        new ClienteVinculosRepository(fixture.Cadastro), new ClienteVinculosUso(fixture.DbContext),
        new OperacaoCadastroAuditada(fixture.Cadastro, Operacao(registrador)));
    private CriarApoliceVidaHandler Criar(IRegistradorAuditoria? registrador = null) => new(Vidas, Cadastro,
        new OperacaoVidas(fixture.DbContext, Operacao(registrador), fixture.VinculosVida), fixture.VinculosVida);
    private AlterarApoliceVidaHandler Alterar => new(Vidas, Cadastro, fixture.OperacaoVidas, fixture.VinculosVida);
    private ExcluirApoliceVidaHandler Excluir(IRegistradorAuditoria? registrador = null) => new(Vidas,
        new OperacaoVidas(fixture.DbContext, Operacao(registrador), fixture.VinculosVida));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExcluirParticipacaoAtivaOuEncerradaPreservaClienteMatriculaHistoricoEAuditoria(bool encerrada)
    {
        var (cliente, estip, apolice, vinculo) = await Preparar();
        var vidaId = await Participar(cliente, apolice, vinculo.PublicId);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == vidaId);
        if (encerrada) { vida.Ativo = false; vida.Status = "encerrada"; await fixture.DbContext.SaveChangesAsync(); }
        var matriculaAntes = System.Text.Json.JsonSerializer.Serialize(await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == vinculo.Id));
        var usuario = Guid.NewGuid();
        await Excluir().Handle(new(apolice.PublicId, vidaId, usuario), default);
        var depois = await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == vidaId);
        Assert.NotNull(depois.DeletedAt); Assert.False(depois.Ativo); Assert.Equal(vinculo.Id, depois.ClienteVinculoId);
        Assert.Equal(matriculaAntes, System.Text.Json.JsonSerializer.Serialize(await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == vinculo.Id)));
        Assert.True(await fixture.Cadastro.Clientes.AnyAsync(c => c.Id == cliente.Id && c.DeletedAt == null));
        Assert.True(await fixture.DbContext.ApoliceHistoricos.AnyAsync(h => h.ApoliceId == apolice.Id && h.Acao == "Exclusão Vida" && h.UsuarioPublicId == usuario));
        Assert.True(await fixture.Auditoria.RegistrosAuditoria.AnyAsync(a => a.RecursoId == vidaId.ToString() && a.Acao == "VIDA_EXCLUIDA"));
        var queries = new ApolicesQueries(fixture.DbContext, cadastro: Cadastro);
        Assert.Empty((await queries.ListarVidasPaginadoAsync(apolice.PublicId, 1, 20, null, null, null, null, null, default)).Items);
        Assert.Null(await queries.ObterApoliceVidaPorPublicIdAsync(apolice.PublicId, vidaId, default));
        Assert.Empty(Assert.Single((await Vinculos().ListarAsync(cliente.PublicId, 1, 20, default)).Items).Apolices);
        await Assert.ThrowsAsync<ValidacaoException>(() => Excluir().Handle(new(apolice.PublicId, vidaId, usuario), default));
        // Corrigir o lançamento não exige criar outra matrícula.
        var novaVida = await Participar(cliente, apolice, vinculo.PublicId);
        Assert.NotEqual(vidaId, novaVida);
        Assert.Single(await fixture.Cadastro.Vinculos.Where(v => v.ClienteId == cliente.Id).ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NaoExcluiParticipacaoComPropostaMesmoExcluida(bool propostaExcluida)
    {
        var (cliente, estip, apolice, vinculo) = await Preparar();
        var vidaId = await Participar(cliente, apolice, vinculo.PublicId);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == vidaId);
        var proposta = await Proposta(vida, cliente.PessoaId, estip.Id);
        if (propostaExcluida) { proposta.DeletedAt = DateTime.UtcNow; await fixture.DbContext.SaveChangesAsync(); }
        var erro = await Assert.ThrowsAsync<ValidacaoException>(() => Excluir().Handle(new(apolice.PublicId, vidaId, Guid.NewGuid()), default));
        Assert.Contains("possui propostas", erro.Message);
        Assert.Null((await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == vidaId)).DeletedAt);
        Assert.False(await fixture.DbContext.ApoliceHistoricos.AnyAsync(h => h.ApoliceId == apolice.Id && h.Acao == "Exclusão Vida"));
        Assert.False(await fixture.Auditoria.RegistrosAuditoria.AnyAsync(a => a.RecursoId == vidaId.ToString() && a.Acao == "VIDA_EXCLUIDA"));
    }

    [Fact]
    public async Task ExclusaoRejeitaParticipacaoDeOutraApoliceESemRegistro()
    {
        var (cliente, _, apolice, vinculo) = await Preparar();
        var vidaId = await Participar(cliente, apolice, vinculo.PublicId);
        var outra = (await Preparar()).Apolice;
        await Assert.ThrowsAsync<ValidacaoException>(() => Excluir().Handle(new(outra.PublicId, vidaId, Guid.NewGuid()), default));
        await Assert.ThrowsAsync<ValidacaoException>(() => Excluir().Handle(new(apolice.PublicId, Guid.NewGuid(), Guid.NewGuid()), default));
        await Assert.ThrowsAsync<ValidacaoException>(() => Excluir().Handle(new(Guid.NewGuid(), vidaId, Guid.NewGuid()), default));
        Assert.Null((await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == vidaId)).DeletedAt);
    }

    [Fact]
    public async Task FalhaDaAuditoriaReverteExclusaoEHistorico()
    {
        var (cliente, _, apolice, vinculo) = await Preparar();
        var vidaId = await Participar(cliente, apolice, vinculo.PublicId);
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Excluir(falha.Object).Handle(new(apolice.PublicId, vidaId, Guid.NewGuid()), default));
        var vida = await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == vidaId);
        Assert.Null(vida.DeletedAt); Assert.True(vida.Ativo);
        Assert.False(await fixture.DbContext.ApoliceHistoricos.AnyAsync(h => h.ApoliceId == apolice.Id && h.Acao == "Exclusão Vida"));
        await Excluir().Handle(new(apolice.PublicId, vidaId, Guid.NewGuid()), default);
    }

    [Fact]
    public async Task ExclusaoImpedeCriacaoConcorrenteDePropostaAposVerificacao()
    {
        var (cliente, estip, apolice, vinculo) = await Preparar();
        var vidaId = await Participar(cliente, apolice, vinculo.PublicId);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == vidaId);
        var status = new PropostaStatus { Id = (short)((await fixture.DbContext.PropostaStatuses.MaxAsync(s => (short?)s.Id) ?? 0) + 1), Codigo = Guid.NewGuid().ToString("N")[..20], Nome = "Concorrência" };
        fixture.DbContext.PropostaStatuses.Add(status); await fixture.DbContext.SaveChangesAsync();
        var excluiu = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var registrador = new Mock<IRegistradorAuditoria>();
        registrador.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) =>
            {
                excluiu.TrySetResult();
                await liberar.Task.WaitAsync(ct);
                await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct);
            });
        var exclusao = Excluir(registrador.Object).Handle(new(apolice.PublicId, vidaId, Guid.NewGuid()), default);
        await excluiu.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await using var outra = new NpgsqlConnection(fixture.ConnectionString); await outra.OpenAsync();
        await using var db = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(
            new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(outra).UseSnakeCaseNamingConvention().Options);
        db.Proposta.Add(new() { PublicId = Guid.NewGuid(), ApoliceId = apolice.Id, ApoliceVidaId = vida.Id,
            ClienteId = cliente.Id, ClienteVinculoId = vinculo.Id, PessoaId = cliente.PessoaId, EstipulanteId = estip.Id,
            SeguradoraId = 1, StatusId = status.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        var proposta = db.SaveChangesAsync();
        var bloqueada = false;
        try
        {
            var limite = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < limite && !proposta.IsCompleted)
            {
                await using var cmd = new NpgsqlCommand("SELECT cardinality(pg_blocking_pids(@pid)) > 0", fixture.Connection,
                    (NpgsqlTransaction)fixture.DbContext.Database.CurrentTransaction!.GetDbTransaction());
                cmd.Parameters.AddWithValue("pid", outra.ProcessID);
                bloqueada = (bool)(await cmd.ExecuteScalarAsync())!;
                if (bloqueada) break;
                await Task.Delay(10);
            }
        }
        finally { liberar.TrySetResult(); }
        await exclusao;
        var erro = await Assert.ThrowsAsync<DbUpdateException>(() => proposta);
        Assert.Equal("ck_proposta_vinculo", Assert.IsType<PostgresException>(erro.InnerException).ConstraintName);
        Assert.True(bloqueada);
        Assert.False(await fixture.DbContext.Proposta.AnyAsync(p => p.ApoliceVidaId == vida.Id));
    }

    private async Task<(Cliente Cliente, EstipulanteModel Estipulante, ApoliceModel Apolice, ClienteVinculoModel Vinculo)> Preparar()
    {
        var pessoa = new PessoaModel(1, "Pessoa dos Vínculos", null, null, false, null, null, null);
        fixture.Cadastro.Pessoas.Add(pessoa); await fixture.Cadastro.SaveChangesAsync();
        var cliente = new Cliente(pessoa.Id, 1);
        var estipulante = new EstipulanteModel { PublicId = Guid.NewGuid(), Nome = "Empresa dos Vínculos", Ativo = true };
        fixture.Cadastro.AddRange(cliente, estipulante); await fixture.Cadastro.SaveChangesAsync();
        var vinculo = new ClienteVinculoModel(cliente.Id, pessoa.Id, estipulante.Id, "AbC-001");
        fixture.Cadastro.Vinculos.Add(vinculo); await fixture.Cadastro.SaveChangesAsync();
        var apolice = new ApoliceModel { PublicId = Guid.NewGuid(), Nome = "Apólice dos Vínculos", EstipulanteId = estipulante.Id, SeguradoraId = 1, Ativo = true, DataInicioVigencia = new(2026, 1, 1) };
        fixture.DbContext.Apolices.Add(apolice); await fixture.DbContext.SaveChangesAsync();
        return (cliente, estipulante, apolice, vinculo);
    }

    private Task<Guid> Participar(Cliente c, ApoliceModel a, Guid? v) => Criar().Handle(new()
    { ApolicePublicId = a.PublicId, ClientePublicId = c.PublicId, ClienteVinculoPublicId = v }, default);

    [Fact]
    public async Task CadastroIdentificaApolicesDeCadaMatriculaNoMesmoEstipulanteSemDuplicarParticipacoesOuNumeros()
    {
        var (cliente, estip, primeira, vinculo) = await Preparar();
        var segunda = new ApoliceModel { PublicId = Guid.NewGuid(), Nome = "Outra Apólice", EstipulanteId = estip.Id,
            SeguradoraId = 1, Ativo = true, DataInicioVigencia = new(2026, 1, 1) };
        fixture.DbContext.Apolices.Add(segunda); await fixture.DbContext.SaveChangesAsync();
        var outraMatricula = new ClienteVinculoModel(cliente.Id, cliente.PessoaId, estip.Id, "OUTRA");
        var semApolice = new ClienteVinculoModel(cliente.Id, cliente.PessoaId, estip.Id, "SEM-APOLICE");
        fixture.Cadastro.Vinculos.AddRange(outraMatricula, semApolice); await fixture.Cadastro.SaveChangesAsync();
        await Participar(cliente, primeira, vinculo.PublicId);
        await Participar(cliente, segunda, vinculo.PublicId);
        await Participar(cliente, segunda, outraMatricula.PublicId);
        // Participação histórica no mesmo contrato não deve duplicar a apólice na consulta.
        fixture.DbContext.ApoliceVidas.Add(new() { PublicId = Guid.NewGuid(), ApoliceId = primeira.Id,
            ClienteId = cliente.Id, ClienteVinculoId = vinculo.Id, Ativo = false, Status = "inativa" });
        foreach (var numero in new[] { "aBc-123", "aBc-123", "456" })
        {
            var ramo = new RamoModel { PublicId = Guid.NewGuid(), Codigo = Guid.NewGuid().ToString(), Nome = "Ramo de Teste", Ativo = true };
            fixture.DbContext.Ramos.Add(ramo); await fixture.DbContext.SaveChangesAsync();
            fixture.DbContext.ApoliceRamos.Add(new() { ApoliceId = primeira.Id, RamoId = ramo.Id, NumeroApolice = numero, Ativo = true });
        }
        await fixture.DbContext.SaveChangesAsync();
        var resultado = await Vinculos().ListarAsync(cliente.PublicId, 1, 2, default);
        Assert.Equal(3, resultado.TotalCount); Assert.Equal(2, resultado.Items.Count());
        var apolices = resultado.Items.Single(v => v.PublicId == vinculo.PublicId).Apolices;
        Assert.Equal(2, apolices.Count);
        var apolice = Assert.Single(apolices, a => a.PublicId == primeira.PublicId);
        Assert.Equal(primeira.Nome, apolice.Nome); Assert.True(apolice.ParticipacaoAtiva);
        Assert.Equal(new[] { "456", "aBc-123" }, apolice.Numeros);
        Assert.Equal(segunda.PublicId, Assert.Single(resultado.Items.Single(v => v.PublicId == outraMatricula.PublicId).Apolices).PublicId);
        var pagina2 = await Vinculos().ListarAsync(cliente.PublicId, 2, 2, default);
        Assert.Empty(Assert.Single(pagina2.Items).Apolices);
        var vazio = await Vinculos().ListarAsync(cliente.PublicId, 3, 2, default);
        Assert.Empty(vazio.Items); Assert.Equal(3, vazio.TotalCount);
    }

    [Fact]
    public async Task ConsultaContextualPreservaInativasExcluiRemovidasEIsolaCliente()
    {
        var (cliente, _, apolice, vinculo) = await Preparar();
        var vidaId = await Participar(cliente, apolice, vinculo.PublicId);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == vidaId);
        vida.Ativo = false; vida.Status = "inativa"; apolice.Ativo = false;
        await fixture.DbContext.SaveChangesAsync();
        var uso = new ClienteVinculosUso(fixture.DbContext);
        var inativas = await uso.ListarApolicesAsync(cliente.Id, [vinculo.Id], default);
        var item = Assert.Single(inativas[vinculo.Id]);
        Assert.False(item.Ativa); Assert.False(item.ParticipacaoAtiva); Assert.Empty(item.Numeros);
        Assert.Empty(await uso.ListarApolicesAsync((await Preparar()).Cliente.Id, [vinculo.Id], default));
        Assert.Empty(await uso.ListarApolicesAsync(cliente.Id, [], default));
        vida.DeletedAt = DateTimeOffset.UtcNow; await fixture.DbContext.SaveChangesAsync();
        Assert.Empty(await uso.ListarApolicesAsync(cliente.Id, [vinculo.Id], default));
        vida.DeletedAt = null; await fixture.DbContext.SaveChangesAsync();
        apolice.DeletedAt = DateTimeOffset.UtcNow; await fixture.DbContext.SaveChangesAsync();
        Assert.Empty(await uso.ListarApolicesAsync(cliente.Id, [vinculo.Id], default));
        // Ocultar removidos na apresentação não libera sua identidade histórica para edição.
        Assert.True(await uso.EstaEmUsoAsync(vinculo.Id, default));
    }

    private async Task<ClienteVinculoModel> SemEstipulante(Cliente cliente, string matricula)
    {
        var vinculo = new ClienteVinculoModel();
        fixture.Cadastro.Vinculos.Add(vinculo);
        fixture.Cadastro.Entry(vinculo).Property(v => v.ClienteId).CurrentValue = cliente.Id;
        fixture.Cadastro.Entry(vinculo).Property(v => v.PessoaId).CurrentValue = cliente.PessoaId;
        fixture.Cadastro.Entry(vinculo).Property(v => v.Matricula).CurrentValue = matricula;
        fixture.Cadastro.Entry(vinculo).Property(v => v.Ativo).CurrentValue = true;
        fixture.Cadastro.Entry(vinculo).Property(v => v.CreatedAt).CurrentValue = DateTime.UtcNow;
        await fixture.Cadastro.SaveChangesAsync();
        return vinculo;
    }

    [Fact]
    public async Task ListaMatriculasSemEstipulanteComPaginacaoEIndicacaoDeComplemento()
    {
        var (cliente, _, apolice, _) = await Preparar();
        var primeira = await SemEstipulante(cliente, "123");
        var segunda = await SemEstipulante(cliente, "12345");
        var inativa = await SemEstipulante(cliente, "INATIVA");
        inativa.DefinirSituacao(false); await fixture.Cadastro.SaveChangesAsync();
        await SemEstipulante(cliente, "   ");
        var handler = new ListarVinculosVidaHandler(Vidas, Cadastro);
        var pagina1 = await handler.Handle(apolice.PublicId, cliente.PublicId, 1, 2, default);
        var pagina2 = await handler.Handle(apolice.PublicId, cliente.PublicId, 2, 2, default);
        Assert.Equal(3, pagina1.TotalCount); Assert.Equal(3, pagina2.TotalCount);
        var itens = pagina1.Items.Concat(pagina2.Items).ToArray();
        Assert.Contains(itens, v => v.PublicId == primeira.PublicId && v.RequerEstipulante);
        Assert.Contains(itens, v => v.PublicId == segunda.PublicId && v.RequerEstipulante);
        Assert.Single(itens, v => !v.RequerEstipulante);
        Assert.DoesNotContain(itens, v => v.PublicId == inativa.PublicId);
    }

    [Fact]
    public async Task EdicaoCompletaMatriculaEscolhidaSemDuplicarOuReescreverOutrosCampos()
    {
        var (cliente, estip, apolice, original) = await Preparar();
        var legado = await SemEstipulante(cliente, " aBc-123 ");
        var persistidoAntes = await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == legado.Id);
        var antes = System.Text.Json.JsonSerializer.SerializeToNode(persistidoAntes)!.AsObject();
        antes.Remove("EstipulanteId"); antes.Remove("UpdatedAt");
        var vidaId = await Participar(cliente, apolice, original.PublicId);
        await Alterar.Handle(new()
        {
            ApolicePublicId = apolice.PublicId, ApoliceVidaPublicId = vidaId,
            ClienteVinculoPublicId = legado.PublicId, CompletarVinculoSemEstipulante = true
        }, default);
        var depois = await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == legado.Id);
        Assert.Equal(estip.Id, depois.EstipulanteId);
        var campos = System.Text.Json.JsonSerializer.SerializeToNode(depois)!.AsObject();
        campos.Remove("EstipulanteId"); campos.Remove("UpdatedAt");
        Assert.True(System.Text.Json.Nodes.JsonNode.DeepEquals(antes, campos));
        Assert.Equal(2, await fixture.Cadastro.Vinculos.CountAsync(v => v.ClienteId == cliente.Id));
        var vida = await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == vidaId);
        Assert.Equal(legado.Id, vida.ClienteVinculoId);
        Assert.True(await fixture.Auditoria.RegistrosAuditoria.AnyAsync(e => e.RecursoId == legado.PublicId.ToString() && e.Acao == "VINCULO_COMPLETADO"));
        // Repetir a intenção com a associação já compatível não duplica nem reescreve o vínculo.
        await Alterar.Handle(new() { ApolicePublicId = apolice.PublicId, ApoliceVidaPublicId = vidaId,
            ClienteVinculoPublicId = legado.PublicId, CompletarVinculoSemEstipulante = true }, default);
        Assert.Equal(2, await fixture.Cadastro.Vinculos.CountAsync(v => v.ClienteId == cliente.Id));
    }

    [Fact]
    public async Task InclusaoExigeIntencaoExplicitaParaCompletarVinculo()
    {
        var (cliente, estip, apolice, _) = await Preparar();
        var legado = await SemEstipulante(cliente, "EXISTENTE");
        await Assert.ThrowsAsync<ValidacaoException>(() => Participar(cliente, apolice, legado.PublicId));
        var id = await Criar().Handle(new() { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId,
            ClienteVinculoPublicId = legado.PublicId, CompletarVinculoSemEstipulante = true }, default);
        Assert.Equal(estip.Id, (await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == legado.Id)).EstipulanteId);
        Assert.Equal(legado.Id, (await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == id)).ClienteVinculoId);
    }

    [Theory]
    [InlineData("outro-cliente")]
    [InlineData("outro-estipulante")]
    [InlineData("inativo")]
    [InlineData("duplicado")]
    [InlineData("pessoa-incompativel")]
    public async Task RejeitaCompletarVinculoInvalidoSemAlterarIdentidade(string caso)
    {
        var (cliente, estip, apolice, original) = await Preparar();
        var legado = await SemEstipulante(cliente, caso == "duplicado" ? original.Matricula! : "LEGADO");
        if (caso == "inativo") legado.DefinirSituacao(false);
        if (caso == "outro-estipulante") legado.CompletarEstipulante((await Preparar()).Estipulante.Id);
        if (caso == "outro-cliente") legado = await SemEstipulante((await Preparar()).Cliente, "OUTRO");
        if (caso == "pessoa-incompativel") fixture.Cadastro.Entry(legado).Property(v => v.PessoaId).CurrentValue = (await Preparar()).Cliente.PessoaId;
        await fixture.Cadastro.SaveChangesAsync();
        var identidade = legado.EstipulanteId;
        Task<Guid> Tentar() => Criar().Handle(new() { ApolicePublicId = apolice.PublicId,
            ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = legado.PublicId, CompletarVinculoSemEstipulante = true }, default);
        if (caso is "outro-cliente" or "pessoa-incompativel") await Assert.ThrowsAsync<ValidacaoException>(Tentar);
        else await Assert.ThrowsAsync<ClienteInvalidoException>(Tentar);
        Assert.Equal(identidade, (await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == legado.Id)).EstipulanteId);
        Assert.False(await fixture.DbContext.ApoliceVidas.AnyAsync(v => v.ApoliceId == apolice.Id));
    }

    [Fact]
    public async Task FalhaNaAuditoriaDaVidaReverteComplementoDoVinculoExistente()
    {
        var (cliente, _, apolice, _) = await Preparar();
        var legado = await SemEstipulante(cliente, "REVERTER");
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha artificial"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Criar(falha.Object).Handle(new() { ApolicePublicId = apolice.PublicId,
            ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = legado.PublicId, CompletarVinculoSemEstipulante = true }, default));
        await using var leitura = new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(fixture.ConnectionString).UseSnakeCaseNamingConvention().Options);
        Assert.Null((await leitura.Vinculos.AsNoTracking().SingleAsync(v => v.Id == legado.Id)).EstipulanteId);
        Assert.False(await fixture.Auditoria.RegistrosAuditoria.AnyAsync(e => e.RecursoId == legado.PublicId.ToString()));
        Assert.False(await fixture.DbContext.ApoliceVidas.AnyAsync(v => v.ApoliceId == apolice.Id));
    }

    [Fact]
    public async Task VinculoSemEstipulanteUsadoEmPropostaLegadaNaoApareceENaoPodeSerCompletado()
    {
        var (cliente, estip, apolice, _) = await Preparar();
        var legado = await SemEstipulante(cliente, "EM-USO");
        var status = new PropostaStatus { Id = (short)((await fixture.DbContext.PropostaStatuses.MaxAsync(s => (short?)s.Id) ?? 0) + 1), Codigo = Guid.NewGuid().ToString("N")[..20], Nome = "Status Legado" };
        fixture.DbContext.PropostaStatuses.Add(status); await fixture.DbContext.SaveChangesAsync();
        fixture.DbContext.Proposta.Add(new Propostum { PublicId = Guid.NewGuid(), ApoliceId = apolice.Id, ClienteId = cliente.Id,
            PessoaId = cliente.PessoaId, ClienteVinculoId = legado.Id, EstipulanteId = estip.Id, SeguradoraId = 1, StatusId = status.Id,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await fixture.DbContext.SaveChangesAsync();
        var lista = await new ListarVinculosVidaHandler(Vidas, Cadastro).Handle(apolice.PublicId, cliente.PublicId, 1, 20, default);
        Assert.Equal(1, lista.TotalCount); Assert.DoesNotContain(lista.Items, v => v.PublicId == legado.PublicId);
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => Criar().Handle(new() { ApolicePublicId = apolice.PublicId,
            ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = legado.PublicId, CompletarVinculoSemEstipulante = true }, default));
        Assert.Null((await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == legado.Id)).EstipulanteId);
    }

    [Fact]
    public async Task ComplementoConcorrenteParaEstipulantesDiferentesPreservaSomenteAEscolhaConfirmada()
    {
        var (cliente, _, primeira, _) = await Preparar();
        var segunda = (await Preparar()).Apolice;
        var legado = await SemEstipulante(cliente, "DISPUTADA");
        var prontos = 0;
        var iniciar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<bool> Completar(ApoliceModel apolice)
        {
            await using var connection = new NpgsqlConnection(fixture.ConnectionString); await connection.OpenAsync();
            await using var cadastro = new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var seguro = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(
                new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var auditoria = new AuditoriaDbContext(new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            var contexto = Mock.Of<IContextoAuditoria>();
            var registrador = new RegistradorAuditoria(auditoria);
            var uso = new ClienteVinculosUso(seguro);
            var escritor = new CadastroVinculosVida(cadastro, new ClienteVinculosRepository(cadastro), registrador, contexto, uso);
            var handler = new CriarApoliceVidaHandler(new ApoliceVinculosRepository(seguro), new CadastroSeguroQueries(cadastro, uso),
                new OperacaoVidas(seguro, new OperacaoAuditada(auditoria, registrador, contexto), escritor), escritor);
            if (Interlocked.Increment(ref prontos) == 2) iniciar.TrySetResult();
            await iniciar.Task.WaitAsync(TimeSpan.FromSeconds(10));
            try
            {
                await handler.Handle(new() { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId,
                    ClienteVinculoPublicId = legado.PublicId, CompletarVinculoSemEstipulante = true }, default);
                return true;
            }
            catch (ClienteInvalidoException) { return false; }
        }
        var resultados = await Task.WhenAll(Completar(primeira), Completar(segunda));
        Assert.Single(resultados, r => r);
        var confirmado = await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == legado.Id);
        var vida = await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.ClienteVinculoId == legado.Id);
        var apoliceConfirmada = await fixture.DbContext.Apolices.AsNoTracking().SingleAsync(a => a.Id == vida.ApoliceId);
        Assert.Equal(apoliceConfirmada.EstipulanteId, confirmado.EstipulanteId);
        Assert.Equal(2, await fixture.Cadastro.Vinculos.CountAsync(v => v.ClienteId == cliente.Id));
    }

    [Fact]
    public async Task CriaMatriculaNaVidaComEstipulanteDaApoliceEAuditaAsDuasGravacoes()
    {
        var (cliente, estip, apolice, _) = await Preparar();
        var id = await Criar().Handle(new()
        {
            ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId,
            NovaMatricula = "  aBc-X009  ", Observacao = "PARTICIPAÇÃO NATIVA"
        }, default);
        var vida = await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == id);
        var vinculo = await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == vida.ClienteVinculoId);
        Assert.Equal(cliente.Id, vinculo.ClienteId); Assert.Equal(cliente.PessoaId, vinculo.PessoaId);
        Assert.Equal(estip.Id, vinculo.EstipulanteId); Assert.Equal("aBc-X009", vinculo.Matricula);
        Assert.Equal("ABC-X009", vinculo.MatriculaNormalizada); Assert.True(vinculo.Ativo);
        Assert.Equal("Participação Nativa", vida.Observacao);
        var eventos = await fixture.Auditoria.RegistrosAuditoria.AsNoTracking()
            .Where(a => a.RecursoId == id.ToString() || a.RecursoId == vinculo.PublicId.ToString()).ToListAsync();
        Assert.Equal(2, eventos.Count);
        Assert.Contains(eventos, e => e.Acao == "VINCULO_CRIADO");
        Assert.Contains(eventos, e => e.Acao == "VIDA_CRIADA");
        Assert.All(eventos, e => Assert.DoesNotContain("aBc-X009", e.DadosPosteriores!.RootElement.GetRawText()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901234567890123456789012345678901")]
    [InlineData(" abc-001 ")]
    public async Task RejeitaMatriculaInvalidaOuDuplicadaSemCriarVida(string matricula)
    {
        var (cliente, _, apolice, _) = await Preparar();
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => Criar().Handle(new()
        { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId, NovaMatricula = matricula }, default));
        Assert.Equal(1, await fixture.Cadastro.Vinculos.CountAsync(v => v.ClienteId == cliente.Id));
        Assert.False(await fixture.DbContext.ApoliceVidas.AnyAsync(v => v.ApoliceId == apolice.Id));
    }

    [Fact]
    public async Task RejeitaSelecaoENovaMatriculaSimultaneas()
    {
        var (cliente, _, apolice, vinculo) = await Preparar();
        await Assert.ThrowsAsync<ValidacaoException>(() => Criar().Handle(new()
        {
            ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId,
            ClienteVinculoPublicId = vinculo.PublicId, NovaMatricula = "NOVA"
        }, default));
        Assert.Equal(1, await fixture.Cadastro.Vinculos.CountAsync(v => v.ClienteId == cliente.Id));
    }

    [Fact]
    public async Task CadastroContextualExigeEstipulanteAtivo()
    {
        var (cliente, estip, apolice, _) = await Preparar();
        estip.Ativo = false; await fixture.Cadastro.SaveChangesAsync();
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => Criar().Handle(new()
        { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId, NovaMatricula = "NOVA" }, default));
        Assert.Equal(1, await fixture.Cadastro.Vinculos.CountAsync(v => v.ClienteId == cliente.Id));
    }

    [Fact]
    public async Task EdicaoCriaOutroVinculoSemReescreverVinculosAnteriores()
    {
        var (cliente, estip, apolice, anterior) = await Preparar();
        var id = await Participar(cliente, apolice, anterior.PublicId);
        await Alterar.Handle(new() { ApolicePublicId = apolice.PublicId, ApoliceVidaPublicId = id, NovaMatricula = "Ed-002" }, default);
        var vida = await fixture.DbContext.ApoliceVidas.AsNoTracking().SingleAsync(v => v.PublicId == id);
        var vinculo = await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == vida.ClienteVinculoId);
        Assert.Equal("Ed-002", vinculo.Matricula); Assert.Equal(estip.Id, vinculo.EstipulanteId);
        Assert.NotEqual(anterior.Id, vinculo.Id);
        Assert.Equal("AbC-001", (await fixture.Cadastro.Vinculos.AsNoTracking().SingleAsync(v => v.Id == anterior.Id)).Matricula);
        await Proposta(await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == id), cliente.PessoaId, estip.Id);
        await Assert.ThrowsAsync<ValidacaoException>(() => Alterar.Handle(new()
        { ApolicePublicId = apolice.PublicId, ApoliceVidaPublicId = id, NovaMatricula = "BLOQUEADA" }, default));
        Assert.False(await fixture.Cadastro.Vinculos.AnyAsync(v => v.ClienteId == cliente.Id && v.Matricula == "BLOQUEADA"));
    }

    [Fact]
    public async Task FalhaNaAuditoriaDaVidaReverteTambemNovoVinculoESeuEvento()
    {
        var (cliente, _, apolice, _) = await Preparar();
        var eventosAntes = await fixture.Auditoria.RegistrosAuditoria.CountAsync();
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha artificial após criação do vínculo"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Criar(falha.Object).Handle(new()
        { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId, NovaMatricula = "ATOMICA" }, default));
        await using var connection = new NpgsqlConnection(fixture.ConnectionString); await connection.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT (SELECT count(*) FROM cadastro.cliente_vinculo WHERE cliente_id = @c AND matricula = 'ATOMICA'), (SELECT count(*) FROM seguro.apolice_vida WHERE apolice_id = @a), (SELECT count(*) FROM auditoria.registros_auditoria)", connection);
        cmd.Parameters.AddWithValue("c", cliente.Id); cmd.Parameters.AddWithValue("a", apolice.Id);
        await using var reader = await cmd.ExecuteReaderAsync(); await reader.ReadAsync();
        Assert.Equal(0, reader.GetInt64(0)); Assert.Equal(0, reader.GetInt64(1)); Assert.Equal(eventosAntes, reader.GetInt64(2));
        Assert.Null(fixture.Cadastro.Database.CurrentTransaction);
        // A mesma requisição de negócio pode ser tentada novamente após o rollback.
        await Criar().Handle(new() { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId, NovaMatricula = "ATOMICA" }, default);
    }

    [Fact]
    public async Task DuasInclusoesConcorrentesDaMesmaMatriculaCriamApenasUmVinculoEUmaVida()
    {
        var (cliente, _, apolice, _) = await Preparar();
        async Task<bool> Incluir()
        {
            await using var connection = new NpgsqlConnection(fixture.ConnectionString); await connection.OpenAsync();
            await using var cadastro = new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var seguro = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext(
                new DbContextOptionsBuilder<WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.SeguroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            await using var auditoria = new AuditoriaDbContext(new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
            var registrador = new RegistradorAuditoria(auditoria);
            var contexto = Mock.Of<IContextoAuditoria>();
            var escritor = new CadastroVinculosVida(cadastro, new ClienteVinculosRepository(cadastro), registrador, contexto, new ClienteVinculosUso(seguro));
            var handler = new CriarApoliceVidaHandler(new ApoliceVinculosRepository(seguro), new CadastroSeguroQueries(cadastro, new ClienteVinculosUso(seguro)),
                new OperacaoVidas(seguro, new OperacaoAuditada(auditoria, registrador, contexto), escritor), escritor);
            try
            {
                await handler.Handle(new() { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId, NovaMatricula = "CONCORRENTE-NOVA" }, default);
                return true;
            }
            catch (ClienteInvalidoException) { return false; }
        }
        var resultados = await Task.WhenAll(Incluir(), Incluir());
        Assert.Single(resultados, r => r);
        Assert.Single(await fixture.Cadastro.Vinculos.AsNoTracking().Where(v => v.ClienteId == cliente.Id && v.Matricula == "CONCORRENTE-NOVA").ToListAsync());
        Assert.Single(await fixture.DbContext.ApoliceVidas.AsNoTracking().Where(v => v.ApoliceId == apolice.Id).ToListAsync());
    }

    [Fact]
    public async Task CancelamentoAposCriarVinculoReverteAsDuasGravacoes()
    {
        var (cliente, _, apolice, _) = await Preparar();
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        await Assert.ThrowsAsync<OperationCanceledException>(() => Criar(falha.Object).Handle(new()
        { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId, NovaMatricula = "CANCELADA" }, default));
        Assert.False(await fixture.Cadastro.Vinculos.AsNoTracking().AnyAsync(v => v.ClienteId == cliente.Id && v.Matricula == "CANCELADA"));
        Assert.False(await fixture.DbContext.ApoliceVidas.AsNoTracking().AnyAsync(v => v.ApoliceId == apolice.Id));
        Assert.Null(fixture.Cadastro.Database.CurrentTransaction);
    }

    [Fact]
    public async Task CadastroPermiteMultiplasMatriculasEEstipulantesMasRejeitaDuplicidadeAtiva()
    {
        var (cliente, estip, apolice, _) = await Preparar();
        var handler = Vinculos();
        var segundo = await handler.SalvarAsync(cliente.PublicId, null, new(estip.PublicId, "XyZ-002"), default);
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => handler.SalvarAsync(cliente.PublicId, null, new(estip.PublicId, " xyz-002 "), default));
        var outro = new EstipulanteModel { PublicId = Guid.NewGuid(), Nome = "Outra Empresa", Ativo = true };
        fixture.Cadastro.Estipulantes.Add(outro); await fixture.Cadastro.SaveChangesAsync();
        await handler.SalvarAsync(cliente.PublicId, null, new(outro.PublicId, "XyZ-002"), default);
        var lista = await handler.ListarAsync(cliente.PublicId, 1, 20, default);
        Assert.Equal(3, lista.TotalCount);
        Assert.Equal("XyZ-002", lista.Items.Single(v => v.PublicId == segundo).Matricula);
        var opcoes = await new ListarVinculosVidaHandler(Vidas, Cadastro).Handle(apolice.PublicId, cliente.PublicId, 1, 1, default);
        Assert.Equal(2, opcoes.TotalCount); Assert.Single(opcoes.Items);
        var vida = await Participar(cliente, apolice, segundo);
        var detalhe = await new ApolicesQueries(fixture.DbContext, cadastro: Cadastro).ObterApoliceVidaPorPublicIdAsync(apolice.PublicId, vida, default);
        Assert.Equal(segundo, detalhe!.ClienteVinculoPublicId); Assert.Equal("XyZ-002", detalhe.VinculoMatricula);
        Assert.False(detalhe.PossuiPropostas);
    }

    [Fact]
    public async Task InativaLegadoIncompletoSemReescreverIdentidade()
    {
        var (cliente, _, _, _) = await Preparar();
        var legado = new ClienteVinculoModel(); fixture.Cadastro.Vinculos.Add(legado);
        fixture.Cadastro.Entry(legado).Property(v => v.ClienteId).CurrentValue = cliente.Id;
        fixture.Cadastro.Entry(legado).Property(v => v.PessoaId).CurrentValue = cliente.PessoaId;
        fixture.Cadastro.Entry(legado).Property(v => v.Ativo).CurrentValue = true;
        fixture.Cadastro.Entry(legado).Property(v => v.CreatedAt).CurrentValue = DateTime.UtcNow;
        await fixture.Cadastro.SaveChangesAsync();
        await Vinculos().SituacaoAsync(cliente.PublicId, legado.PublicId, false, default);
        Assert.False(legado.Ativo); Assert.Null(legado.EstipulanteId); Assert.Null(legado.Matricula);
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => Vinculos().SituacaoAsync(cliente.PublicId, legado.PublicId, true, default));
    }

    [Theory]
    [InlineData("ausente")]
    [InlineData("outro-cliente")]
    [InlineData("outro-estipulante")]
    [InlineData("inativo")]
    public async Task VidaRejeitaVinculoInvalidoSemGravarParticipacao(string caso)
    {
        var (cliente, estip, apolice, vinculo) = await Preparar();
        Guid? id = vinculo.PublicId;
        if (caso == "ausente") id = null;
        if (caso == "inativo") { vinculo.Alterar(estip.Id, vinculo.Matricula!, false); await fixture.Cadastro.SaveChangesAsync(); }
        if (caso == "outro-cliente") id = (await Preparar()).Vinculo.PublicId;
        if (caso == "outro-estipulante")
        {
            var outro = (await Preparar()).Estipulante;
            vinculo.Alterar(outro.Id, vinculo.Matricula!, true); await fixture.Cadastro.SaveChangesAsync();
        }
        await Assert.ThrowsAsync<ValidacaoException>(() => Participar(cliente, apolice, id));
        Assert.False(await fixture.DbContext.ApoliceVidas.AnyAsync(v => v.ApoliceId == apolice.Id));
    }

    [Fact]
    public async Task TrocaAntesDaPropostaEInativacaoPreservamHistorico()
    {
        var (cliente, estip, apolice, primeiro) = await Preparar();
        var segundo = new ClienteVinculoModel(cliente.Id, cliente.PessoaId, estip.Id, "SEG-002");
        fixture.Cadastro.Vinculos.Add(segundo); await fixture.Cadastro.SaveChangesAsync();
        var vidaPublicId = await Participar(cliente, apolice, primeiro.PublicId);
        await Alterar.Handle(new() { ApolicePublicId = apolice.PublicId, ApoliceVidaPublicId = vidaPublicId, ClienteVinculoPublicId = segundo.PublicId }, default);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == vidaPublicId);
        Assert.Equal(segundo.Id, vida.ClienteVinculoId);
        var proposta = await Proposta(vida, cliente.PessoaId, estip.Id);
        await Assert.ThrowsAsync<ValidacaoException>(() => Alterar.Handle(new()
        {
            ApolicePublicId = apolice.PublicId,
            ApoliceVidaPublicId = vidaPublicId,
            ClienteVinculoPublicId = primeiro.PublicId
        }, default));
        await Assert.ThrowsAsync<ClienteInvalidoException>(() => Vinculos().SalvarAsync(cliente.PublicId, segundo.PublicId, new(estip.PublicId, "OUTRA"), default));
        var erroSql = await Assert.ThrowsAsync<PostgresException>(() => fixture.Cadastro.Database.ExecuteSqlInterpolatedAsync($"UPDATE cadastro.cliente_vinculo SET matricula='ALTERADA' WHERE id={segundo.Id}"));
        Assert.Equal("ck_cliente_vinculo_identidade_em_uso", erroSql.ConstraintName);
        await Vinculos().SalvarAsync(cliente.PublicId, segundo.PublicId, new(estip.PublicId, segundo.Matricula!, false), default);
        await Alterar.Handle(new()
        {
            ApolicePublicId = apolice.PublicId,
            ApoliceVidaPublicId = vidaPublicId,
            ClienteVinculoPublicId = segundo.PublicId,
            Observacao = "EDIÇÃO PERMITIDA"
        }, default);
        var detalhe = await new ApolicesQueries(fixture.DbContext, cadastro: Cadastro).ObterApoliceVidaPorPublicIdAsync(apolice.PublicId, vidaPublicId, default);
        Assert.True(detalhe!.PossuiPropostas); Assert.Equal(segundo.PublicId, detalhe.ClienteVinculoPublicId);
        Assert.Equal("Edição Permitida", detalhe.Observacao);
        Assert.Equal(segundo.Id, (await fixture.DbContext.Proposta.AsNoTracking().SingleAsync(p => p.Id == proposta.Id)).ClienteVinculoId);
        var disponiveis = await Cadastro.ListarVinculosAsync(cliente.PublicId, estip.Id, 1, 20, default);
        Assert.DoesNotContain(disponiveis.Items, v => v.PublicId == segundo.PublicId);
    }

    private async Task<Propostum> Proposta(ApoliceVidaModel vida, long pessoaId, long estipulanteId)
    {
        var status = new PropostaStatus { Id = (short)((await fixture.DbContext.PropostaStatuses.MaxAsync(s => (short?)s.Id) ?? 0) + 1), Codigo = Guid.NewGuid().ToString("N")[..20], Nome = "Status do Teste" };
        fixture.DbContext.PropostaStatuses.Add(status); await fixture.DbContext.SaveChangesAsync();
        var proposta = new Propostum
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = vida.ApoliceId,
            ApoliceVidaId = vida.Id,
            ClienteId = vida.ClienteId,
            PessoaId = pessoaId,
            ClienteVinculoId = vida.ClienteVinculoId!.Value,
            EstipulanteId = estipulanteId,
            SeguradoraId = 1,
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        fixture.DbContext.Proposta.Add(proposta); await fixture.DbContext.SaveChangesAsync();
        return proposta;
    }

    [Fact]
    public async Task FalhaDaAuditoriaReverteCadastroEVidaEmConexaoIndependente()
    {
        var (cliente, estip, apolice, vinculo) = await Preparar();
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Vinculos(falha.Object).SalvarAsync(cliente.PublicId, null, new(estip.PublicId, "ROLLBACK"), default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Criar(falha.Object).Handle(new()
        { ApolicePublicId = apolice.PublicId, ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = vinculo.PublicId }, default));
        await using var leitura = new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(fixture.ConnectionString).UseSnakeCaseNamingConvention().Options);
        Assert.False(await leitura.Vinculos.AnyAsync(v => v.ClienteId == cliente.Id && v.Matricula == "ROLLBACK"));
        await using var conexao = new NpgsqlConnection(fixture.ConnectionString); await conexao.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT count(*) FROM seguro.apolice_vida WHERE apolice_id = @id", conexao);
        cmd.Parameters.AddWithValue("id", apolice.Id); Assert.Equal(0L, await cmd.ExecuteScalarAsync());
    }

    [Fact]
    public async Task MigracaoGeraUuidsDistintosSemAlterarMatriculasOriginais()
    {
        var (cliente, estip, _, _) = await Preparar();
        fixture.Cadastro.Vinculos.Add(new(cliente.Id, cliente.PessoaId, estip.Id, "SeG-Original")); await fixture.Cadastro.SaveChangesAsync();
        var antes = await fixture.Cadastro.Vinculos.OrderBy(v => v.Id).Select(v => v.Matricula).ToListAsync();
        await using var tx = await fixture.Cadastro.Database.BeginTransactionAsync();
        await fixture.Cadastro.Database.ExecuteSqlRawAsync("ALTER TABLE cadastro.cliente_vinculo DROP COLUMN public_id");
        var migration = new WebApolice.Modulos.Cadastro.Infrastructure.Migrations.IdentificarClienteVinculo();
        var comandos = fixture.Cadastro.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, fixture.Cadastro.Model);
        foreach (var comando in comandos) await fixture.Cadastro.Database.ExecuteSqlRawAsync(comando.CommandText);
        var depois = await fixture.Cadastro.Vinculos.AsNoTracking().OrderBy(v => v.Id).ToListAsync();
        Assert.Equal(antes, depois.Select(v => v.Matricula));
        Assert.Equal(depois.Count, depois.Select(v => v.PublicId).Distinct().Count()); Assert.DoesNotContain(depois, v => v.PublicId == Guid.Empty);
        await tx.RollbackAsync(); fixture.Cadastro.ChangeTracker.Clear();
    }

    [Fact]
    public async Task BancoImpedeTrocaConcorrenteComGravacaoDeProposta()
    {
        var (cliente, estip, apolice, vinculo) = await Preparar();
        var segundo = new ClienteVinculoModel(cliente.Id, cliente.PessoaId, estip.Id, "CONCORRENTE");
        fixture.Cadastro.Vinculos.Add(segundo); await fixture.Cadastro.SaveChangesAsync();
        var vidaId = await Participar(cliente, apolice, vinculo.PublicId);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.PublicId == vidaId);
        await using var tx = await fixture.DbContext.Database.BeginTransactionAsync();
        await Proposta(vida, cliente.PessoaId, estip.Id);
        await using var outra = new NpgsqlConnection(fixture.ConnectionString); await outra.OpenAsync();
        await using var cmd = new NpgsqlCommand("UPDATE seguro.apolice_vida SET cliente_vinculo_id=@v WHERE id=@id", outra);
        cmd.Parameters.AddWithValue("v", segundo.Id); cmd.Parameters.AddWithValue("id", vida.Id);
        var tentativa = cmd.ExecuteNonQueryAsync();
        var limite = DateTime.UtcNow.AddSeconds(5);
        bool bloqueada = false;
        while (DateTime.UtcNow < limite && !tentativa.IsCompleted)
        {
            await using var consulta = new NpgsqlCommand("SELECT cardinality(pg_blocking_pids(@pid)) > 0", fixture.Connection);
            consulta.Parameters.AddWithValue("pid", outra.ProcessID);
            bloqueada = (bool)(await consulta.ExecuteScalarAsync())!;
            if (bloqueada) break;
            await Task.Delay(10);
        }
        Assert.True(bloqueada);
        await tx.CommitAsync();
        var erro = await Assert.ThrowsAsync<PostgresException>(() => tentativa);
        Assert.Equal("ck_apolice_vida_vinculo", erro.ConstraintName);
        fixture.DbContext.ChangeTracker.Clear();
        Assert.Equal(vinculo.Id, (await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Id == vida.Id)).ClienteVinculoId);
    }
}
