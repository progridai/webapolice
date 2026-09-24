using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApoliceVida;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApoliceVida;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarApoliceVida;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApoliceVida;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;
using WebApolice.SharedKernel.Application.Exceptions;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

[Collection("SeguroTests")]
public class ApoliceVidaTests : IClassFixture<SeguroIntegrationTestFixture>
{
    private readonly SeguroIntegrationTestFixture _fixture;

    public ApoliceVidaTests(SeguroIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(Guid apolicePublicId, Guid clientePublicId, Guid apoliceSubgrupoPublicId, Guid apoliceModuloPublicId)> SeedDataAsync()
    {
        var apolicePublicId = Guid.NewGuid();
        var clientePublicId = Guid.NewGuid();
        var subgrupoPublicId = Guid.NewGuid();
        var moduloPublicId = Guid.NewGuid();

        var apolice = new ApoliceModel
        {
            PublicId = apolicePublicId,
            EstipulanteId = 1,
            SeguradoraId = 1,
            Nome = "Apólice Teste Vidas",
            DataInicioVigencia = new DateOnly(2025, 1, 1),
            Status = "Vigente",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var conn = _fixture.DbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                INSERT INTO core.pessoa (id, tipo, nome, documento_principal, created_at, updated_at) VALUES (2001, 'F', 'Cliente Teste Vida', '12345678901', now(), now()) ON CONFLICT DO NOTHING;
                INSERT INTO cadastro.cliente (id, pessoa_id, public_id, status_id, ativo, created_at, updated_at) VALUES (2001, 2001, '{clientePublicId}', 1, true, now(), now()) ON CONFLICT DO NOTHING;
                
                INSERT INTO cadastro.modulo (id, public_id, nome, ativo, created_at, updated_at) VALUES (2002, '{Guid.NewGuid()}', 'Módulo Teste Vida', true, now(), now()) ON CONFLICT DO NOTHING;";
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }

        var apoliceSubgrupo = new ApoliceSubgrupoModel
        {
            ApoliceId = apolice.Id,
            PublicId = subgrupoPublicId,
            Nome = "Subgrupo Teste Vida",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.ApoliceSubgrupos.Add(apoliceSubgrupo);

        var apoliceModulo = new ApoliceModuloModel
        {
            ApoliceId = apolice.Id,
            ModuloId = 2002,
            PublicId = moduloPublicId,
            DataInicio = new DateOnly(2025, 1, 1),
            DataFim = new DateOnly(2025, 12, 31),
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.ApoliceModulos.Add(apoliceModulo);

        await _fixture.DbContext.SaveChangesAsync();

        return (apolicePublicId, clientePublicId, subgrupoPublicId, moduloPublicId);
    }

    [Fact]
    public async Task CriarVida_CombinacoesValidas()
    {
        var data = await SeedDataAsync();
        var handler = new CriarApoliceVidaHandler(_fixture.DbContext);

        // Direto
        var vidaDiretaId = await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            DataInicioVigencia = new DateOnly(2025, 2, 1),
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        vidaDiretaId.Should().NotBeEmpty();

        // Subgrupo
        var vidaSubId = await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            ApoliceSubgrupoPublicId = data.apoliceSubgrupoPublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        vidaSubId.Should().NotBeEmpty();

        // Módulo
        var vidaModId = await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            ApoliceModuloPublicId = data.apoliceModuloPublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        vidaModId.Should().NotBeEmpty();

        // Subgrupo + Módulo
        var vidaSubModId = await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            ApoliceSubgrupoPublicId = data.apoliceSubgrupoPublicId,
            ApoliceModuloPublicId = data.apoliceModuloPublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        vidaSubModId.Should().NotBeEmpty();

        // Queries
        IApolicesQueries queries = new ApolicesQueries(_fixture.DbContext);
        
        var list = await queries.ListarVidasPaginadoAsync(data.apolicePublicId, 1, 50, null, null, null, null, null, CancellationToken.None);
        list.TotalCount.Should().BeGreaterThanOrEqualTo(4);
        
        var vDireto = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaDiretaId, CancellationToken.None);
        vDireto.Should().NotBeNull();
        vDireto!.ApoliceSubgrupoPublicId.Should().BeNull();
        vDireto.ApoliceModuloPublicId.Should().BeNull();

        var vSub = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaSubId, CancellationToken.None);
        vSub!.ApoliceSubgrupoPublicId.Should().Be(data.apoliceSubgrupoPublicId);
        vSub.ApoliceModuloPublicId.Should().BeNull();
        
        var vMod = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaModId, CancellationToken.None);
        vMod!.ApoliceSubgrupoPublicId.Should().BeNull();
        vMod.ApoliceModuloPublicId.Should().Be(data.apoliceModuloPublicId);

        var vSubMod = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaSubModId, CancellationToken.None);
        vSubMod!.ApoliceSubgrupoPublicId.Should().Be(data.apoliceSubgrupoPublicId);
        vSubMod.ApoliceModuloPublicId.Should().Be(data.apoliceModuloPublicId);
    }

    [Fact]
    public async Task CriarVida_SubgrupoOuModuloDeOutraApolice_RetornaErro()
    {
        var data = await SeedDataAsync();
        var handler = new CriarApoliceVidaHandler(_fixture.DbContext);

        var outraApolice = new ApoliceModel
        {
            PublicId = Guid.NewGuid(),
            EstipulanteId = 1,
            SeguradoraId = 1,
            Nome = "Outra Apólice",
            DataInicioVigencia = new DateOnly(2025, 1, 1),
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Apolices.Add(outraApolice);
        await _fixture.DbContext.SaveChangesAsync();

        var outroSubgrupo = new ApoliceSubgrupoModel
        {
            ApoliceId = outraApolice.Id,
            PublicId = Guid.NewGuid(),
            Nome = "Outro",
            Ativo = true
        };
        _fixture.DbContext.ApoliceSubgrupos.Add(outroSubgrupo);
        await _fixture.DbContext.SaveChangesAsync();

        var act1 = async () => await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            ApoliceSubgrupoPublicId = outroSubgrupo.PublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        await act1.Should().ThrowAsync<ValidacaoException>().WithMessage("*pertence a outra Apólice*");
    }

    [Fact]
    public async Task CriarVida_ForaDaVigenciaDaApolice_RetornaErro()
    {
        var data = await SeedDataAsync();
        var handler = new CriarApoliceVidaHandler(_fixture.DbContext);

        var act = async () => await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            DataInicioVigencia = new DateOnly(2024, 12, 31), // Apólice inicia em 01/01/2025
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*não pode ser anterior à data de início de Apólice*");
    }

    [Fact]
    public async Task AlterarEInativarVida_FluxoCompleto()
    {
        var data = await SeedDataAsync();
        var criarHandler = new CriarApoliceVidaHandler(_fixture.DbContext);
        
        var vidaId = await criarHandler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        var alterarHandler = new AlterarApoliceVidaHandler(_fixture.DbContext);
        await alterarHandler.Handle(new AlterarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ApoliceVidaPublicId = vidaId,
            ApoliceSubgrupoPublicId = data.apoliceSubgrupoPublicId, // Adiciona subgrupo
            ApoliceModuloPublicId = data.apoliceModuloPublicId,     // Adiciona modulo
            DataInicioVigencia = new DateOnly(2025, 3, 1),
            Observacao = "Atualizado",
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        IApolicesQueries queries = new ApolicesQueries(_fixture.DbContext);
        var vidaAtualizada = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaId, CancellationToken.None);
        vidaAtualizada!.DataInicioVigencia.Should().Be(new DateOnly(2025, 3, 1));
        vidaAtualizada.Observacao.Should().Be("Atualizado");
        vidaAtualizada.ApoliceSubgrupoPublicId.Should().Be(data.apoliceSubgrupoPublicId);
        vidaAtualizada.ApoliceModuloPublicId.Should().Be(data.apoliceModuloPublicId);

        // Remover Subgrupo
        await alterarHandler.Handle(new AlterarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ApoliceVidaPublicId = vidaId,
            ApoliceSubgrupoPublicId = null, // Remove subgrupo
            ApoliceModuloPublicId = data.apoliceModuloPublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        var vidaAtualizada2 = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaId, CancellationToken.None);
        vidaAtualizada2!.ApoliceSubgrupoPublicId.Should().BeNull();
        vidaAtualizada2.ApoliceModuloPublicId.Should().Be(data.apoliceModuloPublicId);

        var inativarHandler = new InativarApoliceVidaHandler(_fixture.DbContext);
        await inativarHandler.Handle(new InativarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ApoliceVidaPublicId = vidaId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        var vidaInativada = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaId, CancellationToken.None);
        vidaInativada!.Ativo.Should().BeFalse();
        vidaInativada.Status.Should().Be("encerrada");

        // Idempotência
        var act = async () => await inativarHandler.Handle(new InativarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ApoliceVidaPublicId = vidaId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*já está encerrada*");
    }
}
