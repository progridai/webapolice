using System;

namespace WebApolice.Modulos.Clientes.Api.Requests;

public sealed record CadastrarClienteRequest(
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
    EnderecoRequest? Endereco
);

public sealed record AlterarClienteRequest(
    string Nome,
    DateOnly? DataNascimento,
    short? Sexo,
    string? Observacao,
    bool Falecido,
    DateOnly? DataObito,
    string? Email,
    string? Telefone,
    string? Celular,
    EnderecoRequest? Endereco
);

public sealed record EnderecoRequest(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    long? CidadeId,
    string? Uf
);
