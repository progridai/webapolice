using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;

public sealed record ApoliceListagemItemResult(
    Guid PublicId,
    string NumeroPrincipal,
    string EstipulanteNome,
    string SeguradoraNome,
    DateOnly? DataInicioVigencia,
    DateOnly? DataFimVigencia,
    string Status,
    bool Ativo,
    int QuantidadeRamos,
    string ResumoRamos
);
