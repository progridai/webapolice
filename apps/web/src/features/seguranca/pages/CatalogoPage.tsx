import React, { useEffect } from 'react';
import { useCatalogo } from '../hooks/useCatalogo';
import { PageHeader, Breadcrumbs, Alert, Skeleton, Badge, Button } from '../../../components/ui';

export const CatalogoPage: React.FC = () => {
  const { data: modulos, isLoading, error, retry } = useCatalogo();

  useEffect(() => {
    document.title = 'Catálogo de Permissões | WebApolice';
  }, []);

  return (
    <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none" tabIndex={-1}>
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
        <div className="flex flex-col gap-4">
          <Alert variant="error" title="Não foi possível carregar o catálogo">
            {error.message}
          </Alert>
          <div className="flex">
            <Button onClick={retry} variant="secondary" loading={isLoading}>
              Tentar novamente
            </Button>
          </div>
        </div>
      ) : isLoading ? (
        <div aria-busy="true" aria-live="polite" className="flex flex-col gap-4">
          <Skeleton className="w-full h-32 rounded-lg" />
          <Skeleton className="w-full h-32 rounded-lg" />
          <Skeleton className="w-full h-32 rounded-lg" />
        </div>
      ) : !modulos || modulos.length === 0 ? (
        <Alert variant="info" title="Catálogo vazio">
          Nenhum módulo encontrado no catálogo.
        </Alert>
      ) : (
        <div className="flex flex-col gap-3">
          {modulos.map((modulo) => (
            <div key={modulo.publicId} className="p-4 rounded-lg bg-fundo-aplicacao border border-borda flex flex-col gap-3">
              {/* Módulo */}
              <div className="flex flex-col gap-1">
                <div className="flex items-center gap-3">
                  <span className="text-xl font-bold text-texto-principal">{modulo.nome}</span>
                  {modulo.codigo.toLowerCase() !== modulo.nome.toLowerCase() && (
                    <Badge variant="neutral">{modulo.codigo}</Badge>
                  )}
                </div>
                {modulo.descricao && (
                  <p className="text-sm text-texto-secundario">{modulo.descricao}</p>
                )}
              </div>

              {/* Recursos */}
              {modulo.recursos.length > 0 ? (
                <div className="flex flex-col gap-2 mt-2">
                  {modulo.recursos.map((recurso) => (
                    <div key={recurso.publicId} className="p-3 rounded-md bg-fundo-superficie border border-borda flex flex-col gap-2">
                      <div className="flex flex-col gap-1">
                        <div className="flex items-center gap-2">
                          <span className="text-lg font-semibold text-texto-principal">{recurso.nome}</span>
                          {recurso.codigo.toLowerCase() !== recurso.nome.toLowerCase() && (
                            <Badge variant="neutral">{recurso.codigo}</Badge>
                          )}
                        </div>
                        {recurso.descricao && (
                          <p className="text-sm text-texto-secundario">{recurso.descricao}</p>
                        )}
                      </div>

                      {/* Permissões */}
                      {recurso.permissoes.length > 0 ? (
                        <div className="flex flex-col gap-2 mt-1 border-t border-borda pt-3">
                          {recurso.permissoes.map((permissao) => (
                            <div key={permissao.publicId} className="flex flex-col lg:flex-row lg:items-center justify-between p-2 rounded-md hover:bg-fundo-aplicacao transition-colors gap-2">
                              <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                                <span className="text-sm font-medium text-texto-principal">{permissao.nome}</span>
                                {permissao.codigo.toLowerCase() !== permissao.nome.toLowerCase() && (
                                  <code className="text-xs text-texto-secundario bg-fundo-aplicacao px-1.5 py-0.5 rounded border border-borda font-mono">
                                    {permissao.codigo}
                                  </code>
                                )}
                              </div>
                              {permissao.descricao && (
                                <span className="text-sm text-texto-secundario lg:text-right">{permissao.descricao}</span>
                              )}
                            </div>
                          ))}
                        </div>
                      ) : (
                        <p className="text-sm text-texto-secundario italic p-2 border-t border-borda mt-1 pt-3">Nenhuma permissão neste recurso.</p>
                      )}
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-texto-secundario italic p-2">Nenhum recurso neste módulo.</p>
              )}
            </div>
          ))}
        </div>
      )}
    </main>
  );
};
