import React, { useState } from 'react';
import { useApoliceVidas } from '../../hooks/useApoliceVidas';
import { DataTable, StatusBadge, Pagination, Alert, Button, EmptyState } from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import type { ApoliceVidaListItem } from '../../types/apolice.types';

interface VidasTabProps {
  publicId: string;
}

export const VidasTab: React.FC<VidasTabProps> = ({ publicId }) => {
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const { data, isLoading, error, retry } = useApoliceVidas(publicId, page, pageSize);

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar as vidas">
          {error.message}
        </Alert>
        <Button onClick={retry} size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  const hasData = data && data.items && data.items.length > 0;

  if (!isLoading && !hasData) {
    return (
      <EmptyState
        title="Nenhuma vida encontrada"
        description="Esta apólice não possui beneficiários ou vidas cadastradas."
      />
    );
  }

  const columns: Column<ApoliceVidaListItem>[] = [
    {
      key: 'cliente',
      label: 'Cliente / Vida',
      render: (vida) => (
        <span className="font-medium text-texto-principal">{vida.clienteNome}</span>
      ),
    },
    {
      key: 'contexto',
      label: 'Contexto / Subestipulante',
      render: (vida) => (
        <div className="flex flex-col">
          {vida.subestipulanteNome && <span className="text-sm text-texto-secundario">{vida.subestipulanteNome}</span>}
          {vida.moduloNome && <span className="text-xs text-texto-terciario">Módulo: {vida.moduloNome}</span>}
          {!vida.subestipulanteNome && !vida.moduloNome && <span className="text-sm text-texto-secundario">Direta (Apólice)</span>}
        </div>
      ),
    },
    {
      key: 'vigencia',
      label: 'Vigência',
      render: (vida) => (
        <span className="text-texto-secundario">
          {vida.dataInicioVigencia ? new Date(vida.dataInicioVigencia).toLocaleDateString('pt-BR') : '—'} 
          {' até '} 
          {vida.dataFimVigencia ? new Date(vida.dataFimVigencia).toLocaleDateString('pt-BR') : '—'}
        </span>
      ),
    },
    {
      key: 'status',
      label: 'Status',
      render: (vida) => <StatusBadge status={vida.ativo ? 'ativo' : 'inativo'} label={vida.status} />,
    },
  ];

  return (
    <div className="flex flex-col gap-4">
      <DataTable
        data={data?.items || []}
        columns={columns}
        keyExtractor={(item) => item.publicId}
        isLoading={isLoading}
        aria-label="Lista de Vidas da Apólice"
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
