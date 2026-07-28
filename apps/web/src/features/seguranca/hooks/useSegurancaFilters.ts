/**
 * useSegurancaFilters.ts — Hook genérico para filtros de listagem de Segurança.
 * Padrão idêntico ao useClientesFilters.ts.
 */
import { useSearchParams } from 'react-router-dom';
import { useCallback, useMemo } from 'react';

export interface SegurancaFilters {
  page?: number;
  pageSize?: number;
  busca?: string;
  ativo?: boolean | '';
}

export function useSegurancaFilters() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters = useMemo<SegurancaFilters>(() => {
    const page = parseInt(searchParams.get('page') || '1', 10);
    const pageSize = parseInt(searchParams.get('pageSize') || '20', 10);
    const busca = searchParams.get('busca') || '';
    const ativoRaw = searchParams.get('ativo');
    const ativo: boolean | '' =
      ativoRaw === 'true' ? true : ativoRaw === 'false' ? false : '';

    return {
      page: isNaN(page) || page < 1 ? 1 : page,
      pageSize: isNaN(pageSize) || pageSize < 1 ? 20 : pageSize,
      busca,
      ativo,
    };
  }, [searchParams]);

  const setFilters = useCallback(
    (newFilters: Partial<SegurancaFilters>) => {
      setSearchParams(
        (prev) => {
          const next = new URLSearchParams(prev);
          Object.entries(newFilters).forEach(([key, value]) => {
            if (value === '' || value === null || value === undefined) {
              next.delete(key);
            } else {
              next.set(key, String(value));
            }
          });
          const isPageOrSort = Object.keys(newFilters).every((k) => k === 'page');
          if (!isPageOrSort) next.set('page', '1');
          return next;
        },
        { replace: true }
      );
    },
    [setSearchParams]
  );

  const clearFilters = useCallback(() => {
    setSearchParams(new URLSearchParams(), { replace: true });
  }, [setSearchParams]);

  return { filters, setFilters, clearFilters };
}
