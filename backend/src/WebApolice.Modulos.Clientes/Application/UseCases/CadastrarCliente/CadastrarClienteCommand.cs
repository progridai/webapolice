using System;

namespace WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;

public sealed record CadastrarClienteCommand(
    short TipoPessoa,
    string Nome,
    string Documento,
    DateOnly? DataNascimento,
    short? Sexo,
    string? Observacao,
    bool Falecido,
    DateOnly? DataObito,
    string? Email,
    string? Telefone,
    string? Celular,
    EnderecoCommand? Endereco
);

public sealed record EnderecoCommand(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    long? CidadeId,
    string? Uf
);

public sealed record CadastrarClienteResult(
    Guid PublicId,
    string Nome,
    string DocumentoMascarado,
    string Status,
    DateTime DataCadastroUtc
);
