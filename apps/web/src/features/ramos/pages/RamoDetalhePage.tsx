import { useCallback, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import {
  Alert,
  Breadcrumbs,
  Button,
  DescriptionList,
  DetailsSection,
  EmptyState,
  PageHeader,
  Spinner,
  StatusBadge,
} from '../../../components/ui';
import { DescriptionItem } from '../../../components/ui/DescriptionList';
import { HttpApiError } from '../../../services/http/httpError';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';
import { ramosApi } from '../api/ramos.api';

export function RamoDetalhePage() {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();
  const contentRef = useRef<HTMLDivElement>(null);
  const load = useCallback(
    async (signal: AbortSignal) => {
      if (!publicId) return null;
      return ramosApi.obter(publicId, signal);
    },
    [publicId],
  );
  const { data, isLoading, error, refetch } = useConsulta(
    publicId ? load : null,
    'Não foi possível carregar os dados do ramo.',
  );

  useEffect(() => {
    if (!isLoading) contentRef.current?.focus();
  }, [isLoading, publicId]);

  const naoEncontrado =
    !publicId ||
    (error instanceof HttpApiError && error.isNotFound()) ||
    (!isLoading && !error && !data);
  const acessoNegado = error instanceof HttpApiError && error.isForbidden();

  return (
    <div
      ref={contentRef}
      tabIndex={-1}
      className="flex flex-col gap-3 w-full focus:outline-none"
      aria-busy={isLoading}
    >
      <PageHeader
        title={data && !error ? data.nome : 'Detalhes do Ramo'}
        titleExtras={
          data && !error ? (
            <StatusBadge status={data.ativo ? 'ativo' : 'inativo'} />
          ) : undefined
        }
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Ramos', href: ROUTES.RAMOS },
              { label: 'Detalhes' },
            ]}
          />
        }
        actions={
          <div className="flex flex-wrap gap-2">
            <Button variant="secondary" onClick={() => navigate(ROUTES.RAMOS)}>
              Voltar para ramos
            </Button>
            {data &&
              !isLoading &&
              !error &&
              (possuiAcessoTotal() || possuiPermissao('ramos.alterar')) && (
                <Button
                  onClick={() =>
                    navigate(
                      createPath(ROUTES.RAMOS_EDITAR, {
                        publicId: data.publicId,
                      }),
                    )
                  }
                >
                  Editar ramo
                </Button>
              )}
          </div>
        }
      />
      {isLoading ? (
        <Spinner aria-label="Carregando ramo" />
      ) : naoEncontrado ? (
        <EmptyState
          title="Ramo não encontrado"
          description="O ramo solicitado não existe ou não está mais disponível."
        />
      ) : error ? (
        <Alert
          variant="error"
          title={acessoNegado ? 'Acesso negado' : 'Erro ao carregar ramo'}
        >
          <p>
            {acessoNegado
              ? 'Você não tem permissão para visualizar este ramo.'
              : 'Não foi possível carregar os dados do ramo.'}
          </p>
          {!acessoNegado && (
            <Button variant="secondary" onClick={() => void refetch()}>
              Tentar novamente
            </Button>
          )}
        </Alert>
      ) : data ? (
        <DetailsSection title="Dados Gerais">
          <DescriptionList columns={2} density="compact">
            {data.codigo.toLowerCase() !== data.nome.toLowerCase() && (
              <DescriptionItem label="Código" value={data.codigo} />
            )}
            <DescriptionItem label="Nome" value={data.nome} />
            <DescriptionItem
              label="Descrição"
              value={data.descricao}
              className="sm:col-span-2 whitespace-pre-wrap break-words"
            />
            <DescriptionItem
              label="Data de cadastro"
              value={formatarDataOuVazio(data.createdAt)}
            />
            <DescriptionItem
              label="Última atualização"
              value={formatarDataOuVazio(data.updatedAt)}
            />
          </DescriptionList>
        </DetailsSection>
      ) : null}
    </div>
  );
}
