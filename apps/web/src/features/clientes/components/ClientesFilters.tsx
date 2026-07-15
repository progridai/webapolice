import React from 'react';
import { Select, Button, FilterBar, SearchField } from '../../../components/ui';
import type { ClientesQuery, StatusClienteEnum } from '../types/cliente.types';
import './ClientesFilters.css';

interface ClientesFiltersProps {
  filters: ClientesQuery;
  onFilterChange: (filters: Partial<ClientesQuery>) => void;
  onClearFilters: () => void;
  isLoading?: boolean;
}

export const ClientesFilters: React.FC<ClientesFiltersProps> = ({
  filters,
  onFilterChange,
  onClearFilters,
  isLoading,
}) => {
  const handleSearchChange = (value: string) => {
    const trimmed = value.trim();
    const isCpf = /^[\d.-]+$/.test(trimmed) && trimmed.length > 0;
    const nextNome = isCpf ? '' : trimmed;
    const nextCpf = isCpf ? trimmed : '';

    if (nextNome === filters.nome && nextCpf === filters.cpf) return;

    onFilterChange({
      nome: nextNome,
      cpf: nextCpf,
      page: 1, // Reset to first page on search
    });
  };

  const hasActiveFilters = Boolean(filters.nome || filters.cpf || filters.status);
  const searchValue = filters.nome || filters.cpf || '';

  return (
    <FilterBar>
      <div className="clientes-filter-search">
        <label htmlFor="busca-cliente" className="clientes-filter-label">
          Buscar cliente
        </label>
        <SearchField
          id="busca-cliente"
          placeholder="Nome ou CPF"
          value={searchValue}
          onChange={handleSearchChange}
          disabled={isLoading}
        />
      </div>

      <div className="clientes-filter-status">
        <label htmlFor="status-cliente" className="clientes-filter-label">
          Status
        </label>
        <Select
          id="status-cliente"
          value={filters.status || ''}
          onChange={(event) =>
            onFilterChange({ 
              status: event.target.value as unknown as StatusClienteEnum | '',
              page: 1 // Reset to first page on filter change
            })
          }
          disabled={isLoading}
          options={[
            { label: 'Todos', value: '' },
            { label: 'Ativo', value: '1' },
            { label: 'Inativo', value: '2' },
          ]}
        />
      </div>

      <div className="clientes-filter-actions">
        <Button
          variant="secondary"
          onClick={onClearFilters}
          disabled={!hasActiveFilters || isLoading}
          aria-label="Limpar filtros"
          className="clientes-clear-button"
        >
          Limpar
        </Button>
      </div>
    </FilterBar>
  );
};
