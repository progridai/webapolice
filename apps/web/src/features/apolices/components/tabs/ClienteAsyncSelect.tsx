import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Input, Spinner } from '../../../../components/ui';
import { listarClientes } from '../../../clientes/api/clientesApi';
import type { ClienteListItem } from '../../../clientes/types/cliente.types';

// Assuming there is a useDebounce hook. If not, I will implement one here.
function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    return () => clearTimeout(handler);
  }, [value, delay]);
  return debouncedValue;
}

interface ClienteAsyncSelectProps {
  value: string | null;
  onChange: (clientePublicId: string) => void;
  error?: boolean;
  disabled?: boolean;
}

export const ClienteAsyncSelect: React.FC<ClienteAsyncSelectProps> = ({
  value,
  onChange,
  error,
  disabled
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [options, setOptions] = useState<ClienteListItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  
  const [selectedCliente, setSelectedCliente] = useState<ClienteListItem | null>(null);

  const debouncedSearchTerm = useDebounce(searchTerm, 500);
  const wrapperRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!value) {
      setSearchTerm('');
      setSelectedCliente(null);
    }
  }, [value]);

  const fetchClientes = useCallback(async (query: string) => {
    setLoading(true);
    try {
      const response = await listarClientes({
        page: 1,
        pageSize: 50,
        nome: query || undefined,
        status: 1 // StatusClienteEnum.Ativo
      });
      setOptions(response.itens || []);
    } catch (err) {
      console.error('Erro ao buscar clientes', err);
      setOptions([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (isOpen) {
      fetchClientes(debouncedSearchTerm);
    }
  }, [debouncedSearchTerm, isOpen, fetchClientes]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) {
        setIsOpen(false);
        if (selectedCliente) {
          setSearchTerm(selectedCliente.nome);
        } else {
          setSearchTerm('');
        }
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [selectedCliente]);

  const handleSelect = (cliente: ClienteListItem) => {
    setSelectedCliente(cliente);
    setSearchTerm(cliente.nome);
    setIsOpen(false);
    onChange(cliente.id);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setIsOpen(true);
    if (selectedCliente) {
      setSelectedCliente(null);
      onChange('');
    }
  };

  return (
    <div ref={wrapperRef} className="relative w-full">
      <div className="relative">
        <Input
          type="text"
          placeholder="Buscar por nome..."
          value={searchTerm}
          onChange={handleChange}
          onFocus={() => setIsOpen(true)}
          error={error}
          disabled={disabled}
          className="w-full pr-10"
          autoComplete="off"
        />
        {loading && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2">
            <Spinner size="small" />
          </div>
        )}
      </div>

      {isOpen && !disabled && (
        <div className="absolute z-10 w-full mt-1 bg-fundo-elevado border border-borda rounded shadow-lg max-h-60 overflow-y-auto">
          {options.length === 0 && !loading ? (
            <div className="p-3 text-sm text-texto-secundario text-center">Nenhum cliente ativo encontrado.</div>
          ) : (
            <ul className="py-1">
              {options.map((cliente) => (
                <li
                  key={cliente.id}
                  className={`px-3 py-2 cursor-pointer text-sm hover:bg-fundo-superficie transition-colors ${
                    selectedCliente?.id === cliente.id ? 'bg-fundo-superficie font-medium' : ''
                  }`}
                  onClick={() => handleSelect(cliente)}
                >
                  <div className="text-texto-principal">{cliente.nome}</div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
};
