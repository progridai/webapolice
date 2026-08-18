using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AtualizarEstipulante;

public sealed class AtualizarEstipulanteHandler
{
    private readonly IEstipulanteRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;

    public AtualizarEstipulanteHandler(IEstipulanteRepository repository, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
    }

    public async Task Handle(AtualizarEstipulanteCommand command, CancellationToken cancellationToken)
    {
        if (command.Configuracao.DataFimVigencia.HasValue && command.Configuracao.DataFimVigencia.Value < command.Configuracao.DataInicioVigencia)
            throw new EstipulanteInvalidoException("A data de fim de vigência não pode ser menor que a data de início.");

        if (command.Configuracao.Carencia.HasValue && command.Configuracao.Carencia.Value < 0)
            throw new EstipulanteInvalidoException("A carência não pode ser negativa.");

        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            var estipulante = await _repository.ObterParaEdicaoPorPublicIdAsync(command.PublicId, cancellationToken);
            if (estipulante == null)
                throw new EstipulanteInvalidoException("Estipulante não encontrado ou inativo.");

            if (estipulante.PessoaId == null)
                throw new EstipulanteInvalidoException("Estipulante não possui pessoa associada.");

            var pessoa = await _repository.LocalizarPessoaPorIdAsync(estipulante.PessoaId.Value, cancellationToken);
            if (pessoa == null)
                throw new EstipulanteInvalidoException("Pessoa do estipulante não encontrada.");

            var pessoaCompartilhada = await _repository.VerificarPessoaCompartilhadaAsync(pessoa.Id, estipulante.Id, cancellationToken);

            if (pessoaCompartilhada && pessoa.Nome != command.RazaoSocial)
            {
                throw new EstipulanteConflitoException("Os dados centrais dessa pessoa (Razão Social) são compartilhados com outros papéis no sistema e não podem ser alterados por este fluxo.");
            }

            if (command.GrupoId.HasValue)
            {
                var grupoExiste = await _repository.GrupoExisteAsync(command.GrupoId.Value, cancellationToken);
                if (!grupoExiste)
                    throw new EstipulanteInvalidoException("O Grupo informado não existe.");
                estipulante.GrupoId = command.GrupoId.Value;
            }
            else
            {
                estipulante.GrupoId = null;
            }

            if (command.SeguradoraPublicId.HasValue)
            {
                var seguradoraId = await _repository.ObterSeguradoraIdPorPublicIdAsync(command.SeguradoraPublicId.Value, cancellationToken);
                if (!seguradoraId.HasValue)
                    throw new EstipulanteInvalidoException("A Seguradora informada não existe.");
                estipulante.SeguradoraId = seguradoraId.Value;
            }
            else
            {
                estipulante.SeguradoraId = null;
            }

            // Atualiza Dados Pessoais apenas se for alterado e permitido (validação de compartilhada acima já cuida disso)
            if (!pessoaCompartilhada)
            {
                pessoa.Nome = command.RazaoSocial;
            }

            estipulante.Nome = command.RazaoSocial;
            estipulante.NomeFormatado = command.NomeFantasia;
            estipulante.Codigo = command.Codigo;
            estipulante.Observacao = command.Observacao;

            // Atualização de Endereço (Inativa o atual se houver mudança e cria um novo)
            if (command.Endereco != null)
            {
                if (command.Endereco.CidadeId > 0 && !await _repository.CidadeExisteAsync(command.Endereco.CidadeId.Value, cancellationToken))
                    throw new EstipulanteInvalidoException("A Cidade informada não existe.");

                var enderecoAtual = await _repository.ObterEnderecoPrincipalAsync(pessoa.Id, cancellationToken);
                bool precisaNovoEndereco = true;

                if (enderecoAtual != null)
                {
                    if (enderecoAtual.Cep == command.Endereco.Cep &&
                        enderecoAtual.Logradouro == command.Endereco.Logradouro &&
                        enderecoAtual.Numero == command.Endereco.Numero &&
                        enderecoAtual.Complemento == command.Endereco.Complemento &&
                        enderecoAtual.Bairro == command.Endereco.Bairro &&
                        enderecoAtual.CidadeId == command.Endereco.CidadeId &&
                        enderecoAtual.Uf == command.Endereco.Uf)
                    {
                        precisaNovoEndereco = false;
                    }
                    else
                    {
                        enderecoAtual.Ativo = false;
                    }
                }

                if (precisaNovoEndereco)
                {
                    var novoEndereco = new PessoaEnderecoModel
                    {
                        PessoaId = pessoa.Id,
                        Cep = command.Endereco.Cep,
                        Logradouro = command.Endereco.Logradouro,
                        Numero = command.Endereco.Numero,
                        Complemento = command.Endereco.Complemento,
                        Bairro = command.Endereco.Bairro,
                        CidadeId = command.Endereco.CidadeId,
                        Uf = command.Endereco.Uf,
                        TipoEndereco = "PRINCIPAL",
                        Principal = true,
                        Ativo = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    _repository.AdicionarEndereco(novoEndereco);
                }
            }

            // Atualização de Contatos
            var contatosAtuais = await _repository.ObterContatosAtivosAsync(pessoa.Id, cancellationToken);
            var contatosManter = new System.Collections.Generic.HashSet<long>();

            if (command.Contatos != null)
            {
                foreach (var incoming in command.Contatos)
                {
                    if (string.IsNullOrWhiteSpace(incoming.Valor)) continue;

                    var tipo = incoming.TipoContato.ToUpperInvariant();
                    var valorLimpo = tipo != "EMAIL" ? new string(incoming.Valor.Where(char.IsDigit).ToArray()) : incoming.Valor.ToUpperInvariant();

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
                        _repository.AdicionarContato(new PessoaContatoModel
                        {
                            PessoaId = pessoa.Id,
                            TipoContato = tipo,
                            Valor = incoming.Valor,
                            ValorNormalizado = valorLimpo,
                            Principal = incoming.Principal,
                            Ativo = true,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }
                }
            }

            foreach (var contato in contatosAtuais)
            {
                if (!contatosManter.Contains(contato.Id))
                {
                    contato.Ativo = false;
                }
            }

            // Atualização de Contatos Institucionais
            var contatosInstAtuais = await _repository.ObterContatosInstitucionaisAtivosAsync(pessoa.Id, cancellationToken);
            var contatosInstManter = new System.Collections.Generic.HashSet<long>();

            if (command.ContatosInstitucionais != null)
            {
                foreach (var incoming in command.ContatosInstitucionais)
                {
                    if (string.IsNullOrWhiteSpace(incoming.Nome) && string.IsNullOrWhiteSpace(incoming.Departamento)) continue;

                    var matching = contatosInstAtuais.FirstOrDefault(c =>
                        c.Nome.Equals(incoming.Nome, StringComparison.OrdinalIgnoreCase) &&
                        c.Departamento.Equals(incoming.Departamento, StringComparison.OrdinalIgnoreCase)
                    );

                    if (matching != null)
                    {
                        // Atualiza dados adicionais caso tenham mudado
                        matching.Email = incoming.Email;
                        matching.Telefone = incoming.Telefone;
                        matching.Ramal = incoming.Ramal;
                        contatosInstManter.Add(matching.Id);
                    }
                    else
                    {
                        _repository.AdicionarContatoInstitucional(new PessoaContatoInstitucionalModel
                        {
                            PessoaId = pessoa.Id,
                            Nome = incoming.Nome,
                            Departamento = incoming.Departamento,
                            Email = incoming.Email,
                            Telefone = incoming.Telefone,
                            Ramal = incoming.Ramal,
                            Ativo = true,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }
                }
            }

            foreach (var contato in contatosInstAtuais)
            {
                if (!contatosInstManter.Contains(contato.Id))
                {
                    contato.Ativo = false;
                }
            }

            var config = await _repository.ObterConfiguracaoPorEstipulanteIdAsync(estipulante.Id, cancellationToken);

            if (config == null)
            {
                config = new EstipulanteConfiguracaoModel
                {
                    EstipulanteId = estipulante.Id,
                    DataInicioVigencia = command.Configuracao.DataInicioVigencia,
                    DataFimVigencia = command.Configuracao.DataFimVigencia,
                    Carencia = command.Configuracao.Carencia,
                    AdesaoPor = command.Configuracao.AdesaoPor,
                    Custeio = command.Configuracao.Custeio,
                    Adesao = command.Configuracao.Adesao,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _repository.AdicionarConfiguracao(config);
            }
            else
            {
                config.DataInicioVigencia = command.Configuracao.DataInicioVigencia;
                config.DataFimVigencia = command.Configuracao.DataFimVigencia;
                config.Carencia = command.Configuracao.Carencia;
                config.AdesaoPor = command.Configuracao.AdesaoPor;
                config.Custeio = command.Configuracao.Custeio;
                config.Adesao = command.Configuracao.Adesao;
                config.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "ESTIPULANTE_ALTERADO",
                Modulo = "Estipulantes",
                Recurso = "estipulante",
                RecursoId = estipulante.PublicId.ToString(),
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { command.RazaoSocial, command.NomeFantasia })
            }, cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
