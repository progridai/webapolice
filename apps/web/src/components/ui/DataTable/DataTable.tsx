import React, { ReactNode } from 'react';
import { Table, TableHeader, TableBody, TableRow, TableCell } from '../Table';
import { SortIcon } from '../Icons';
import { Skeleton } from '../Skeleton';
import { EmptyState } from '../EmptyState';
import './DataTable.css';

export interface Column<T> {
  key: string;
  label: string;
  sortable?: boolean;
  align?: 'left' | 'center' | 'right';
  render?: (item: T) => ReactNode;
}

export interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  keyExtractor: (item: T) => string | number;
  isLoading?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyAction?: ReactNode;
  sortBy?: string;
  direction?: 'asc' | 'desc';
  onSort?: (columnKey: string) => void;
  'aria-label'?: string;
}

export function DataTable<T>({
  data,
  columns,
  keyExtractor,
  isLoading = false,
  emptyTitle = 'Nenhum registro encontrado',
  emptyDescription = '',
  emptyAction,
  sortBy,
  direction,
  onSort,
  'aria-label': ariaLabel = 'Tabela de dados',
}: DataTableProps<T>) {
  
  if (isLoading && data.length === 0) {
    return (
      <div className="data-table-loading" aria-busy="true" aria-live="polite">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="data-table-skeleton-row" />
        ))}
      </div>
    );
  }

  if (data.length === 0 && !isLoading) {
    return (
      <EmptyState
        title={emptyTitle}
        description={emptyDescription}
        action={emptyAction}
      />
    );
  }

  const renderSortHeader = (column: Column<T>) => {
    if (!column.sortable || !onSort) {
      return <span>{column.label}</span>;
    }

    const isActive = sortBy === column.key;
    const sortDir = isActive ? (direction === 'desc' ? 'descending' : 'ascending') : 'none';

    return (
      <button
        type="button"
        className="data-table-sort-button"
        onClick={() => onSort(column.key)}
        disabled={isLoading}
        aria-sort={sortDir as React.AriaAttributes['aria-sort']}
      >
        <span>{column.label}</span>
        <SortIcon size={14} aria-hidden="true" />
      </button>
    );
  };

  return (
    <div className="data-table-container">
      <Table aria-label={ariaLabel}>
        <TableHeader>
          <TableRow>
            {columns.map((col) => (
              <TableCell key={col.key} header align={col.align}>
                {renderSortHeader(col)}
              </TableCell>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.map((item) => (
            <TableRow key={keyExtractor(item)}>
              {columns.map((col) => (
                <TableCell key={col.key} align={col.align}>
                  {col.render ? col.render(item) : String((item as Record<string, unknown>)[col.key] ?? '')}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
