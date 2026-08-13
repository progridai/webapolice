using System;
using System.Collections.Generic;

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
    IReadOnlyList<ContatoRequest> Contatos,
    IReadOnlyList<EnderecoRequest> Enderecos,
    string? Re = null
);

public sealed record AlterarClienteRequest(
    string Nome,
    string? Documento,
    DateOnly? DataNascimento,
    short? Sexo,
    string? Observacao,
    bool Falecido,
    DateOnly? DataObito,
    IReadOnlyList<ContatoRequest> Contatos,
    IReadOnlyList<EnderecoRequest> Enderecos,
    string? Re = null
);

public sealed record ContatoRequest(
    string TipoContato,
    string Valor,
    bool Principal
);

public sealed record EnderecoRequest(
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
