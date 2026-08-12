/**
 * useEstipulantes.ts
 *
 * Hook para gerenciar a requisição assíncrona de estipulantes.
 */
import { useState, useEffect, useCallback, useRef } from 'react';
import { listarEstipulantes } from '../api/estipulantes.api';
import type { EstipulanteListItem, EstipulantesQuery, PagedResult } from '../types/estipulante.types';
import { HttpApiError } from '../../../services/http';

interface UseEstipulantesState {
  data: PagedResult<EstipulanteListItem> | null;
  isLoading: boolean;
  error: Error | null;
}

export function useEstipulantes(query: EstipulantesQuery, refreshTrigger?: number) {
  const [state, setState] = useState<UseEstipulantesState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchEstipulantes = useCallback(async (currentQuery: EstipulantesQuery) => {
    // Cancela a requisição anterior se houver
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const data = await listarEstipulantes(currentQuery, controller.signal);
      setState({ data, isLoading: false, error: null });
    } catch (err: unknown) {
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
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchEstipulantes(query);

    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchEstipulantes, query, refreshTrigger]);

  const retry = useCallback(() => {
    fetchEstipulantes(query);
  }, [fetchEstipulantes, query]);

  return {
    ...state,
    retry,
  };
}
