import React, { useEffect, useState } from 'react';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { useEstipulantes } from '../hooks/useEstipulantes';
import { useEstipulantesFilters } from '../hooks/useEstipulantesFilters';
import { inativarEstipulante, reativarEstipulante } from '../api/estipulantes.api';
import { EstipulantesFilters } from '../components/EstipulantesFilters';
import { EstipulantesTable } from '../components/EstipulantesTable';
import { EstipulantesMobileList } from '../components/EstipulantesMobileList';
import { useNavigate, useLocation } from 'react-router-dom';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import {
  Pagination, EmptyState, Alert, Button, Skeleton, ResultsSummary,
  PageHeader, Breadcrumbs, BriefcaseIcon, PlusIcon
} from '../../../components/ui';
import './EstipulantesListPage.css';

export const EstipulantesListPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { filters, setFilters, clearFilters } = useEstipulantesFilters();
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const { data, isLoading, error, retry } = useEstipulantes(filters, refreshTrigger);
  const [isMobile, setIsMobile] = useState(false);
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();

  const podeInserir = possuiAcessoTotal() || possuiPermissao('estipulantes.inserir');
  const podeAlterar = possuiAcessoTotal() || possuiPermissao('estipulantes.alterar');
  
  const podeInativar = possuiAcessoTotal() || possuiPermissao('estipulantes.inativar');
  const podeReativar = possuiAcessoTotal() || possuiPermissao('estipulantes.reativar');

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    document.title = 'Estipulantes | WebApolice';
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

  const handleInativar = async (publicId: string) => {
    try {
      await inativarEstipulante(publicId);
      setRefreshTrigger(prev => prev + 1);
    } catch (err) {
      console.error('Erro ao inativar', err);
      // Aqui idealmente teríamos um Toast notification
    }
  };

  const handleReativar = async (publicId: string) => {
    try {
      await reativarEstipulante(publicId);
      setRefreshTrigger(prev => prev + 1);
    } catch (err) {
      console.error('Erro ao reativar', err);
    }
  };

  const handleVerDetalhes = (publicId: string) => {
    navigate(createPath(ROUTES.ESTIPULANTE_DETALHES, { publicId }), {
      state: { fromListagem: true, search: location.search },
    });
  };

  const hasActiveFilters = Boolean(filters.busca || filters.status);
  const hasData = data && data.itens && data.itens.length > 0;

  return (
    <main className="estipulantes-page" tabIndex={-1}>
      <PageHeader
        title="Estipulantes"
        description="Gerencie as empresas e organizações estipulantes cadastradas no WebApólice."
        icon={<BriefcaseIcon size={24} />}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Cadastros', href: '/' },
              { label: 'Estipulantes' },
            ]}
          />
        }
        actions={
          podeInserir ? (
            <Button onClick={() => navigate('/estipulantes/novo')} icon={<PlusIcon size={16} />}>
              Novo Estipulante
            </Button>
          ) : undefined
        }
      />

      <section aria-label="Filtros de estipulantes">
        <EstipulantesFilters
          filters={filters}
          onFilterChange={setFilters}
          onClearFilters={clearFilters}
          isLoading={isLoading && !data}
        />
      </section>

      {error ? (
        <div className="estipulantes-error">
          <Alert variant="error" title="Não foi possível carregar os estipulantes">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading && !data && isMobile ? (
        <div className="estipulantes-skeletons" aria-busy="true" aria-live="polite">
          <Skeleton className="estipulantes-skeleton-row" />
          <Skeleton className="estipulantes-skeleton-row" />
          <Skeleton className="estipulantes-skeleton-row" />
        </div>
      ) : !hasData && isMobile ? (
        <EmptyState
          title={hasActiveFilters ? 'Nenhum estipulante corresponde aos filtros aplicados' : 'Nenhum estipulante cadastrado'}
          description={
            hasActiveFilters
              ? 'Tente mudar sua busca ou limpar os filtros para encontrar o que procura.'
              : 'Ainda não existem estipulantes cadastrados na plataforma.'
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
            Exibindo {data?.itens.length || 0} estipulantes.
          </div>

          {isMobile ? (
            <EstipulantesMobileList 
              estipulantes={data?.itens || []} 
              podeInativar={podeInativar}
              podeReativar={podeReativar}
              podeAlterar={podeAlterar}
              onInativar={handleInativar}
              onReativar={handleReativar}
              onEditar={(id) => navigate(`/estipulantes/${id}/editar`)}
              onDetalhar={handleVerDetalhes}
            />
          ) : (
            <EstipulantesTable
              estipulantes={data?.itens || []}
              isLoading={isLoading}
              sortBy={filters.sortBy}
              direction={filters.direction}
              onSort={handleSort}
              hasActiveFilters={hasActiveFilters}
              onClearFilters={clearFilters}
              podeInativar={podeInativar}
              podeReativar={podeReativar}
              podeAlterar={podeAlterar}
              onInativar={handleInativar}
              onReativar={handleReativar}
              onEditar={(id) => navigate(`/estipulantes/${id}/editar`)}
              onDetalhar={handleVerDetalhes}
            />
          )}

          {data && data.totalPaginas > 1 && (
            <div className="estipulantes-pagination">
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
