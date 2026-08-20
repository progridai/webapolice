import React, { useEffect, useState } from 'react';
import { useApolices } from '../hooks/useApolices';
import { useApolicesFilters } from '../hooks/useApolicesFilters';
import { ApolicesFilters } from '../components/ApolicesFilters';
import { ApolicesTable } from '../components/ApolicesTable';
import { ApolicesMobileList } from '../components/ApolicesMobileList';
import { useNavigate, useLocation } from 'react-router-dom';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import {
  Pagination, EmptyState, Alert, Button, Skeleton, ResultsSummary,
  PageHeader, Breadcrumbs
} from '../../../components/ui';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

export const ApolicesListPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { filters, setFilters, clearFilters } = useApolicesFilters();
  const { data, isLoading, error, retry } = useApolices(filters);
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    document.title = 'Apólices | WebApolice';
  }, []);

  const handlePageChange = (page: number) => {
    setFilters({ page });
  };

  const handleVerDetalhes = (publicId: string) => {
    navigate(createPath(ROUTES.APOLICE_DETALHES, { publicId }), {
      state: { fromListagem: true, search: location.search },
    });
  };

  const hasActiveFilters = Boolean(filters.busca || filters.status || filters.ativo !== undefined || filters.tipoRamo);
  const hasData = data && data.items && data.items.length > 0;
  const { possuiPermissao } = useAuthorization();
  const podeInserir = possuiPermissao('apolices.inserir');

  return (
    <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none" tabIndex={-1}>
      <PageHeader
        title="Apólices"
        description="Gestão de apólices e contratos vigentes no WebApólice."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Seguros', href: '/' },
              { label: 'Apólices' },
            ]}
          />
        }
        actions={
          podeInserir ? (
            <Button onClick={() => navigate(ROUTES.APOLICE_NOVA)}>Nova Apólice</Button>
          ) : undefined
        }
      />

      <section aria-label="Filtros de apólices" style={{ position: 'relative', zIndex: 10 }}>
        <ApolicesFilters
          filters={filters}
          onFilterChange={setFilters}
          onClearFilters={clearFilters}
          isLoading={isLoading && !data}
        />
      </section>

      {error ? (
        <div className="flex flex-col gap-4 items-start mt-4">
          <Alert variant="error" title="Não foi possível carregar as apólices">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading && !data && isMobile ? (
        <div className="flex flex-col gap-4 mt-4" aria-busy="true" aria-live="polite">
          <Skeleton className="h-[120px] rounded-lg" />
          <Skeleton className="h-[120px] rounded-lg" />
          <Skeleton className="h-[120px] rounded-lg" />
        </div>
      ) : !hasData && isMobile ? (
        <EmptyState
          title={hasActiveFilters ? 'Nenhuma apólice corresponde aos filtros aplicados' : 'Nenhuma apólice cadastrada'}
          description={
            hasActiveFilters
              ? 'Tente mudar sua busca ou limpar os filtros para encontrar o que procura.'
              : 'Ainda não existem apólices cadastradas na plataforma.'
          }
          action={
            hasActiveFilters ? (
              <Button onClick={clearFilters}>
                Limpar filtros
              </Button>
            ) : undefined
          }
        />
      ) : (
        <div className="flex flex-col gap-3">
          {data && hasData && (
            <ResultsSummary
              currentPage={data.page}
              pageSize={data.pageSize}
              totalItems={data.totalCount}
            />
          )}

          {isMobile ? (
            <ApolicesMobileList 
              apolices={data?.items || []} 
              onDetalhar={handleVerDetalhes}
            />
          ) : (
            <ApolicesTable
              apolices={data?.items || []}
              isLoading={isLoading}
              hasActiveFilters={hasActiveFilters}
              onClearFilters={clearFilters}
              onDetalhar={handleVerDetalhes}
            />
          )}

          {data && Math.ceil(data.totalCount / data.pageSize) > 1 && (
            <div className="flex justify-center md:justify-end mt-4 pt-4 border-t border-borda">
              <Pagination
                currentPage={data.page}
                totalPages={Math.ceil(data.totalCount / data.pageSize)}
                onPageChange={handlePageChange}
                disabled={isLoading}
              />
            </div>
          )}
        </div>
      )}
    </main>
  );
};
