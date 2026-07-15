import React from 'react';
import { DataTable, StatusBadge, RowActions, EyeIcon, EditIcon, Button } from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import { useNavigate, useLocation } from 'react-router-dom';
import type { ClienteListItem } from '../types/cliente.types';
import './ClientesTable.css';

interface ClientesTableProps {
  clientes: ClienteListItem[];
  isLoading: boolean;
  sortBy?: string;
  direction?: 'asc' | 'desc';
  onSort: (column: string) => void;
  hasActiveFilters?: boolean;
  onClearFilters?: () => void;
}

export const ClientesTable: React.FC<ClientesTableProps> = ({
  clientes,
  isLoading,
  sortBy,
  direction,
  onSort,
  hasActiveFilters,
  onClearFilters,
}) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleVerDetalhes = (id: number) => {
    navigate(createPath(ROUTES.CLIENTE_DETALHES, { id: String(id) }), {
      state: { fromListagem: true, search: location.search },
    });
  };

  const handleEditar = (id: number) => {
    navigate(`/clientes/${id}/editar`);
  };

  const columns: Column<ClienteListItem>[] = [
    {
      key: 'nome',
      label: 'Nome do cliente',
      sortable: true,
      render: (cliente) => <span className="clientes-table-name">{cliente.nome}</span>,
    },
    {
      key: 'cpfMascarado',
      label: 'CPF',
      render: (cliente) => <span className="clientes-table-muted">{cliente.cpfMascarado}</span>,
    },
    {
      key: 'status',
      label: 'Status',
      sortable: true,
      render: (cliente) => <StatusBadge status={cliente.status} />,
    },
    {
      key: 'data_cadastro',
      label: 'Data de cadastro',
      sortable: true,
      render: (cliente) => (
        <span className="clientes-table-muted">
          {new Date(cliente.dataCadastroUtc).toLocaleDateString('pt-BR')}
        </span>
      ),
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (cliente) => (
        <RowActions
          primaryAction={{
            label: 'Detalhes',
            icon: <EyeIcon />,
            onClick: () => handleVerDetalhes(cliente.id),
          }}
          actions={[
            {
              label: 'Editar',
              icon: <EditIcon />,
              onClick: () => handleEditar(cliente.id),
            },
          ]}
          ariaLabel={`Ações para ${cliente.nome}`}
        />
      ),
    },
  ];

  return (
    <DataTable
      data={clientes}
      columns={columns}
      keyExtractor={(item) => item.id}
      isLoading={isLoading}
      sortBy={sortBy}
      direction={direction}
      onSort={onSort}
      aria-label="Lista de clientes"
      emptyTitle={hasActiveFilters ? 'Nenhum cliente encontrado' : 'Nenhum cliente cadastrado'}
      emptyDescription={
        hasActiveFilters
          ? 'Não encontramos nenhum cliente com os filtros informados.'
          : 'Ainda não existem clientes cadastrados na plataforma.'
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
