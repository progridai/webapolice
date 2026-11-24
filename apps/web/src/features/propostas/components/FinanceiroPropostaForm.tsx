import { useCallback, useRef, useState } from 'react';
import {
  Alert,
  Button,
  DescriptionItem,
  DescriptionList,
  FormActions,
  FormField,
  Input,
  Select,
} from '../../../components/ui';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import {
  formatarDataOuVazio,
  formatarMoedaDecimal,
} from '../../../shared/utils/formatters';
import { obterRamosSimulacao } from '../api/simulacaoMensal.api';
import { conferirFinanceiro, obterFinanceiro } from '../api/financeiro.api';
import type {
  ConferenciaFinanceiraDados,
  FinanceiroConfirmacaoDados,
} from '../types/financeiro.types';
import type { PropostaDetalhe } from '../types/proposta.types';

export function FinanceiroPropostaForm({
  proposta,
  onCancelar,
  onSalvar,
}: {
  proposta: PropostaDetalhe;
  onCancelar: () => void;
  onSalvar: (
    financeiro: FinanceiroConfirmacaoDados,
    inicio: string,
  ) => Promise<void>;
}) {
  const load = useCallback(
    async (signal: AbortSignal) => {
      const [ramos, estado] = await Promise.all([
        obterRamosSimulacao(proposta.publicId, signal),
        obterFinanceiro(proposta.publicId, signal),
      ]);
      return { ramos, estado };
    },
    [proposta.publicId],
  );
  const consulta = useConsulta(
    load,
    'Não foi possível carregar os dados financeiros.',
  );
  return (
    <div className="flex flex-col gap-3">
      {consulta.isLoading && <p role="status">Carregando dados financeiros…</p>}
      {consulta.error && (
        <Alert variant="error" title="Falha ao carregar financeiro">
          {consulta.error.message}
          <Button
            type="button"
            variant="secondary"
            onClick={() => void consulta.refetch()}
          >
            Tentar novamente
          </Button>
        </Alert>
      )}
      {consulta.data && !consulta.isLoading && !consulta.error && (
        <ConferenciaForm
          key={`${proposta.publicId}:${proposta.revisaoPreenchimento}`}
          proposta={proposta}
          opcoes={consulta.data}
          onCancelar={onCancelar}
          onSalvar={onSalvar}
        />
      )}
      {!consulta.data && (
        <Button type="button" variant="secondary" onClick={onCancelar}>
          Cancelar conferência financeira
        </Button>
      )}
    </div>
  );
}
function ConferenciaForm({
  proposta,
  opcoes,
  onCancelar,
  onSalvar,
}: {
  proposta: PropostaDetalhe;
  opcoes: {
    ramos: Awaited<ReturnType<typeof obterRamosSimulacao>>;
    estado: Awaited<ReturnType<typeof obterFinanceiro>>;
  };
  onCancelar: () => void;
  onSalvar: (
    financeiro: FinanceiroConfirmacaoDados,
    inicio: string,
  ) => Promise<void>;
}) {
  const [inicio, setInicio] = useState(proposta.dataInicioVigencia ?? '');
  const [competencia, setCompetencia] = useState(
    opcoes.estado.competenciaInicial
      ? `${String(opcoes.estado.competenciaInicial).slice(0, -2).padStart(4, '0')}-${String(opcoes.estado.competenciaInicial).slice(-2)}`
      : '',
  );
  const [vencimento, setVencimento] = useState(
    opcoes.estado.vencimentoInicial ?? '',
  );
  const [ramo, setRamo] = useState(opcoes.estado.ramoPublicId ?? '');
  const [selecao, setSelecao] = useState<ConferenciaFinanceiraDados | null>(
    null,
  );
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const enviando = useRef(false);
  const load = useCallback(
    (signal: AbortSignal) =>
      conferirFinanceiro(proposta.publicId, selecao!, signal),
    [proposta.publicId, selecao],
  );
  const consulta = useConsulta(
    selecao ? load : null,
    'Não foi possível conferir os valores.',
  );
  async function salvar() {
    if (
      enviando.current ||
      !consulta.data ||
      !selecao ||
      consulta.isLoading ||
      consulta.error
    )
      return;
    enviando.current = true;
    setSalvando(true);
    setErro(null);
    try {
      await onSalvar(
        {
          competenciaInicial: selecao.competenciaInicial,
          vencimentoInicial: selecao.vencimentoInicial,
          ramoPublicId: selecao.ramoPublicId,
          conferencia: consulta.data.conferencia,
        },
        selecao.dataInicioVigencia,
      );
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível salvar o financeiro.',
      );
      setSelecao(null);
    } finally {
      enviando.current = false;
      setSalvando(false);
    }
  }
  const valores = consulta.data?.valores;
  return (
    <div className="flex flex-col gap-3">
      <p>
        A primeira parcela será gerada na efetivação da vigência pelo valor
        mensal integral. As seguintes serão geradas em lote manual. Os valores e
        o IOF conferidos ficarão congelados, sem emitir título nem enviar
        cobrança.
      </p>
      {opcoes.estado.totalMensal && (
        <Alert
          variant={opcoes.estado.conferenciaAtual ? 'info' : 'warning'}
          title="Financeiro registrado"
        >
          Total mensal {formatarMoedaDecimal(opcoes.estado.totalMensal)}.{' '}
          {opcoes.estado.conferenciaAtual
            ? 'Condições conferidas.'
            : 'As condições precisam de nova conferência.'}
        </Alert>
      )}
      {opcoes.estado.competenciaReferencia && (
        <p>
          Referência original de vencimentos: competência{' '}
          {opcoes.estado.competenciaReferencia}, vencimento{' '}
          {formatarDataOuVazio(opcoes.estado.vencimentoReferencia)}. Preserve o
          dia e o deslocamento em meses.
        </p>
      )}
      {!opcoes.ramos.length && (
        <Alert variant="info" title="Nenhum Ramo disponível">
          Vincule um Ramo ativo à mesma Apólice.
        </Alert>
      )}
      <form
        className="flex flex-col gap-3"
        onSubmit={(event) => {
          event.preventDefault();
          if (salvando || consulta.isLoading) return;
          const dados = {
            revisaoEsperada: proposta.revisaoPreenchimento,
            dataInicioVigencia: inicio,
            competenciaInicial: Number(competencia.replace('-', '')),
            vencimentoInicial: vencimento,
            ramoPublicId: ramo,
          };
          setErro(null);
          if (selecao) void consulta.refetch();
          else setSelecao(dados);
        }}
      >
        <FormField label="Data de início da vigência" required>
          <Input
            id="financeiro-inicio"
            type="date"
            required
            value={inicio}
            disabled={salvando || proposta.confirmadaEm !== null}
            onChange={(e) => {
              setInicio(e.target.value);
              setSelecao(null);
            }}
          />
        </FormField>
        <FormField label="Competência inicial" required>
          <Input
            id="financeiro-competencia"
            type="month"
            required
            value={competencia}
            disabled={salvando}
            onChange={(e) => {
              setCompetencia(e.target.value);
              setSelecao(null);
            }}
          />
        </FormField>
        <FormField label="Primeiro vencimento" required>
          <Input
            id="financeiro-vencimento"
            type="date"
            required
            value={vencimento}
            disabled={salvando}
            onChange={(e) => {
              setVencimento(e.target.value);
              setSelecao(null);
            }}
          />
        </FormField>
        <FormField label="Ramo para o IOF" required>
          <Select
            id="financeiro-ramo"
            required
            placeholder="Selecione explicitamente um Ramo"
            value={ramo}
            disabled={salvando}
            onChange={(e) => {
              setRamo(e.target.value);
              setSelecao(null);
            }}
          >
            {opcoes.ramos.map((r) => (
              <option key={r.publicId} value={r.publicId}>
                {r.codigo} — {r.nome} — IOF {r.iofPercentual ?? 'não informado'}
                %
              </option>
            ))}
          </Select>
        </FormField>
        <Button
          type="submit"
          variant="secondary"
          loading={consulta.isLoading}
          disabled={salvando || !inicio || !competencia || !vencimento || !ramo}
        >
          Conferir primeira parcela
        </Button>
      </form>
      {consulta.error && (
        <Alert variant="error" title="Falha na conferência">
          {consulta.error.message}
          <Button
            type="button"
            variant="secondary"
            disabled={salvando}
            onClick={() => void consulta.refetch()}
          >
            Tentar novamente
          </Button>
        </Alert>
      )}
      {valores && !consulta.isLoading && !consulta.error && (
        <DescriptionList>
          <DescriptionItem
            label="Líquido mensal"
            value={formatarMoedaDecimal(valores.liquidoTotal)}
          />
          <DescriptionItem
            label="Alíquota de IOF"
            value={`${valores.ramo.iofPercentual}%`}
          />
          <DescriptionItem
            label="IOF mensal"
            value={formatarMoedaDecimal(valores.iof)}
          />
          <DescriptionItem
            label="Total mensal da parcela"
            value={formatarMoedaDecimal(valores.totalMensal)}
          />
        </DescriptionList>
      )}
      {erro && (
        <Alert variant="error" title="Falha ao salvar">
          {erro}
        </Alert>
      )}
      <FormActions sticky={false}>
        <Button
          type="button"
          variant="secondary"
          disabled={salvando}
          onClick={onCancelar}
        >
          Cancelar conferência financeira
        </Button>
        <Button
          type="button"
          loading={salvando}
          disabled={!consulta.data || consulta.isLoading || !!consulta.error}
          onClick={() => void salvar()}
        >
          {proposta.confirmadaEm
            ? 'Salvar financeiro conferido'
            : 'Confirmar proposta e valores'}
        </Button>
      </FormActions>
    </div>
  );
}
