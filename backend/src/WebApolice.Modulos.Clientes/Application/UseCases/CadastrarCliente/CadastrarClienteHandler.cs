using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using System.Text.Json;

namespace WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;

public sealed class CadastrarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly ClientesDbContext _dbContext;
    private readonly IRegistradorAuditoria _auditoria;

    public CadastrarClienteHandler(IClienteRepository repository, ClientesDbContext dbContext, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _dbContext = dbContext;
        _auditoria = auditoria;
    }

    public async Task<CadastrarClienteResult> Handle(CadastrarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ClienteInvalidoException("O nome é obrigatório.");

        var documentoLimpo = LimparDocumento(command.Documento);
        if (string.IsNullOrWhiteSpace(documentoLimpo))
            throw new ClienteInvalidoException("Documento é obrigatório.");

        if (!command.DataNascimento.HasValue)
            throw new ClienteInvalidoException("A data de nascimento é obrigatória.");

        var documentoValido = ValidarDocumento(documentoLimpo, command.TipoPessoa);
        if (!documentoValido)
            throw new ClienteInvalidoException("Documento inválido.");

        if (command.Falecido && !command.DataObito.HasValue)
            throw new ClienteInvalidoException("Data de óbito é obrigatória para clientes falecidos.");

        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var statusAtivo = await _repository.ObterStatusPorCodigoAsync(ClienteStatusCodigos.Ativo, cancellationToken)
                ?? throw new ClienteInvalidoException($"Status '{ClienteStatusCodigos.Ativo}' não encontrado no catálogo.");

            var statusInativo = await _repository.ObterStatusPorCodigoAsync(ClienteStatusCodigos.Inativo, cancellationToken)
                ?? throw new ClienteInvalidoException($"Status '{ClienteStatusCodigos.Inativo}' não encontrado no catálogo.");

            var pessoa = await _repository.LocalizarPessoaPorDocumentoAsync(documentoLimpo, cancellationToken);
            long pessoaId;

            if (pessoa != null)
            {
                var clienteExistente = await _repository.LocalizarClientePorPessoaIdAsync(pessoa.Id, cancellationToken);
                if (clienteExistente != null && clienteExistente.StatusId == statusAtivo.Id)
                {
                    throw new ClienteJaCadastradoException($"Já existe um cliente ativo para o documento informado.");
                }

                // Verificar divergência
                bool divergente = false;
                if (!string.Equals(pessoa.Nome, command.Nome, StringComparison.OrdinalIgnoreCase)) divergente = true;
                if (pessoa.TipoPessoa != command.TipoPessoa) divergente = true;
                if (pessoa.DataNascimento != command.DataNascimento) divergente = true;

                if (divergente)
                    throw new ClienteJaCadastradoException("O documento informado já pertence a outra pessoa com dados divergentes no sistema.");

                pessoaId = pessoa.Id;
            }
            else
            {
                pessoa = new PessoaModel(
                    command.TipoPessoa,
                    command.Nome,
                    command.Documento,
                    documentoLimpo,
                    documentoValido,
                    command.DataNascimento,
                    command.Sexo,
                    command.Observacao
                );
                _repository.AdicionarPessoa(pessoa);
                await _repository.SalvarAlteracoesAsync(cancellationToken); // Precisa salvar para obter o ID
                pessoaId = pessoa.Id;
            }

            var cliente = new Cliente(pessoaId, statusAtivo.Id);
            if (command.Falecido && command.DataObito.HasValue)
            {
                cliente.RegistrarObito(command.DataObito.Value);
            }
            else if (!string.IsNullOrWhiteSpace(command.Observacao))
            {
                cliente.AtualizarDados(false, null, command.Observacao);
            }
            
            _repository.AdicionarCliente(cliente);

            // Contatos
            if (command.Contatos != null)
            {
                foreach (var contato in command.Contatos)
                {
                    if (string.IsNullOrWhiteSpace(contato.Valor)) continue;
                    var tipo = contato.TipoContato.ToUpperInvariant();
                    var valorLimpo = tipo != "EMAIL" ? LimparDocumento(contato.Valor) : contato.Valor.ToUpperInvariant();
                    _repository.AdicionarContato(new PessoaContatoModel(pessoaId, tipo, contato.Valor, valorLimpo, contato.Principal));
                }
            }

            // Endereços
            if (command.Enderecos != null)
            {
                foreach (var end in command.Enderecos)
                {
                    _repository.AdicionarEndereco(new PessoaEnderecoModel(
                        pessoaId,
                        end.CidadeId,
                        end.TipoEndereco.ToUpperInvariant(),
                        end.Cep,
                        end.Logradouro,
                        end.Numero,
                        end.Complemento,
                        end.Bairro,
                        end.Uf,
                        end.Principal
                    ));
                }
            }

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "CLIENTE_CRIADO",
                Modulo = "Clientes",
                Recurso = "cliente",
                RecursoId = cliente.PublicId.ToString(),
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { command.Nome, command.Documento, command.TipoPessoa })
            }, cancellationToken);

            var docMascarado = MascararDocumento(documentoLimpo, command.TipoPessoa);
            var statusStr = cliente.StatusId == statusAtivo.Id ? statusAtivo.Nome : statusInativo.Nome;

            return new CadastrarClienteResult(
                cliente.PublicId,
                pessoa.Nome,
                docMascarado,
                statusStr,
                cliente.CreatedAt
            );
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static string LimparDocumento(string? doc)
    {
        if (string.IsNullOrWhiteSpace(doc)) return "";
        return new string(doc.Where(char.IsDigit).ToArray());
    }

    private static bool ValidarDocumento(string limpo, short tipoPessoa)
    {
        // Simplificado para o escopo
        if (tipoPessoa == 1) return limpo.Length == 11;
        if (tipoPessoa == 2) return limpo.Length == 14;
        return false;
    }

    private static string MascararDocumento(string limpo, short tipoPessoa)
    {
        if (tipoPessoa == 1 && limpo.Length == 11)
            return $"{limpo.Substring(0, 3)}.{limpo.Substring(3, 3)}.{limpo.Substring(6, 3)}-{limpo.Substring(9, 2)}";
        if (tipoPessoa == 2 && limpo.Length == 14)
            return $"{limpo.Substring(0, 2)}.{limpo.Substring(2, 3)}.{limpo.Substring(5, 3)}/{limpo.Substring(8, 4)}-{limpo.Substring(12, 2)}";
        
        return limpo;
    }
}
