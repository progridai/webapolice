using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarModuloApolice;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarModuloApolice;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModuloApolice;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

[Collection("Seguro Integration")]
public class ApolicesModulosTests : IClassFixture<SeguroIntegrationTestFixture>, IAsyncLifetime
{
    private readonly SeguroIntegrationTestFixture _fixture;
    private Guid _usuarioId = Guid.NewGuid();

    public ApolicesModulosTests(SeguroIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Limpar tabelas para isolamento entre testes
        await _fixture.DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE seguro.apolice_modulo CASCADE;");
        await _fixture.DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE seguro.apolice CASCADE;");
        await _fixture.DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE cadastro.modulo CASCADE;");
        await _fixture.DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE seguro.apolice_historico CASCADE;");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<ApoliceModel> CriarApoliceValidaAsync()
    {
        var apolice = new ApoliceModel
        {
            PublicId = Guid.NewGuid(),
            Nome = "Apólice Teste Módulos",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DataInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            DataFimVigencia = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))
        };
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();
        return apolice;
    }

    private async Task<(long Id, Guid PublicId)> CriarModuloGlobalAsync(bool ativo = true)
    {
        // Insere fisicamente na tabela cadastro.modulo pois o DbContext de Seguro não gerencia essa escrita
        var moduloPublicId = Guid.NewGuid();
        var sql = $@"
            INSERT INTO cadastro.modulo (public_id, codigo, nome, descricao, icone, ordem, ativo, habilitado, created_at, updated_at)
            VALUES ('{moduloPublicId}', 'TESTE_{Guid.NewGuid().ToString().Substring(0,4)}', 'Módulo de Teste', 'Desc', 'icon', 1, {(ativo ? "true" : "false")}, true, now(), now())
            RETURNING id;
        ";
        
        await using var command = _fixture.DbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        await _fixture.DbContext.Database.OpenConnectionAsync();
        var id = (long)await command.ExecuteScalarAsync();
        
        return (id, moduloPublicId);
    }

    [Fact]
    public async Task Identificadores_Devem_Ser_Diferentes()
    {
        var apolice = await CriarApoliceValidaAsync();
        var moduloGlobal = await CriarModuloGlobalAsync();

        var handler = new CriarModuloApoliceHandler(_fixture.DbContext);
        var command = new CriarModuloApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            ModuloPublicId = moduloGlobal.PublicId,
            UsuarioPublicId = _usuarioId
        };

        var apoliceModuloPublicId = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, apoliceModuloPublicId);
        Assert.NotEqual(moduloGlobal.PublicId, apoliceModuloPublicId);

        var vinculoNoBanco = await _fixture.DbContext.ApoliceModulos.FirstOrDefaultAsync(x => x.PublicId == apoliceModuloPublicId);
        Assert.NotNull(vinculoNoBanco);
        Assert.Equal(apoliceModuloPublicId, vinculoNoBanco.PublicId);
        Assert.Equal(moduloGlobal.Id, vinculoNoBanco.ModuloId);
        
        // Histórico funcional criado
        var historico = await _fixture.DbContext.ApoliceHistoricos.FirstOrDefaultAsync(h => h.ApoliceId == apolice.Id);
        Assert.NotNull(historico);
        Assert.Contains("Módulo de Teste", historico.Descricao);
    }

    [Fact]
    public async Task Nao_Deve_Permitir_Modulo_Global_Inativo()
    {
        var apolice = await CriarApoliceValidaAsync();
        var moduloInativo = await CriarModuloGlobalAsync(ativo: false);

        var handler = new CriarModuloApoliceHandler(_fixture.DbContext);
        var command = new CriarModuloApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            ModuloPublicId = moduloInativo.PublicId,
            UsuarioPublicId = _usuarioId
        };

        var exception = await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("O Módulo Global está inativo", exception.Message);
    }

    [Fact]
    public async Task Duplicidade_Ativa_Deve_Falhar()
    {
        var apolice = await CriarApoliceValidaAsync();
        var modulo = await CriarModuloGlobalAsync();

        var handler = new CriarModuloApoliceHandler(_fixture.DbContext);
        var command = new CriarModuloApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            ModuloPublicId = modulo.PublicId,
            UsuarioPublicId = _usuarioId
        };

        // Primeira vinculação tem sucesso
        await handler.Handle(command, CancellationToken.None);

        // Segunda vinculação do mesmo módulo para a mesma apólice falha na regra de negócio
        var exception = await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("já está vinculado", exception.Message);
    }

    [Fact]
    public async Task Duplicidade_Pos_Inativacao_Deve_Falhar_Comportamento_PostgreSQL_Unique()
    {
        var apolice = await CriarApoliceValidaAsync();
        var modulo = await CriarModuloGlobalAsync();

        var handlerCriar = new CriarModuloApoliceHandler(_fixture.DbContext);
        var handlerInativar = new InativarModuloApoliceHandler(_fixture.DbContext);

        var apoliceModuloPublicId = await handlerCriar.Handle(new CriarModuloApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            ModuloPublicId = modulo.PublicId,
            UsuarioPublicId = _usuarioId
        }, CancellationToken.None);

        // Inativar
        await handlerInativar.Handle(new InativarModuloApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            ApoliceModuloPublicId = apoliceModuloPublicId,
            UsuarioPublicId = _usuarioId
        }, CancellationToken.None);

        var vinculoNoBanco = await _fixture.DbContext.ApoliceModulos.FirstOrDefaultAsync(x => x.PublicId == apoliceModuloPublicId);
        Assert.False(vinculoNoBanco.Ativo); // Inativado corretamente (ativo=false)
        Assert.Null(vinculoNoBanco.DeletedAt); // Soft delete não é encerramento normal (deleted_at continua nulo)

        // Tentar vincular novamente (mesmo módulo e apólice)
        var exception = await Assert.ThrowsAsync<ValidacaoException>(() => handlerCriar.Handle(new CriarModuloApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            ModuloPublicId = modulo.PublicId,
            UsuarioPublicId = _usuarioId
        }, CancellationToken.None));
        
        Assert.Contains("Já existe um vínculo inativo", exception.Message);

        // Também testamos se tentaríamos forçar via DbContext que a constraint UNIQUE do Postgres (where deleted_at is null) bloquearia
        var vinculoForcadoDuplicado = new ApoliceModuloModel
        {
            ApoliceId = apolice.Id,
            ModuloId = modulo.Id,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.ApoliceModulos.Add(vinculoForcadoDuplicado);
        
        var dbUpdateException = await Assert.ThrowsAsync<DbUpdateException>(() => _fixture.DbContext.SaveChangesAsync());
        Assert.Contains("duplicate key value violates unique constraint", dbUpdateException.InnerException.Message.ToLower());
    }

    [Fact]
    public async Task Isolamento_De_Apolice_Deve_Impedir_Mutacao_Cruzada()
    {
        var apoliceA = await CriarApoliceValidaAsync();
        var apoliceB = await CriarApoliceValidaAsync();
        var modulo = await CriarModuloGlobalAsync();

        var handlerCriar = new CriarModuloApoliceHandler(_fixture.DbContext);
        var vinculoId_A = await handlerCriar.Handle(new CriarModuloApoliceCommand
        {
            ApolicePublicId = apoliceA.PublicId,
            ModuloPublicId = modulo.PublicId,
            UsuarioPublicId = _usuarioId
        }, CancellationToken.None);

        // Tentar alterar vínculo X usando a URL da Apólice B
        var handlerAlterar = new AlterarModuloApoliceHandler(_fixture.DbContext);
        var exAlterar = await Assert.ThrowsAsync<ValidacaoException>(() => handlerAlterar.Handle(new AlterarModuloApoliceCommand
        {
            ApolicePublicId = apoliceB.PublicId,
            ApoliceModuloPublicId = vinculoId_A,
            UsuarioPublicId = _usuarioId
        }, CancellationToken.None));
        Assert.Contains("não pertence à Apólice", exAlterar.Message);

        // Tentar inativar vínculo X usando a URL da Apólice B
        var handlerInativar = new InativarModuloApoliceHandler(_fixture.DbContext);
        var exInativar = await Assert.ThrowsAsync<ValidacaoException>(() => handlerInativar.Handle(new InativarModuloApoliceCommand
        {
            ApolicePublicId = apoliceB.PublicId,
            ApoliceModuloPublicId = vinculoId_A,
            UsuarioPublicId = _usuarioId
        }, CancellationToken.None));
        Assert.Contains("não pertence à Apólice", exInativar.Message);
    }
}
