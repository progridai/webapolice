using System;
using WebApolice.Modulos.Cadastro.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCooperado;

public sealed record CadastrarCooperadoCommand(
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

public sealed record CadastrarCooperadoResult(
    Guid PublicId,
    string Nome,
    string CpfMascarado,
    short Tipo,
    DateTime DataCadastroUtc
);
