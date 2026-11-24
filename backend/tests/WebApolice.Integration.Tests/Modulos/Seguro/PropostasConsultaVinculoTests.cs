using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    [Fact]
    public async Task ConsultaPorVinculoIsolaMatriculasPreservaVersoesEHistoricoInativo()
    {
        var (origem, nova, codigo) = await SubstituicaoAsync(new RelogioPropostas());
        var p = await fixture.DbContext.Proposta.Include(x => x.Apolice).SingleAsync(x => x.PublicId == origem);
        var cliente = await fixture.Cadastro.Clientes.SingleAsync(x => x.Id == p.ClienteId);
        var vinculo = await fixture.Cadastro.Vinculos.SingleAsync(x => x.Id == p.ClienteVinculoId);
        var outro = new ClienteVinculoModel(cliente.Id, cliente.PessoaId, vinculo.EstipulanteId!.Value, "OUTRA-MATRICULA");
        fixture.Cadastro.Add(outro); await fixture.Cadastro.SaveChangesAsync();
        var vida = new ApoliceVidaModel { PublicId = Guid.NewGuid(), ApoliceId = p.ApoliceId!.Value, ClienteId = cliente.Id, ClienteVinculoId = outro.Id, Ativo = true };
        fixture.DbContext.Add(vida); await fixture.DbContext.SaveChangesAsync();
        var outraProposta = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(new CriarPropostaCommand { ClienteId = cliente.Id, ApoliceId = p.Apolice!.PublicId, ApoliceVidaId = vida.PublicId }, default);
        var (outraPessoa, outroCodigo) = await PrepararAsync();
        await CriarHandler(outroCodigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(outraPessoa, default);
        var auditoriaAntes = await fixture.Auditoria.RegistrosAuditoria.CountAsync();
        var query = new ObterPropostasQuery { ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = vinculo.PublicId };
        var resultado = await RepositorySimulacao().ListarAsync(query, default);
        Assert.Equal(new[] { origem, nova }.Order(), resultado.Items.Select(x => x.PublicId).Order());
        Assert.Equal(2, resultado.TotalCount);
        fixture.Cadastro.Entry(vinculo).Property(x => x.Ativo).CurrentValue = false;
        await fixture.Cadastro.SaveChangesAsync();
        var historico = await RepositorySimulacao().ListarAsync(query, default);
        Assert.Equal(new[] { origem, nova }.Order(), historico.Items.Select(x => x.PublicId).Order());
        var outraMatricula = await RepositorySimulacao().ListarAsync(new() { ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = outro.PublicId }, default);
        Assert.Equal(outraProposta, Assert.Single(outraMatricula.Items).PublicId);
        var paginada = await RepositorySimulacao().ListarAsync(new() { ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = vinculo.PublicId, Pagina = 2, TamanhoPagina = 1 }, default);
        Assert.Single(paginada.Items); Assert.Equal(2, paginada.TotalCount);
        Assert.Equal(auditoriaAntes, await fixture.Auditoria.RegistrosAuditoria.CountAsync());
    }

    [Fact]
    public async Task RecusaFiltroParcialVazioDesconhecidoOuVinculoDeOutroCliente()
    {
        var (command, _) = await PrepararAsync();
        var cliente = await fixture.Cadastro.Clientes.SingleAsync(x => x.Id == command.ClienteId);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(x => x.PublicId == command.ApoliceVidaId);
        var vinculo = await fixture.Cadastro.Vinculos.SingleAsync(x => x.Id == vida.ClienteVinculoId);
        var (outro, _) = await PrepararAsync();
        var outroCliente = await fixture.Cadastro.Clientes.SingleAsync(x => x.Id == outro.ClienteId);
        ObterPropostasQuery[] invalidas = [
            new() { ClientePublicId = cliente.PublicId },
            new() { ClienteVinculoPublicId = vinculo.PublicId },
            new() { ClientePublicId = Guid.Empty, ClienteVinculoPublicId = vinculo.PublicId },
            new() { ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = Guid.Empty },
            new() { ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = Guid.NewGuid() },
            new() { ClientePublicId = outroCliente.PublicId, ClienteVinculoPublicId = vinculo.PublicId }
        ];
        foreach (var query in invalidas)
            await Assert.ThrowsAsync<ValidacaoException>(() => RepositorySimulacao().ListarAsync(query, default));
        var vazia = await RepositorySimulacao().ListarAsync(new() { ClientePublicId = cliente.PublicId, ClienteVinculoPublicId = vinculo.PublicId }, default);
        Assert.Empty(vazia.Items); Assert.Equal(0, vazia.TotalCount);
    }
}
