import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { useClientes } from '../hooks/useClientes';
import { useClientesFilters } from '../hooks/useClientesFilters';
import { ClientesFilters } from '../components/ClientesFilters';
import { ClientesTable } from '../components/ClientesTable';
import { ClientesMobileList } from '../components/ClientesMobileList';
import { Pagination, EmptyState, Alert, Button, Skeleton } from '../../../components/ui';
import './ClientesListPage.css';

export const ClientesListPage: React.FC = () => {
  const { filters, setFilters, clearFilters } = useClientesFilters();
  const { data, isLoading, error, retry } = useClientes(filters);
  const [isMobile, setIsMobile] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    document.title = 'Clientes | WebApolice';
  }, []);

  const handleSort = (column: string) => {
    setFilters({
      sortBy: column,
      direction: filters.sortBy === column && filters.direction === 'asc' ? 'desc' : 'asc',
    });
  };

  const handlePageChange = (page: number) => {
    setFilters({ page });
  };

  const hasActiveFilters = Boolean(filters.nome || filters.cpf || filters.status);
  const hasData = data && data.itens && data.itens.length > 0;

  return (
    <div className="clientes-page" role="main">
      <header className="clientes-page-header">
        <div className="clientes-page-header-text">
          <h1 className="clientes-page-title">Clientes</h1>
          <p className="clientes-page-subtitle">
            Gerencie e consulte os clientes da plataforma.
          </p>
        </div>
        <Button onClick={() => navigate(ROUTES.CLIENTE_NOVO)} variant="primary">
          Novo Cliente
        </Button>
      </header>

      <section aria-label="Filtros de clientes">
        <ClientesFilters
          filters={filters}
          onFilterChange={setFilters}
          onClearFilters={clearFilters}
          isLoading={isLoading && !data}
        />
      </section>

      {error ? (
        <div className="clientes-error">
          <Alert variant="error" title="Não foi possível carregar os clientes">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading && !data ? (
        <div className="clientes-skeletons" aria-busy="true" aria-live="polite">
          <Skeleton className="clientes-skeleton-row" />
          <Skeleton className="clientes-skeleton-row" />
          <Skeleton className="clientes-skeleton-row" />
        </div>
      ) : !hasData ? (
        <EmptyState
          title={hasActiveFilters ? 'Nenhum cliente encontrado' : 'Nenhum cliente cadastrado'}
          description={
            hasActiveFilters
              ? 'Não encontramos nenhum cliente com os filtros informados.'
              : 'Ainda não existem clientes cadastrados na plataforma.'
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
        <>
          <div aria-live="polite" className="sr-only">
            Exibindo {data.itens.length} clientes.
          </div>

          {isMobile ? (
            <ClientesMobileList clientes={data.itens} />
          ) : (
            <ClientesTable
              clientes={data.itens}
              isLoading={isLoading}
              sortBy={filters.sortBy}
              direction={filters.direction}
              onSort={handleSort}
            />
          )}

          {data.totalPaginas > 1 && (
            <div className="clientes-pagination">
              <span className="clientes-pagination-summary">
                Exibindo página {data.paginaAtual} de {data.totalPaginas} ({data.totalItens} total)
              </span>
              <Pagination
                currentPage={data.paginaAtual}
                totalPages={data.totalPaginas}
                onPageChange={handlePageChange}
                disabled={isLoading}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
};
