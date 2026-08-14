import React from 'react';
import { Select, Button, FilterBar, SearchField } from '../../../components/ui';
import type { EstipulantesQuery, StatusEstipulanteEnum } from '../types/estipulante.types';

interface EstipulantesFiltersProps {
  filters: EstipulantesQuery;
  onFilterChange: (filters: Partial<EstipulantesQuery>) => void;
  onClearFilters: () => void;
  isLoading?: boolean;
}

export const EstipulantesFilters: React.FC<EstipulantesFiltersProps> = ({
  filters,
  onFilterChange,
  onClearFilters,
  isLoading,
}) => {
  const handleSearchChange = (value: string) => {
    const trimmed = value.trim();
    if (trimmed === filters.busca) return;

    onFilterChange({
      busca: trimmed,
      page: 1, // Reset to first page on search
    });
  };

  const hasActiveFilters = Boolean(filters.busca || filters.status);
  const searchValue = filters.busca || '';

  return (
    <FilterBar>
      <div className="estipulantes-filter-search">
        <SearchField
          id="busca-estipulante"
          placeholder="Razão Social, CNPJ ou Código"
          value={searchValue}
          onChange={handleSearchChange}
          disabled={isLoading}
          aria-label="Buscar estipulante"
        />
      </div>

      <div className="estipulantes-filter-status">
        <Select
          id="status-estipulante"
          value={filters.status || ''}
          onChange={(event) =>
            onFilterChange({ 
              status: event.target.value as unknown as StatusEstipulanteEnum | '',
              page: 1
            })
          }
          disabled={isLoading}
          aria-label="Status do estipulante"
        >
          <option value="">Todos</option>
          <option value="1">Ativos</option>
          <option value="2">Inativos</option>
        </Select>
      </div>

      <div className="estipulantes-filter-actions">
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
