import React, { useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Alert,
  Breadcrumbs,
  Button,
  DescriptionList,
  DetailsSection,
  PageHeader,
  Skeleton,
} from '../../../components/ui';
import { DescriptionItem } from '../../../components/ui/DescriptionList';
import { useAuditoriaDetalhe } from '../hooks/useAuditoria';
import { ROUTES } from '../../../app/routes/routePaths';
import './Seguranca.css';

function formatDate(iso: string): string {
  try {
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    }).format(new Date(iso));
  } catch {
    return iso;
  }
}

function formatJson(value: string | null): string | null {
  if (!value) return null;
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}

function JsonDisplay({ value }: { value: string | null }) {
  const formatted = formatJson(value);
  if (!formatted) return <span className="seguranca-empty-badges">—</span>;
  return (
    <pre className="catalogo-json-preview" aria-label="Dados formatados">
      {formatted}
    </pre>
  );
}

export const AuditoriaDetalhesPage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  const mainRef = useRef<HTMLElement>(null);

  const { data: auditoria, isLoading, error, retry } = useAuditoriaDetalhe(publicId ?? '');

  useEffect(() => {
    if (!isLoading && mainRef.current) mainRef.current.focus();
  }, [isLoading]);

  useEffect(() => {
    document.title = auditoria
      ? `${auditoria.acao} | Auditoria | WebApolice`
      : 'Detalhes de Auditoria | WebApolice';
  }, [auditoria]);

  return (
    <main className="seguranca-page" tabIndex={-1} ref={mainRef}>
      <PageHeader
        title="Detalhes de Auditoria"
        description="Informações detalhadas do registro de auditoria selecionado."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Auditoria', href: ROUTES.SEGURANCA_AUDITORIA },
              { label: 'Detalhes' },
            ]}
          />
        }
        actions={
          <Button variant="secondary" onClick={() => navigate(ROUTES.SEGURANCA_AUDITORIA)}>
            Voltar
          </Button>
        }
      />

      {error ? (
        <div className="seguranca-error">
          <Alert variant="error" title="Não foi possível carregar o registro">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading ? (
        <div aria-busy="true" aria-live="polite" className="seguranca-skeletons">
          <Skeleton className="seguranca-skeleton-row h-40" />
          <Skeleton className="seguranca-skeleton-row h-40" />
        </div>
      ) : auditoria ? (
        <div className="seguranca-content">
          <DetailsSection title="Informações do Evento">
            <DescriptionList>
              <DescriptionItem label="Ação" value={auditoria.acao} />
              <DescriptionItem label="Entidade" value={auditoria.entidadeTipo} />
              <DescriptionItem
                label="ID da Entidade"
                value={<code className="catalogo-permissao-codigo">{auditoria.entidadeId}</code>}
              />
              <DescriptionItem label="Data" value={formatDate(auditoria.createdAt)} />
              <DescriptionItem
                label="ID do Registro"
                value={<code className="catalogo-permissao-codigo">{auditoria.publicId}</code>}
              />
            </DescriptionList>
          </DetailsSection>

          <DetailsSection title="Dados Alterados">
            <DescriptionList columns={1}>
              <DescriptionItem
                label="Dados Anteriores"
                value={<JsonDisplay value={auditoria.dadosAnteriores} />}
              />
              <DescriptionItem
                label="Dados Novos"
                value={<JsonDisplay value={auditoria.dadosNovos} />}
              />
            </DescriptionList>
          </DetailsSection>
        </div>
      ) : (
        <Alert variant="error" title="Registro não encontrado">
          O registro de auditoria solicitado não foi encontrado.
        </Alert>
      )}
    </main>
  );
};
