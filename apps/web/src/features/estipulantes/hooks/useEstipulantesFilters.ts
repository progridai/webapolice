/**
 * useEstipulantesFilters.ts
 *
 * Hook para gerenciar os filtros da listagem de estipulantes sincronizados com a URL.
 */
import { useSearchParams } from 'react-router-dom';
import { useCallback, useMemo } from 'react';
import type { EstipulantesQuery, StatusEstipulanteEnum } from '../types/estipulante.types';

export function useEstipulantesFilters() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters = useMemo<EstipulantesQuery>(() => {
    const page = parseInt(searchParams.get('page') || '1', 10);
    const pageSize = parseInt(searchParams.get('pageSize') || '20', 10);
    const busca = searchParams.get('busca') || '';
    const status = searchParams.get('status') as unknown as StatusEstipulanteEnum | '' || '';
    const sortBy = searchParams.get('sortBy') || '';
    const direction = searchParams.get('direction') as 'asc' | 'desc' | null || 'asc';

    return {
      page: isNaN(page) || page < 1 ? 1 : page,
      pageSize: isNaN(pageSize) || pageSize < 1 ? 20 : pageSize,
      busca,
      status,
      sortBy,
      direction,
    };
  }, [searchParams]);

  const setFilters = useCallback((newFilters: Partial<EstipulantesQuery>) => {
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
    }, { replace: true });
  }, [setSearchParams]);

  const clearFilters = useCallback(() => {
    setSearchParams(new URLSearchParams(), { replace: true });
  }, [setSearchParams]);

  return { filters, setFilters, clearFilters };
}
