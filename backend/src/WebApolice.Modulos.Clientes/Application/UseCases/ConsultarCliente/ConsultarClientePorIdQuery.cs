using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Clientes.Application.UseCases.ConsultarCliente;

public sealed record ConsultarClientePorIdQuery(Guid Id);

public sealed record ClienteStatusResponse(string Codigo, string Nome);

public sealed record ClienteContatoResponse(string Tipo, string Valor, bool Principal, bool Ativo);

public sealed record ClienteEnderecoResponse(string Tipo, string Cep, string Logradouro, string Numero, string Complemento, string Bairro, string Cidade, long? CidadeId, string Uf, bool Principal, bool Ativo);

public sealed record ClienteVinculoResponse(string Matricula, bool Ativo, string Estipulante, string Subestipulante, string Grupo, string Subgrupo, string Lotacao);

public sealed record ClienteDependenteResponse(string Nome, string TipoRelacao, string DocumentoMascarado, DateOnly? DataNascimento);

public sealed record ConsultarClienteResult(
    Guid Id,
    string Nome,
    string Documento,
    string DocumentoMascarado,
    ClienteStatusResponse Status,
    DateOnly? DataNascimento,
    short? Sexo,
    bool Falecido,
    DateOnly? DataObito,
    IReadOnlyList<ClienteContatoResponse> Contatos,
    IReadOnlyList<ClienteEnderecoResponse> Enderecos,
    IReadOnlyList<ClienteVinculoResponse> Vinculos,
    IReadOnlyList<ClienteDependenteResponse> Dependentes,
    string? Re = null
);
