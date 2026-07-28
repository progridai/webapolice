/**
 * useUsuarios.ts — Hook para listagem paginada de usuários.
 * Segue exatamente o mesmo padrão de useClientes.ts.
 */
import { useState, useEffect, useCallback, useRef } from 'react';
import { listarUsuarios } from '../api/usuariosApi';
import type { UsuarioListDto, UsuariosQuery, PagedResult } from '../types/seguranca.types';
import { HttpApiError } from '../../../services/http';

interface UseUsuariosState {
  data: PagedResult<UsuarioListDto> | null;
  isLoading: boolean;
  error: Error | null;
}

export function useUsuarios(query: UsuariosQuery) {
  const [state, setState] = useState<UseUsuariosState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchUsuarios = useCallback(async (currentQuery: UsuariosQuery) => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const data = await listarUsuarios(currentQuery, controller.signal);
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
    fetchUsuarios(query);
    return () => {
      if (abortControllerRef.current) abortControllerRef.current.abort();
    };
  }, [fetchUsuarios, query]);

  const retry = useCallback(() => {
    fetchUsuarios(query);
  }, [fetchUsuarios, query]);

  return { ...state, retry };
}
