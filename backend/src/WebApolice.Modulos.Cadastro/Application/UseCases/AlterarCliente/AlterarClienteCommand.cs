using System;
using System.Collections.Generic;
using WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCliente;

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
    IReadOnlyList<EnderecoCommand> Enderecos,
    string? Re = null
);
