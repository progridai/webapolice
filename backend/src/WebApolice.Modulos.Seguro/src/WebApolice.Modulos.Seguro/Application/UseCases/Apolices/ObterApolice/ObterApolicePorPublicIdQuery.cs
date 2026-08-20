using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice;

public sealed record ObterApolicePorPublicIdQuery(Guid PublicId);

public sealed record ApoliceDetalheResult(
    Guid PublicId,
    string Nome,
    long EstipulanteId,
    string EstipulanteNome,
    long SeguradoraId,
    string SeguradoraNome,
    long? CorretoraId,
    string? CorretoraNome,
    DateOnly? DataInicioVigencia,
    DateOnly? DataFimVigencia,
    DateOnly? DataAniversario,
    string Status,
    bool Ativo,
    string? Observacao,
    IEnumerable<ApoliceRamoResult> Ramos,
    ApoliceConfiguracaoResult? Configuracao
);

public sealed record ApoliceRamoResult(
    Guid PublicId,
    string RamoCodigo,
    string RamoNome,
    string? NumeroApolice,
    decimal? IofPercentual,
    bool Ativo
);

public sealed record ApoliceConfiguracaoResult(
    string? TipoAdesao,
    string? Custeio,
    int? CarenciaDias,
    int? MesBaseReajuste,
    string? IndiceReajuste,
    bool CobreConjuge,
    bool ControlaExcedente,
    int? DiaCorteFaturamento,
    int? PrazoAvisoSinistroDias
);
