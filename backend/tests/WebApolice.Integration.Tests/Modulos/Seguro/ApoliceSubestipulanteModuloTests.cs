using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularModulo;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarModulo;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModulo;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
using Xunit;
using Npgsql;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

[Collection("SeguroTests")]
public class ApoliceSubestipulanteModuloTests : IClassFixture<SeguroIntegrationTestFixture>
{
    private readonly SeguroIntegrationTestFixture _fixture;

    public ApoliceSubestipulanteModuloTests(SeguroIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task SeedDataAsync(Guid apolicePublicId, Guid subestipulantePublicId, Guid moduloPublicId, bool seedVinculoPai = true)
    {
        // 1. Apólice
        var apolice = new ApoliceModel
        {
            PublicId = apolicePublicId,
            EstipulanteId = 1,
            SeguradoraId = 1,
            Nome = "Apolice Teste Módulo",
            DataInicioVigencia = new DateOnly(2025, 1, 1),
            Status = "Vigente",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        // 2. Subestipulante (Cross-module mock)
        var conn = _fixture.DbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                INSERT INTO core.pessoa (id, tipo, nome, created_at, updated_at) VALUES (1001, 'J', 'Pessoa Sub', now(), now()) ON CONFLICT DO NOTHING;
                INSERT INTO cadastro.subestipulante (id, pessoa_id, public_id, ativo, created_at, updated_at) 
                VALUES (1001, 1001, '{subestipulantePublicId}', true, now(), now()) ON CONFLICT DO NOTHING;";
            await cmd.ExecuteNonQueryAsync();

            using var cmdModulo = conn.CreateCommand();
            cmdModulo.CommandText = $@"
                INSERT INTO cadastro.modulo (id, public_id, nome, ativo, created_at, updated_at) 
                VALUES (1001, '{moduloPublicId}', 'Módulo Teste', true, now(), now()) ON CONFLICT DO NOTHING;";
            await cmdModulo.ExecuteNonQueryAsync();
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }

        // 3. Vínculo Pai
        if (seedVinculoPai)
        {
            var subVinculo = new ApoliceSubestipulanteModel
            {
                ApoliceId = apolice.Id,
                SubestipulanteId = 1001,
                DataInicio = new DateOnly(2025, 1, 1),
                DataFim = new DateOnly(2025, 12, 31),
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _fixture.DbContext.ApoliceSubestipulantes.Add(subVinculo);
            await _fixture.DbContext.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task VincularModulo_ComSucesso()
    {
        var apolicePublicId = Guid.NewGuid();
        var subestipulantePublicId = Guid.NewGuid();
        var moduloPublicId = Guid.NewGuid();
        
        await SeedDataAsync(apolicePublicId, subestipulantePublicId, moduloPublicId);

        var handler = new VincularModuloApoliceHandler(_fixture.DbContext);
        var command = new VincularModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            SubestipulantePublicId = subestipulantePublicId,
            ModuloPublicId = moduloPublicId,
            DataInicio = new DateOnly(2025, 2, 1),
            DataFim = new DateOnly(2025, 11, 30),
            UsuarioPublicId = Guid.NewGuid()
        };

        var vinculoId = await handler.Handle(command, CancellationToken.None);

        vinculoId.Should().BeGreaterThan(0);

        var vinculo = await _fixture.DbContext.ApoliceSubestipulanteModulos.FindAsync(vinculoId);
        vinculo.Should().NotBeNull();
        vinculo!.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task VincularModulo_DuplicidadeAtiva_RetornaErro()
    {
        var apolicePublicId = Guid.NewGuid();
        var subestipulantePublicId = Guid.NewGuid();
        var moduloPublicId = Guid.NewGuid();
        
        await SeedDataAsync(apolicePublicId, subestipulantePublicId, moduloPublicId);

        var handler = new VincularModuloApoliceHandler(_fixture.DbContext);
        var command = new VincularModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            SubestipulantePublicId = subestipulantePublicId,
            ModuloPublicId = moduloPublicId,
            DataInicio = new DateOnly(2025, 2, 1)
        };

        await handler.Handle(command, CancellationToken.None); // Primeiro vínculo funciona

        // Segundo deve falhar com erro de negócio (antes de chegar na constraint do banco)
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*já está vinculado ativamente*");
    }

    [Fact]
    public async Task AtualizarModulo_VigenciaForaDoPai_RetornaErro()
    {
        var apolicePublicId = Guid.NewGuid();
        var subestipulantePublicId = Guid.NewGuid();
        var moduloPublicId = Guid.NewGuid();
        
        await SeedDataAsync(apolicePublicId, subestipulantePublicId, moduloPublicId);

        var vincularHandler = new VincularModuloApoliceHandler(_fixture.DbContext);
        await vincularHandler.Handle(new VincularModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            SubestipulantePublicId = subestipulantePublicId,
            ModuloPublicId = moduloPublicId
        }, CancellationToken.None);

        var handler = new AtualizarModuloApoliceHandler(_fixture.DbContext);
        var command = new AtualizarModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            SubestipulantePublicId = subestipulantePublicId,
            ModuloPublicId = moduloPublicId,
            DataInicio = new DateOnly(2024, 12, 31) // Antes do pai (01/01/2025)
        };

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*não pode ser anterior à data de início do vínculo pai*");
    }

    [Fact]
    public async Task InativarModulo_ComVidaAtiva_Bloqueia()
    {
        var apolicePublicId = Guid.NewGuid();
        var subestipulantePublicId = Guid.NewGuid();
        var moduloPublicId = Guid.NewGuid();
        
        await SeedDataAsync(apolicePublicId, subestipulantePublicId, moduloPublicId);

        var vincularHandler = new VincularModuloApoliceHandler(_fixture.DbContext);
        var vinculoId = await vincularHandler.Handle(new VincularModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            SubestipulantePublicId = subestipulantePublicId,
            ModuloPublicId = moduloPublicId
        }, CancellationToken.None);

        // Adiciona Vida ativa vinculada ao módulo
        var apolice = await _fixture.DbContext.Apolices.FirstAsync(a => a.PublicId == apolicePublicId);
        var vinculoPai = await _fixture.DbContext.ApoliceSubestipulantes.FirstAsync(s => s.ApoliceId == apolice.Id);
        
        _fixture.DbContext.ApoliceVidas.Add(new ApoliceVidaModel
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ApoliceSubestipulanteId = vinculoPai.Id,
            ApoliceSubestipulanteModuloId = vinculoId,
            ClienteId = 1,
            Ativo = true
        });
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new InativarModuloApoliceHandler(_fixture.DbContext);
        var act = async () => await handler.Handle(new InativarModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            SubestipulantePublicId = subestipulantePublicId,
            ModuloPublicId = moduloPublicId
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*existem Vidas ativas associadas*");
    }
}
