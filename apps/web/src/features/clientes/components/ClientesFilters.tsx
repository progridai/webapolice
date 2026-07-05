import React, { useEffect, useState } from 'react';
import { Input, Select, Button, SearchIcon } from '../../../components/ui';
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
  const [localSearch, setLocalSearch] = useState(filters.nome || filters.cpf || '');

  useEffect(() => {
    // Sincroniza o campo quando a URL muda por navegação/limpeza de filtros.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLocalSearch(filters.nome || filters.cpf || '');
  }, [filters.nome, filters.cpf]);

  useEffect(() => {
    const handler = setTimeout(() => {
      const trimmed = localSearch.trim();
      const isCpf = /^[\d.-]+$/.test(trimmed) && trimmed.length > 0;
      const nextNome = isCpf ? '' : trimmed;
      const nextCpf = isCpf ? trimmed : '';

      if (nextNome === filters.nome && nextCpf === filters.cpf) return;

      onFilterChange({
        nome: nextNome,
        cpf: nextCpf,
      });
    }, 500);

    return () => clearTimeout(handler);
  }, [filters.cpf, filters.nome, localSearch, onFilterChange]);

  const hasActiveFilters = Boolean(filters.nome || filters.cpf || filters.status);

  return (
    <div className="clientes-filters">
      <div className="clientes-filter-search">
        <label htmlFor="busca-cliente" className="clientes-filter-label">
          Buscar cliente
        </label>
        <Input
          id="busca-cliente"
          type="text"
          placeholder="Nome ou CPF"
          value={localSearch}
          onChange={(event) => setLocalSearch(event.target.value)}
          disabled={isLoading}
          icon={<SearchIcon />}
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
            onFilterChange({ status: event.target.value as unknown as StatusClienteEnum | '' })
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
    </div>
  );
};
