using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;

public sealed class AlterarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly ClientesDbContext _dbContext;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext _segurancaDbContext;

    public AlterarClienteHandler(IClienteRepository repository, ClientesDbContext dbContext, IRegistradorAuditoria auditoria, WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext segurancaDbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
        _auditoria = auditoria;
        _segurancaDbContext = segurancaDbContext;
    }

    public async Task Handle(AlterarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.Re))
        {
            var recursoRe = await _segurancaDbContext.Recursos
                .Include(r => r.Modulo)
                .Where(r => r.Codigo == "RE" && r.Modulo.Codigo == "CLIENTES")
                .FirstOrDefaultAsync(cancellationToken);

            if (recursoRe == null || !recursoRe.Habilitado || !recursoRe.Ativo || !recursoRe.Modulo.Habilitado || !recursoRe.Modulo.Ativo)
            {
                throw new ClienteInvalidoException("O campo RE não está habilitado no sistema.");
            }
        }

        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ClienteInvalidoException("O nome é obrigatório.");

        if (!command.DataNascimento.HasValue)
            throw new ClienteInvalidoException("A data de nascimento é obrigatória.");

        if (command.Falecido && !command.DataObito.HasValue)
            throw new ClienteInvalidoException("Data de óbito é obrigatória para clientes falecidos.");

        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var cliente = await _repository.ObterParaEdicaoPorPublicIdAsync(command.Id, cancellationToken);
            if (cliente == null)
                throw new ClienteNaoEncontradoException("Cliente não encontrado ou excluído.");

            var pessoa = await _repository.LocalizarPessoaPorIdAsync(cliente.PessoaId, cancellationToken);
            if (pessoa == null)
                throw new ClienteNaoEncontradoException("Pessoa associada não encontrada.");

            // Verifica se a pessoa está compartilhada com outros papéis
            var compartilhada = await _repository.VerificarPessoaCompartilhadaAsync(pessoa.Id, cliente.Id, cancellationToken);
            if (compartilhada)
                throw new ClienteInvalidoException("Os dados dessa pessoa são compartilhados com outros papéis no sistema e não podem ser alterados diretamente por aqui.");

            // Atualiza Dados Pessoais e Cliente
            pessoa.AtualizarDadosPessoais(command.Nome, command.DataNascimento, command.Sexo, command.Observacao);
            if (!string.IsNullOrWhiteSpace(command.Documento))
            {
                var documentoLimpo = LimparDocumento(command.Documento);
                var documentoValido = ValidarDocumento(documentoLimpo, pessoa.TipoPessoa);
                if (!documentoValido)
                    throw new ClienteInvalidoException("Documento inválido.");
                pessoa.AtualizarDocumento(command.Documento, documentoLimpo);
            }

            cliente.AtualizarDados(command.Falecido, command.DataObito, command.Observacao, command.Re);

            // Contatos
            var contatosAtuais = await _dbContext.Contatos
                .Where(x => x.PessoaId == pessoa.Id && x.Ativo)
                .ToListAsync(cancellationToken);

            var contatosManter = new HashSet<long>();

            if (command.Contatos != null)
            {
                foreach (var incoming in command.Contatos)
                {
                    if (string.IsNullOrWhiteSpace(incoming.Valor)) continue;

                    var tipo = incoming.TipoContato.ToUpperInvariant();
                    var valorLimpo = NormalizarContatoValor(tipo, incoming.Valor);

                    var matching = contatosAtuais.FirstOrDefault(c =>
                        c.TipoContato.Equals(tipo, StringComparison.OrdinalIgnoreCase) &&
                        c.ValorNormalizado == valorLimpo &&
                        c.Principal == incoming.Principal
                    );

                    if (matching != null)
                    {
                        contatosManter.Add(matching.Id);
                    }
                    else
                    {
                        _repository.AdicionarContato(new PessoaContatoModel(
                            pessoa.Id,
                            tipo,
                            incoming.Valor,
                            valorLimpo,
                            incoming.Principal
                        ));
                    }
                }
            }

            foreach (var contato in contatosAtuais)
            {
                if (!contatosManter.Contains(contato.Id))
                {
                    contato.Inativar();
                }
            }

            // Endereços
            var enderecosAtuais = await _dbContext.Enderecos
                .Where(x => x.PessoaId == pessoa.Id && x.Ativo)
                .ToListAsync(cancellationToken);

            var enderecosManter = new HashSet<long>();

            if (command.Enderecos != null)
            {
                foreach (var incoming in command.Enderecos)
                {
                    var tipoEnd = incoming.TipoEndereco.ToUpperInvariant();
                    var matching = enderecosAtuais.FirstOrDefault(e =>
                        e.TipoEndereco.Equals(tipoEnd, StringComparison.OrdinalIgnoreCase) &&
                        e.Cep == incoming.Cep &&
                        e.Logradouro == incoming.Logradouro &&
                        e.Numero == incoming.Numero &&
                        e.Complemento == incoming.Complemento &&
                        e.Bairro == incoming.Bairro &&
                        e.CidadeId == incoming.CidadeId &&
                        e.Uf == incoming.Uf &&
                        e.Principal == incoming.Principal
                    );

                    if (matching != null)
                    {
                        enderecosManter.Add(matching.Id);
                    }
                    else
                    {
                        _repository.AdicionarEndereco(new PessoaEnderecoModel(
                            pessoa.Id,
                            incoming.CidadeId,
                            tipoEnd,
                            incoming.Cep,
                            incoming.Logradouro,
                            incoming.Numero,
                            incoming.Complemento,
                            incoming.Bairro,
                            incoming.Uf,
                            incoming.Principal
                        ));
                    }
                }
            }

            foreach (var end in enderecosAtuais)
            {
                if (!enderecosManter.Contains(end.Id))
                {
                    end.Inativar();
                }
            }

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "CLIENTE_ALTERADO",
                Modulo = "Clientes",
                Recurso = "cliente",
                RecursoId = cliente.PublicId.ToString(),
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { command.Nome, command.Documento })
            }, cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static string NormalizarContatoValor(string tipo, string valor)
    {
        return tipo.ToUpperInvariant() != "EMAIL" ? LimparDocumento(valor) : valor.ToUpperInvariant();
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
}
