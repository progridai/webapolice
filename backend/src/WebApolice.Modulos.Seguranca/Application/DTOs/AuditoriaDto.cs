using System;

namespace WebApolice.Modulos.Seguranca.Application.DTOs;

public sealed record AuditoriaDto(
    Guid PublicId,
    string Acao,
    string EntidadeTipo,
    string EntidadeId,
    DateTime CreatedAt,
    string? DadosAnteriores,
    string? DadosNovos
);
