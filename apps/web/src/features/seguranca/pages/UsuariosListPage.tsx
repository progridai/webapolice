import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { useUsuarios } from '../hooks/useUsuarios';
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
import type { UsuarioListDto } from '../types/seguranca.types';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

export const UsuariosListPage: React.FC = () => {
  const { filters, setFilters, clearFilters } = useSegurancaFilters();
  const { data, isLoading, error, retry } = useUsuarios(filters);
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
    document.title = 'Usuários | WebApolice';
  }, []);

  const hasActiveFilters = Boolean(filters.busca || filters.ativo !== '');
  const hasData = data && data.itens && data.itens.length > 0;
  
  const podeEditar = possuiAcessoTotal() || possuiPermissao('seguranca.usuarios.alterar');
  const podeCriar = possuiAcessoTotal() || possuiPermissao('seguranca.usuarios.inserir');

  const columns: Column<UsuarioListDto>[] = [
    {
      key: 'nome',
      label: 'Usuário',
      render: (u) => (
        <div className="seguranca-flex-col">
          <span className="seguranca-user-name">{u.nome}</span>
          <span className="seguranca-user-username">{u.username}</span>
        </div>
      ),
    },
    {
      key: 'email',
      label: 'E-mail',
      render: (u) => <span className="text-sm">{u.email}</span>,
    },
    {
      key: 'perfis',
      label: 'Perfis',
      render: (u) => (
        <div className="seguranca-badge-list">
          {u.perfis.length > 0 ? (
            u.perfis.map((p, i) => (
              <Badge key={i} variant="neutral">
                {p}
              </Badge>
            ))
          ) : (
            <span className="seguranca-empty-badges">Nenhum</span>
          )}
        </div>
      ),
    },
    {
      key: 'ativo',
      label: 'Status',
      render: (u) => <StatusBadge status={u.ativo ? 'ativo' : 'inativo'} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (u) => (
        <RowActions
          primaryAction={{
            label: 'Visualizar',
            icon: <EyeIcon />,
            onClick: () => navigate(`/seguranca/usuarios/${u.publicId}`),
          }}
          actions={
            podeEditar ? [
              {
                label: 'Alterar',
                icon: <EditIcon />,
                onClick: () => navigate(`/seguranca/usuarios/${u.publicId}/editar`),
              },
            ] : []
          }
          ariaLabel={`Ações para ${u.nome}`}
        />
      ),
    },
  ];

  return (
    <main className="seguranca-page" tabIndex={-1}>
      <PageHeader
        title="Usuários"
        description="Gerencie os usuários que possuem acesso ao sistema."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Usuários' },
            ]}
          />
        }
        actions={
          podeCriar ? (
            <Button onClick={() => navigate(ROUTES.SEGURANCA_USUARIO_NOVO)} variant="primary">
              Novo Usuário
            </Button>
          ) : undefined
        }
      />

      <section aria-label="Filtros de usuários">
        <FilterBar>
          <div>
            <label htmlFor="busca-usuario" className="block text-sm mb-1 font-medium text-slate-700 dark:text-slate-300">
              Buscar usuário
            </label>
            <SearchField
              id="busca-usuario"
              placeholder="Nome, e-mail ou username"
              value={filters.busca || ''}
              onChange={(v) => setFilters({ busca: v })}
              disabled={isLoading && !data}
            />
          </div>

          <div>
            <label htmlFor="status-usuario" className="block text-sm mb-1 font-medium text-slate-700 dark:text-slate-300">
              Status
            </label>
            <Select
              id="status-usuario"
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
          <Alert variant="error" title="Não foi possível carregar os usuários">
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
          title={hasActiveFilters ? 'Nenhum usuário encontrado' : 'Nenhum usuário cadastrado'}
          description={
            hasActiveFilters
              ? 'Não encontramos nenhum usuário com os filtros informados.'
              : 'Ainda não existem usuários cadastrados na plataforma.'
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
            aria-label="Lista de usuários"
            emptyTitle={hasActiveFilters ? 'Nenhum usuário encontrado' : 'Nenhum usuário cadastrado'}
            emptyDescription={
              hasActiveFilters
                ? 'Não encontramos nenhum usuário com os filtros informados.'
                : 'Ainda não existem usuários cadastrados na plataforma.'
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
