import { useState, useCallback, useRef, useEffect } from 'react';
import { listarCooperados } from '../api/cooperadosApi';
import type { CooperadoListDto, CooperadosFiltersState, ListagemPaginadaResult } from '../types/cooperados.types';
import { HttpApiError } from '../../../services/http';

interface UseCooperadosState {
  data: ListagemPaginadaResult<CooperadoListDto> | null;
  isLoading: boolean;
  error: Error | null;
}

export function useCooperados(query: CooperadosFiltersState) {
  const [state, setState] = useState<UseCooperadosState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchCooperados = useCallback(async (currentQuery: CooperadosFiltersState) => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const data = await listarCooperados(currentQuery, controller.signal);
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
          ? new Error(err.message || 'Erro ao comunicar com a API')
          : err instanceof Error
          ? err
          : new Error('Erro desconhecido');

      setState((prev) => ({ ...prev, isLoading: false, error: normalizedError }));
    }
  }, []);

  useEffect(() => {
    fetchCooperados(query);
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [
    query.page,
    query.limit,
    query.nome,
    query.cpf,
    query.status,
    query.sortBy,
    query.direction,
    fetchCooperados,
  ]);

  const retry = useCallback(() => {
    fetchCooperados(query);
  }, [fetchCooperados, query]);

  return {
    ...state,
    retry,
  };
}
