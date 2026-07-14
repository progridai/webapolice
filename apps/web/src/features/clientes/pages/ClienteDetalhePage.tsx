import React, { useEffect, useRef } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { Button, Alert, Skeleton, EmptyState } from '../../../components/ui';
import { useClienteDetalhe } from '../hooks/useClienteDetalhe';
import {
  ClienteResumoCard,
  ClienteDadosPessoaisCard,
  ClienteContatosCard,
  ClienteEnderecosCard,
  ClienteVinculosCard,
  ClienteDependentesCard
} from '../components';

export const ClienteDetalhePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const mainRef = useRef<HTMLElement>(null);
  
  const { data: cliente, isLoading, error, retry } = useClienteDetalhe(id);

  // Foco acessível ao entrar na página
  useEffect(() => {
    if (!isLoading && mainRef.current) {
      mainRef.current.focus();
    }
  }, [isLoading]);

  // Atualizar document title
  useEffect(() => {
    document.title = cliente ? `Detalhes do cliente: ${cliente.nome} | webapolice` : 'Detalhes do cliente | webapolice';
  }, [cliente]);

  const handleVoltar = () => {
    // Retorna para a listagem preservando filtros se existirem no state
    if (location.state?.fromListagem) {
      navigate('/clientes', { state: location.state });
    } else {
      navigate('/clientes');
    }
  };

  if (isLoading) {
    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none" aria-busy="true">
        <div className="mb-6">
          <Skeleton className="w-32 h-10 mb-4" />
          <Skeleton className="w-full h-32" />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Skeleton className="w-full h-48" />
          <Skeleton className="w-full h-48" />
          <Skeleton className="w-full h-48" />
          <Skeleton className="w-full h-48" />
        </div>
      </main>
    );
  }

  if (error) {
    if (error.name === 'HttpApiError') {
      const httpError = error as unknown as { status: number };
      if (httpError.status === 404) {
        return (
          <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
            <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
              &larr; Voltar para clientes
            </Button>
            <EmptyState
              title="Cliente não encontrado"
              description="O cliente que você tentou acessar não existe ou foi excluído."
            />
          </main>
        );
      }
      if (httpError.status === 403) {
        return (
          <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
            <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
              &larr; Voltar para clientes
            </Button>
            <Alert variant="error" title="Acesso negado" role="alert">
              Você não tem permissão para visualizar os detalhes deste cliente.
            </Alert>
          </main>
        );
      }
    }

    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
        <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
          &larr; Voltar para clientes
        </Button>
        <Alert
          variant="error"
          title="Erro ao carregar detalhes"
          role="alert"
          action={
            <Button variant="primary" size="sm" onClick={retry}>
              Tentar novamente
            </Button>
          }
        >
          {error.message || 'Não foi possível carregar os dados do cliente.'}
        </Alert>
      </main>
    );
  }

  if (!cliente) return null;

  return (
    <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none flex flex-col gap-6">
      <nav aria-label="Breadcrumb">
        <ol className="flex items-center space-x-2 text-sm text-slate-500 dark:text-slate-400">
          <li>
            <button onClick={handleVoltar} className="hover:text-slate-900 dark:hover:text-slate-100 underline decoration-slate-300 underline-offset-4">
              Clientes
            </button>
          </li>
          <li>/</li>
          <li className="text-slate-900 dark:text-slate-100 font-medium" aria-current="page">Detalhes</li>
        </ol>
      </nav>

      <div>
        <Button variant="ghost" onClick={handleVoltar} aria-label="Voltar para listagem">
          &larr; Voltar
        </Button>
      </div>

      <ClienteResumoCard cliente={cliente} />

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <div className="flex flex-col gap-6">
          <ClienteDadosPessoaisCard cliente={cliente} />
          <ClienteContatosCard contatos={cliente.contatos} />
          <ClienteEnderecosCard enderecos={cliente.enderecos} />
        </div>
        <div className="flex flex-col gap-6">
          <ClienteVinculosCard vinculos={cliente.vinculos} />
          <ClienteDependentesCard dependentes={cliente.dependentes} />
        </div>
      </div>
    </main>
  );
};
