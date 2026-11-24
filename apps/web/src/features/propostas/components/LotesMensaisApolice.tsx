import { useCallback, useEffect, useRef, useState } from 'react';
import {
  Alert,
  Button,
  DataTable,
  DetailsSection,
  FormActions,
  FormField,
  Input,
  Pagination,
  ResultsSummary,
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import {
  formatarDataOuVazio,
  formatarMoedaDecimal,
} from '../../../shared/utils/formatters';
import {
  baixarParcela,
  conferirLote,
  fecharLote,
  gerarLote,
} from '../api/financeiro.api';
import type { ParcelaLote, VidaLote } from '../types/financeiro.types';
const situacoes = {
  INCLUIR: 'Inclusão',
  EXISTENTE: 'Já gerada',
  EXCLUIDA: 'Exclusão',
  PENDENTE: 'Pendência',
};
const colunas: Column<VidaLote>[] = [
  {
    key: 'vida',
    label: 'Vida / matrícula',
    render: (v) =>
      `${v.clienteNome ?? 'Nome indisponível'} / ${v.matricula ?? 'Sem matrícula'}`,
  },
  {
    key: 'proposta',
    label: 'Proposta / versão',
    render: (v) => `${v.numero ?? v.propostaPublicId} / ${v.versao}`,
  },
  {
    key: 'situacao',
    label: 'Situação no lote',
    render: (v) => situacoes[v.situacao],
  },
  {
    key: 'mensalidade',
    label: 'Nova mensalidade',
    render: (v) => formatarMoedaDecimal(v.novaMensalidade),
  },
  {
    key: 'saldo',
    label: 'Saldo anterior',
    render: (v) => formatarMoedaDecimal(v.saldoAnterior),
  },
  {
    key: 'total',
    label: 'Total acumulado',
    render: (v) => formatarMoedaDecimal(v.totalAcumulado),
  },
];
export function LotesMensaisApolice({
  publicId,
  podeAlterar,
  onBusy,
}: {
  publicId: string;
  podeAlterar: boolean;
  onBusy: (busy: boolean) => void;
}) {
  const [competencia, setCompetencia] = useState('');
  const [pedido, setPedido] = useState<number | null>(null);
  const [pagina, setPagina] = useState(1);
  const [ocupado, setOcupado] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [sucesso, setSucesso] = useState<string | null>(null);
  const enviando = useRef(false);
  const load = useCallback(
    (signal: AbortSignal) => conferirLote(publicId, pedido!, signal),
    [publicId, pedido],
  );
  const consulta = useConsulta(
    pedido ? load : null,
    'Não foi possível conferir a competência.',
  );
  useEffect(() => {
    onBusy(ocupado);
    return () => onBusy(false);
  }, [ocupado, onBusy]);
  async function executar(acao: () => Promise<unknown>, mensagem: string) {
    if (enviando.current) return;
    enviando.current = true;
    setOcupado(true);
    setErro(null);
    setSucesso(null);
    try {
      await acao();
      setSucesso(mensagem);
      await consulta.refetch();
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível concluir a operação.',
      );
      setPedido(null);
    } finally {
      enviando.current = false;
      setOcupado(false);
    }
  }
  const dados = consulta.data;
  const indisponivel = ocupado || consulta.isLoading || !!consulta.error;
  return (
    <DetailsSection title="Parcelas e lote mensal">
      <p>
        Confira uma Apólice e competência por vez. Gere a nova mensalidade,
        registre as baixas e feche o mês para liberar o seguinte. Parcelas
        antigas permanecem abertas e compõem o total acumulado por Vida.
      </p>
      <form
        className="flex flex-col gap-3"
        onSubmit={(event) => {
          event.preventDefault();
          if (ocupado || consulta.isLoading) return;
          setErro(null);
          setSucesso(null);
          setPagina(1);
          const valor = Number(competencia.replace('-', ''));
          if (pedido === valor) void consulta.refetch();
          else setPedido(valor);
        }}
      >
        <FormField label="Competência do lote" required>
          <Input
            id="lote-competencia"
            type="month"
            required
            value={competencia}
            disabled={ocupado}
            onChange={(e) => {
              setCompetencia(e.target.value);
              setPedido(null);
              setSucesso(null);
            }}
          />
        </FormField>
        <div>
          <Button
            type="submit"
            variant="secondary"
            loading={consulta.isLoading}
            disabled={ocupado || !competencia}
          >
            Conferir competência
          </Button>
        </div>
      </form>
      {erro && (
        <Alert variant="error" title="Operação não concluída">
          {erro} Confira a competência novamente.
        </Alert>
      )}
      {sucesso && (
        <Alert variant="success" title="Operação concluída">
          {sucesso}
        </Alert>
      )}
      {consulta.error && (
        <Alert variant="error" title="Falha ao consultar o lote">
          {consulta.error.message}
          <Button
            variant="secondary"
            disabled={ocupado}
            onClick={() => void consulta.refetch()}
          >
            Tentar novamente
          </Button>
        </Alert>
      )}
      {dados && !consulta.error && (
        <>
          <p>
            Competência {dados.competencia}.{' '}
            {dados.fechadaEm
              ? 'Fechada; baixas de parcelas abertas continuam permitidas.'
              : dados.geradaEm
                ? 'Lote gerado, aguardando fechamento.'
                : 'Conferência anterior à geração.'}
          </p>
          {!!dados.impedimentos.length && (
            <Alert variant="warning" title="Pendências para gerar">
              {dados.impedimentos.map((motivo) => (
                <p key={motivo}>{motivo}</p>
              ))}
            </Alert>
          )}
          <ResultsSummary
            currentPage={pagina}
            pageSize={20}
            totalItems={dados.vidas.length}
          />
          <DataTable
            data={dados.vidas.slice((pagina - 1) * 20, pagina * 20)}
            columns={colunas}
            keyExtractor={(v) => v.vidaPublicId ?? v.propostaPublicId}
            isLoading={consulta.isLoading}
            aria-label="Conferência do lote mensal"
            emptyTitle="Nenhuma Vida nesta Apólice"
            emptyDescription="Nenhuma mensalidade será incluída neste lote."
            renderExpandedRow={(vida) => (
              <div className="flex flex-col gap-3">
                {vida.motivo && <p>{vida.motivo}</p>}
                <p>
                  Vida: {vida.vidaPublicId ?? 'Não informada'}. Os valores
                  antigos não são acrescentados à nova parcela.
                </p>
                {vida.parcelas.map((parcela) => (
                  <ParcelaBaixa
                    key={parcela.publicId}
                    parcela={parcela}
                    disabled={indisponivel}
                    podeAlterar={podeAlterar}
                    onBaixar={(data) =>
                      executar(
                        () =>
                          baixarParcela(
                            publicId,
                            parcela.publicId,
                            parcela.revisaoBaixa,
                            data,
                          ),
                        'Baixa integral registrada. Os saldos foram atualizados.',
                      )
                    }
                  />
                ))}
              </div>
            )}
          />
          {dados.vidas.length > 20 && (
            <Pagination
              currentPage={pagina}
              totalPages={Math.ceil(dados.vidas.length / 20)}
              onPageChange={setPagina}
              disabled={indisponivel}
            />
          )}
          {podeAlterar && (
            <FormActions sticky={false}>
              <Button
                disabled={indisponivel || !dados.podeGerar}
                onClick={() =>
                  void executar(
                    () =>
                      gerarLote(publicId, dados.competencia, dados.conferencia),
                    'Lote gerado. Foram criadas somente as novas mensalidades elegíveis.',
                  )
                }
              >
                Confirmar geração do lote
              </Button>
              <Button
                variant="secondary"
                disabled={indisponivel || !dados.podeFechar}
                onClick={() =>
                  void executar(
                    () =>
                      fecharLote(
                        publicId,
                        dados.competencia,
                        dados.conferencia,
                      ),
                    'Competência fechada. As parcelas não pagas permanecem em aberto.',
                  )
                }
              >
                Confirmar fechamento, preservando abertas
              </Button>
            </FormActions>
          )}
        </>
      )}
    </DetailsSection>
  );
}
function ParcelaBaixa({
  parcela,
  disabled,
  podeAlterar,
  onBaixar,
}: {
  parcela: ParcelaLote;
  disabled: boolean;
  podeAlterar: boolean;
  onBaixar: (data: string) => Promise<void>;
}) {
  const [data, setData] = useState('');
  return (
    <div className="flex flex-col gap-3 border border-borda rounded-md p-3">
      <p>
        Parcela {parcela.parcela} — competência {parcela.competencia} —
        vencimento {formatarDataOuVazio(parcela.vencimento)} — total{' '}
        {formatarMoedaDecimal(parcela.total)} — aberto{' '}
        {formatarMoedaDecimal(parcela.aberto)}.
      </p>
      {parcela.dataPagamento && (
        <p>
          Pagamento registrado em {formatarDataOuVazio(parcela.dataPagamento)}.
        </p>
      )}
      {podeAlterar && parcela.aberto !== '0.00' && (
        <form
          className="flex flex-col gap-3"
          onSubmit={(event) => {
            event.preventDefault();
            if (!disabled && data) void onBaixar(data);
          }}
        >
          <FormField
            label={`Data do pagamento da parcela ${parcela.parcela}`}
            required
          >
            <Input
              id={`baixa-${parcela.publicId}`}
              type="date"
              required
              disabled={disabled}
              value={data}
              onChange={(e) => setData(e.target.value)}
            />
          </FormField>
          <div>
            <Button
              type="submit"
              variant="secondary"
              disabled={disabled || !data}
            >
              Confirmar baixa integral da parcela {parcela.parcela}
            </Button>
          </div>
        </form>
      )}
    </div>
  );
}
