using System;

namespace WebApolice.SharedKernel.Application.Models;

public record EnderecoProviderDto(
    string Cep,
    string? Logradouro,
    string? Bairro,
    string Cidade,
    string Uf
);

public record EnderecoConsultaResult(
    string Cep,
    string? Logradouro,
    string? Bairro,
    string Cidade,
    string Uf,
    long? CidadeId,
    long? EstadoId
);
