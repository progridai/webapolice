/**
 * useClientes.ts
 *
 * Hook para gerenciar a requisição assíncrona de clientes.
 */
import { useState, useEffect, useCallback, useRef } from 'react';
import { listarClientes } from '../api/clientesApi';
import type { ClienteListItem, ClientesQuery, PagedResult } from '../types/cliente.types';
import { HttpApiError } from '../../../services/http';

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

      const normalizedError =
        err instanceof HttpApiError
          ? err
          : new Error('Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.');

      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: normalizedError,
      }));
    }
  }, []);

  useEffect(() => {
    // Carrega os dados quando a query muda; o estado local do hook representa o ciclo da requisicao.
    // eslint-disable-next-line react-hooks/set-state-in-effect
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
