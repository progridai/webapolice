import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createPath, ROUTES } from '../../../app/routes/routePaths';
import { useAuditoriaList } from '../hooks/useAuditoria';
import {
  Alert,
  Breadcrumbs,
  Button,
  DataTable,
  EmptyState,
  EyeIcon,
  PageHeader,
  Pagination,
  ResultsSummary,
  RowActions,
  Skeleton,
} from '../../../components/ui';
import './Seguranca.css';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import type { AuditoriaListDto } from '../types/seguranca.types';

const PAGE_SIZE = 20;

function formatDate(iso: string): string {
  try {
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(iso));
  } catch {
    return iso;
  }
}

export const AuditoriaListPage: React.FC = () => {
  const [page, setPage] = useState(1);
  const navigate = useNavigate();

  const { data, isLoading, error, retry } = useAuditoriaList({ page, pageSize: PAGE_SIZE });

  useEffect(() => {
    document.title = 'Auditoria | WebApolice';
  }, []);

  const hasData = data && data.itens && data.itens.length > 0;

  const columns: Column<AuditoriaListDto>[] = [
    {
      key: 'acao',
      label: 'Ação',
      render: (a) => (
        <span className="seguranca-user-name">{a.acao}</span>
      ),
    },
    {
      key: 'entidadeTipo',
      label: 'Entidade',
      render: (a) => (
        <div className="seguranca-flex-col">
          <span>{a.entidadeTipo}</span>
          <span className="seguranca-user-username">{a.entidadeId}</span>
        </div>
      ),
    },
    {
      key: 'createdAt',
      label: 'Data',
      render: (a) => <span className="text-sm">{formatDate(a.createdAt)}</span>,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (a) => (
        <RowActions
          primaryAction={{
            label: 'Visualizar',
            icon: <EyeIcon />,
            onClick: () =>
              navigate(createPath(ROUTES.SEGURANCA_AUDITORIA_DETALHES, { publicId: a.publicId })),
          }}
          actions={[]}
          ariaLabel={`Ver detalhes da auditoria ${a.publicId}`}
        />
      ),
    },
  ];

  return (
    <main className="seguranca-page" tabIndex={-1}>
      <PageHeader
        title="Auditoria"
        description="Registros de auditoria de ações realizadas no sistema."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Auditoria' },
            ]}
          />
        }
      />

      {error ? (
        <div className="seguranca-error">
          <Alert variant="error" title="Não foi possível carregar a auditoria">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading && !data ? (
        <div aria-busy="true" aria-live="polite" className="seguranca-skeletons">
          <Skeleton className="seguranca-skeleton-row" />
          <Skeleton className="seguranca-skeleton-row" />
          <Skeleton className="seguranca-skeleton-row" />
        </div>
      ) : !hasData && !isLoading ? (
        <EmptyState
          title="Nenhum registro de auditoria"
          description="Ainda não há eventos de auditoria registrados no sistema."
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
            aria-label="Lista de registros de auditoria"
            emptyTitle="Nenhum registro de auditoria"
            emptyDescription="Ainda não há eventos de auditoria registrados no sistema."
          />

          {data && data.totalPaginas > 1 && (
            <Pagination
              currentPage={data.paginaAtual}
              totalPages={data.totalPaginas}
              onPageChange={setPage}
              disabled={isLoading}
            />
          )}
        </div>
      )}
    </main>
  );
};
