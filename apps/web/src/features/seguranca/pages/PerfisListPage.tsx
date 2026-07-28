import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { usePerfis } from '../hooks/usePerfis';
import { useSegurancaFilters } from '../hooks/useSegurancaFilters';
import {
  Pagination,
  EmptyState,
  Alert,
  Button,
  Skeleton,
  ResultsSummary,
  PageHeader,
  Breadcrumbs,
  FilterBar,
  SearchField,
  Select,
  DataTable,
  StatusBadge,
  RowActions,
  EyeIcon,
  EditIcon,
  Badge,
} from '../../../components/ui';
import './Seguranca.css';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import type { PerfilDto } from '../types/seguranca.types';
import { useAuthorization } from '../../../auth/AuthorizationProvider';



export const PerfisListPage: React.FC = () => {
  const { filters, setFilters, clearFilters } = useSegurancaFilters();
  const { data, isLoading, error, retry } = usePerfis(filters);
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();
  const [isMobile, setIsMobile] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    document.title = 'Perfis de Acesso | WebApolice';
  }, []);

  const hasActiveFilters = Boolean(filters.busca || filters.ativo !== '');
  const hasData = data && data.itens && data.itens.length > 0;

  const podeEditar = possuiAcessoTotal() || possuiPermissao('seguranca.perfis.alterar');
  const podeCriar = possuiAcessoTotal() || possuiPermissao('seguranca.perfis.inserir');

  const columns: Column<PerfilDto>[] = [
    {
      key: 'nome',
      label: 'Perfil',
      render: (p) => (
        <div className="seguranca-flex-col">
          <span className="seguranca-user-name">{p.nome}</span>
          <span className="seguranca-user-username text-xs">{p.codigo}</span>
        </div>
      ),
    },
    {
      key: 'descricao',
      label: 'Descrição',
      render: (p) => <span className="text-sm truncate max-w-xs block">{p.descricao || '-'}</span>,
    },
    {
      key: 'tipo',
      label: 'Tipo',
      render: (p) => (
        <Badge variant={p.perfilSistema ? 'primary' : 'neutral'}>
          {p.perfilSistema ? 'Sistema' : 'Customizado'}
        </Badge>
      ),
    },
    {
      key: 'acesso',
      label: 'Acesso',
      render: (p) => (
        <span className="text-sm">
          {p.acessoTotal ? (
            <span className="text-primary-600 font-medium">Acesso Total</span>
          ) : (
            'Restrito'
          )}
        </span>
      ),
    },
    {
      key: 'ativo',
      label: 'Status',
      render: (p) => <StatusBadge status={p.ativo ? 'ativo' : 'inativo'} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (p) => (
        <RowActions
          primaryAction={{
            label: 'Visualizar',
            icon: <EyeIcon />,
            onClick: () => navigate(`/seguranca/perfis/${p.publicId}`),
          }}
          actions={
            podeEditar && !p.perfilSistema ? [
              {
                label: 'Alterar',
                icon: <EditIcon />,
                onClick: () => navigate(`/seguranca/perfis/${p.publicId}/editar`),
              },
            ] : []
          }
          ariaLabel={`Ações para ${p.nome}`}
        />
      ),
    },
  ];

  return (
    <main className="seguranca-page" tabIndex={-1}>
      <PageHeader
        title="Perfis de Acesso"
        description="Gerencie os perfis e suas permissões no sistema."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Perfis' },
            ]}
          />
        }
        actions={
          podeCriar ? (
            <Button onClick={() => navigate(ROUTES.SEGURANCA_PERFIL_NOVO)} variant="primary">
              Novo Perfil
            </Button>
          ) : undefined
        }
      />

      <section aria-label="Filtros de perfis">
        <FilterBar>
          <div>
            <label htmlFor="busca-perfil" className="block text-sm mb-1 font-medium text-slate-700 dark:text-slate-300">
              Buscar perfil
            </label>
            <SearchField
              id="busca-perfil"
              placeholder="Nome ou código"
              value={filters.busca || ''}
              onChange={(v) => setFilters({ busca: v })}
              disabled={isLoading && !data}
            />
          </div>

          <div>
            <label htmlFor="status-perfil" className="block text-sm mb-1 font-medium text-slate-700 dark:text-slate-300">
              Status
            </label>
            <Select
              id="status-perfil"
              value={filters.ativo === '' ? '' : String(filters.ativo)}
              onChange={(e) => {
                const v = e.target.value;
                setFilters({ ativo: v === '' ? '' : v === 'true' });
              }}
              disabled={isLoading && !data}
              options={[
                { label: 'Todos', value: '' },
                { label: 'Ativo', value: 'true' },
                { label: 'Inativo', value: 'false' },
              ]}
            />
          </div>

          <div className="flex items-end">
            <Button
              variant="secondary"
              onClick={clearFilters}
              disabled={!hasActiveFilters}
            >
              Limpar
            </Button>
          </div>
        </FilterBar>
      </section>

      {error ? (
        <div className="seguranca-error">
          <Alert variant="error" title="Não foi possível carregar os perfis">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading && !data && isMobile ? (
        <div aria-busy="true" aria-live="polite" className="seguranca-skeletons">
          <Skeleton className="seguranca-skeleton-row" />
          <Skeleton className="seguranca-skeleton-row" />
          <Skeleton className="seguranca-skeleton-row" />
        </div>
      ) : !hasData && !isLoading ? (
        <EmptyState
          title={hasActiveFilters ? 'Nenhum perfil encontrado' : 'Nenhum perfil cadastrado'}
          description={
            hasActiveFilters
              ? 'Não encontramos nenhum perfil com os filtros informados.'
              : 'Ainda não existem perfis cadastrados na plataforma.'
          }
          action={hasActiveFilters ? <Button onClick={clearFilters}>Limpar filtros</Button> : undefined}
        />
      ) : (
        <div className="seguranca-content">
          {data && hasData && (
            <ResultsSummary
              currentPage={data.paginaAtual}
              pageSize={data.tamanhoPagina}
              totalItems={data.totalItens}
            />
          )}

          <DataTable
            data={data?.itens || []}
            columns={columns}
            keyExtractor={(item) => item.publicId}
            isLoading={isLoading}
            aria-label="Lista de perfis"
            emptyTitle={hasActiveFilters ? 'Nenhum perfil encontrado' : 'Nenhum perfil cadastrado'}
            emptyDescription={
              hasActiveFilters
                ? 'Não encontramos nenhum perfil com os filtros informados.'
                : 'Ainda não existem perfis cadastrados na plataforma.'
            }
          />

          {data && data.totalPaginas > 1 && (
            <Pagination
              currentPage={data.paginaAtual}
              totalPages={data.totalPaginas}
              onPageChange={(p) => setFilters({ page: p })}
              disabled={isLoading}
            />
          )}
        </div>
      )}
    </main>
  );
};
