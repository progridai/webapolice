import React, { useState } from 'react';
import { useApoliceHistorico } from '../../hooks/useApoliceHistorico';
import { DataTable, Alert, Button, EmptyState, Pagination } from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import type { ApoliceHistoricoResult } from '../../types/apolice.types';

interface HistoricoTabProps {
  publicId: string;
}

type LinhaHistorico = ApoliceHistoricoResult & { chaveLinha: string };

export const HistoricoTab: React.FC<HistoricoTabProps> = ({ publicId }) => {
  const [page, setPage] = useState(1);
  const pageSize = 15;
  const { data, isLoading, error } = useApoliceHistorico(publicId, page, pageSize);

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar o histórico">
          {error.message}
        </Alert>
        <Button onClick={() => window.location.reload()} size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  const hasData = data && data.items && data.items.length > 0;

  if (!isLoading && !hasData) {
    return (
      <EmptyState
        title="Nenhum Histórico"
        description="Esta apólice ainda não possui eventos registrados."
      />
    );
  }

  // O contrato histórico não expõe ID. A chave é local à página, sem alterar a evidência recebida.
  const registros: LinhaHistorico[] = (data?.items || []).map((item, index) => ({
    ...item,
    chaveLinha: `${publicId}-${data?.page ?? page}-${index}`,
  }));
  const columns: Column<LinhaHistorico>[] = [
    {
      key: 'acao',
      label: 'Ação',
      render: (item) => (
        <span className="font-medium text-texto-principal">{item.acao}</span>
      ),
    },
    {
      key: 'descricao',
      label: 'Descrição',
      render: (item) => (
        <span className="text-texto-secundario">{item.descricao || '—'}</span>
      ),
    },
    {
      key: 'usuario',
      label: 'Usuário',
      render: (item) => (
        <span className="text-sm text-texto-terciario">{item.usuarioPublicId || 'Sistema'}</span>
      ),
    },
    {
      key: 'data',
      label: 'Data / Hora',
      render: (item) => (
        <span className="text-sm text-texto-secundario whitespace-nowrap">
          {new Date(item.dataAcao).toLocaleString('pt-BR')}
        </span>
      ),
    },
  ];

  return (
    <div className="flex flex-col gap-4">
      <DataTable
        data={registros}
        columns={columns}
        keyExtractor={(item) => item.chaveLinha}
        isLoading={isLoading}
        aria-label="Histórico da Apólice"
      />
      
      {data && Math.ceil(data.totalCount / data.pageSize) > 1 && (
        <div className="flex justify-center md:justify-end mt-4 pt-4 border-t border-borda">
          <Pagination
            currentPage={data.page}
            totalPages={Math.ceil(data.totalCount / data.pageSize)}
            onPageChange={setPage}
            disabled={isLoading}
          />
        </div>
      )}
    </div>
  );
};
