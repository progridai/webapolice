using System;
using System.Collections.Generic;
using WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;

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
    IReadOnlyList<ContatoCommand> Contatos,
    IReadOnlyList<EnderecoCommand> Enderecos
);
