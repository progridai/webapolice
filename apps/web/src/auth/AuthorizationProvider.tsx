import { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useAuth } from './useAuth';
import { httpClient } from '../services/http/httpClient';

export interface UsuarioAutenticadoResponse {
  id: string;
  usuario: string;
  usuarioEncontrado: boolean;
  usuarioAtivo: boolean;
  acessoTotal: boolean;
  operadorSistema: boolean;
  modulosHabilitados: string[];
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
  permissoes: string[];
  possuiModulo: (codigo: string) => boolean;
  possuiPermissao: (codigo: string) => boolean;
  possuiAcessoTotal: () => boolean;
  ehOperadorSistema: () => boolean;
}

const AuthorizationContext = createContext<AuthorizationContextData | undefined>(undefined);

export function AuthorizationProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const [data, setData] = useState<UsuarioAutenticadoResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    async function loadData() {
      if (!isAuthenticated) {
        setIsLoading(false);
        return;
      }

      try {
        console.log('[AuthorizationProvider] Iniciando requisição para /api/seguranca/me');
        const res = await httpClient.get<UsuarioAutenticadoResponse>('/api/seguranca/me');
        console.log('[AuthorizationProvider] Resposta recebida:', res);
        setData(res.data);
      } catch (err) {
        console.error('[AuthorizationProvider] Erro ao buscar /api/seguranca/me:', err);
        setError(err instanceof Error ? err : new Error(String(err)));
      } finally {
        setIsLoading(false);
      }
    }

    loadData();
  }, [isAuthenticated]);



  const value: AuthorizationContextData = {
    isLoading,
    error: error !== null,
    usuarioEncontrado: data?.usuarioEncontrado ?? false,
    usuarioAtivo: data?.usuarioAtivo ?? false,
    acessoTotal: data?.acessoTotal ?? false,
    operadorSistema: data?.operadorSistema ?? false,
    modulosHabilitados: data?.modulosHabilitados ?? [],
    permissoes: data?.permissoes ?? [],
    possuiModulo: (codigo: string) => data?.modulosHabilitados.includes(codigo) ?? false,
    possuiPermissao: (codigo: string) => data?.permissoes.includes(codigo) ?? false,
    possuiAcessoTotal: () => data?.acessoTotal ?? false,
    ehOperadorSistema: () => data?.operadorSistema ?? false,
  };

  if (error) {
    return (
      <div style={{ padding: '2rem', backgroundColor: '#fee2e2', color: '#991b1b', border: '1px solid #f87171', borderRadius: '4px', margin: '2rem' }}>
        <h2>Erro de Autorização</h2>
        <p>Não foi possível carregar as permissões do usuário a partir da API (/api/seguranca/me).</p>
        <p>Por favor, abra o Console do Navegador (F12) para ver detalhes do erro.</p>
        <pre style={{ background: '#fff', padding: '1rem', marginTop: '1rem', borderRadius: '4px', overflow: 'auto' }}>
          {error instanceof Error ? error.message : JSON.stringify(error, null, 2)}
        </pre>
        <button onClick={() => window.location.reload()} style={{ marginTop: '1rem', padding: '0.5rem 1rem', background: '#991b1b', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>Recarregar Página</button>
      </div>
    );
  }

  return (
    <AuthorizationContext.Provider value={value}>
      {children}
    </AuthorizationContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuthorization() {
  const context = useContext(AuthorizationContext);
  if (context === undefined) {
    throw new Error('useAuthorization must be used within an AuthorizationProvider');
  }
  return context;
}
