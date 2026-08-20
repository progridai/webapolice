using System;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;

public sealed record ListarApolicesQuery(
    int Pagina,
    int TamanhoPagina,
    string? Busca, // Busca textual em Numero, EstipulanteNome, SeguradoraNome
    string? Status,
    bool? Ativo,
    Guid? EstipulanteId,
    Guid? SeguradoraId,
    string? TipoRamo,
    DateTime? VigenciaDataReferencia
);
