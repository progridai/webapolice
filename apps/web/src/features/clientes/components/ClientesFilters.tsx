import React from 'react';
import { Select, Button, FilterBar, SearchField } from '../../../components/ui';
import type { ClientesQuery } from '../types/cliente.types';

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

  const hasActiveFilters = Boolean(
    filters.nome || filters.cpf || filters.status,
  );
  const searchValue = filters.nome || filters.cpf || '';

  return (
    <FilterBar>
      <div className="w-full min-w-0 md:flex-[1_1_280px] md:w-auto">
        <label htmlFor="busca-cliente" className="form-label block mb-1">
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

      <div className="w-full md:flex-[0_1_180px] md:min-w-[120px] md:w-auto">
        <label htmlFor="status-cliente" className="form-label block mb-1">
          Status
        </label>
        <Select
          id="status-cliente"
          value={filters.status || ''}
          onChange={(event) =>
            onFilterChange({
              status:
                event.target.value === '1'
                  ? 1
                  : event.target.value === '2'
                    ? 2
                    : '',
              page: 1, // Reset to first page on filter change
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

      <div className="flex items-end shrink-0">
        <Button
          variant="secondary"
          onClick={onClearFilters}
          disabled={!hasActiveFilters || isLoading}
          aria-label="Limpar filtros"
        >
          Limpar
        </Button>
      </div>
    </FilterBar>
  );
};
