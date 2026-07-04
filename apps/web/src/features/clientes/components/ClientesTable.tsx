import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Table, Button } from '../../../components/ui';
import { EyeIcon } from '../../../components/ui/Icons';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import type { ClienteListItem } from '../types/cliente.types';
import { ClienteStatusBadge } from './ClienteStatusBadge';

interface ClientesTableProps {
  clientes: ClienteListItem[];
  isLoading: boolean;
  sortBy?: string;
  direction?: 'asc' | 'desc';
  onSort: (column: string) => void;
}

export const ClientesTable: React.FC<ClientesTableProps> = ({
  clientes,
  isLoading,
  sortBy,
  direction,
  onSort,
}) => {
  const navigate = useNavigate();

  const handleSort = (column: string) => {
    onSort(column);
  };

  const getSortDirection = (column: string) => {
    return sortBy === column ? direction : undefined;
  };

  const columns = [
    { 
      key: 'nome', 
      label: 'Nome do Cliente', 
      sortable: true,
      sortDirection: getSortDirection('nome')
    },
    { 
      key: 'cpf', 
      label: 'CPF', 
      sortable: false 
    },
    { 
      key: 'status', 
      label: 'Status', 
      sortable: true,
      sortDirection: getSortDirection('status')
    },
    { 
      key: 'dataCadastroUtc', 
      label: 'Data de Cadastro', 
      sortable: true,
      sortDirection: getSortDirection('dataCadastroUtc')
    },
    { 
      key: 'acoes', 
      label: 'Ações', 
      sortable: false,
      align: 'right' as const
    },
  ];

  return (
    <Table
      columns={columns}
      isLoading={isLoading}
      onSort={handleSort}
      aria-label="Lista de clientes"
    >
      {clientes.map((cliente) => (
        <tr key={cliente.id} className="hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
          <td className="px-6 py-4 whitespace-nowrap font-medium">{cliente.nome}</td>
          <td className="px-6 py-4 whitespace-nowrap text-gray-500 dark:text-gray-400">
            {cliente.cpfMascarado}
          </td>
          <td className="px-6 py-4 whitespace-nowrap">
            <ClienteStatusBadge status={cliente.status} />
          </td>
          <td className="px-6 py-4 whitespace-nowrap text-gray-500 dark:text-gray-400">
            {new Date(cliente.dataCadastroUtc).toLocaleDateString('pt-BR')}
          </td>
          <td className="px-6 py-4 whitespace-nowrap text-right">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => navigate(createPath(ROUTES.CLIENTE_DETALHES, { id: String(cliente.id) }))}
              aria-label={`Visualizar detalhes de ${cliente.nome}`}
              icon={<EyeIcon />}
            >
              Visualizar
            </Button>
          </td>
        </tr>
      ))}
    </Table>
  );
};
