import React from 'react';
import { Select, Button, FilterBar, SearchField } from '../../../components/ui';
import type { CooperadosFiltersState } from '../types/cooperados.types';

interface CooperadosFiltersProps {
  filters: CooperadosFiltersState;
  onFilterChange: (filters: Partial<CooperadosFiltersState>) => void;
  onClearFilters: () => void;
  isLoading?: boolean;
}

export const CooperadosFilters: React.FC<CooperadosFiltersProps> = ({
  filters,
  onFilterChange,
  onClearFilters,
  isLoading,
}) => {
  const handleSearchChange = (value: string) => {
    const trimmed = value.trim();
    // Identifica se é CPF/CNPJ (só números e pontuação)
    const isDocument = /^[\d.-]+$/.test(trimmed) && trimmed.length > 0;
    
    // Na API de cooperados, CPF, Nome ou Código são buscáveis
    // Vamos alocar no campo nome se não for documento, pois o backend busca código também no "termoBusca/nome" se ajustado
    // Se o backend tiver campos separados, mapeamos apropriadamente. Aqui manteremos o pattern do cliente.
    const nextNome = isDocument ? '' : trimmed;
    const nextCpf = isDocument ? trimmed : '';

    if (nextNome === filters.nome && nextCpf === filters.cpf) return;

    onFilterChange({
      nome: nextNome,
      cpf: nextCpf,
      page: 1,
    });
  };

  const hasActiveFilters = Boolean(filters.nome || filters.cpf || filters.status);
  const searchValue = filters.nome || filters.cpf || '';

  return (
    <FilterBar>
      <div className="flex flex-col gap-1.5 flex-1 min-w-[250px]">
        <label htmlFor="busca-cooperado" className="text-xs font-semibold uppercase text-texto-secundario">
          Buscar cooperado
        </label>
        <SearchField
          id="busca-cooperado"
          placeholder="Nome, Código ou CPF"
          value={searchValue}
          onChange={handleSearchChange}
          disabled={isLoading}
        />
      </div>

      <div className="flex flex-col gap-1.5 w-[200px]">
        <label htmlFor="status-cooperado" className="text-xs font-semibold uppercase text-texto-secundario">
          Situação
        </label>
        <Select
          id="status-cooperado"
          value={filters.status || ''}
          onChange={(event) =>
            onFilterChange({ 
              status: event.target.value,
              page: 1 
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

      <div className="flex items-end h-[54px] ml-auto">
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
