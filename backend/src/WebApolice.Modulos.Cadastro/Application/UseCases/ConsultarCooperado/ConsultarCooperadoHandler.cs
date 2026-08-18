using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCooperado;

public sealed record ConsultarCooperadoQuery(Guid PublicId);

public sealed record CooperadoDetalheDto(
    Guid PublicId,
    string Nome,
    string Cpf,
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
    short Tipo,
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
    string? Observacao,
    bool Desativado,
    DateOnly? DataDesativado
);

public sealed class ConsultarCooperadoHandler
{
    private readonly ICooperadoRepository _repository;

    public ConsultarCooperadoHandler(ICooperadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<CooperadoDetalheDto> Handle(ConsultarCooperadoQuery query, CancellationToken cancellationToken)
    {
        var agenciador = await _repository.ObterPorPublicIdAsync(query.PublicId, cancellationToken);
        if (agenciador == null || !agenciador.PessoaId.HasValue)
            throw new CooperadoNaoEncontradoException("Cooperado não encontrado.");

        var pessoa = await _repository.LocalizarPessoaPorIdAsync(agenciador.PessoaId.Value, cancellationToken)
            ?? throw new CooperadoNaoEncontradoException("Dados pessoais do Cooperado não encontrados.");

        var telefone = await _repository.ObterContatoPrincipalAsync(pessoa.Id, "TELEFONE", cancellationToken);
        var email = await _repository.ObterContatoPrincipalAsync(pessoa.Id, "EMAIL", cancellationToken);
        var endereco = await _repository.ObterEnderecoPrincipalAsync(pessoa.Id, cancellationToken);
        var docRg = await _repository.ObterDocumentoPrincipalAsync(pessoa.Id, "RG", cancellationToken);

        return new CooperadoDetalheDto(
            agenciador.PublicId,
            pessoa.Nome,
            pessoa.DocumentoPrincipalLimpo ?? "",
            pessoa.DataNascimento.HasValue ? DateOnly.FromDateTime(pessoa.DataNascimento.Value) : null,
            telefone?.Valor,
            email?.Valor,
            endereco?.Cep,
            endereco?.Logradouro,
            endereco?.Numero,
            endereco?.Complemento,
            endereco?.Bairro,
            endereco?.CidadeId,
            endereco?.Uf,
            (short)agenciador.Tipo,
            agenciador.Codigo,
            docRg?.Numero,
            docRg?.OrgaoEmissor,
            docRg?.DataEmissao.HasValue == true ? DateOnly.FromDateTime(docRg.DataEmissao.Value) : null,
            agenciador.Susep,
            agenciador.Inss,
            agenciador.Issqn,
            agenciador.NumeroDependentes,
            agenciador.DataInscricao,
            agenciador.Credenciado,
            agenciador.CoordenadorId,
            agenciador.BancoId,
            agenciador.Agencia,
            agenciador.ContaCorrente,
            agenciador.Observacao,
            agenciador.Desativado,
            agenciador.DataDesativado
        );
    }
}
