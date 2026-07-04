using System;

namespace WebApolice.Modulos.Clientes.Api.Requests;

public sealed record CadastrarClienteRequest(
    string Nome,
    string Cpf,
    DateOnly? DataNascimento,
    string? Email,
    string? Telefone,
    long? CodigoLegado
);

public sealed record AlterarClienteRequest(
    string Nome,
    DateOnly? DataNascimento,
    string? Email,
    string? Telefone
);
