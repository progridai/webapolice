using System;

namespace WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;

public sealed record AlterarClienteCommand(
    long Id,
    string Nome,
    DateOnly? DataNascimento,
    string? Email,
    string? Telefone
);
