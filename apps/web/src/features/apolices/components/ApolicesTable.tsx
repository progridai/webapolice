import React from 'react';
import { DataTable, StatusBadge, RowActions, Button, EyeIcon } from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import type { ApoliceListItem } from '../types/apolice.types';

interface ApolicesTableProps {
  apolices: ApoliceListItem[];
  isLoading: boolean;
  hasActiveFilters?: boolean;
  onClearFilters?: () => void;
  onDetalhar?: (publicId: string) => void;
}

export const ApolicesTable: React.FC<ApolicesTableProps> = ({
  apolices,
  isLoading,
  hasActiveFilters,
  onClearFilters,
  onDetalhar,
}) => {

  const formatDate = (dateString: string | undefined | null) => {
    if (!dateString) return '—';
    try {
      const d = new Date(dateString);
      return new Intl.DateTimeFormat('pt-BR').format(d);
    } catch {
      return dateString;
    }
  };

  const columns: Column<ApoliceListItem>[] = [
    {
      key: 'identificacao',
      label: 'Identificação',
      render: (apolice) => (
        <span className="font-medium text-texto-principal">{apolice.numeroPrincipal || '—'}</span>
      ),
    },
    {
      key: 'estipulante',
      label: 'Estipulante',
      render: (apolice) => <span className="text-texto-secundario">{apolice.estipulanteNome}</span>,
    },
    {
      key: 'seguradora',
      label: 'Seguradora',
      render: (apolice) => <span className="text-texto-secundario">{apolice.seguradoraNome}</span>,
    },
    {
      key: 'ramos',
      label: 'Ramos',
      render: (apolice) => (
        <span className="text-texto-secundario font-mono text-sm">
          {apolice.resumoRamos || (apolice.quantidadeRamos > 0 ? `${apolice.quantidadeRamos} Ramos` : '—')}
        </span>
      ),
    },
    {
      key: 'vigencia',
      label: 'Vigência',
      render: (apolice) => (
        <span className="text-texto-secundario">
          {formatDate(apolice.dataInicioVigencia)} até {formatDate(apolice.dataFimVigencia)}
        </span>
      ),
    },
    {
      key: 'status',
      label: 'Status',
      render: (apolice) => <StatusBadge status={apolice.ativo ? 'ativo' : 'inativo'} label={apolice.status} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (apolice) => {
        return (
          <RowActions
            primaryAction={onDetalhar ? {
              label: 'Visualizar',
              icon: <EyeIcon />,
              onClick: () => onDetalhar(apolice.publicId),
            } : undefined}
            actions={[]}
            ariaLabel={`Ações para Apólice ${apolice.numeroPrincipal}`}
          />
        );
      },
    },
  ];

  return (
    <DataTable
      data={apolices}
      columns={columns}
      keyExtractor={(item) => item.publicId}
      isLoading={isLoading}
      aria-label="Lista de apólices"
      emptyTitle={hasActiveFilters ? 'Nenhuma apólice encontrada' : 'Nenhuma apólice cadastrada'}
      emptyDescription={
        hasActiveFilters
          ? 'Não encontramos nenhuma apólice com os filtros informados.'
          : 'Ainda não existem apólices cadastradas na plataforma.'
      }
      emptyAction={
        hasActiveFilters && onClearFilters ? (
          <Button onClick={onClearFilters}>
            Limpar filtros
          </Button>
        ) : undefined
      }
    />
  );
};
