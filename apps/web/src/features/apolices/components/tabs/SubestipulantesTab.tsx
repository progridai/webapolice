import React from 'react';
import { useApoliceSubestipulantes } from '../../hooks/useApoliceSubestipulantes';
import { DataTable, StatusBadge, Alert, Button, EmptyState } from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import type { ApoliceSubestipulanteResult } from '../../types/apolice.types';

interface SubestipulantesTabProps {
  publicId: string;
}

export const SubestipulantesTab: React.FC<SubestipulantesTabProps> = ({ publicId }) => {
  const { data, isLoading, error } = useApoliceSubestipulantes(publicId);

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar subestipulantes">
          {error.message}
        </Alert>
        <Button onClick={() => window.location.reload()} size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  const hasData = data && data.length > 0;

  if (!isLoading && !hasData) {
    return (
      <EmptyState
        title="Nenhum Subestipulante"
        description="Esta apólice não possui subestipulantes vinculados."
      />
    );
  }

  const columns: Column<ApoliceSubestipulanteResult>[] = [
    {
      key: 'subestipulante',
      label: 'Subestipulante',
      render: (item) => (
        <span className="font-medium text-texto-principal">
          Subestipulante {item.subestipulanteIdInternal}
        </span>
      ),
    },
    {
      key: 'vigencia',
      label: 'Vigência do Vínculo',
      render: (item) => (
        <span className="text-texto-secundario">
          {item.dataInicio ? new Date(item.dataInicio).toLocaleDateString('pt-BR') : '—'} 
          {' até '} 
          {item.dataFim ? new Date(item.dataFim).toLocaleDateString('pt-BR') : '—'}
        </span>
      ),
    },
    {
      key: 'modulos',
      label: 'Módulos',
      render: (item) => (
        <div className="flex flex-wrap gap-2">
          {item.modulos && item.modulos.length > 0 ? (
            item.modulos.map((mod) => (
              <span key={mod.moduloIdInternal} className="inline-flex items-center rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10">
                Módulo {mod.moduloIdInternal}
              </span>
            ))
          ) : (
            <span className="text-sm text-texto-secundario">Sem módulos</span>
          )}
        </div>
      ),
    },
    {
      key: 'status',
      label: 'Status',
      render: (item) => <StatusBadge status={item.ativo ? 'ativo' : 'inativo'} label={item.ativo ? 'Ativo' : 'Inativo'} />,
    },
  ];

  return (
    <div className="flex flex-col gap-4">
      <DataTable
        data={data || []}
        columns={columns}
        keyExtractor={(item) => item.subestipulanteIdInternal.toString()}
        isLoading={isLoading}
        aria-label="Lista de Subestipulantes da Apólice"
      />
    </div>
  );
};
