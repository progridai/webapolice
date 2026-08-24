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

    private async Task<(Guid apolicePublicId, Guid clientePublicId, Guid subestipulantePublicId, Guid moduloPublicId)> SeedDataAsync()
    {
        var apolicePublicId = Guid.NewGuid();
        var clientePublicId = Guid.NewGuid();
        var subestipulantePublicId = Guid.NewGuid();
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

                INSERT INTO core.pessoa (id, tipo, nome, created_at, updated_at) VALUES (2002, 'J', 'Sub Teste Vida', now(), now()) ON CONFLICT DO NOTHING;
                INSERT INTO cadastro.subestipulante (id, pessoa_id, public_id, ativo, created_at, updated_at) VALUES (2002, 2002, '{subestipulantePublicId}', true, now(), now()) ON CONFLICT DO NOTHING;

                INSERT INTO cadastro.modulo (id, public_id, nome, ativo, created_at, updated_at) VALUES (2002, '{moduloPublicId}', 'Módulo Teste Vida', true, now(), now()) ON CONFLICT DO NOTHING;";
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }

        var subVinculo = new ApoliceSubestipulanteModel
        {
            ApoliceId = apolice.Id,
            SubestipulanteId = 2002,
            DataInicio = new DateOnly(2025, 1, 1),
            DataFim = new DateOnly(2025, 12, 31),
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.ApoliceSubestipulantes.Add(subVinculo);
        await _fixture.DbContext.SaveChangesAsync();

        var moduloVinculo = new ApoliceSubestipulanteModuloModel
        {
            ApoliceSubestipulanteId = subVinculo.Id,
            ModuloId = 2002,
            DataInicio = new DateOnly(2025, 1, 1),
            DataFim = new DateOnly(2025, 12, 31),
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.ApoliceSubestipulanteModulos.Add(moduloVinculo);
        await _fixture.DbContext.SaveChangesAsync();

        return (apolicePublicId, clientePublicId, subestipulantePublicId, moduloPublicId);
    }

    [Fact]
    public async Task CriarVida_TresContextosValidos()
    {
        var data = await SeedDataAsync();
        var handler = new CriarApoliceVidaHandler(_fixture.DbContext);

        // Contexto A: Direto
        var vidaDiretaId = await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            DataInicioVigencia = new DateOnly(2025, 2, 1),
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        vidaDiretaId.Should().NotBeEmpty();

        // Contexto B: Subestipulante
        var vidaSubId = await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            SubestipulantePublicId = data.subestipulantePublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        vidaSubId.Should().NotBeEmpty();

        // Contexto C: Subestipulante + Módulo
        var vidaModId = await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            SubestipulantePublicId = data.subestipulantePublicId,
            ModuloPublicId = data.moduloPublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);
        vidaModId.Should().NotBeEmpty();

        // Queries
        IApolicesQueries queries = new ApolicesQueries(_fixture.DbContext);
        
        var list = await queries.ListarVidasPaginadoAsync(data.apolicePublicId, 1, 50, null, null, null, null, null, CancellationToken.None);
        list.TotalCount.Should().BeGreaterThanOrEqualTo(3);
        
        var vDireto = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaDiretaId, CancellationToken.None);
        vDireto.Should().NotBeNull();
        vDireto!.Contexto.Should().Be("direto");

        var vSub = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaSubId, CancellationToken.None);
        vSub!.Contexto.Should().Be("subestipulante");
        
        var vMod = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaModId, CancellationToken.None);
        vMod!.Contexto.Should().Be("modulo");
    }

    [Fact]
    public async Task CriarVida_ModuloSemSubestipulante_RetornaErro()
    {
        var data = await SeedDataAsync();
        var handler = new CriarApoliceVidaHandler(_fixture.DbContext);

        var act = async () => await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            SubestipulantePublicId = null,
            ModuloPublicId = data.moduloPublicId,
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*Não é possível vincular um Módulo sem informar o Subestipulante*");
    }

    [Fact]
    public async Task CriarVida_ForaDaVigenciaDoContexto_RetornaErro()
    {
        var data = await SeedDataAsync();
        var handler = new CriarApoliceVidaHandler(_fixture.DbContext);

        var act = async () => await handler.Handle(new CriarApoliceVidaCommand
        {
            ApolicePublicId = data.apolicePublicId,
            ClientePublicId = data.clientePublicId,
            SubestipulantePublicId = data.subestipulantePublicId, // Pai vai de 01/01/2025 a 31/12/2025
            DataInicioVigencia = new DateOnly(2024, 12, 31),
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*não pode ser anterior à data de início do contexto pai*");
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
            DataInicioVigencia = new DateOnly(2025, 3, 1),
            Observacao = "Atualizado",
            UsuarioPublicId = Guid.NewGuid()
        }, CancellationToken.None);

        IApolicesQueries queries = new ApolicesQueries(_fixture.DbContext);
        var vidaAtualizada = await queries.ObterApoliceVidaPorPublicIdAsync(data.apolicePublicId, vidaId, CancellationToken.None);
        vidaAtualizada!.DataInicioVigencia.Should().Be(new DateOnly(2025, 3, 1));
        vidaAtualizada.Observacao.Should().Be("Atualizado");

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
