using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApolice;

public sealed record AlterarApoliceCommand(
    Guid PublicId,
    long EstipulanteId,
    long SeguradoraId,
    long? CorretoraId,
    string Nome,
    DateOnly DataInicioVigencia,
    DateOnly? DataFimVigencia,
    DateOnly? DataAniversario,
    string? Observacao
);
