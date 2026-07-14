import React from 'react';
import { Table, TableBody, TableCell, TableHeader, TableRow, SortIcon, Button } from '../../../components/ui';
import { EyeIcon } from '../../../components/ui/Icons';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import { useNavigate, useLocation } from 'react-router-dom';
import type { ClienteListItem } from '../types/cliente.types';
import { ClienteStatusBadge } from './ClienteStatusBadge';
import './ClientesTable.css';

interface ClientesTableProps {
  clientes: ClienteListItem[];
  isLoading: boolean;
  sortBy?: string;
  direction?: 'asc' | 'desc';
  onSort: (column: string) => void;
}

const sortableColumns = [
  { key: 'nome', label: 'Nome do cliente' },
  { key: 'status', label: 'Status' },
  { key: 'data_cadastro', label: 'Data de cadastro' },
];

export const ClientesTable: React.FC<ClientesTableProps> = ({
  clientes,
  isLoading,
  sortBy,
  direction,
  onSort,
}) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleVerDetalhes = (id: number) => {
    navigate(createPath(ROUTES.CLIENTE_DETALHES, { id: String(id) }), {
      state: { fromListagem: true, search: location.search },
    });
  };

  const renderSortHeader = (column: { key: string; label: string }) => {
    const isActive = sortBy === column.key;

    return (
      <button
        type="button"
        className="clientes-table-sort"
        onClick={() => onSort(column.key)}
        disabled={isLoading}
        aria-sort={isActive ? (direction === 'desc' ? 'descending' : 'ascending') : 'none'}
      >
        <span>{column.label}</span>
        <SortIcon size={14} aria-hidden="true" />
      </button>
    );
  };

  return (
    <Table aria-label="Lista de clientes">
      <TableHeader>
        <TableRow>
          <TableCell header>{renderSortHeader(sortableColumns[0])}</TableCell>
          <TableCell header>CPF</TableCell>
          <TableCell header>{renderSortHeader(sortableColumns[1])}</TableCell>
          <TableCell header>{renderSortHeader(sortableColumns[2])}</TableCell>
          <TableCell header align="right">Ações</TableCell>
        </TableRow>
      </TableHeader>
      <TableBody>
        {clientes.map((cliente) => (
          <TableRow key={cliente.id}>
            <TableCell className="clientes-table-name">{cliente.nome}</TableCell>
            <TableCell className="clientes-table-muted">{cliente.cpfMascarado}</TableCell>
            <TableCell>
              <ClienteStatusBadge status={cliente.status} />
            </TableCell>
            <TableCell className="clientes-table-muted">
              {new Date(cliente.dataCadastroUtc).toLocaleDateString('pt-BR')}
            </TableCell>
            <TableCell align="right">
              <Button
                variant="outline"
                size="sm"
                icon={<EyeIcon />}
                onClick={() => handleVerDetalhes(cliente.id)}
                aria-label={`Ver detalhes de ${cliente.nome}`}
              >
                Detalhes
              </Button>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
};
