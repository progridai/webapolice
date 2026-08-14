using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCliente;

public sealed class CadastrarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly CadastroDbContext _dbContext;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext _segurancaDbContext;

    public CadastrarClienteHandler(IClienteRepository repository, CadastroDbContext dbContext, IRegistradorAuditoria auditoria, WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext segurancaDbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
        _auditoria = auditoria;
        _segurancaDbContext = segurancaDbContext;
    }

    public async Task<CadastrarClienteResult> Handle(CadastrarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.Re))
        {
            var recursoRe = await _segurancaDbContext.Recursos
                .Include(r => r.Modulo)
                .Where(r => r.Codigo == "RE" && r.Modulo.Codigo == "CLIENTES")
                .FirstOrDefaultAsync(cancellationToken);

            if (recursoRe == null || !recursoRe.Habilitado || !recursoRe.Ativo || !recursoRe.Modulo.Habilitado || !recursoRe.Modulo.Ativo)
            {
                throw new ClienteInvalidoException("O campo RE nÃƒÂ£o estÃƒÂ¡ habilitado no sistema.");
            }
        }

        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ClienteInvalidoException("O nome ÃƒÂ© obrigatÃƒÂ³rio.");

        var documentoLimpo = LimparDocumento(command.Documento);
        if (string.IsNullOrWhiteSpace(documentoLimpo))
            throw new ClienteInvalidoException("Documento ÃƒÂ© obrigatÃƒÂ³rio.");

        if (!command.DataNascimento.HasValue)
            throw new ClienteInvalidoException("A data de nascimento ÃƒÂ© obrigatÃƒÂ³ria.");

        var documentoValido = ValidarDocumento(documentoLimpo, command.TipoPessoa);
        if (!documentoValido)
            throw new ClienteInvalidoException("Documento invÃƒÂ¡lido.");

        if (command.Falecido && !command.DataObito.HasValue)
            throw new ClienteInvalidoException("Data de ÃƒÂ³bito ÃƒÂ© obrigatÃƒÂ³ria para clientes falecidos.");

        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var statusAtivo = await _repository.ObterStatusPorCodigoAsync(ClienteStatusCodigos.Ativo, cancellationToken)
                ?? throw new ClienteInvalidoException($"Status '{ClienteStatusCodigos.Ativo}' nÃƒÂ£o encontrado no catÃƒÂ¡logo.");

            var statusInativo = await _repository.ObterStatusPorCodigoAsync(ClienteStatusCodigos.Inativo, cancellationToken)
                ?? throw new ClienteInvalidoException($"Status '{ClienteStatusCodigos.Inativo}' nÃƒÂ£o encontrado no catÃƒÂ¡logo.");

            var pessoa = await _repository.LocalizarPessoaPorDocumentoAsync(documentoLimpo, cancellationToken);
            DateTime? dataNascimentoDt = command.DataNascimento.HasValue ? command.DataNascimento.Value.ToDateTime(TimeOnly.MinValue) : null;
            long pessoaId;

            if (pessoa != null)
            {
                var clienteExistente = await _repository.LocalizarClientePorPessoaIdAsync(pessoa.Id, cancellationToken);
                if (clienteExistente != null && clienteExistente.StatusId == statusAtivo.Id)
                {
                    throw new ClienteJaCadastradoException($"JÃƒÂ¡ existe um cliente ativo para o documento informado.");
                }

                // Verificar divergÃƒÂªncia
                
                bool divergente = false;
                if (!string.Equals(pessoa.Nome, command.Nome, StringComparison.OrdinalIgnoreCase)) divergente = true;
                if (pessoa.TipoPessoa != command.TipoPessoa) divergente = true;
                if (pessoa.DataNascimento != dataNascimentoDt) divergente = true;

                if (divergente)
                    throw new ClienteJaCadastradoException("O documento informado jÃƒÂ¡ pertence a outra pessoa com dados divergentes no sistema.");

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
                    dataNascimentoDt,
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
            cliente.AtualizarDados(command.Falecido, command.DataObito, command.Observacao, command.Re);
            
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

            // EndereÃƒÂ§os
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
