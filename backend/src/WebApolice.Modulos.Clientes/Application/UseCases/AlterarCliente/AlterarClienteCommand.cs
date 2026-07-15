using System;

namespace WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;

public sealed record AlterarClienteCommand(
    Guid Id,
    string Nome,
    string? Documento,
    DateOnly? DataNascimento,
    short? Sexo,
    string? Observacao,
    bool Falecido,
    DateOnly? DataObito,
    string? Email,
    string? Telefone,
    string? Celular,
    WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente.EnderecoCommand? Endereco
);
