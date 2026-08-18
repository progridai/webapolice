using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApolice;

public sealed record CriarApoliceCommand(
    long EstipulanteId,
    long SeguradoraId,
    long? CorretoraId,
    string Nome,
    DateOnly DataInicioVigencia,
    DateOnly? DataFimVigencia,
    DateOnly? DataAniversario,
    IReadOnlyList<CriarApoliceRamoCommand>? Ramos,
    IReadOnlyList<long>? SubestipulantesIds,
    string? Observacao
);

public sealed record CriarApoliceRamoCommand(
    string TipoRamo,
    string? NumeroApolice,
    decimal? IofPercentual
);

public sealed record CriarApoliceResult(
    Guid PublicId,
    long Id,
    string Nome,
    string Status,
    DateTimeOffset CreatedAt
);
