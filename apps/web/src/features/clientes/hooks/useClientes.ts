/**
 * useClientes.ts
 *
 * Hook para gerenciar a requisição assíncrona de clientes.
 */
import { useState, useEffect, useCallback, useRef } from 'react';
import { listarClientes } from '../api/clientesApi';
import type { ClienteListItem, ClientesQuery, PagedResult } from '../types/cliente.types';

interface UseClientesState {
  data: PagedResult<ClienteListItem> | null;
  isLoading: boolean;
  error: Error | null;
}

export function useClientes(query: ClientesQuery) {
  const [state, setState] = useState<UseClientesState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchClientes = useCallback(async (currentQuery: ClientesQuery) => {
    // Cancela a requisição anterior se houver
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const data = await listarClientes(currentQuery, controller.signal);
      setState({ data, isLoading: false, error: null });
    } catch (err: unknown) {
      // Ignora erro se foi causado por cancelamento
      if (err instanceof Error && err.name === 'AbortError') {
        return;
      }
      if (err instanceof DOMException && err.name === 'AbortError') {
        return;
      }

      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: err instanceof Error ? err : new Error('Erro desconhecido ao carregar clientes.'),
      }));
    }
  }, []);

  useEffect(() => {
    fetchClientes(query);

    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchClientes, query]);

  const retry = useCallback(() => {
    fetchClientes(query);
  }, [fetchClientes, query]);

  return {
    ...state,
    retry,
  };
}
