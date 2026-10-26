import { createContext, useContext, useMemo, type ReactNode } from 'react';
import { useAuth } from './useAuth';
import { httpClient } from '../services/http/httpClient';
import { useConsulta } from '../shared/hooks/useConsulta';
import { Alert, Button } from '../components/ui';

export interface UsuarioAutenticadoResponse {
  id: string;
  usuario: string;
  usuarioEncontrado: boolean;
  usuarioAtivo: boolean;
  acessoTotal: boolean;
  operadorSistema: boolean;
  modulosHabilitados: string[];
  recursosHabilitados: string[];
  permissoes: string[];
}

interface AuthorizationContextData {
  isLoading: boolean;
  error: boolean;
  usuarioEncontrado: boolean;
  usuarioAtivo: boolean;
  acessoTotal: boolean;
  operadorSistema: boolean;
  modulosHabilitados: string[];
  recursosHabilitados: string[];
  permissoes: string[];
  possuiModulo: (codigo: string) => boolean;
  possuiRecurso: (codigo: string) => boolean;
  possuiPermissao: (codigo: string) => boolean;
  possuiAcessoTotal: () => boolean;
  ehOperadorSistema: () => boolean;
  refresh: () => Promise<void>;
}

const AuthorizationContext = createContext<
  AuthorizationContextData | undefined
>(undefined);

export function AuthorizationProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading: authLoading, user } = useAuth();
  const session = useMemo(
    () => (isAuthenticated && !authLoading ? { userId: user?.id } : null),
    [isAuthenticated, authLoading, user?.id],
  );
  const load = useMemo(() => {
    if (!session) return null;
    return async (signal: AbortSignal) => {
      try {
        const response = await httpClient.get<UsuarioAutenticadoResponse>(
          '/api/seguranca/me',
          { signal },
        );
        return response.data;
      } catch (cause: unknown) {
        // Falha/cancelamento da operação atual não revalida permissões anteriores.
        // Operações abortadas pelo ciclo da consulta são descartadas pelo sinal.
        throw new Error('Não foi possível carregar as permissões do usuário.', {
          cause,
        });
      }
    };
  }, [session]);
  const consultation = useConsulta(
    load,
    'Não foi possível carregar as permissões do usuário.',
  );
  const isLoading = authLoading || consultation.isLoading;
  const error = consultation.error;
  // Só o resultado concluído da sessão atual pode alimentar as decisões de acesso.
  const data = session && !isLoading && !error ? consultation.data : null;

  const value: AuthorizationContextData = {
    isLoading,
    error: error !== null,
    usuarioEncontrado: data?.usuarioEncontrado ?? false,
    usuarioAtivo: data?.usuarioAtivo ?? false,
    acessoTotal: data?.acessoTotal ?? false,
    operadorSistema: data?.operadorSistema ?? false,
    modulosHabilitados: data?.modulosHabilitados ?? [],
    recursosHabilitados: data?.recursosHabilitados ?? [],
    permissoes: data?.permissoes ?? [],
    possuiModulo: (codigo: string) =>
      data?.modulosHabilitados.includes(codigo) ?? false,
    possuiRecurso: (codigo: string) =>
      data?.recursosHabilitados.includes(codigo) ?? false,
    possuiPermissao: (codigo: string) =>
      data?.acessoTotal === true ||
      (data?.permissoes.includes(codigo) ?? false),
    possuiAcessoTotal: () => data?.acessoTotal ?? false,
    ehOperadorSistema: () => data?.operadorSistema ?? false,
    refresh: consultation.refetch,
  };

  return (
    <AuthorizationContext.Provider value={value}>
      {error ? (
        <section
          className="p-4 md:p-6 max-w-2xl mx-auto"
          aria-label="Erro de autorização"
        >
          <Alert variant="error" title="Erro de Autorização">
            Não foi possível carregar as permissões do usuário. Tente novamente.
            <div className="mt-4">
              <Button
                variant="secondary"
                onClick={() => void consultation.refetch()}
              >
                Tentar novamente
              </Button>
            </div>
          </Alert>
        </section>
      ) : (
        children
      )}
    </AuthorizationContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuthorization() {
  const context = useContext(AuthorizationContext);
  if (context === undefined) {
    throw new Error(
      'useAuthorization must be used within an AuthorizationProvider',
    );
  }
  return context;
}
