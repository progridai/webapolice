import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { useClientes } from '../hooks/useClientes';
import { useClientesFilters } from '../hooks/useClientesFilters';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { ClientesFilters } from '../components/ClientesFilters';
import { ClientesTable } from '../components/ClientesTable';
import { ClientesMobileList } from '../components/ClientesMobileList';
import {
  Pagination, EmptyState, Alert, Button, Skeleton, ResultsSummary,
  PageHeader, Breadcrumbs, UsersIcon
} from '../../../components/ui';
import './ClientesListPage.css';


export const ClientesListPage: React.FC = () => {
  const { filters, setFilters, clearFilters } = useClientesFilters();
  const { data, isLoading, error, retry } = useClientes(filters);
  const [isMobile, setIsMobile] = useState(false);
  const navigate = useNavigate();
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();

  const podeInserir = possuiAcessoTotal() || possuiPermissao('clientes.inserir');
  const podeAlterar = possuiAcessoTotal() || possuiPermissao('clientes.alterar');

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
    <main className="clientes-page" tabIndex={-1}>
      <PageHeader
        title="Clientes"
        description="Gerencie e consulte os clientes da plataforma."
        icon={<UsersIcon size={24} />}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Clientes' },
            ]}
          />
        }
        actions={
          podeInserir && (
            <Button onClick={() => navigate(ROUTES.CLIENTE_NOVO)} variant="primary">
              Novo Cliente
            </Button>
          )
        }
      />

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
      ) : isLoading && !data && isMobile ? (
        <div className="clientes-skeletons" aria-busy="true" aria-live="polite">
          <Skeleton className="clientes-skeleton-row" />
          <Skeleton className="clientes-skeleton-row" />
          <Skeleton className="clientes-skeleton-row" />
        </div>
      ) : !hasData && isMobile ? (
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
        <div className="flex flex-col gap-3">
          {data && hasData && (
            <ResultsSummary
              currentPage={data.paginaAtual}
              pageSize={data.tamanhoPagina}
              totalItems={data.totalItens}
            />
          )}

          <div aria-live="polite" className="sr-only">
            Exibindo {data?.itens.length || 0} clientes.
          </div>

          {isMobile ? (
            <ClientesMobileList clientes={data?.itens || []} podeAlterar={podeAlterar} />
          ) : (
            <ClientesTable
              clientes={data?.itens || []}
              isLoading={isLoading}
              sortBy={filters.sortBy}
              direction={filters.direction}
              onSort={handleSort}
              hasActiveFilters={hasActiveFilters}
              onClearFilters={clearFilters}
              podeAlterar={podeAlterar}
            />
          )}

          {data && data.totalPaginas > 1 && (
            <div className="clientes-pagination">
              <Pagination
                currentPage={data.paginaAtual}
                totalPages={data.totalPaginas}
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


