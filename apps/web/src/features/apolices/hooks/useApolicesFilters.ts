import { useSearchParams } from 'react-router-dom';
import { useCallback, useMemo } from 'react';
import type { ApolicesQuery } from '../types/apolice.types';

export function useApolicesFilters() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters = useMemo<ApolicesQuery>(() => {
    const page = parseInt(searchParams.get('page') || '1', 10);
    const pageSize = parseInt(searchParams.get('pageSize') || '20', 10);
    const busca = searchParams.get('busca') || '';
    const status = searchParams.get('status') || '';
    const ativoStr = searchParams.get('ativo');
    const estipulanteId = searchParams.get('estipulanteId') || '';
    const seguradoraId = searchParams.get('seguradoraId') || '';
    const tipoRamo = searchParams.get('tipoRamo') || '';

    const ativo = ativoStr === 'true' ? true : ativoStr === 'false' ? false : undefined;

    return {
      page: isNaN(page) || page < 1 ? 1 : page,
      pageSize: isNaN(pageSize) || pageSize < 1 ? 20 : pageSize,
      busca,
      status,
      ativo,
      estipulanteId,
      seguradoraId,
      tipoRamo,
    };
  }, [searchParams]);

  const setFilters = useCallback((newFilters: Partial<ApolicesQuery>) => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      
      Object.entries(newFilters).forEach(([key, value]) => {
        if (value === '' || value === null || value === undefined) {
          next.delete(key);
        } else {
          next.set(key, String(value));
        }
      });

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
