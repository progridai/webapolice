using WebApolice.Modulos.Cadastro.Domain.Validators;
using WebApolice.Modulos.Seguranca.Contracts;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;


namespace WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCliente;

public sealed class AlterarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly IOperacaoCadastroAuditada _operacao;
    private readonly IDisponibilidadeCatalogo _catalogo;

    public AlterarClienteHandler(IClienteRepository repository, IOperacaoCadastroAuditada operacao, IDisponibilidadeCatalogo catalogo)
    {
        _repository = repository;
        _operacao = operacao;
        _catalogo = catalogo;
    }

    public async Task Handle(AlterarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.Re))
        {
            if (!await _catalogo.RecursoDisponivelAsync("CLIENTES", "RE", cancellationToken))
            {
                throw new ClienteInvalidoException("O campo RE não está habilitado no sistema.");
            }
        }

        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ClienteInvalidoException("O nome é obrigatório.");

        if (!(command.DataNascimento.HasValue ? command.DataNascimento.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null).HasValue)
            throw new ClienteInvalidoException("A data de nascimento é obrigatória.");

        if (command.Falecido && !command.DataObito.HasValue)
            throw new ClienteInvalidoException("Data de óbito é obrigatória para clientes falecidos.");

        await _operacao.ExecutarAsync<bool>("Clientes", "cliente", "CLIENTE_ALTERADO", async ct =>
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
            pessoa.AtualizarDadosPessoais(command.Nome, (command.DataNascimento.HasValue ? command.DataNascimento.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null), command.Sexo, command.Observacao);
            if (!string.IsNullOrWhiteSpace(command.Documento))
            {
                var documentoLimpo = LimparDocumento(command.Documento);
                var documentoValido = ValidarDocumento(documentoLimpo, pessoa.TipoPessoa);
                if (!documentoValido && documentoLimpo != pessoa.DocumentoPrincipalLimpo)
                    throw new ClienteInvalidoException("Documento inválido.");
                // Documento legado inválido e inalterado permanece como evidência original.
                if (documentoValido)
                {
                    pessoa.AtualizarDocumento(command.Documento, documentoLimpo);
                    pessoa.DocumentoValido = true;
                }
            }

            cliente.AtualizarDados(command.Falecido, command.DataObito, command.Observacao, command.Re);

            // Contatos
            var contatosAtuais = await _repository.ObterContatosAtivosAsync(pessoa.Id, cancellationToken);

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
            var enderecosAtuais = await _repository.ObterEnderecosAtivosAsync(pessoa.Id, cancellationToken);

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

            return (true, cliente.PublicId.ToString(), null, new { command.Nome, command.Documento });
        }, cancellationToken);
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

    private static bool ValidarDocumento(string limpo, short tipoPessoa) => DocumentoValidator.IsValid(limpo, tipoPessoa);
}
