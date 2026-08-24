import React, { ReactNode, useState } from 'react';
import { Table, TableHeader, TableBody, TableRow, TableCell } from '../Table';
import { SortIcon, ChevronRightIcon } from '../Icons';
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
  renderExpandedRow?: (item: T) => ReactNode;
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
  renderExpandedRow,
  'aria-label': ariaLabel = 'Tabela de dados',
}: DataTableProps<T>) {
  const [expandedKeys, setExpandedKeys] = useState<Set<string | number>>(new Set());

  const toggleRow = (key: string | number) => {
    const next = new Set(expandedKeys);
    if (next.has(key)) next.delete(key);
    else next.add(key);
    setExpandedKeys(next);
  };
  
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
            {renderExpandedRow && <TableCell header align="center" style={{ width: '48px' }} />}
            {columns.map((col) => (
              <TableCell key={col.key} header align={col.align}>
                {renderSortHeader(col)}
              </TableCell>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.map((item) => {
            const key = keyExtractor(item);
            const isExpanded = expandedKeys.has(key);
            return (
              <React.Fragment key={key}>
                <TableRow className={isExpanded ? 'data-table-expanded-row' : ''}>
                  {renderExpandedRow && (
                    <TableCell align="center" style={{ width: '48px' }}>
                      <button 
                        type="button" 
                        onClick={() => toggleRow(key)}
                        className="p-1 rounded transition-colors"
                        style={{ backgroundColor: isExpanded ? 'var(--cor-fundo-aplicacao)' : 'transparent' }}
                        aria-expanded={isExpanded}
                      >
                        <ChevronRightIcon 
                          size={16} 
                          className="transition-transform duration-200" 
                          style={{ 
                            transform: isExpanded ? 'rotate(90deg)' : 'none',
                            color: 'var(--cor-texto-secundario)'
                          }}
                        />
                      </button>
                    </TableCell>
                  )}
                  {columns.map((col) => (
                    <TableCell key={col.key} align={col.align}>
                      {col.render ? col.render(item) : String((item as Record<string, unknown>)[col.key] ?? '')}
                    </TableCell>
                  ))}
                </TableRow>
                {isExpanded && renderExpandedRow && (
                  <TableRow className="data-table-expanded-content">
                    <TableCell colSpan={columns.length + 1} className="p-0 border-t-0">
                      <div className="p-4" style={{ borderLeft: '2px solid var(--cor-marca-principal)' }}>
                        {renderExpandedRow(item)}
                      </div>
                    </TableCell>
                  </TableRow>
                )}
              </React.Fragment>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}
