using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MediatR;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ConfirmarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EfetivarVigencia;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.PreencherRascunho;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Contracts;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Api.Tests.Modulos.Apolices;

public sealed class PropostasFluxoAuthTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Theory]
    [InlineData("lista", "propostas.visualizar")]
    [InlineData("detalhe", "propostas.visualizar")]
    [InlineData("criar", "propostas.inserir")]
    [InlineData("preencher", "propostas.alterar")]
    [InlineData("confirmar", "propostas.alterar")]
    [InlineData("efetivar", "propostas.alterar")]
    [InlineData("reagendar", "propostas.alterar")]
    [InlineData("cancelar", "propostas.alterar")]
    [InlineData("cancelar_vigencia", "propostas.alterar")]
    [InlineData("efetivar_cancelamento", "propostas.alterar")]
    [InlineData("descartar", "propostas.alterar")]
    public async Task ValidaPermissaoEspecificaECatalogoAntesDoHandler(string operacao, string permissao)
    {
        var seguranca = new SegurancaStub(); var handlers = new Handlers();
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.AddScoped<IPermissoesEfetivasService>(_ => seguranca);
            services.AddScoped<IDisponibilidadeCatalogo>(_ => seguranca);
            services.AddScoped<IRequestHandler<ObterPropostasQuery, PagedResult<PropostaDto>>>(_ => handlers);
            services.AddScoped<IRequestHandler<ObterPropostaQuery, PropostaDetalheDto?>>(_ => handlers);
            services.AddScoped<IRequestHandler<SolicitarCancelamentoVigenciaCommand, CancelamentoVigenciaResultado>>(_ => handlers);
            services.AddScoped<IRequestHandler<EfetivarCancelamentoVigenciaCommand, CancelamentoVigenciaResultado>>(_ => handlers);
            services.AddScoped<IRequestHandler<DescartarRascunhoCommand, GestaoAgendamentoResultado>>(_ => handlers);
            services.AddScoped<IRequestHandler<ReagendarPropostaCommand, GestaoAgendamentoResultado>>(_ => handlers);
            services.AddScoped<IRequestHandler<CancelarAgendamentoCommand, GestaoAgendamentoResultado>>(_ => handlers);
            services.AddScoped<IRequestHandler<ConfirmarPropostaCommand, ConfirmarPropostaResultado>>(_ => handlers);
            services.AddScoped<IRequestHandler<EfetivarVigenciaPropostaCommand, EfetivarVigenciaResultado>>(_ => handlers);
            services.AddScoped<IRequestHandler<PreencherRascunhoCommand, int>>(_ => handlers);
            services.AddScoped<IRequestHandler<CriarPropostaDaVidaCommand, Guid>>(_ => handlers);
        }));
        using var client = host.CreateClient();
        var apolice = Guid.NewGuid(); var vida = Guid.NewGuid(); var origem = Guid.NewGuid();
        async Task<HttpResponseMessage> Enviar() => operacao switch
        {
            "cancelar_vigencia" => await client.PostAsJsonAsync($"/api/propostas/{handlers.Id}/vigencia/cancelamento", new { revisaoEsperada = 8, dataCancelamentoVigencia = "2026-11-12", observacao = "ENCERRAMENTO", publicId = Guid.NewGuid() }),
            "efetivar_cancelamento" => await client.PostAsJsonAsync($"/api/propostas/{handlers.Id}/vigencia/cancelamento/efetivar", new { revisaoEsperada = 8, publicId = Guid.NewGuid() }),
            "descartar" => await client.PostAsJsonAsync($"/api/propostas/{handlers.Id}/rascunho/descarte", new { revisaoEsperada = 8, observacao = "ENCERRAMENTO", publicId = Guid.NewGuid() }),
            "reagendar" => await client.PostAsJsonAsync($"/api/propostas/{handlers.Id}/agendamento/reagendar", new { revisaoEsperada = 8, dataInicioVigencia = "2026-12-01", observacao = "AÇÃO DE PROTEÇÃO", publicId = Guid.NewGuid() }),
            "cancelar" => await client.PostAsJsonAsync($"/api/propostas/{handlers.Id}/agendamento/cancelar", new { revisaoEsperada = 8, observacao = "AÇÃO DE PROTEÇÃO", publicId = Guid.NewGuid() }),
            "lista" => await client.GetAsync($"/api/propostas?apolicePublicId={apolice}&apoliceVidaPublicId={vida}"),
            "confirmar" => await client.PostAsJsonAsync($"/api/propostas/{handlers.Id}/confirmacao", new { revisaoEsperada = 7, dataInicioVigencia = "2026-11-12", publicId = Guid.NewGuid() }),
            "efetivar" => await client.PostAsJsonAsync($"/api/propostas/{handlers.Id}/vigencia/efetivar", new { revisaoEsperada = 8, publicId = Guid.NewGuid() }),
            "preencher" => await client.PutAsJsonAsync($"/api/propostas/{handlers.Id}/rascunho", new { revisaoEsperada = 0, observacao = "AÇÃO DE PROTEÇÃO", beneficiarios = Array.Empty<object>(), coberturas = Array.Empty<object>(), publicId = Guid.NewGuid() }),
            "detalhe" => await client.GetAsync($"/api/propostas/{handlers.Id}"),
            _ => await client.PostAsJsonAsync($"/api/apolices/{apolice}/vidas/{vida}/propostas", new { propostaOrigemPublicId = origem, clienteId = -999 })
        };
        using (var response = await Enviar()) Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: "fixture-propostas"));
        seguranca.Permissao = operacao == "criar" ? "propostas.alterar" : "propostas.inserir";
        using (var response = await Enviar()) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        seguranca.Permissao = permissao; seguranca.Disponivel = false; seguranca.AcessoTotal = true;
        using (var response = await Enviar()) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, handlers.Chamadas);
        seguranca.Disponivel = true; seguranca.AcessoTotal = false;
        using (var response = await Enviar()) Assert.Equal(operacao == "criar" ? HttpStatusCode.Created : HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, handlers.Chamadas);
        if (operacao == "criar") Assert.Equal(new CriarPropostaDaVidaCommand(apolice, vida, origem), handlers.Criacao);
        if (operacao == "lista") { Assert.Equal(apolice, handlers.Lista!.ApolicePublicId); Assert.Equal(vida, handlers.Lista.ApoliceVidaPublicId); }
        if (operacao == "preencher") { Assert.Equal(handlers.Id, handlers.Preenchimento!.PublicId); Assert.Equal("Ação de Proteção", handlers.Preenchimento.Dados.Observacao); }
        if (operacao == "confirmar") { Assert.Equal(handlers.Id, handlers.Confirmacao!.PublicId); Assert.Equal(new DateOnly(2026, 11, 12), handlers.Confirmacao.Dados.DataInicioVigencia); Assert.Equal(7, handlers.Confirmacao.Dados.RevisaoEsperada); }
        if (operacao == "efetivar") { Assert.Equal(handlers.Id, handlers.Efetivacao!.PublicId); Assert.Equal(8, handlers.Efetivacao.RevisaoEsperada); }
        if (operacao == "reagendar") { Assert.Equal(handlers.Id, handlers.Reagendamento!.PublicId); Assert.Equal("Ação de Proteção", handlers.Reagendamento.Dados.Observacao); Assert.Equal(new DateOnly(2026, 12, 1), handlers.Reagendamento.Dados.DataInicioVigencia); Assert.Equal(8, handlers.Reagendamento.Dados.RevisaoEsperada); }
        if (operacao == "cancelar") { Assert.Equal(handlers.Id, handlers.Cancelamento!.PublicId); Assert.Equal("Ação de Proteção", handlers.Cancelamento.Dados.Observacao); Assert.Equal(8, handlers.Cancelamento.Dados.RevisaoEsperada); }
        if (operacao == "cancelar_vigencia") { Assert.Equal(handlers.Id, handlers.CancelamentoVigencia!.PublicId); Assert.Equal("Encerramento", handlers.CancelamentoVigencia.Dados.Observacao); Assert.Equal(8, handlers.CancelamentoVigencia.Dados.RevisaoEsperada); Assert.Equal(new DateOnly(2026, 11, 12), handlers.CancelamentoVigencia.Dados.DataCancelamentoVigencia); }
        if (operacao == "efetivar_cancelamento") { Assert.Equal(handlers.Id, handlers.EfetivarCancelamento!.PublicId); Assert.Equal(8, handlers.EfetivarCancelamento.RevisaoEsperada); }
        if (operacao == "descartar") { Assert.Equal(handlers.Id, handlers.Descarte!.PublicId); Assert.Equal("Encerramento", handlers.Descarte.Dados.Observacao); Assert.Equal(8, handlers.Descarte.Dados.RevisaoEsperada); }
        if (operacao == "detalhe") Assert.Equal(handlers.Id, handlers.Detalhe!.PublicId);
    }

    private sealed class Handlers : IRequestHandler<SolicitarCancelamentoVigenciaCommand, CancelamentoVigenciaResultado>, IRequestHandler<EfetivarCancelamentoVigenciaCommand, CancelamentoVigenciaResultado>, IRequestHandler<DescartarRascunhoCommand, GestaoAgendamentoResultado>, IRequestHandler<ReagendarPropostaCommand, GestaoAgendamentoResultado>, IRequestHandler<CancelarAgendamentoCommand, GestaoAgendamentoResultado>, IRequestHandler<ObterPropostasQuery, PagedResult<PropostaDto>>,
        IRequestHandler<ObterPropostaQuery, PropostaDetalheDto?>, IRequestHandler<CriarPropostaDaVidaCommand, Guid>, IRequestHandler<PreencherRascunhoCommand, int>, IRequestHandler<ConfirmarPropostaCommand, ConfirmarPropostaResultado>, IRequestHandler<EfetivarVigenciaPropostaCommand, EfetivarVigenciaResultado>
    {
        public SolicitarCancelamentoVigenciaCommand? CancelamentoVigencia { get; private set; }
        public EfetivarCancelamentoVigenciaCommand? EfetivarCancelamento { get; private set; }
        public DescartarRascunhoCommand? Descarte { get; private set; }
        public Task<CancelamentoVigenciaResultado> Handle(SolicitarCancelamentoVigenciaCommand request, CancellationToken ct) { Chamadas++; CancelamentoVigencia = request; return Task.FromResult(new CancelamentoVigenciaResultado(true, false, 9)); }
        public Task<CancelamentoVigenciaResultado> Handle(EfetivarCancelamentoVigenciaCommand request, CancellationToken ct) { Chamadas++; EfetivarCancelamento = request; return Task.FromResult(new CancelamentoVigenciaResultado(true, false, 9)); }
        public Task<GestaoAgendamentoResultado> Handle(DescartarRascunhoCommand request, CancellationToken ct) { Chamadas++; Descarte = request; return Task.FromResult(new GestaoAgendamentoResultado(9)); }
        public ReagendarPropostaCommand? Reagendamento { get; private set; }
        public CancelarAgendamentoCommand? Cancelamento { get; private set; }
        public Task<GestaoAgendamentoResultado> Handle(ReagendarPropostaCommand request, CancellationToken ct) { Chamadas++; Reagendamento = request; return Task.FromResult(new GestaoAgendamentoResultado(9)); }
        public Task<GestaoAgendamentoResultado> Handle(CancelarAgendamentoCommand request, CancellationToken ct) { Chamadas++; Cancelamento = request; return Task.FromResult(new GestaoAgendamentoResultado(9)); }
        public ConfirmarPropostaCommand? Confirmacao { get; private set; }
        public EfetivarVigenciaPropostaCommand? Efetivacao { get; private set; }
        public Task<ConfirmarPropostaResultado> Handle(ConfirmarPropostaCommand request, CancellationToken ct) { Chamadas++; Confirmacao = request; return Task.FromResult(new ConfirmarPropostaResultado(true, false, 8)); }
        public Task<EfetivarVigenciaResultado> Handle(EfetivarVigenciaPropostaCommand request, CancellationToken ct) { Chamadas++; Efetivacao = request; return Task.FromResult(new EfetivarVigenciaResultado(true, false, 9, null)); }
        public PreencherRascunhoCommand? Preenchimento { get; private set; }
        public Task<int> Handle(PreencherRascunhoCommand request, CancellationToken ct) { Chamadas++; Preenchimento = request; return Task.FromResult(1); }
        public Guid Id { get; } = Guid.NewGuid();
        public int Chamadas { get; private set; }
        public CriarPropostaDaVidaCommand? Criacao { get; private set; }
        public ObterPropostasQuery? Lista { get; private set; }
        public ObterPropostaQuery? Detalhe { get; private set; }
        public Task<Guid> Handle(CriarPropostaDaVidaCommand request, CancellationToken ct) { Chamadas++; Criacao = request; return Task.FromResult(Id); }
        public Task<PropostaDetalheDto?> Handle(ObterPropostaQuery request, CancellationToken ct) { Chamadas++; Detalhe = request; return Task.FromResult<PropostaDetalheDto?>(new() { PublicId = Id }); }
        public Task<PagedResult<PropostaDto>> Handle(ObterPropostasQuery request, CancellationToken ct) { Chamadas++; Lista = request; return Task.FromResult(new PagedResult<PropostaDto> { Items = [], Page = 1, PageSize = 20 }); }
    }
    private sealed class SegurancaStub : IPermissoesEfetivasService, IDisponibilidadeCatalogo
    {
        public string Permissao { get; set; } = "";
        public bool Disponivel { get; set; } = true;
        public bool AcessoTotal { get; set; }
        public Task<PermissoesEfetivasUsuario> CalcularPermissoesAsync(string sub, CancellationToken cancellationToken = default) => Task.FromResult(new PermissoesEfetivasUsuario(true, true, AcessoTotal, false, ["APOLICES"], ["PROPOSTAS"], [Permissao]));
        public Task<bool> PermissaoDisponivelAsync(string permissao, CancellationToken ct) => Task.FromResult(Disponivel);
        public Task<bool> RecursoDisponivelAsync(string modulo, string recurso, CancellationToken ct) => Task.FromResult(Disponivel);
    }
}
