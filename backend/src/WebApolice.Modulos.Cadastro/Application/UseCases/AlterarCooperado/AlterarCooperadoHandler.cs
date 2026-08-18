using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCooperado;

public sealed record AlterarCooperadoCommand(
    Guid PublicId,
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

public sealed class AlterarCooperadoHandler
{
    private readonly ICooperadoRepository _repository;
    private readonly CadastroDbContext _dbContext;
    private readonly IRegistradorAuditoria _auditoria;

    public AlterarCooperadoHandler(ICooperadoRepository repository, CadastroDbContext dbContext, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _dbContext = dbContext;
        _auditoria = auditoria;
    }

    public async Task Handle(AlterarCooperadoCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new CooperadoInvalidoException("O nome é obrigatório.");

        if (!command.DataNascimento.HasValue)
            throw new CooperadoInvalidoException("A data de nascimento é obrigatória.");

        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var agenciador = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken)
                ?? throw new CooperadoNaoEncontradoException("Cooperado não encontrado.");

            if (!agenciador.PessoaId.HasValue)
                throw new CooperadoInvalidoException("Dados pessoais do Cooperado não encontrados.");

            if (agenciador.Tipo == TipoAgenciador.Cooperado && command.CoordenadorId.HasValue)
            {
                var coordExiste = await _repository.CoordenadorAtivoExisteAsync(command.CoordenadorId.Value, cancellationToken);
                if (!coordExiste)
                    throw new CooperadoInvalidoException("Coordenador selecionado é inválido, inexistente ou inativo.");
            }

            var pessoa = await _repository.LocalizarPessoaPorIdAsync(agenciador.PessoaId.Value, cancellationToken)
                ?? throw new CooperadoInvalidoException("Dados de pessoa do Cooperado não encontrados.");

            // Atualizar Pessoa
            DateTime? dataNascimentoDt = command.DataNascimento.Value.ToDateTime(TimeOnly.MinValue);
            pessoa.AtualizarDadosPessoais(command.Nome, dataNascimentoDt, null, null);

            // Atualizar Contatos
            var telefoneAtual = await _repository.ObterContatoPrincipalAsync(pessoa.Id, "TELEFONE", cancellationToken);
            if (telefoneAtual != null)
            {
                telefoneAtual.Valor = command.Telefone ?? "";
            }
            else if (!string.IsNullOrWhiteSpace(command.Telefone))
            {
                _repository.AdicionarContato(new PessoaContatoModel { PessoaId = pessoa.Id, TipoContato = "TELEFONE", Valor = command.Telefone, Principal = true, Ativo = true, CreatedAt = DateTimeOffset.UtcNow });
            }

            var emailAtual = await _repository.ObterContatoPrincipalAsync(pessoa.Id, "EMAIL", cancellationToken);
            if (emailAtual != null)
            {
                emailAtual.Valor = command.Email ?? "";
            }
            else if (!string.IsNullOrWhiteSpace(command.Email))
            {
                _repository.AdicionarContato(new PessoaContatoModel { PessoaId = pessoa.Id, TipoContato = "EMAIL", Valor = command.Email, Principal = false, Ativo = true, CreatedAt = DateTimeOffset.UtcNow });
            }

            // Atualizar Endereço
            var endAtual = await _repository.ObterEnderecoPrincipalAsync(pessoa.Id, cancellationToken);
            if (endAtual != null)
            {
                endAtual.Cep = command.Cep;
                endAtual.Logradouro = command.Logradouro;
                endAtual.Numero = command.Numero;
                endAtual.Complemento = command.Complemento;
                endAtual.Bairro = command.Bairro;
                endAtual.CidadeId = command.CidadeId;
                endAtual.Uf = command.Uf;
            }
            else if (!string.IsNullOrWhiteSpace(command.Cep))
            {
                _repository.AdicionarEndereco(new PessoaEnderecoModel { PessoaId = pessoa.Id, TipoEndereco = "RESIDENCIAL", Cep = command.Cep, Logradouro = command.Logradouro, Numero = command.Numero, Complemento = command.Complemento, Bairro = command.Bairro, CidadeId = command.CidadeId, Uf = command.Uf, Principal = true, Ativo = true, CreatedAt = DateTimeOffset.UtcNow });
            }

            // Atualizar RG
            if (!string.IsNullOrWhiteSpace(command.Rg))
            {
                var rgLimpo = new string(command.Rg.Where(char.IsLetterOrDigit).ToArray());
                var docRg = await _repository.ObterDocumentoPrincipalAsync(pessoa.Id, "RG", cancellationToken);
                var dtEmissaoRg = command.DataEmissaoRg.HasValue ? command.DataEmissaoRg.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;

                if (docRg == null)
                {
                    _repository.AdicionarDocumento(new PessoaDocumentoModel(
                        pessoa.Id,
                        "RG",
                        command.Rg,
                        rgLimpo,
                        command.OrgaoEmissor,
                        dtEmissaoRg,
                        true
                    ));
                }
                else
                {
                    docRg.AtualizarDocumento(command.Rg, rgLimpo, command.OrgaoEmissor, dtEmissaoRg);
                }
            }

            // Atualizar Agenciador
            agenciador.AtualizarDados(
                command.Codigo ?? "",
                command.Susep,
                command.Inss,
                command.Issqn,
                command.NumeroDependentes,
                command.DataInscricao,
                command.Credenciado,
                command.CoordenadorId,
                command.BancoId,
                command.Agencia,
                command.ContaCorrente,
                command.Observacao
            );

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var payloadAuditoria = JsonSerializer.SerializeToDocument(new { PublicId = agenciador.PublicId, Codigo = command.Codigo });
            await _auditoria.RegistrarAsync(new RegistroAuditoria { Acao = "ALTERAR", Modulo = "Cadastro", Recurso = "COOPERADOS", RecursoId = agenciador.PublicId.ToString(), Resultado = ResultadoAuditoria.Sucesso, DadosPosteriores = payloadAuditoria }, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
