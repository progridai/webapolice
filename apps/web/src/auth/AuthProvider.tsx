/**
 * AuthProvider.tsx
 *
 * Provider oficial de autenticação via Keycloak.
 *
 * Responsabilidades:
 * - Inicializar o Keycloak com PKCE (S256) de forma segura
 * - Aguardar resolução antes de renderizar rotas privadas
 * - Expor estado de usuário, roles e helpers de autorização
 * - Renovar token proativamente antes da expiração
 * - Encerrar sessão de forma segura em caso de falha de renovação
 * - Não armazenar tokens manualmente (Keycloak gerencia internamente)
 * - Não expor tokens em logs
 */
import React, { useCallback, useEffect, useRef, useState, useMemo } from 'react';
import { AuthContext } from './AuthContext';
import { getKeycloakInstance, initKeycloakOnce } from './keycloak';
import {
  hasAllRoles as hasAllRolesUtil,
  hasAnyRole as hasAnyRoleUtil,
  hasRole as hasRoleUtil,
} from './roles';
import type { AuthContextValue, AuthStatus, AuthUser } from './auth.types';
import { PageLoading } from '../components/application/PageLoading';
import { getOidcRedirectUri, restorePostLoginRedirect } from './authRedirect';
import { setTokenProvider } from '../services/http';

interface AuthProviderProps {
  children: React.ReactNode;
}

/** Extrai o modelo interno de usuário a partir dos dados do token Keycloak */
function extractUser(kc: ReturnType<typeof getKeycloakInstance>): AuthUser | null {
  if (!kc.authenticated || !kc.tokenParsed) return null;

  const token = kc.tokenParsed as Record<string, unknown>;
  const realmRoles = (token['realm_access'] as { roles?: string[] })?.roles ?? [];

  return {
    id: (token['sub'] as string) ?? '',
    username: (token['preferred_username'] as string) ?? '',
    name:
      (token['name'] as string) ??
      [(token['given_name'] as string) ?? '', (token['family_name'] as string) ?? '']
        .filter(Boolean)
        .join(' '),
    email: (token['email'] as string) ?? '',
    roles: realmRoles,
  };
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [status, setStatus] = useState<AuthStatus>('initializing');
  const [user, setUser] = useState<AuthUser | null>(null);
  const refreshingRef = useRef(false);
  const tokenRefreshIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const kc = getKeycloakInstance();

  const isLoading = status === 'initializing';
  const isAuthenticated = status === 'authenticated';
  const roles = useMemo(() => user?.roles ?? [], [user?.roles]);

  const { hasRole, hasAnyRole, hasAllRoles } = useMemo(() => ({
    hasRole: (role: string) => hasRoleUtil(roles, role),
    hasAnyRole: (rolesToCheck: string[]) => hasAnyRoleUtil(roles, rolesToCheck),
    hasAllRoles: (rolesToCheck: string[]) => hasAllRolesUtil(roles, rolesToCheck),
  }), [roles]);

  const refreshToken = useCallback(async (): Promise<boolean> => {
    if (refreshingRef.current) return false;
    refreshingRef.current = true;
    try {
      const refreshed = await kc.updateToken(60);
      if (refreshed) {
        const updatedUser = extractUser(kc);
        setUser(updatedUser);
      }
      return true;
    } catch {
      // Falha de renovação — encerra sessão de forma segura
      setTokenProvider(null);
      setStatus('unauthenticated');
      setUser(null);
      try {
        await kc.logout();
      } catch {
        // Silencia erro de logout para não bloquear o fluxo
      }
      return false;
    } finally {
      refreshingRef.current = false;
    }
  }, [kc]);

  useEffect(() => {
    if (!isAuthenticated) {
      setTokenProvider(null);
      return;
    }

    setTokenProvider(async () => {
      await kc.updateToken(60);
      return kc.token;
    });

    return () => setTokenProvider(null);
  }, [isAuthenticated, kc]);

  useEffect(() => {
    let mounted = true;

    async function initKeycloak() {
      try {
        const authenticated = await initKeycloakOnce({
          onLoad: 'check-sso',
          silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
          pkceMethod: 'S256',
          checkLoginIframe: false,
          redirectUri: getOidcRedirectUri(), // Previne que Keycloak adicione queries no meio do hash (HashRouter)
          responseMode: 'query', // Isola os parâmetros do Keycloak na querystring, deixando o HashRouter intacto
        });

        if (!mounted) return;

        if (authenticated) {
          const extractedUser = extractUser(kc);
          setUser(extractedUser);
          setStatus('authenticated');
          restorePostLoginRedirect();
        } else {
          setStatus('unauthenticated');
        }
      } catch {
        if (!mounted) return;
        setStatus('error');
      }
    }

    initKeycloak();

    return () => {
      mounted = false;
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Renovação proativa do token — verifica a cada 30 segundos
  useEffect(() => {
    if (!isAuthenticated) return;

    tokenRefreshIntervalRef.current = setInterval(() => {
      refreshToken();
    }, 30_000);

    return () => {
      if (tokenRefreshIntervalRef.current) {
        clearInterval(tokenRefreshIntervalRef.current);
      }
    };
  }, [isAuthenticated, refreshToken]);

  const login = useCallback(
    async (redirectUri?: string) => {
      await kc.login({ redirectUri: redirectUri ?? window.location.href });
    },
    [kc]
  );

  const logout = useCallback(
    async (redirectUri?: string) => {
      setTokenProvider(null);
      setUser(null);
      setStatus('unauthenticated');
      await kc.logout({ redirectUri: redirectUri ?? window.location.origin });
    },
    [kc]
  );

  const value: AuthContextValue = {
    status,
    isLoading,
    isAuthenticated,
    user,
    roles,
    login,
    logout,
    refreshToken,
    hasRole,
    hasAnyRole,
    hasAllRoles,
  };

  if (isLoading) {
    // Importante: atrasamos a renderização dos filhos (ex: HashRouter) para que a URL 
    // original seja preservada enquanto o OIDC valida o token.
    return <PageLoading />;
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
