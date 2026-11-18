import { useCallback, useEffect, useState } from 'react';
import { ConfirmacaoProposta } from './ConfirmacaoProposta';
import { SimulacaoMensalProposta } from './SimulacaoMensalProposta';
import { ReinclusaoProposta } from './ReinclusaoProposta';
import { RevisaoCondicoesProposta } from './RevisaoCondicoesProposta';
import { PreenchimentoRascunhoForm } from './PreenchimentoRascunhoForm';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { useCriarProposta } from '../hooks/useCriarProposta';
import {
  Alert,
  Badge,
  Button,
  DescriptionItem,
  DescriptionList,
  DetailsSection,
  Skeleton,
} from '../../../components/ui';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';
import { obterProposta } from '../api/propostas.api';
import type { PropostaDetalhe } from '../types/proposta.types';

function descricaoVigencia(proposta: PropostaDetalhe) {
  if (proposta.situacaoVigenciaCodigo === 'CANCELAMENTO_AGENDADO')
    return 'Vigente com cancelamento agendado';
  if (proposta.situacaoVigenciaCodigo === 'CANCELAMENTO_PENDENTE')
    return 'Vigente com cancelamento pendente';
  if (proposta.situacaoVigenciaCodigo === 'DESCARTADA')
    return 'Pré proposta descartada';
  if (proposta.situacaoVigenciaCodigo === 'CANCELADA')
    return proposta.dataFimVigencia
      ? 'Vigência cancelada'
      : 'Cancelada sem vigência';
  if (proposta.vigente) return 'Vigente';
  const situacoes = {
    AGENDADA: 'Agendada',
    ATIVACAO_PENDENTE: 'Ativação pendente',
    SUBSTITUIDA: 'Substituída',
    SEM_VIGENCIA: 'Sem vigência',
    VIGENTE: 'Vigente',
  };
  return situacoes[proposta.situacaoVigenciaCodigo];
}

const reais = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});
function moeda(valor: number | null) {
  return valor === null ? 'Não informado' : reais.format(valor);
}

export function PropostaDetalhes({
  publicId,
  onVoltar,
  onNovaVersao,
  onBusy,
}: {
  publicId: string;
  onVoltar: () => void;
  onNovaVersao: (id: string) => void;
  onBusy?: (busy: boolean) => void;
}) {
  const { possuiPermissao } = useAuthorization();
  const [confirmandoVersao, setConfirmandoVersao] = useState(false);
  const criacao = useCriarProposta();
  const [editando, setEditando] = useState(false);
  const [salvo, setSalvo] = useState(false);
  const [confirmacaoOcupada, setConfirmacaoOcupada] = useState(false);
  const [reinclusaoOcupada, setReinclusaoOcupada] = useState(false);
  const [revisaoOcupada, setRevisaoOcupada] = useState(false);
  const [mensagemVigencia, setMensagemVigencia] = useState<string | null>(null);
  const operacaoEmAberto =
    editando || confirmacaoOcupada || reinclusaoOcupada || revisaoOcupada;
  useEffect(() => {
    if (operacaoEmAberto) onBusy?.(true);
    return () => {
      if (operacaoEmAberto) onBusy?.(false);
    };
  }, [operacaoEmAberto, onBusy]);
  const load = useCallback(
    (signal: AbortSignal) => obterProposta(publicId, signal),
    [publicId],
  );
  const { data, error, isLoading, refetch } = useConsulta(
    load,
    'Não foi possível carregar a proposta.',
  );
  return (
    <div className="flex flex-col gap-3">
      <div>
        <Button
          variant="secondary"
          onClick={onVoltar}
          disabled={criacao.isSubmitting || operacaoEmAberto}
        >
          Voltar às propostas
        </Button>
      </div>
      {isLoading ? (
        <div aria-label="Carregando proposta" aria-busy="true">
          <Skeleton className="h-32" />
        </div>
      ) : error ? (
        <>
          <Alert variant="error" title="Não foi possível carregar a proposta">
            {error.message}
          </Alert>
          <div>
            <Button onClick={() => void refetch()}>Tentar novamente</Button>
          </div>
        </>
      ) : (
        data && (
          <>
            {mensagemVigencia && (
              <Alert variant="success" title="Confirmação e vigência">
                {mensagemVigencia}
              </Alert>
            )}
            <ReinclusaoProposta
              proposta={data}
              podeInserir={possuiPermissao('propostas.inserir')}
              disabled={
                editando ||
                confirmacaoOcupada ||
                criacao.isSubmitting ||
                revisaoOcupada
              }
              onBusy={setReinclusaoOcupada}
              onCriada={onNovaVersao}
              onRecarregar={() => void refetch()}
            />
            {data.reinclusao && (
              <Alert variant="info" title="Nova versão de reinclusão">
                A proposta de origem permanece cancelada. O início não pode
                anteceder {formatarDataOuVazio(data.dataFimVigenciaOrigem)}.
                Eventual intervalo até o novo início permanece sem cobertura.
                Confira condições e beneficiários antes de confirmar.
              </Alert>
            )}
            <ConfirmacaoProposta
              proposta={data}
              podeAlterar={possuiPermissao('propostas.alterar')}
              disabled={
                editando ||
                criacao.isSubmitting ||
                reinclusaoOcupada ||
                revisaoOcupada
              }
              onBusy={setConfirmacaoOcupada}
              onConcluida={(mensagem) => {
                setMensagemVigencia(mensagem);
                setSalvo(false);
                void refetch();
              }}
            />
            <RevisaoCondicoesProposta
              proposta={data}
              podeAlterar={possuiPermissao('propostas.alterar')}
              disabled={
                editando ||
                confirmacaoOcupada ||
                reinclusaoOcupada ||
                criacao.isSubmitting ||
                confirmandoVersao
              }
              onBusy={setRevisaoOcupada}
              onRecarregar={() => {
                setMensagemVigencia(null);
                void refetch();
              }}
              onConcluida={() => {
                setMensagemVigencia(
                  'Condições revisadas. A Vida e os beneficiários foram preservados.',
                );
                void refetch();
              }}
            />
            {data.condicoesProprias && (
              <Alert variant="info" title="Condições próprias da proposta">
                Os vínculos da Vida foram preservados. Itens e coberturas
                inativos representam condições anteriores à revisão.
              </Alert>
            )}
            <SimulacaoMensalProposta
              proposta={data}
              disabled={
                operacaoEmAberto || criacao.isSubmitting || confirmandoVersao
              }
            />
            {salvo && (
              <Alert variant="success" title="Pré proposta salva">
                O preenchimento foi salvo sem ativar a proposta.
              </Alert>
            )}
            {data.podeEditarRascunho &&
              possuiPermissao('propostas.alterar') && (
                <DetailsSection title="Preenchimento da pré proposta">
                  {editando ? (
                    <PreenchimentoRascunhoForm
                      key={data.publicId}
                      proposta={data}
                      onCancelar={() => setEditando(false)}
                      onRecarregar={() => {
                        setEditando(false);
                        void refetch();
                      }}
                      onSalvo={() => {
                        setEditando(false);
                        setSalvo(true);
                        void refetch();
                      }}
                    />
                  ) : (
                    <Button
                      variant="secondary"
                      disabled={
                        confirmacaoOcupada ||
                        reinclusaoOcupada ||
                        revisaoOcupada
                      }
                      onClick={() => {
                        setMensagemVigencia(null);
                        setSalvo(false);
                        setEditando(true);
                      }}
                    >
                      Editar pré proposta
                    </Button>
                  )}
                </DetailsSection>
              )}
            <DetailsSection
              title={
                data.numero ? `Proposta ${data.numero}` : 'Proposta sem número'
              }
            >
              <DescriptionList columns={3} density="compact">
                <DescriptionItem
                  label="Situação"
                  value={<Badge>{data.status}</Badge>}
                />
                <DescriptionItem
                  label="Vigência registrada"
                  value={descricaoVigencia(data)}
                />
                <DescriptionItem
                  label="Início da vigência"
                  value={formatarDataOuVazio(data.dataInicioVigencia)}
                />
                <DescriptionItem
                  label="Fim da vigência (exclusivo)"
                  value={formatarDataOuVazio(data.dataFimVigencia)}
                />
                <DescriptionItem
                  label="Cancelamento registrado em"
                  value={formatarDataOuVazio(data.canceladaEm)}
                />
                <DescriptionItem
                  label="Cancelamento solicitado em"
                  value={formatarDataOuVazio(data.cancelamentoSolicitadoEm)}
                />
                <DescriptionItem
                  label="Data solicitada de cancelamento"
                  value={formatarDataOuVazio(data.dataCancelamentoVigencia)}
                />
                <DescriptionItem
                  label="Descarte registrado em"
                  value={formatarDataOuVazio(data.descartadaEm)}
                />
                <DescriptionItem
                  label="Confirmação registrada em"
                  value={formatarDataOuVazio(data.confirmadaEm)}
                />
                <DescriptionItem
                  label="Substituição registrada em"
                  value={formatarDataOuVazio(data.substituidaEm)}
                />
                <DescriptionItem label="Versão" value={data.versao} />
                <DescriptionItem
                  label="Cliente (cadastro atual)"
                  value={data.clienteNome}
                />
                <DescriptionItem
                  label="Matrícula"
                  value={data.vinculoMatricula}
                />
                <DescriptionItem
                  label="Subgrupo registrado"
                  value={data.subgrupoNomeRegistrado}
                />
                <DescriptionItem
                  label="Módulo registrado"
                  value={data.moduloNomeRegistrado}
                />
                <DescriptionItem
                  label="Apólice (cadastro atual)"
                  value={data.apoliceNome}
                />
                <DescriptionItem
                  label="Data de inclusão"
                  value={formatarDataOuVazio(data.dataInclusao)}
                />
                <DescriptionItem
                  label="Data de criação"
                  value={formatarDataOuVazio(data.createdAt)}
                />
                <DescriptionItem
                  label="Integração com Vida"
                  value={
                    data.integradaApolice
                      ? 'Vinculada à Vida'
                      : 'Sem vínculo completo com Vida'
                  }
                />
                <DescriptionItem
                  label="Observação"
                  value={data.observacao}
                  className="sm:col-span-2 lg:col-span-3"
                />
              </DescriptionList>
            </DetailsSection>
            {data.propostaOrigemPublicId && (
              <div>
                <Button
                  variant="text"
                  disabled={criacao.isSubmitting || operacaoEmAberto}
                  onClick={() => onNovaVersao(data.propostaOrigemPublicId!)}
                >
                  Consultar proposta anterior
                </Button>
              </div>
            )}
            {data.vigente &&
              !data.cancelamentoSolicitadoEm &&
              data.apolicePublicId &&
              data.apoliceVidaPublicId &&
              possuiPermissao('propostas.inserir') && (
                <DetailsSection title="Nova versão">
                  {!confirmandoVersao ? (
                    <Button
                      variant="secondary"
                      onClick={() => setConfirmandoVersao(true)}
                      disabled={operacaoEmAberto || criacao.isSubmitting}
                    >
                      Preparar nova versão
                    </Button>
                  ) : (
                    <div className="flex flex-col gap-3">
                      <p>
                        Será criada uma nova versão de pré proposta com as
                        condições atuais do Módulo da Vida. A proposta anterior
                        continuará vigente. Esta etapa não confirma a
                        substituição nem gera cobrança.
                      </p>
                      {criacao.error && (
                        <Alert
                          variant="error"
                          title="Não foi possível preparar a versão"
                        >
                          {criacao.error}
                        </Alert>
                      )}
                      <div className="flex flex-wrap gap-2">
                        <Button
                          loading={criacao.isSubmitting}
                          onClick={() => {
                            onBusy?.(true);
                            void criacao
                              .criar(
                                data.apolicePublicId!,
                                data.apoliceVidaPublicId!,
                                onNovaVersao,
                                data.publicId,
                              )
                              .finally(() => onBusy?.(false));
                          }}
                        >
                          Criar versão de pré proposta
                        </Button>
                        <Button
                          variant="secondary"
                          disabled={criacao.isSubmitting || operacaoEmAberto}
                          onClick={() => setConfirmandoVersao(false)}
                        >
                          Cancelar
                        </Button>
                      </div>
                    </div>
                  )}
                </DetailsSection>
              )}
            <DetailsSection
              title="Condições financeiras registradas"
              description="Consulta dos valores existentes. Esta tela não calcula valores nem gera cobranças."
            >
              <DescriptionList columns={3} density="compact">
                <DescriptionItem
                  label="Prêmio líquido"
                  value={moeda(data.premioLiquido)}
                />
                <DescriptionItem
                  label="Valor da parcela"
                  value={moeda(data.valorParcela)}
                />
                <DescriptionItem label="IOF" value={moeda(data.iofValor)} />
                <DescriptionItem
                  label="IOF percentual"
                  value={
                    data.iofPercentual === null
                      ? null
                      : `${data.iofPercentual.toLocaleString('pt-BR')}%`
                  }
                />
                <DescriptionItem
                  label="Primeiro vencimento"
                  value={formatarDataOuVazio(data.dataPrimeiroVencimento)}
                />
                <DescriptionItem
                  label="Próximo vencimento"
                  value={formatarDataOuVazio(data.dataProximoVencimento)}
                />
              </DescriptionList>
            </DetailsSection>
            <DetailsSection
              title="Histórico de versões"
              isEmpty={(data.historico ?? []).length === 0}
            >
              <div className="flex flex-col gap-3">
                {(data.historico ?? []).map((evento, index) => (
                  <div
                    key={index}
                    className="flex flex-col gap-2 border-b border-borda pb-3"
                  >
                    <DescriptionList columns={3} density="compact">
                      <DescriptionItem
                        label="Data da alteração"
                        value={formatarDataOuVazio(evento.dataAlteracao)}
                      />
                      <DescriptionItem label="Motivo" value={evento.motivo} />
                      <DescriptionItem
                        label="Observação do histórico"
                        value={evento.observacao}
                      />
                    </DescriptionList>
                    <div className="flex flex-wrap gap-2">
                      {evento.propostaAnteriorPublicId &&
                        evento.propostaAnteriorPublicId !== data.publicId && (
                          <Button
                            variant="text"
                            disabled={criacao.isSubmitting || operacaoEmAberto}
                            onClick={() =>
                              onNovaVersao(evento.propostaAnteriorPublicId!)
                            }
                          >
                            Consultar versão {evento.versaoAnterior}
                          </Button>
                        )}
                      {evento.propostaNovaPublicId &&
                        evento.propostaNovaPublicId !== data.publicId && (
                          <Button
                            variant="text"
                            disabled={criacao.isSubmitting || operacaoEmAberto}
                            onClick={() =>
                              onNovaVersao(evento.propostaNovaPublicId!)
                            }
                          >
                            Consultar versão {evento.versaoNova}
                          </Button>
                        )}
                    </div>
                  </div>
                ))}
              </div>
            </DetailsSection>
            <DetailsSection
              title="Itens"
              isEmpty={data.itens.length === 0}
              description="Nomes atuais são referências de cadastro. Os valores abaixo são os registrados na proposta."
            >
              <div className="flex flex-col gap-3">
                {data.itens.map((item, index) => (
                  <DescriptionList
                    key={item.publicId}
                    columns={3}
                    density="compact"
                  >
                    <DescriptionItem
                      label={`Item ${index + 1} — Produto atual`}
                      value={item.produtoNomeAtual}
                    />
                    <DescriptionItem
                      label="Plano atual"
                      value={item.planoNomeAtual}
                    />
                    <DescriptionItem
                      label="Plano registrado"
                      value={item.planoNomeRegistrado}
                    />
                    <DescriptionItem
                      label="Valor registrado"
                      value={moeda(item.valor)}
                    />
                    <DescriptionItem
                      label="Situação do item"
                      value={item.ativo ? 'Ativo' : 'Inativo'}
                    />
                    <DescriptionItem
                      label="Origem das condições"
                      value={
                        item.planoContextual
                          ? 'Plano do Módulo'
                          : 'Estrutura anterior'
                      }
                    />
                    <DescriptionItem
                      label="Ramo registrado"
                      value={item.ramo}
                    />
                    <DescriptionItem
                      label="Paga (Plano)"
                      value={
                        item.planoPaga === null
                          ? null
                          : item.planoPaga
                            ? 'Sim'
                            : 'Não'
                      }
                    />
                    <DescriptionItem
                      label="Reajuste (Plano)"
                      value={
                        item.planoReajuste === null
                          ? null
                          : item.planoReajuste
                            ? 'Sim'
                            : 'Não'
                      }
                    />
                  </DescriptionList>
                ))}
              </div>
            </DetailsSection>
            <DetailsSection
              title="Coberturas"
              isEmpty={data.coberturas.length === 0}
            >
              <div className="flex flex-col gap-3">
                {data.coberturas.map((cobertura) => (
                  <DescriptionList
                    key={cobertura.publicId}
                    columns={3}
                    density="compact"
                  >
                    <DescriptionItem
                      label="Nome registrado"
                      value={cobertura.nomeRegistrado}
                    />
                    <DescriptionItem
                      label="Nome atual do cadastro"
                      value={cobertura.nomeAtual}
                    />
                    <DescriptionItem
                      label="Nome reduzido registrado"
                      value={cobertura.nomeReduzidoRegistrado}
                    />
                    <DescriptionItem
                      label="Básica registrada"
                      value={cobertura.basicaRegistrada}
                    />
                    <DescriptionItem
                      label="Reajuste registrado"
                      value={
                        cobertura.reajusteRegistrado === null
                          ? null
                          : cobertura.reajusteRegistrado
                            ? 'Sim'
                            : 'Não'
                      }
                    />
                    <DescriptionItem
                      label="Situação da cobertura"
                      value={cobertura.ativo ? 'Ativa' : 'Inativa'}
                    />
                    <DescriptionItem
                      label="Prêmio titular registrado"
                      value={moeda(cobertura.premioTitular)}
                    />
                    <DescriptionItem
                      label="Prêmio cônjuge registrado"
                      value={moeda(cobertura.premioConjuge)}
                    />
                    <DescriptionItem
                      label="Item"
                      value={
                        cobertura.itemPublicId
                          ? `Item ${data.itens.findIndex((i) => i.publicId === cobertura.itemPublicId) + 1}`
                          : 'Diretamente na proposta'
                      }
                    />
                    {cobertura.premioAjustado && (
                      <>
                        <DescriptionItem
                          label="Prêmio titular de origem"
                          value={moeda(cobertura.premioTitularInicial)}
                        />
                        <DescriptionItem
                          label="Prêmio cônjuge de origem"
                          value={moeda(cobertura.premioConjugeInicial)}
                        />
                        <DescriptionItem
                          label="Justificativa do último ajuste"
                          value={cobertura.justificativaAjustePremio}
                        />
                      </>
                    )}
                  </DescriptionList>
                ))}
              </div>
            </DetailsSection>
            <DetailsSection
              title="Beneficiários"
              isEmpty={data.beneficiarios.length === 0}
            >
              <div className="flex flex-col gap-3">
                {data.beneficiarios.map((beneficiario) => (
                  <DescriptionList
                    key={beneficiario.publicId}
                    columns={3}
                    density="compact"
                  >
                    <DescriptionItem
                      label="Nome registrado"
                      value={beneficiario.nome}
                    />
                    <DescriptionItem
                      label="Parentesco registrado"
                      value={beneficiario.parentesco}
                    />
                    <DescriptionItem
                      label="Participação registrada"
                      value={
                        beneficiario.percentualParticipacao === null
                          ? null
                          : `${beneficiario.percentualParticipacao.toLocaleString('pt-BR', { maximumFractionDigits: 4 })}%`
                      }
                    />
                  </DescriptionList>
                ))}
              </div>
            </DetailsSection>
          </>
        )
      )}
    </div>
  );
}
