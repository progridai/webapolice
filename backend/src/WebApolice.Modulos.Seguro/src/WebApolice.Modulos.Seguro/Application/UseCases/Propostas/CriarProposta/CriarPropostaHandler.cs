using Microsoft.Extensions.Options;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ReincluirProposta;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;

public class CriarPropostaHandler : IRequestHandler<CriarPropostaCommand, Guid>
{
    private readonly IPropostasRepository _repository;

    private readonly IClientesPropostas _clientes;
    private readonly ConfiguracaoPropostas _configuracao;
    private readonly IOperacaoPropostas _operacao;
    private readonly ICadastroSeguroQueries _cadastro;

    public CriarPropostaHandler(IPropostasRepository dbContext, IClientesPropostas clientes,
        IOptions<ConfiguracaoPropostas> configuracao, IOperacaoPropostas operacao, ICadastroSeguroQueries cadastro)
    {
        _repository = dbContext;
        _clientes = clientes;
        _configuracao = configuracao.Value;
        _operacao = operacao;
        _cadastro = cadastro;
    }

    public async Task<Guid> Handle(CriarPropostaCommand request, CancellationToken cancellationToken)
    {
        if (request.ClienteId <= 0)
            throw new ValidacaoException("Cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(_configuracao.StatusInicialCodigo))
            throw new ValidacaoException("O status inicial de Propostas ainda não foi definido.");
        return await _operacao.ExecutarAsync<Guid>("PROPOSTA_CRIADA", async ct =>
        {
            await _repository.BloquearApoliceAsync(request.ApoliceId, ct);
            var status = await _repository.ObterStatusAsync(_configuracao.StatusInicialCodigo, cancellationToken)
                ?? throw new ValidacaoException("Status inicial de Propostas não encontrado no catálogo.");
            if (status.Finalizador || !status.VisivelOperacional || status.PermiteMovimentacao)
                throw new ValidacaoException("O status inicial deve representar preenchimento sem movimentação financeira.");
            // 1. Carregar Entidades Mestre
            var apolice = await _repository.ObterApoliceAsync(request.ApoliceId, cancellationToken);

            if (apolice == null)
                throw new ValidacaoException("Apólice não encontrada.");
            if (!apolice.Ativo || apolice.DeletedAt != null)
                throw new ValidacaoException("A Apólice deve estar ativa para criar uma proposta.");

            var apoliceVida = await _repository.ObterVidaAsync(request.ApoliceVidaId, cancellationToken);

            if (apoliceVida == null)
                throw new ValidacaoException("ApoliceVida não encontrada.");

            // 2. Travar Segurança e Integridade Cross-Contexto
            if (apoliceVida.ApoliceId != apolice.Id)
                throw new ValidacaoException("A ApoliceVida informada não pertence à Apólice informada.");

            if (apoliceVida.ClienteId != request.ClienteId)
                throw new ValidacaoException("O Cliente informado não é o titular desta Inscrição (ApoliceVida).");

            if (!apoliceVida.Ativo || apoliceVida.DeletedAt != null)
                throw new ValidacaoException("A Inscrição está inativa e não pode gerar nova Proposta.");

            if (apoliceVida.ClienteVinculoId is not > 0)
                throw new ValidacaoException("A Vida deve possuir vínculo de Cliente/Pessoa.");
            var identidade = await _clientes.ResolverAsync(request.ClienteId, apoliceVida.ClienteVinculoId.Value, cancellationToken)
                ?? throw new ValidacaoException("Vínculo de Cliente/Pessoa inexistente, inativo ou incompatível.");

            if (identidade.EstipulanteId != apolice.EstipulanteId)
                throw new ValidacaoException("O vínculo do cliente pertence a outro estipulante ou está sem estipulante.");

            var anteriores = await _repository.ObterPropostasDaVidaAsync(apoliceVida.Id, ct);
            if (anteriores.Any(p => p.StatusId == status.Id && p.DeletedAt == null))
                throw new ValidacaoException("Esta Vida já possui uma proposta em preenchimento. Consulte a proposta existente.");
            Propostum? origem = null;
            var reinclusao = request.OrigemCondicoesReinclusao != null;
            if (reinclusao)
            {
                if (request.OrigemCondicoesReinclusao is not ("ANTERIOR" or "MODULO") || request.PropostaOrigemPublicId is not Guid origemId)
                    throw new ValidacaoException("Escolha explicitamente a origem e as condições da reinclusão.");
                origem = await _repository.ObterParaPreenchimentoAsync(origemId, ct)
                    ?? throw new ValidacaoException("Proposta de origem não encontrada.");
                if (request.RevisaoOrigemEsperada is null or < 0 || origem.RevisaoPreenchimento != request.RevisaoOrigemEsperada)
                    throw new ValidacaoException("A proposta de origem foi alterada. Recarregue antes de preparar a reinclusão.");
                if (origem.ApoliceVidaId != apoliceVida.Id || origem.ApoliceId != apolice.Id || origem.ClienteId != identidade.ClienteId ||
                    origem.ClienteVinculoId != identidade.ClienteVinculoId || origem.PessoaId != identidade.PessoaId ||
                    !ReinclusaoPropostas.SemImpedimentos(origem, anteriores, _configuracao))
                    throw new ValidacaoException("Selecione a última proposta nativa cancelada com vigência da mesma Vida, sem vigente ou tentativa posterior em aberto.");
            }
            else if (request.PropostaOrigemPublicId.HasValue)
            {
                origem = anteriores.SingleOrDefault(p => p.PublicId == request.PropostaOrigemPublicId && p.CancelamentoSolicitadoEm == null && p.Vigente && p.DeletedAt == null && p.VisivelOperacional &&
                    p.ApoliceId == apolice.Id && p.ClienteId == apoliceVida.ClienteId && p.ClienteVinculoId == identidade.ClienteVinculoId && !p.Status.Finalizador)
                    ?? throw new ValidacaoException("A proposta de origem deve estar vigente e pertencer à mesma Vida.");
                if (anteriores.Any(p => p.PropostaOrigemId == origem.Id && !p.TentativaEncerradaSemVigencia(_configuracao.StatusCanceladaCodigo, _configuracao.StatusDescartadaCodigo)))
                    throw new ValidacaoException("Esta proposta já possui uma versão sucessora. Consulte o histórico antes de continuar.");
                if (origem.Versao < 1 || origem.Versao == int.MaxValue)
                    throw new ValidacaoException("O limite de versões desta proposta foi atingido.");
            }
            else if (anteriores.Any(p => !p.TentativaEncerradaSemVigencia(_configuracao.StatusCanceladaCodigo, _configuracao.StatusDescartadaCodigo)))
                throw new ValidacaoException("Esta Vida já possui propostas. Selecione explicitamente uma proposta vigente para criar uma nova versão.");

            var maiorVersao = anteriores.Select(p => p.Versao).DefaultIfEmpty(0).Max();
            if (maiorVersao == int.MaxValue) throw new ValidacaoException("O limite de versões desta Vida foi atingido.");

            if (apoliceVida.ApoliceSubgrupoId.HasValue && (apoliceVida.ApoliceSubgrupo is not { Ativo: true, DeletedAt: null } subgrupo || subgrupo.ApoliceId != apolice.Id))
                throw new ValidacaoException("O Subgrupo da Vida está inativo ou é incompatível com a Apólice.");
            ModuloSeguro? modulo = null;
            ApoliceModuloPlanoModel? planoModulo = null;
            if (apoliceVida.ApoliceModuloId.HasValue)
            {
                var vinculoModulo = apoliceVida.ApoliceModulo;
                if (vinculoModulo is not { Ativo: true, DeletedAt: null } || vinculoModulo.ApoliceId != apolice.Id)
                    throw new ValidacaoException("O Módulo da Vida está inativo ou é incompatível com a Apólice.");
                modulo = await _cadastro.ObterModuloPorIdAsync(vinculoModulo.ModuloId, ct);
                if (modulo is not null) modulo = await _cadastro.ObterModuloAsync(modulo.PublicId, ct);
                if (modulo is not { Ativo: true }) throw new ValidacaoException("O cadastro do Módulo da Vida está indisponível.");
                planoModulo = await _repository.ObterPlanoModuloAsync(vinculoModulo.Id, ct);
                if (planoModulo is not { Ativo: true }) throw new ValidacaoException("Configure um Plano ativo no Módulo da Vida antes de criar a proposta.");
            }
            // 3. Montar Snapshot Protegido
            var proposta = new Propostum
            {
                PublicId = Guid.NewGuid(),
                ApoliceId = apolice.Id,
                ApoliceVidaId = apoliceVida.Id,
                ApoliceSubgrupoId = apoliceVida.ApoliceSubgrupoId,
                ApoliceModuloId = apoliceVida.ApoliceModuloId,
                SubgrupoNomeRegistrado = apoliceVida.ApoliceSubgrupo?.Nome,
                ModuloNomeRegistrado = modulo?.Nome,

                // Snapshot obrigatório de hierarquia
                EstipulanteId = apolice.EstipulanteId,
                SeguradoraId = apolice.SeguradoraId,

                // Snapshot Híbrido (Se a apólice forçar corretora, usamos ela. Se não, permitimos o input da requisição)
                CorretoraId = apolice.CorretoraId ?? request.CorretoraId,

                // Vínculos Físicos (Vida)
                ClienteId = apoliceVida.ClienteId,
                ClienteVinculoId = identidade.ClienteVinculoId,
                PessoaId = identidade.PessoaId,

                // Campos Financeiros/Pessoais Mutáveis (Input Livre)
                ConvenioCobrancaId = request.ConvenioCobrancaId,
                ContaCobrancaId = request.ContaCobrancaId,
                PremioLiquido = request.PremioLiquido,
                ValorParcela = request.ValorParcela,
                DataPrimeiroVencimento = request.DataPrimeiroVencimento,
                DataProximoVencimento = request.DataProximoVencimento,
                BancoAgencia = request.BancoAgencia,
                BancoContaCorrente = request.BancoContaCorrente,

                StatusId = status.Id,
                Vigente = false,
                VisivelOperacional = true,
                Versao = checked(maiorVersao + 1),
                PropostaOrigemId = origem?.Id,
                IncluiConjuge = origem?.IncluiConjuge,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (reinclusao && request.OrigemCondicoesReinclusao == "ANTERIOR")
                ReinclusaoPropostas.CopiarCondicoes(origem!, proposta, planoModulo);
            else if (planoModulo is not null)
            {
                var item = new PropostaItem
                {
                    PublicId = Guid.NewGuid(), Proposta = proposta,
                    ApoliceModuloPlanoId = planoModulo.Id, PlanoNomeRegistrado = planoModulo.Nome,
                    Ramo = planoModulo.Ramo, PlanoPaga = planoModulo.Paga, PlanoReajuste = planoModulo.Reajuste,
                    Ativo = true, CreatedAt = proposta.CreatedAt, UpdatedAt = proposta.UpdatedAt
                };
                proposta.PropostaItems.Add(item);
                foreach (var cobertura in planoModulo.Coberturas.Where(c => c.Ativo && c.DeletedAt == null))
                {
                    // Apenas as condições contratadas no Módulo; não calcula prêmio líquido, IOF ou parcela.
                    proposta.PropostaCoberturas.Add(new PropostaCobertura
                    {
                        PublicId = Guid.NewGuid(), Proposta = proposta, PropostaItem = item,
                        CoberturaId = cobertura.CoberturaId, ApoliceModuloCoberturaId = cobertura.Id,
                        CoberturaNomeRegistrado = cobertura.Cobertura.Nome,
                        CoberturaNomeReduzidoRegistrado = cobertura.Cobertura.NomeReduzido,
                        CoberturaBasicaRegistrada = cobertura.Cobertura.Basica,
                        CoberturaReajuste = cobertura.Cobertura.Reajuste,
                        PremioTitular = cobertura.PremioTitular, PremioConjuge = cobertura.PremioConjuge,
                        Ativo = true, CreatedAt = proposta.CreatedAt, UpdatedAt = proposta.UpdatedAt
                    });
                }
            }
            if (origem is not null)
            {
                if (reinclusao) ReinclusaoPropostas.CopiarBeneficiarios(origem, proposta);
                proposta.PropostaHistoricoPropostaNovas.Add(new PropostaHistorico
                {
                    PropostaAnteriorId = origem.Id, PropostaNova = proposta,
                    Motivo = reinclusao ? "Preparação de Reinclusão" : "Preparação de Nova Versão",
                    Observacao = reinclusao ? request.OrigemCondicoesReinclusao == "ANTERIOR" ? "Condições da Proposta Anterior. Beneficiários Nativos Copiados para Revisão." : "Condições Atuais do Módulo. Beneficiários Nativos Copiados para Revisão." : null,
                    DataAlteracao = proposta.CreatedAt, CreatedAt = proposta.CreatedAt
                });
            }

            _repository.Adicionar(proposta);
            await _repository.SalvarAlteracoesAsync(cancellationToken);

            return (proposta.PublicId, proposta.PublicId.ToString(), null, new { proposta.PublicId, VidaPublicId = apoliceVida.PublicId,
                proposta.Versao, PropostaOrigemPublicId = origem?.PublicId, PlanoModuloPublicId = planoModulo?.PublicId,
                OrigemCondicoesReinclusao = request.OrigemCondicoesReinclusao, BeneficiariosCopiados = proposta.PropostaBeneficiarios.Count });
        }, cancellationToken);
    }
}
