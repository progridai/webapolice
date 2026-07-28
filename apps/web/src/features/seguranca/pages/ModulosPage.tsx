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
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '16px' }}>
            {modulos.map((modulo) => {
              const isSeguranca = modulo.codigo === 'SEGURANCA';
              return (
                <div key={modulo.codigo} style={{ display: 'flex', alignItems: 'center', gap: '16px', padding: '16px', border: '1px solid var(--cor-borda)', borderRadius: '8px', backgroundColor: 'var(--cor-fundo-superficie)' }}>
                  <input
                    type="checkbox"
                    checked={modulo.habilitado}
                    onChange={() => handleToggleModulo(modulo.publicId, modulo.habilitado)}
                    disabled={isSeguranca}
                    aria-label={`Habilitar módulo ${modulo.nome}`}
                    style={{ width: '20px', height: '20px', cursor: isSeguranca ? 'not-allowed' : 'pointer' }}
                  />
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{ fontWeight: '600', fontSize: '1.125rem', color: 'var(--cor-texto-principal)' }}>
                      {modulo.nome}
                    </span>
                    {isSeguranca && <Badge variant="primary">Essencial</Badge>}
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
