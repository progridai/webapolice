import React, { useState, useEffect } from 'react';
import { Input, Select, Button, SearchIcon } from '../../../components/ui';
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
  isLoading
}) => {
  const [localSearch, setLocalSearch] = useState(filters.nome || filters.cpf || '');

  // Sincroniza o estado local caso a URL mude por fora (ex: botão voltar do navegador)
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLocalSearch(filters.nome || filters.cpf || '');
  }, [filters.nome, filters.cpf]);

  // Aplica debounce na busca por texto
  useEffect(() => {
    const handler = setTimeout(() => {
      const trimmed = localSearch.trim();
      
      // Heurística simples para diferenciar CPF de Nome.
      // Se tiver só números ou formatado como CPF, manda como cpf.
      // Caso contrário manda como nome. O ideal seria o backend ter uma busca unificada `q`,
      // mas como temos campos separados, fazemos essa distinção básica.
      const isCpf = /^[\d.-]+$/.test(trimmed) && trimmed.length > 0;
      
      onFilterChange({
        nome: isCpf ? '' : trimmed,
        cpf: isCpf ? trimmed : '',
      });
    }, 500);

    return () => clearTimeout(handler);
  }, [localSearch, onFilterChange]);

  const hasActiveFilters = Boolean(filters.nome || filters.cpf || filters.status);

  return (
    <div className="flex flex-col sm:flex-row gap-4 items-end mb-6">
      <div className="flex-1 w-full">
        <label htmlFor="busca-cliente" className="block text-sm font-medium mb-1">
          Buscar cliente
        </label>
        <Input
          id="busca-cliente"
          type="text"
          placeholder="Nome ou CPF..."
          value={localSearch}
          onChange={(e) => setLocalSearch(e.target.value)}
          disabled={isLoading}
          icon={<SearchIcon />}
        />
      </div>
      
      <div className="w-full sm:w-48">
        <label htmlFor="status-cliente" className="block text-sm font-medium mb-1">
          Status
        </label>
        <Select
          id="status"
          value={filters.status || ''}
          onChange={(e) => onFilterChange({ status: e.target.value as unknown as StatusClienteEnum | '' })}
          disabled={isLoading}
          options={[
            { label: 'Todos', value: '' },
            { label: 'Ativo', value: '1' },
            { label: 'Inativo', value: '2' },
          ]}
        />
      </div>

      <div className="w-full sm:w-auto">
        <Button 
          variant="outline" 
          onClick={onClearFilters} 
          disabled={!hasActiveFilters || isLoading}
          aria-label="Limpar filtros"
          fullWidth
        >
          Limpar
        </Button>
      </div>
    </div>
  );
};
