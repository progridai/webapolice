using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Estipulantes.Application.Ports;
using WebApolice.Modulos.Estipulantes.Domain.Exceptions;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Estipulantes.Application.UseCases.CriarEstipulante;

public sealed class CriarEstipulanteHandler
{
    private readonly IEstipulanteRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;

    public CriarEstipulanteHandler(IEstipulanteRepository repository, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
    }

    public async Task<CriarEstipulanteResult> Handle(CriarEstipulanteCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RazaoSocial))
            throw new EstipulanteInvalidoException("A Razão Social é obrigatória.");

        var cnpjLimpo = LimparDocumento(command.Cnpj);
        if (string.IsNullOrWhiteSpace(cnpjLimpo))
            throw new EstipulanteInvalidoException("CNPJ é obrigatório.");

        if (!ValidarCnpj(cnpjLimpo))
            throw new EstipulanteInvalidoException("CNPJ inválido.");

        if (command.Configuracao.DataFimVigencia.HasValue && command.Configuracao.DataFimVigencia < command.Configuracao.DataInicioVigencia)
            throw new EstipulanteInvalidoException("A data de fim de vigência não pode ser menor que a data de início.");

        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            // Valida Grupo
            if (command.GrupoId.HasValue)
            {
                var grupoExiste = await _repository.GrupoExisteAsync(command.GrupoId.Value, cancellationToken);
                if (!grupoExiste) throw new EstipulanteInvalidoException("Grupo informado não existe.");
            }

            // Valida Seguradora
            long? seguradoraId = null;
            if (command.SeguradoraPublicId.HasValue)
            {
                seguradoraId = await _repository.ObterSeguradoraIdPorPublicIdAsync(command.SeguradoraPublicId.Value, cancellationToken);
                if (seguradoraId == null) throw new EstipulanteInvalidoException("Seguradora informada não existe.");
            }

            // Valida Pessoa e Concorrência
            var pessoa = await _repository.LocalizarPessoaPorDocumentoAsync(cnpjLimpo, cancellationToken);
            long pessoaId;

            if (pessoa != null)
            {
                var estipulanteExistente = await _repository.LocalizarEstipulantePorPessoaIdAsync(pessoa.Id, cancellationToken);
                if (estipulanteExistente != null && estipulanteExistente.Ativo)
                {
                    throw new EstipulanteConflitoException("Já existe um Estipulante ativo para o CNPJ informado.");
                }

                // Verifica divergência de dados
                if (!string.Equals(pessoa.Nome, command.RazaoSocial, StringComparison.OrdinalIgnoreCase))
                    throw new EstipulanteConflitoException("O CNPJ informado já pertence a outra pessoa com Razão Social divergente no sistema.");

                if (pessoa.TipoPessoa != 2) // 2 = PJ
                    throw new EstipulanteConflitoException("O documento informado já está associado a uma Pessoa Física.");

                pessoaId = pessoa.Id;
            }
            else
            {
                pessoa = new PessoaModel
                {
                    TipoPessoa = 2, // PJ
                    Nome = command.RazaoSocial,
                    DocumentoPrincipal = command.Cnpj,
                    DocumentoPrincipalLimpo = cnpjLimpo,
                    DocumentoValido = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _repository.AdicionarPessoa(pessoa);
                await _repository.SalvarAlteracoesAsync(cancellationToken);
                pessoaId = pessoa.Id;
            }

            // Cria o Estipulante
            var estipulante = new EstipulanteModel
            {
                PessoaId = pessoaId,
                PublicId = Guid.NewGuid(),
                Nome = command.RazaoSocial,
                NomeFormatado = command.NomeFantasia,
                Codigo = command.Codigo,
                GrupoId = command.GrupoId,
                SeguradoraId = seguradoraId,
                Cnpj = command.Cnpj,
                CnpjLimpo = cnpjLimpo,
                Observacao = command.Observacao,
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            
            _repository.AdicionarEstipulante(estipulante);

            // Contatos
            if (command.Contatos != null && command.Contatos.Any())
            {
                foreach (var contato in command.Contatos)
                {
                    if (string.IsNullOrWhiteSpace(contato.Valor)) continue;

                    var tipo = contato.TipoContato.ToUpperInvariant();
                    var valorLimpo = tipo != "EMAIL" ? LimparDocumento(contato.Valor) : contato.Valor.ToUpperInvariant();

                    _repository.AdicionarContato(new PessoaContatoModel
                    {
                        PessoaId = pessoaId,
                        TipoContato = tipo,
                        Valor = contato.Valor,
                        ValorNormalizado = valorLimpo,
                        Principal = contato.Principal,
                        Ativo = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            // Contatos Institucionais
            if (command.ContatosInstitucionais != null && command.ContatosInstitucionais.Any())
            {
                foreach (var contatoInst in command.ContatosInstitucionais)
                {
                    if (string.IsNullOrWhiteSpace(contatoInst.Nome) && string.IsNullOrWhiteSpace(contatoInst.Departamento)) continue;

                    _repository.AdicionarContatoInstitucional(new PessoaContatoInstitucionalModel
                    {
                        PessoaId = pessoaId,
                        Nome = contatoInst.Nome,
                        Departamento = contatoInst.Departamento,
                        Email = contatoInst.Email,
                        Telefone = contatoInst.Telefone,
                        Ramal = contatoInst.Ramal,
                        Ativo = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }


            // Endereço
            if (command.Endereco != null)
            {
                var end = command.Endereco;
                if (!string.IsNullOrWhiteSpace(end.Logradouro) || !string.IsNullOrWhiteSpace(end.Cep) || end.CidadeId.HasValue || !string.IsNullOrWhiteSpace(end.Uf))
                {
                    if (end.CidadeId.HasValue)
                    {
                        var cidadeExiste = await _repository.CidadeExisteAsync(end.CidadeId.Value, cancellationToken);
                        if (!cidadeExiste) throw new EstipulanteInvalidoException("A Cidade informada não existe.");
                    }

                    _repository.AdicionarEndereco(new PessoaEnderecoModel
                    {
                        PessoaId = pessoaId,
                        CidadeId = end.CidadeId,
                        TipoEndereco = "PRINCIPAL",
                        Cep = end.Cep,
                        Logradouro = end.Logradouro,
                        Numero = end.Numero,
                        Complemento = end.Complemento,
                        Bairro = end.Bairro,
                        Uf = end.Uf,
                        Principal = true
                    });
                }
            }

            // Configuração
            var configuracao = new EstipulanteConfiguracaoModel
            {
                Estipulante = estipulante,
                DataInicioVigencia = command.Configuracao.DataInicioVigencia,
                DataFimVigencia = command.Configuracao.DataFimVigencia,
                Carencia = command.Configuracao.Carencia,
                AdesaoPor = command.Configuracao.AdesaoPor,
                Custeio = command.Configuracao.Custeio,
                Adesao = command.Configuracao.Adesao,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            
            _repository.AdicionarConfiguracao(configuracao);

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Auditoria
            var cnpjMascarado = MascararDocumento(cnpjLimpo);
            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "ESTIPULANTE_CRIADO",
                Modulo = "Estipulantes",
                Recurso = "estipulante",
                RecursoId = estipulante.PublicId.ToString(),
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { 
                    RazaoSocial = command.RazaoSocial, 
                    Cnpj = cnpjMascarado, 
                    TipoPessoa = 2 
                })
            }, cancellationToken);

            return new CriarEstipulanteResult(
                estipulante.PublicId,
                estipulante.Nome,
                estipulante.Cnpj,
                "Ativo",
                estipulante.CreatedAt
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
        if (string.IsNullOrWhiteSpace(doc)) return string.Empty;
        return new string(doc.Where(char.IsDigit).ToArray());
    }

    private static bool ValidarCnpj(string limpo)
    {
        return limpo.Length == 14;
    }

    private static string MascararDocumento(string limpo)
    {
        if (limpo.Length == 14)
            return $"{limpo.Substring(0, 2)}.{limpo.Substring(2, 3)}.{limpo.Substring(5, 3)}/{limpo.Substring(8, 4)}-{limpo.Substring(12, 2)}";
        
        return limpo;
    }
}
