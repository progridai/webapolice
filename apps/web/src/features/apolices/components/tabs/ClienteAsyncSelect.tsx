import React from 'react';
import { EntitySearchField } from '../forms/AsyncEntityField';
import { listarClientes } from '../../../clientes/api/clientesApi';

interface ClienteAsyncSelectProps extends Pick<
  React.ComponentProps<typeof EntitySearchField>,
  'id' | 'aria-describedby' | 'aria-invalid' | 'onBlur'
> {
  value: string | null;
  onChange: (clientePublicId: string) => void;
  error?: boolean;
  disabled?: boolean;
  initialNome?: string;
}

async function buscarClientes(term: string, signal?: AbortSignal) {
  const response = await listarClientes(
    {
      page: 1,
      pageSize: 50,
      nome: term || undefined,
      status: 1,
    },
    signal,
  );
  return (response.itens || []).map((cliente) => ({
    value: cliente.id,
    label: cliente.nome,
  }));
}

export const ClienteAsyncSelect: React.FC<ClienteAsyncSelectProps> = ({
  value,
  initialNome,
  ...props
}) => (
  <EntitySearchField
    {...props}
    value={value || ''}
    initialLabel={initialNome}
    placeholder="Buscar por nome..."
    searcher={buscarClientes}
    searchDelay={500}
    clearSearchOnFocus={false}
    emptyMessage="Nenhum cliente ativo encontrado."
  />
);
