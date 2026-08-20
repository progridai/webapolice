import React from 'react';
import { Select, Button, FilterBar, SearchField } from '../../../components/ui';
import type { ApolicesQuery } from '../types/apolice.types';

interface ApolicesFiltersProps {
  filters: ApolicesQuery;
  onFilterChange: (filters: Partial<ApolicesQuery>) => void;
  onClearFilters: () => void;
  isLoading?: boolean;
}

export const ApolicesFilters: React.FC<ApolicesFiltersProps> = ({
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
      page: 1,
    });
  };

  const hasActiveFilters = Boolean(
    filters.busca || 
    filters.status || 
    filters.ativo !== undefined ||
    filters.tipoRamo
  );
  const searchValue = filters.busca || '';

  return (
    <FilterBar>
      <div className="flex-1 min-w-[200px]">
        <SearchField
          id="busca-apolice"
          placeholder="Nome, Número da Apólice..."
          value={searchValue}
          onChange={handleSearchChange}
          disabled={isLoading}
          aria-label="Buscar apólice"
        />
      </div>

      <div className="w-[150px]">
        <Select
          id="status-apolice"
          value={filters.status || ''}
          onChange={(event) =>
            onFilterChange({ 
              status: event.target.value,
              page: 1
            })
          }
          disabled={isLoading}
          aria-label="Status da apólice"
        >
          <option value="">Status (Todos)</option>
          <option value="EmImplantacao">Em Implantação</option>
          <option value="Ativa">Ativa</option>
          <option value="Inativa">Inativa</option>
          <option value="Cancelada">Cancelada</option>
          <option value="Renovada">Renovada</option>
        </Select>
      </div>

      <div className="w-[150px]">
        <Select
          id="ativo-apolice"
          value={filters.ativo === undefined ? '' : filters.ativo.toString()}
          onChange={(event) => {
            const val = event.target.value;
            onFilterChange({ 
              ativo: val === '' ? undefined : val === 'true',
              page: 1
            });
          }}
          disabled={isLoading}
          aria-label="Ativo/Inativo"
        >
          <option value="">Ativo/Inativo</option>
          <option value="true">Ativos</option>
          <option value="false">Inativos</option>
        </Select>
      </div>

      <div className="w-[120px]">
        <Button
          variant="secondary"
          onClick={onClearFilters}
          disabled={!hasActiveFilters || isLoading}
          aria-label="Limpar filtros"
          className="w-full"
        >
          Limpar
        </Button>
      </div>
    </FilterBar>
  );
};
