import React, { useEffect, useState } from 'react';

interface EntityNameResolverProps {
  idValue?: string;
  fetcher: (id: string) => Promise<{ razaoSocial?: string; nome?: string; nomeFantasia?: string }>;
  label?: string;
}

export const EntityNameResolver: React.FC<EntityNameResolverProps> = ({ idValue, fetcher, label }) => {
  const [name, setName] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(false);

  useEffect(() => {
    // Only fetch if it looks like a UUID (36 chars) to avoid useless requests
    if (typeof idValue !== 'string' || idValue.trim() === '' || idValue.length !== 36) {
      setName(null);
      setError(false);
      return;
    }

    let isMounted = true;
    setLoading(true);
    setError(false);

    fetcher(idValue)
      .then((data) => {
        if (isMounted) {
          setName(data.razaoSocial || data.nomeFantasia || data.nome || 'Nome não especificado');
        }
      })
      .catch(() => {
        if (isMounted) {
          setError(true);
          setName(null);
        }
      })
      .finally(() => {
        if (isMounted) {
          setLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [idValue, fetcher]);

  if (typeof idValue !== 'string' || idValue.trim() === '' || idValue.length !== 36) return null;

  return (
    <div className="mt-1 text-sm font-medium">
      {loading && <span className="text-texto-terciario">Buscando {label || 'entidade'}...</span>}
      {error && <span className="text-status-erro">{label || 'Entidade'} não encontrada</span>}
      {name && (
        <span className="text-status-sucesso flex items-center gap-1">
          <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
            <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
          </svg>
          {name}
        </span>
      )}
    </div>
  );
};
