using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AtualizarEstipulante;

public record AtualizarEstipulanteCommand(
    Guid PublicId,
    string RazaoSocial,
    string NomeFantasia,
    string? Codigo,
    long? GrupoId,
    Guid? SeguradoraPublicId,
    string? Observacao,
    AtualizarEstipulanteEnderecoCommand? Endereco,
    IReadOnlyList<AtualizarEstipulanteContatoCommand>? Contatos,
    IReadOnlyList<AtualizarEstipulanteContatoInstitucionalCommand>? ContatosInstitucionais,
    AtualizarEstipulanteConfiguracaoCommand Configuracao
);

public record AtualizarEstipulanteConfiguracaoCommand(
    DateOnly DataInicioVigencia,
    DateOnly? DataFimVigencia,
    int? Carencia,
    string? AdesaoPor,
    string? Custeio,
    string? Adesao
);

public record AtualizarEstipulanteEnderecoCommand(
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    long? CidadeId,
    string Uf
);

public record AtualizarEstipulanteContatoCommand(
    string TipoContato,
    string Valor,
    bool Principal
);

public record AtualizarEstipulanteContatoInstitucionalCommand(
    string Nome,
    string Departamento,
    string? Email,
    string? Telefone,
    string? Ramal
);
