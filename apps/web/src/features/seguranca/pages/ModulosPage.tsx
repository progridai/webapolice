import React, { useEffect } from 'react';
import { useModulos } from '../hooks/useModulos';
import { PageHeader, Breadcrumbs, Alert, Skeleton, Badge } from '../../../components/ui';
import './Seguranca.css';

export const ModulosPage: React.FC = () => {
  const { modulos, isLoading, error, toggleError, handleToggleModulo, recarregar } = useModulos();

  useEffect(() => {
    document.title = 'Módulos do Sistema | WebApolice';
  }, []);

  return (
    <main className="seguranca-page" tabIndex={-1}>
      <PageHeader
        title="Módulos do Sistema"
        description="Habilite ou desabilite os módulos do sistema. Esta configuração afeta todos os usuários."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Módulos' },
            ]}
          />
        }
      />

      {toggleError && (
        <Alert variant="error" title="Erro ao alterar módulo">
          {toggleError}
        </Alert>
      )}

      {error ? (
        <div className="seguranca-error">
          <Alert variant="error" title="Não foi possível carregar os módulos">
            {error}
          </Alert>
          <button onClick={recarregar} className="btn-secondary mt-4">
            Tentar novamente
          </button>
        </div>
      ) : isLoading && modulos.length === 0 ? (
        <div aria-busy="true" aria-live="polite" className="seguranca-skeletons">
          <Skeleton className="seguranca-skeleton-row h-20" />
          <Skeleton className="seguranca-skeleton-row h-20" />
          <Skeleton className="seguranca-skeleton-row h-20" />
        </div>
      ) : (
        <div className="seguranca-content">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {modulos.map((modulo) => {
              const isSeguranca = modulo.codigo === 'SEGURANCA';
              return (
                <div key={modulo.codigo} className="flex items-center justify-between p-4 bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700">
                  <div className="flex flex-col">
                    <span className="font-semibold text-slate-900 dark:text-white flex items-center gap-2">
                      {modulo.nome}
                      {isSeguranca && <Badge variant="primary">Essencial</Badge>}
                    </span>
                  </div>
                  <div className="ml-4 flex items-center">
                    <input
                      type="checkbox"
                      checked={modulo.habilitado}
                      onChange={() => handleToggleModulo(modulo.publicId, modulo.habilitado)}
                      disabled={isSeguranca}
                      aria-label={`Habilitar módulo ${modulo.nome}`}
                      className="w-5 h-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </main>
  );
};
