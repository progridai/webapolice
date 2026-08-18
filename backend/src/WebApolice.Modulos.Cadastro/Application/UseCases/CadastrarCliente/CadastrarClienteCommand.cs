using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente;

public sealed record CadastrarClienteCommand(
    short TipoPessoa,
    string Nome,
    string Documento,
    DateOnly? DataNascimento,
    short? Sexo,
    string? Observacao,
    bool Falecido,
    DateOnly? DataObito,
    IReadOnlyList<ContatoCommand> Contatos,
    IReadOnlyList<EnderecoCommand> Enderecos,
    string? Re = null
);

public sealed record ContatoCommand(
    string TipoContato,
    string Valor,
    bool Principal
);

public sealed record EnderecoCommand(
    string TipoEndereco,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    long? CidadeId,
    string? Uf,
    bool Principal
);

public sealed record CadastrarClienteResult(
    Guid PublicId,
    string Nome,
    string DocumentoMascarado,
    string Status,
    DateTime DataCadastroUtc
);
