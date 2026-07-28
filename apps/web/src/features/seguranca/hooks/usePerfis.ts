/**
 * usePerfis.ts — Hook para listagem paginada de perfis.
 * Segue exatamente o mesmo padrão de useClientes.ts.
 */
import { useState, useEffect, useCallback, useRef } from 'react';
import { listarPerfis } from '../api/perfisApi';
import type { PerfilDto, PerfisQuery, PagedResult } from '../types/seguranca.types';
import { HttpApiError } from '../../../services/http';

interface UsePerfisState {
  data: PagedResult<PerfilDto> | null;
  isLoading: boolean;
  error: Error | null;
}

export function usePerfis(query: PerfisQuery) {
  const [state, setState] = useState<UsePerfisState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchPerfis = useCallback(async (currentQuery: PerfisQuery) => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const data = await listarPerfis(currentQuery, controller.signal);
      setState({ data, isLoading: false, error: null });
    } catch (err: unknown) {
      if (err instanceof Error && err.name === 'AbortError') return;
      if (err instanceof DOMException && err.name === 'AbortError') return;

      const normalizedError =
        err instanceof HttpApiError
          ? err
          : new Error('Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.');

      setState((prev) => ({ ...prev, isLoading: false, error: normalizedError }));
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchPerfis(query);
    return () => {
      if (abortControllerRef.current) abortControllerRef.current.abort();
    };
  }, [fetchPerfis, query]);

  const retry = useCallback(() => {
    fetchPerfis(query);
  }, [fetchPerfis, query]);

  return { ...state, retry };
}
