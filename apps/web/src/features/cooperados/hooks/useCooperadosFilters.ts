import { useSearchParams } from 'react-router-dom';
import { useCallback, useMemo } from 'react';
import type { CooperadosFiltersState } from '../types/cooperados.types';

export function useCooperadosFilters() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters = useMemo<CooperadosFiltersState>(() => {
    const page = parseInt(searchParams.get('page') || '1', 10);
    const limit = parseInt(searchParams.get('limit') || '20', 10);
    const nome = searchParams.get('nome') || '';
    const cpf = searchParams.get('cpf') || '';
    const status = searchParams.get('status') || '';
    const sortBy = searchParams.get('sortBy') || '';
    const direction = searchParams.get('direction') as 'asc' | 'desc' | null || 'asc';

    return {
      page: isNaN(page) || page < 1 ? 1 : page,
      limit: isNaN(limit) || limit < 1 ? 20 : limit,
      nome,
      cpf,
      status,
      sortBy,
      direction,
    };
  }, [searchParams]);

  const setFilters = useCallback((newFilters: Partial<CooperadosFiltersState>) => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      
      Object.entries(newFilters).forEach(([key, value]) => {
        if (value === '' || value === null || value === undefined) {
          next.delete(key);
        } else {
          next.set(key, String(value));
        }
      });

      // Se alterou algum filtro além da página e ordenação, reseta para a página 1
      const isPageOrSortChange = Object.keys(newFilters).every(
        (k) => k === 'page' || k === 'sortBy' || k === 'direction'
      );
      
      if (!isPageOrSortChange) {
        next.set('page', '1');
      }

      return next;
    });
  }, [setSearchParams]);

  const clearFilters = useCallback(() => {
    setSearchParams(new URLSearchParams());
  }, [setSearchParams]);

  return {
    filters,
    setFilters,
    clearFilters,
  };
}
