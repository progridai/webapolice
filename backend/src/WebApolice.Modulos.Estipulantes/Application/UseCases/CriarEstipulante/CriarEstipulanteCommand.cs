using System;

namespace WebApolice.Modulos.Estipulantes.Application.UseCases.CriarEstipulante;

public sealed record CriarEstipulanteCommand(
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    string? Codigo,
    long? GrupoId,
    Guid? SeguradoraPublicId,
    string? Observacao,
    CriarEstipulanteEnderecoCommand? Endereco,
    IReadOnlyList<CriarEstipulanteContatoCommand>? Contatos,
    CriarEstipulanteConfiguracaoCommand Configuracao
);

public sealed record CriarEstipulanteEnderecoCommand(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    long? CidadeId,
    string? Uf
);

public sealed record CriarEstipulanteContatoCommand(
    string TipoContato,
    string Valor,
    bool Principal
);

public sealed record CriarEstipulanteConfiguracaoCommand(
    DateOnly DataInicioVigencia,
    DateOnly? DataFimVigencia,
    int? Carencia,
    string? AdesaoPor,
    string? Custeio,
    string? Adesao
);

public sealed record CriarEstipulanteResult(
    Guid PublicId,
    string RazaoSocial,
    string Cnpj,
    string Status,
    DateTimeOffset CreatedAt
);
