import { useCallback, useEffect, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Alert,
  Button,
  DescriptionItem,
  DescriptionList,
  DetailsSection,
  FormActions,
  FormField,
  Select,
  Skeleton,
  Textarea,
} from '../../../components/ui';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import { formatarPremio } from '../../../shared/utils/formatters';
import {
  aplicarCondicoes,
  compararCondicoes,
  obterOpcoesCondicoes,
} from '../api/revisaoCondicoes.api';
import {
  aplicarCondicoesSchema,
  selecaoCondicoesSchema,
  type AplicarCondicoesForm,
  type SelecaoCondicoesForm,
} from '../schemas/revisaoCondicoes.schema';
import type { PropostaDetalhe } from '../types/proposta.types';
import type {
  ComparacaoCondicoes,
  CondicoesRegistradas,
  OpcoesCondicoes,
  SelecaoCondicoes,
} from '../types/revisaoCondicoes.types';

interface Props {
  proposta: PropostaDetalhe;
  podeAlterar: boolean;
  disabled: boolean;
  onBusy: (busy: boolean) => void;
  onConcluida: () => void;
  onRecarregar: () => void;
}
export function RevisaoCondicoesProposta({
  proposta,
  podeAlterar,
  disabled,
  onBusy,
  onConcluida,
  onRecarregar,
}: Props) {
  const [aberta, setAberta] = useState(false);
  useEffect(() => {
    onBusy(aberta);
    return () => onBusy(false);
  }, [aberta, onBusy]);
  if (!podeAlterar || !proposta.podeRevisarCondicoes) return null;
  return (
    <DetailsSection title="Revisão das condições">
      {aberta ? (
        <RevisaoCarregamento
          proposta={proposta}
          onFechar={() => setAberta(false)}
          onRecarregar={() => {
            setAberta(false);
            onRecarregar();
          }}
          onConcluida={() => {
            setAberta(false);
            onConcluida();
          }}
        />
      ) : (
        <Button
          variant="secondary"
          disabled={disabled}
          onClick={() => setAberta(true)}
        >
          Revisar condições
        </Button>
      )}
    </DetailsSection>
  );
}

function RevisaoCarregamento({
  proposta,
  onFechar,
  onConcluida,
  onRecarregar,
}: {
  proposta: PropostaDetalhe;
  onFechar: () => void;
  onConcluida: () => void;
  onRecarregar: () => void;
}) {
  const load = useCallback(
    (signal: AbortSignal) => obterOpcoesCondicoes(proposta.publicId, signal),
    [proposta.publicId],
  );
  const { data, error, isLoading, refetch } = useConsulta(
    load,
    'Não foi possível carregar os vínculos da Apólice.',
  );
  if (isLoading)
    return (
      <>
        <div aria-label="Carregando vínculos" aria-busy="true">
          <Skeleton className="h-16" />
        </div>
        <Button variant="secondary" onClick={onFechar}>
          Voltar sem aplicar
        </Button>
      </>
    );
  if (error || !data)
    return (
      <>
        <Alert variant="error" title="Não foi possível carregar as opções">
          {error?.message}
        </Alert>
        <Button onClick={() => void refetch()}>Tentar novamente</Button>
        <Button variant="secondary" onClick={onFechar}>
          Voltar sem aplicar
        </Button>
      </>
    );
  if (data.modulos.length === 0)
    return (
      <>
        <Alert variant="info" title="Nenhum Módulo disponível">
          Configure um Módulo ativo na Apólice para revisar as condições.
        </Alert>
        <Button variant="secondary" onClick={onFechar}>
          Voltar sem aplicar
        </Button>
      </>
    );
  return (
    <RevisaoEditor
      proposta={proposta}
      opcoes={data}
      onRecarregar={onRecarregar}
      onFechar={onFechar}
      onConcluida={onConcluida}
    />
  );
}

function RevisaoEditor({
  proposta,
  opcoes,
  onFechar,
  onConcluida,
  onRecarregar,
}: {
  proposta: PropostaDetalhe;
  opcoes: OpcoesCondicoes;
  onFechar: () => void;
  onConcluida: () => void;
  onRecarregar: () => void;
}) {
  const [comparacao, setComparacao] = useState<ComparacaoCondicoes | null>(
    null,
  );
  const [selecao, setSelecao] = useState<SelecaoCondicoes | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const [tratamento, setTratamento] = useState('');
  const comparando = useRef<AbortController | null>(null);
  const enviando = useRef(false);
  useEffect(() => () => comparando.current?.abort(), []);
  const contexto = useForm<SelecaoCondicoesForm>({
    resolver: zodResolver(selecaoCondicoesSchema),
    defaultValues: {
      moduloPublicId: opcoes.moduloPublicId ?? '',
      subgrupoPublicId: opcoes.subgrupoPublicId ?? '',
    },
  });
  const aplicacao = useForm<AplicarCondicoesForm>({
    resolver: zodResolver(aplicarCondicoesSchema),
  });
  const ocupado =
    contexto.formState.isSubmitting || aplicacao.formState.isSubmitting;
  async function comparar(dados: SelecaoCondicoesForm) {
    const controller = new AbortController();
    comparando.current = controller;
    const alvo = {
      ...dados,
      subgrupoPublicId: dados.subgrupoPublicId || null,
      revisaoEsperada: proposta.revisaoPreenchimento,
    };
    setErro(null);
    try {
      const resultado = await compararCondicoes(
        proposta.publicId,
        alvo,
        controller.signal,
      );
      if (!controller.signal.aborted) {
        setSelecao(alvo);
        setComparacao(resultado);
      }
    } catch (cause) {
      if (!controller.signal.aborted)
        setErro(
          cause instanceof Error
            ? cause.message
            : 'Não foi possível comparar as condições.',
        );
    }
  }
  async function aplicar(dados: AplicarCondicoesForm) {
    if (!comparacao || !selecao) return;
    setErro(null);
    try {
      await aplicarCondicoes(proposta.publicId, {
        ...selecao,
        ...dados,
        comparacao: comparacao.comparacao,
      });
      onConcluida();
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível aplicar as condições.',
      );
    }
  }
  const diferente =
    tratamento === 'MANTER'
      ? comparacao?.alteracaoMantendoAjustes
      : comparacao?.alteracaoModulo;
  return (
    <div className="flex flex-col gap-3">
      <Alert variant="info" title="Comparar antes de aplicar">
        Escolha vínculos ativos desta Apólice. A mudança vale somente para esta
        proposta; a Vida, as versões anteriores, os beneficiários e as condições
        substituídas serão preservados. A revisão não ativa vigência nem gera
        cobrança.
        {proposta.confirmadaEm &&
          ' O início agendado será mantido e a elegibilidade será revalidada antes da aplicação.'}
      </Alert>
      {erro && (
        <Alert variant="error" title="Não foi possível concluir a revisão">
          {erro}
          <Button variant="secondary" disabled={ocupado} onClick={onRecarregar}>
            Recarregar proposta e descartar revisão
          </Button>
        </Alert>
      )}
      {!comparacao ? (
        <form
          aria-label="Comparação das condições"
          noValidate
          onSubmit={(event) => {
            if (enviando.current) {
              event.preventDefault();
              return;
            }
            enviando.current = true;
            void contexto
              .handleSubmit(comparar)(event)
              .finally(() => {
                enviando.current = false;
              });
          }}
        >
          <FormField
            label="Módulo da proposta"
            required
            error={contexto.formState.errors.moduloPublicId?.message}
          >
            <Select
              id="revisao-modulo"
              defaultValue={opcoes.moduloPublicId ?? ''}
              placeholder="Selecione o Módulo"
              disabled={ocupado}
              {...contexto.register('moduloPublicId')}
            >
              {opcoes.modulos.map((m) => (
                <option key={m.publicId} value={m.publicId}>
                  {m.nome}
                </option>
              ))}
            </Select>
          </FormField>
          <FormField
            label="Subgrupo da proposta"
            error={contexto.formState.errors.subgrupoPublicId?.message}
          >
            <Select
              id="revisao-subgrupo"
              defaultValue={opcoes.subgrupoPublicId ?? ''}
              disabled={ocupado}
              {...contexto.register('subgrupoPublicId')}
            >
              <option value="">Sem Subgrupo</option>
              {opcoes.subgrupos.map((s) => (
                <option key={s.publicId} value={s.publicId}>
                  {s.nome}
                </option>
              ))}
            </Select>
          </FormField>
          <FormActions sticky={false}>
            <Button variant="secondary" disabled={ocupado} onClick={onFechar}>
              Voltar sem aplicar
            </Button>
            <Button type="submit" loading={contexto.formState.isSubmitting}>
              Comparar condições
            </Button>
          </FormActions>
        </form>
      ) : (
        <>
          <ResumoDiferencas comparacao={comparacao} />
          <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
            <ResumoCondicoes
              titulo="Condições registradas"
              dados={comparacao.atual}
            />
            <ResumoCondicoes
              titulo="Condições atuais do Módulo"
              dados={comparacao.modulo}
            />
          </div>
          <form
            aria-label="Aplicação das condições"
            noValidate
            onSubmit={(event) => {
              if (enviando.current) {
                event.preventDefault();
                return;
              }
              enviando.current = true;
              void aplicacao
                .handleSubmit(aplicar)(event)
                .finally(() => {
                  enviando.current = false;
                });
            }}
          >
            <FormField
              label="Tratamento dos ajustes"
              required
              error={aplicacao.formState.errors.tratamentoAjustes?.message}
            >
              <Select
                id="revisao-ajustes"
                defaultValue=""
                placeholder="Selecione o tratamento"
                disabled={ocupado}
                {...aplicacao.register('tratamentoAjustes', {
                  onChange: (event: React.ChangeEvent<HTMLSelectElement>) =>
                    setTratamento(event.target.value),
                })}
              >
                <option value="MANTER">
                  Manter ajustes nas coberturas correspondentes
                </option>
                <option value="MODULO">Usar prêmios atuais do Módulo</option>
              </Select>
            </FormField>
            {tratamento && (
              <ResumoCondicoes
                titulo="Resultado da revisão"
                dados={
                  tratamento === 'MANTER'
                    ? comparacao.mantendoAjustes
                    : comparacao.modulo
                }
              />
            )}
            {tratamento && !diferente && (
              <Alert variant="info" title="Nenhuma diferença para aplicar">
                Escolha outra configuração ou volte sem aplicar.
              </Alert>
            )}
            <FormField
              label="Justificativa da revisão"
              required
              error={aplicacao.formState.errors.observacao?.message}
            >
              <Textarea
                id="revisao-observacao"
                disabled={ocupado}
                maxLength={1000}
                {...aplicacao.register('observacao')}
              />
            </FormField>
            <FormActions sticky={false}>
              <Button variant="secondary" disabled={ocupado} onClick={onFechar}>
                Voltar sem aplicar
              </Button>
              <Button
                variant="secondary"
                disabled={ocupado}
                onClick={() => {
                  setComparacao(null);
                  setSelecao(null);
                  setTratamento('');
                  aplicacao.reset();
                  setErro(null);
                }}
              >
                Comparar novamente
              </Button>
              <Button
                type="submit"
                loading={aplicacao.formState.isSubmitting}
                disabled={Boolean(tratamento) && !diferente}
              >
                Aplicar condições comparadas
              </Button>
            </FormActions>
          </form>
        </>
      )}
    </div>
  );
}

function ResumoDiferencas({ comparacao }: { comparacao: ComparacaoCondicoes }) {
  const { atual, modulo } = comparacao;
  const atuais = new Set(atual.coberturas.map((c) => c.publicId));
  const novas = new Set(modulo.coberturas.map((c) => c.publicId));
  const incluidas = modulo.coberturas.filter((c) => !atuais.has(c.publicId));
  const removidas = atual.coberturas.filter((c) => !novas.has(c.publicId));
  return (
    <Alert variant="info" title="Diferenças da configuração escolhida">
      <p>
        {atual.moduloPublicId !== modulo.moduloPublicId
          ? 'O vínculo de Módulo será alterado.'
          : 'O vínculo de Módulo será mantido.'}
      </p>
      <p>
        {atual.subgrupoPublicId !== modulo.subgrupoPublicId
          ? 'O vínculo de Subgrupo será alterado.'
          : 'O vínculo de Subgrupo será mantido.'}
      </p>
      <p>
        {atual.plano?.publicId !== modulo.plano?.publicId
          ? 'O vínculo de Plano será alterado.'
          : 'O vínculo de Plano será mantido.'}
      </p>
      <p>
        Coberturas incluídas:{' '}
        {incluidas.map((c) => c.nome ?? 'Sem nome').join(', ') || 'Nenhuma'}.
      </p>
      <p>
        Coberturas removidas das condições ativas:{' '}
        {removidas.map((c) => c.nome ?? 'Sem nome').join(', ') || 'Nenhuma'}.
      </p>
      <p>
        Compare os atributos e prêmios abaixo. Condições anteriores ficarão
        preservadas como inativas; coberturas novas receberão os prêmios atuais.
      </p>
    </Alert>
  );
}

function ResumoCondicoes({
  titulo,
  dados,
}: {
  titulo: string;
  dados: CondicoesRegistradas;
}) {
  const simNao = (valor: boolean | null) =>
    valor === null ? 'Não informado' : valor ? 'Sim' : 'Não';
  return (
    <DetailsSection title={titulo}>
      <DescriptionList columns={2} density="compact">
        <DescriptionItem label="Módulo" value={dados.moduloNomeRegistrado} />
        <DescriptionItem
          label="Subgrupo"
          value={dados.subgrupoNomeRegistrado ?? 'Sem Subgrupo'}
        />
        <DescriptionItem label="Plano" value={dados.plano?.nome} />
        <DescriptionItem label="Ramo" value={dados.plano?.ramo} />
        <DescriptionItem
          label="Plano paga"
          value={simNao(dados.plano?.paga ?? null)}
        />
        <DescriptionItem
          label="Plano reajusta"
          value={simNao(dados.plano?.reajuste ?? null)}
        />
      </DescriptionList>
      {dados.coberturas.length === 0 && <p>Nenhuma cobertura registrada.</p>}
      {dados.coberturas.map((c, index) => (
        <DescriptionList
          key={c.publicId ?? index}
          columns={2}
          density="compact"
        >
          <DescriptionItem label="Cobertura" value={c.nome} />
          <DescriptionItem label="Nome reduzido" value={c.nomeReduzido} />
          <DescriptionItem label="Básica" value={c.basica} />
          <DescriptionItem label="Reajusta" value={simNao(c.reajuste)} />
          <DescriptionItem
            label="Prêmio titular"
            value={formatarPremio(c.premioTitular)}
          />
          <DescriptionItem
            label="Prêmio cônjuge"
            value={formatarPremio(c.premioConjuge)}
          />
          <DescriptionItem
            label="Prêmio ajustado"
            value={simNao(c.premioAjustado)}
          />
          {c.premioAjustado && (
            <>
              <DescriptionItem
                label="Prêmio titular original"
                value={formatarPremio(c.premioTitularInicial)}
              />
              <DescriptionItem
                label="Prêmio cônjuge original"
                value={formatarPremio(c.premioConjugeInicial)}
              />
              <DescriptionItem
                label="Justificativa do ajuste"
                value={c.justificativaAjustePremio}
              />
            </>
          )}
        </DescriptionList>
      ))}
    </DetailsSection>
  );
}
