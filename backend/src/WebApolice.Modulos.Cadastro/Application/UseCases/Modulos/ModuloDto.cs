using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

public record ModuloDto(
    Guid PublicId,
    string Nome,
    string? Descricao,
    bool Ativo,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record ModuloListDto(
    Guid PublicId,
    string Nome,
    string? Descricao,
    bool Ativo
);
