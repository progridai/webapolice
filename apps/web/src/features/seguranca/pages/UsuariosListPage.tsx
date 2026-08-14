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
      <div className="flex flex-col gap-1">
        <span className="font-semibold text-texto-principal">{u.nome}</span>
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
        <div className="flex flex-wrap gap-1">
          {u.perfis.length > 0 ? (
            u.perfis.map((p, i) => (
              <Badge key={i} variant="neutral">
                {p}
              </Badge>
            ))
          ) : (
            <span className="text-xs text-texto-secundario">Nenhum</span>
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
    <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none" tabIndex={-1}>
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

      <section aria-label="Filtros de usuários" style={{ position: 'relative', zIndex: 10 }}>
        <FilterBar>
          <div>
            <label htmlFor="busca-usuario" className="block text-sm mb-1 font-medium text-texto-secundario">
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
            <label htmlFor="status-usuario" className="block text-sm mb-1 font-medium text-texto-secundario">
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
            >
              <option value="">Todos</option>
              <option value="true">Ativo</option>
              <option value="false">Inativo</option>
            </Select>
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
        <div className="flex flex-col items-start gap-4">
          <Alert variant="error" title="Não foi possível carregar os usuários">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading && !data && isMobile ? (
        <div aria-busy="true" aria-live="polite" className="flex flex-col gap-4">
          <Skeleton className="w-full h-12 rounded-lg" />
          <Skeleton className="w-full h-12 rounded-lg" />
          <Skeleton className="w-full h-12 rounded-lg" />
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
        <div className="flex flex-col gap-3">
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
