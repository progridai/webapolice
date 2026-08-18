using System;
using WebApolice.Modulos.Cadastro.Domain;

namespace WebApolice.Modulos.Cadastro.Api.Requests;

public sealed record CadastrarCooperadoRequest(
    string Nome,
    string Cpf,
    DateOnly? DataNascimento,
    
    // Contato e Endereço principais
    string? Telefone,
    string? Email,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    long? CidadeId,
    string? Uf,
    
    // Dados Agenciador
    TipoAgenciador Tipo,
    string? Codigo,
    string? Rg,
    string? OrgaoEmissor,
    DateOnly? DataEmissaoRg,
    string? Susep,
    string? Inss,
    string? Issqn,
    int? NumeroDependentes,
    DateOnly? DataInscricao,
    bool? Credenciado,
    long? CoordenadorId,
    long? BancoId,
    string? Agencia,
    string? ContaCorrente,
    string? Observacao
);

public sealed record AlterarCooperadoRequest(
    string Nome,
    DateOnly? DataNascimento,
    string? Telefone,
    string? Email,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    long? CidadeId,
    string? Uf,
    string? Codigo,
    string? Rg,
    string? OrgaoEmissor,
    DateOnly? DataEmissaoRg,
    string? Susep,
    string? Inss,
    string? Issqn,
    int? NumeroDependentes,
    DateOnly? DataInscricao,
    bool? Credenciado,
    long? CoordenadorId,
    long? BancoId,
    string? Agencia,
    string? ContaCorrente,
    string? Observacao
);
