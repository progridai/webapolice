using System;

namespace WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;

public sealed record CadastrarClienteCommand(
    string Nome,
    string Cpf,
    DateOnly? DataNascimento,
    string? Email,
    string? Telefone,
    long? CodigoLegado
);

public sealed record CadastrarClienteResult(
    long Id,
    string Nome,
    string CpfMascarado,
    string Status,
    DateTime DataCadastroUtc
);
