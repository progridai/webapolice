import React, { useMemo } from 'react';
import { useConsultaApolice } from '../../hooks/useConsultaApolice';

interface EntityNameResolverProps {
  idValue?: string;
  fetcher: (
    id: string,
    signal?: AbortSignal,
  ) => Promise<{ razaoSocial?: string; nome?: string; nomeFantasia?: string }>;
  label?: string;
}

export const EntityNameResolver: React.FC<EntityNameResolverProps> = ({
  idValue,
  fetcher,
  label,
}) => {
  const load = useMemo(() => {
    // Preserva o critério existente de identificador para evitar buscas desnecessárias.
    if (!idValue || idValue.trim() === '' || idValue.length !== 36) return null;
    return (signal: AbortSignal) => fetcher(idValue, signal);
  }, [idValue, fetcher]);
  const { data, isLoading, error } = useConsultaApolice(
    load,
    'Entidade não encontrada',
  );
  if (!load) return null;
  const name =
    data &&
    (data.razaoSocial ||
      data.nomeFantasia ||
      data.nome ||
      'Nome não especificado');

  return (
    <div className="mt-1 text-sm font-medium">
      {isLoading && (
        <span className="text-texto-terciario">
          Buscando {label || 'entidade'}...
        </span>
      )}
      {error && (
        <span className="text-status-erro">
          {label || 'Entidade'} não encontrada
        </span>
      )}
      {name && (
        <span className="text-status-sucesso flex items-center gap-1">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            className="h-4 w-4"
            viewBox="0 0 20 20"
            fill="currentColor"
          >
            <path
              fillRule="evenodd"
              d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
              clipRule="evenodd"
            />
          </svg>
          {name}
        </span>
      )}
    </div>
  );
};
