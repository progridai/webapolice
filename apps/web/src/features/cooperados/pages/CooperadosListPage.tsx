import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { useCooperados } from '../hooks/useCooperados';
import { useCooperadosFilters } from '../hooks/useCooperadosFilters';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { CooperadosFilters } from '../components/CooperadosFilters';
import { CooperadosTable } from '../components/CooperadosTable';
import { CooperadosMobileList } from '../components/CooperadosMobileList';
import {
  Pagination, EmptyState, Alert, Button, Skeleton, ResultsSummary,
  PageHeader, Breadcrumbs, UsersIcon
} from '../../../components/ui';

export const CooperadosListPage: React.FC = () => {
  const { filters, setFilters, clearFilters } = useCooperadosFilters();
  const { data, isLoading, error, retry } = useCooperados(filters);
  const [isMobile, setIsMobile] = useState(false);
  const navigate = useNavigate();
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();

  const podeInserir = possuiAcessoTotal() || possuiPermissao('cooperados.inserir');
  const podeAlterar = possuiAcessoTotal() || possuiPermissao('cooperados.alterar');

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    document.title = 'Cooperados | WebApólice';
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
    <main className="flex flex-col gap-6 p-4 md:p-6 w-full max-w-[1440px] mx-auto focus:outline-none" tabIndex={-1}>
      <PageHeader
        title="Cooperados e Coordenadores"
        description="Gerencie a base de cooperados e coordenadores do sistema."
        icon={<UsersIcon size={24} />}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Cooperados' },
            ]}
          />
        }
        actions={
          podeInserir && (
            <Button onClick={() => navigate(ROUTES.COOPERADOS_NOVO)} variant="primary">
              Novo Cooperado
            </Button>
          )
        }
      />

      <section aria-label="Filtros de cooperados">
        <CooperadosFilters
          filters={filters}
          onFilterChange={setFilters}
          onClearFilters={clearFilters}
          isLoading={isLoading && !data}
        />
      </section>

      {error ? (
        <div className="flex flex-col gap-4 mt-8 max-w-2xl mx-auto text-center items-center">
          <Alert variant="error" title="Não foi possível carregar os cooperados">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading && !data && isMobile ? (
        <div className="flex flex-col gap-4 mt-4" aria-busy="true" aria-live="polite">
          <Skeleton className="w-full h-32 rounded-lg bg-fundo-superficie border border-borda" />
          <Skeleton className="w-full h-32 rounded-lg bg-fundo-superficie border border-borda" />
          <Skeleton className="w-full h-32 rounded-lg bg-fundo-superficie border border-borda" />
        </div>
      ) : !hasData && isMobile ? (
        <EmptyState
          icon={<UsersIcon size={48} className="text-texto-secundario opacity-50" />}
          title={hasActiveFilters ? 'Nenhum cooperado encontrado' : 'Nenhum cooperado cadastrado'}
          description={
            hasActiveFilters
              ? 'Não encontramos nenhum registro com os filtros informados.'
              : 'Ainda não existem cooperados cadastrados na plataforma.'
          }
          action={
            hasActiveFilters && clearFilters ? (
              <Button onClick={clearFilters} variant="secondary">
                Limpar filtros
              </Button>
            ) : undefined
          }
        />
      ) : (
        <section aria-label="Lista de cooperados" className="flex flex-col gap-4">
          {!isMobile && (
            <CooperadosTable
              cooperados={data?.itens || []}
              isLoading={isLoading}
              sortBy={filters.sortBy}
              direction={filters.direction}
              onSort={handleSort}
              hasActiveFilters={hasActiveFilters}
              onClearFilters={clearFilters}
              podeAlterar={podeAlterar}
            />
          )}

          {isMobile && hasData && (
            <CooperadosMobileList 
              cooperados={data.itens} 
              podeAlterar={podeAlterar}
            />
          )}

          {hasData && data && (
            <div className="flex flex-col sm:flex-row items-center justify-between gap-4 mt-2">
              <ResultsSummary
                total={data.totalGeral}
                currentPage={data.paginaAtual}
                pageSize={data.tamanhoPagina}
                itemName="cooperado"
                itemNamePlural="cooperados"
              />
              <Pagination
                currentPage={data.paginaAtual}
                totalPages={data.totalPaginas}
                onPageChange={handlePageChange}
                disabled={isLoading}
              />
            </div>
          )}
        </section>
      )}
    </main>
  );
};
