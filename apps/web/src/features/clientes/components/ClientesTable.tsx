import React from 'react';
import { Table, TableBody, TableCell, TableHeader, TableRow, SortIcon } from '../../../components/ui';
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
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
};
