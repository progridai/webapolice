import React, { useEffect } from 'react';
import { useCatalogo } from '../hooks/useCatalogo';
import { PageHeader, Breadcrumbs, Alert, Skeleton, Badge, Button } from '../../../components/ui';
import './Seguranca.css';

export const CatalogoPage: React.FC = () => {
  const { data: modulos, isLoading, error, retry } = useCatalogo();

  useEffect(() => {
    document.title = 'Catálogo de Permissões | WebApolice';
  }, []);

  return (
    <main className="seguranca-page" tabIndex={-1}>
      <PageHeader
        title="Catálogo de Permissões"
        description="Visão somente leitura dos módulos, recursos e permissões cadastrados no sistema."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Catálogo' },
            ]}
          />
        }
      />

      {error ? (
        <div className="seguranca-error">
          <Alert variant="error" title="Não foi possível carregar o catálogo">
            {error.message}
          </Alert>
          <Button onClick={retry} size="small" loading={isLoading}>
            Tentar novamente
          </Button>
        </div>
      ) : isLoading ? (
        <div aria-busy="true" aria-live="polite" className="seguranca-skeletons">
          <Skeleton className="seguranca-skeleton-row h-32" />
          <Skeleton className="seguranca-skeleton-row h-32" />
          <Skeleton className="seguranca-skeleton-row h-32" />
        </div>
      ) : !modulos || modulos.length === 0 ? (
        <Alert variant="info" title="Catálogo vazio">
          Nenhum módulo encontrado no catálogo.
        </Alert>
      ) : (
        <div className="seguranca-content">
          {modulos.map((modulo) => (
            <div key={modulo.publicId} className="catalogo-modulo">
              {/* Módulo */}
              <div className="catalogo-modulo-header">
                <span className="catalogo-modulo-nome">{modulo.nome}</span>
                <span className="catalogo-modulo-codigo">
                  <Badge variant="neutral">{modulo.codigo}</Badge>
                </span>
              </div>
              {modulo.descricao && (
                <p className="catalogo-descricao">{modulo.descricao}</p>
              )}

              {/* Recursos */}
              {modulo.recursos.length > 0 ? (
                <div className="catalogo-recursos-list">
                  {modulo.recursos.map((recurso) => (
                    <div key={recurso.publicId} className="catalogo-recurso">
                      <div className="catalogo-recurso-header">
                        <span className="catalogo-recurso-nome">{recurso.nome}</span>
                        <Badge variant="neutral">{recurso.codigo}</Badge>
                      </div>
                      {recurso.descricao && (
                        <p className="catalogo-descricao catalogo-descricao--recurso">{recurso.descricao}</p>
                      )}

                      {/* Permissões */}
                      {recurso.permissoes.length > 0 ? (
                        <div className="catalogo-permissoes-list">
                          {recurso.permissoes.map((permissao) => (
                            <div key={permissao.publicId} className="catalogo-permissao">
                              <span className="catalogo-permissao-nome">{permissao.nome}</span>
                              <code className="catalogo-permissao-codigo">{permissao.codigo}</code>
                              {permissao.descricao && (
                                <span className="catalogo-permissao-desc">{permissao.descricao}</span>
                              )}
                            </div>
                          ))}
                        </div>
                      ) : (
                        <p className="seguranca-empty-badges">Nenhuma permissão neste recurso.</p>
                      )}
                    </div>
                  ))}
                </div>
              ) : (
                <p className="seguranca-empty-badges">Nenhum recurso neste módulo.</p>
              )}
            </div>
          ))}
        </div>
      )}
    </main>
  );
};
