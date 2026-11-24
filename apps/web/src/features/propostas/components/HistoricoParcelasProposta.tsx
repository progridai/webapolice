import { useCallback, useState } from 'react';
import {
  Alert,
  Button,
  DataTable,
  DescriptionItem,
  DescriptionList,
  DetailsSection,
  Pagination,
  ResultsSummary,
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import {
  formatarDataOuVazio,
  formatarMoedaDecimal,
} from '../../../shared/utils/formatters';
import { listarParcelas } from '../api/parcelas.api';
import type { ParcelaHistorico } from '../types/parcelaHistorico.types';

const moeda = (valor: string | null) =>
  valor === null ? 'Não informado' : formatarMoedaDecimal(valor);
const columns: Column<ParcelaHistorico>[] = [
  {
    key: 'parcela',
    label: 'Parcela',
    render: (p) => p.parcela ?? 'Não informada',
  },
  {
    key: 'proposta',
    label: 'Proposta / versão',
    render: (p) => `${p.propostaNumero ?? 'Sem número'} / ${p.versao}`,
  },
  {
    key: 'competencia',
    label: 'Competência registrada',
    render: (p) => p.competenciaInt ?? 'Não informada',
  },
  {
    key: 'vencimento',
    label: 'Vencimento',
    render: (p) => formatarDataOuVazio(p.dataVencimento),
  },
  { key: 'liquido', label: 'Líquido', render: (p) => moeda(p.premioLiquido) },
  { key: 'iof', label: 'IOF', render: (p) => moeda(p.iof) },
  {
    key: 'total',
    label: 'Total registrado',
    render: (p) => moeda(p.premioTotal),
  },
  {
    key: 'origem',
    label: 'Origem',
    render: (p) => (p.legado ? 'Legado' : 'Nativo'),
  },
];

export function HistoricoParcelasProposta({
  publicId,
  disabled,
}: {
  publicId: string;
  disabled: boolean;
}) {
  const [aberto, setAberto] = useState(false);
  return (
    <DetailsSection title="Histórico de parcelas e movimentos">
      <p className="text-sm text-texto-secundario">
        Consulte os registros das versões visíveis da mesma Vida. Os valores
        históricos são preservados. Esta consulta não gera parcelas nem títulos.
      </p>
      <Button
        variant="secondary"
        disabled={disabled}
        onClick={() => setAberto(!aberto)}
      >
        {aberto
          ? 'Fechar histórico de parcelas'
          : 'Consultar histórico de parcelas'}
      </Button>
      {aberto && !disabled && (
        <ConsultaParcelas key={publicId} publicId={publicId} />
      )}
    </DetailsSection>
  );
}

function ConsultaParcelas({ publicId }: { publicId: string }) {
  const [pagina, setPagina] = useState(1);
  const load = useCallback(
    (signal: AbortSignal) => listarParcelas(publicId, pagina, signal),
    [publicId, pagina],
  );
  const { data, error, isLoading, refetch } = useConsulta(
    load,
    'Não foi possível carregar o histórico de parcelas.',
  );
  return (
    <div className="flex flex-col gap-3">
      <Button
        variant="text"
        disabled={isLoading}
        onClick={() => void refetch()}
      >
        Recarregar histórico de parcelas
      </Button>
      {error ? (
        <Alert variant="error" title="Falha ao consultar parcelas">
          {error.message}
          <Button variant="secondary" onClick={() => void refetch()}>
            Tentar novamente
          </Button>
        </Alert>
      ) : (
        <>
          {data && !isLoading && (
            <ResultsSummary
              currentPage={data.page}
              pageSize={data.pageSize}
              totalItems={data.totalCount}
            />
          )}
          <DataTable
            data={data?.items ?? []}
            columns={columns}
            keyExtractor={(p) => p.publicId ?? `legado-posicao-${p.posicao}`}
            isLoading={isLoading}
            aria-label="Histórico de parcelas de todas as versões"
            emptyTitle="Nenhum movimento registrado"
            emptyDescription="Ainda não há parcelas ou movimentos disponíveis neste contexto."
            renderExpandedRow={(p) => (
              <DescriptionList>
                <DescriptionItem
                  label="Classificação registrada"
                  value={p.classificacao}
                />
                <DescriptionItem label="Ano registrado" value={p.ano} />
                <DescriptionItem label="Mês registrado" value={p.mes} />
                <DescriptionItem
                  label="Lançamento"
                  value={formatarDataOuVazio(p.dataLancamento)}
                />
                <DescriptionItem
                  label="Pagamento registrado"
                  value={formatarDataOuVazio(p.dataPagamento)}
                />
                <DescriptionItem
                  label="Valor pago registrado"
                  value={moeda(p.valorPago)}
                />
                <DescriptionItem
                  label="Observação registrada"
                  value={p.observacao}
                />
              </DescriptionList>
            )}
          />
          {data && data.totalCount > data.pageSize && (
            <Pagination
              currentPage={data.page}
              totalPages={Math.ceil(data.totalCount / data.pageSize)}
              onPageChange={setPagina}
              disabled={isLoading}
            />
          )}
        </>
      )}
    </div>
  );
}
