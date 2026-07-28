/**
 * useCatalogo.ts — Hook para carregar o catálogo de módulos/recursos/permissões.
 */
import { useState, useEffect, useCallback } from 'react';
import { obterCatalogo } from '../api/catalogoApi';
import type { CatalogoModuloDto } from '../types/seguranca.types';

interface UseCatalogoState {
  data: CatalogoModuloDto[] | null;
  isLoading: boolean;
  error: Error | null;
}

export function useCatalogo() {
  const [state, setState] = useState<UseCatalogoState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const fetch = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const data = await obterCatalogo();
      setState({ data, isLoading: false, error: null });
    } catch {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: new Error('Não foi possível carregar o catálogo de permissões.'),
      }));
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetch();
  }, [fetch]);

  return { ...state, retry: fetch };
}
