using System;

namespace WebApolice.Modulos.Clientes.Application.UseCases.ConsultarCliente;

public sealed record ConsultarClientePorIdQuery(long Id);

public sealed record ConsultarClienteResult(
    long Id,
    string Nome,
    string CpfMascarado,
    DateOnly? DataNascimento,
    string? Email,
    string? Telefone,
    string Status,
    DateTime DataCadastroUtc,
    DateTime DataAtualizacaoUtc,
    long? CodigoLegado
);
