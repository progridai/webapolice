/**
 * tests/auth/AuthProvider.test.tsx
 *
 * Testa o AuthProvider e o hook useAuth.
 * O Keycloak é completamente mockado — nenhum servidor necessário.
 */
import { render, screen, act, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { AuthProvider } from '../../src/auth/AuthProvider';
import { useAuth } from '../../src/auth/useAuth';
import { APP_ROLES } from '../../src/auth/roles';

// ── Mock do módulo keycloak ──────────────────────────────────────────────
let mockKeycloakInit: ReturnType<typeof vi.fn>;
let mockKeycloakLogin: ReturnType<typeof vi.fn>;
let mockKeycloakLogout: ReturnType<typeof vi.fn>;
let mockKeycloakUpdateToken: ReturnType<typeof vi.fn>;
let mockAuthenticated = false;
let mockTokenParsed: Record<string, unknown> | null = null;
const mockSetTokenProvider = vi.hoisted(() => vi.fn());

vi.mock('../../src/auth/keycloak', () => ({
  getKeycloakInstance: () => ({
    get authenticated() { return mockAuthenticated; },
    get tokenParsed() { return mockTokenParsed; },
    init: (...args: unknown[]) => mockKeycloakInit(...args),
    login: (...args: unknown[]) => mockKeycloakLogin(...args),
    logout: (...args: unknown[]) => mockKeycloakLogout(...args),
    updateToken: (...args: unknown[]) => mockKeycloakUpdateToken(...args),
    token: mockAuthenticated ? 'mock-token' : undefined,
  }),
  initKeycloakOnce: (...args: unknown[]) => mockKeycloakInit(...args),
  _resetKeycloakInstance: vi.fn(),
}));

vi.mock('../../src/services/http', () => ({
  setTokenProvider: mockSetTokenProvider,
}));

// Mock das variáveis de ambiente
vi.mock('../../src/app/config/env', () => ({
  ENV: {
    API_BASE_URL: 'http://localhost:5000',
    KEYCLOAK_URL: 'http://localhost:8080',
    KEYCLOAK_REALM: 'webapolice',
    KEYCLOAK_CLIENT_ID: 'webapolice-web',
    ENABLE_DESIGN_SYSTEM: true,
    MODE: 'test',
    IS_DEV: false,
    APP_VERSION: '0.1.0',
  },
}));

// ── Componente auxiliar para testar o hook ────────────────────────────────
const AuthConsumer: React.FC = () => {
  const auth = useAuth();
  return (
    <div>
      <span data-testid="status">{auth.status}</span>
      <span data-testid="isAuthenticated">{String(auth.isAuthenticated)}</span>
      <span data-testid="isLoading">{String(auth.isLoading)}</span>
      <span data-testid="user">{auth.user ? auth.user.username : 'null'}</span>
      <span data-testid="roles">{auth.roles.join(',')}</span>
      <button onClick={() => auth.login()} data-testid="btn-login">Login</button>
      <button onClick={() => auth.logout()} data-testid="btn-logout">Logout</button>
    </div>
  );
};

const renderWithAuth = () =>
  render(
    <AuthProvider>
      <AuthConsumer />
    </AuthProvider>
  );

describe('AuthProvider', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockAuthenticated = false;
    mockTokenParsed = null;
    mockKeycloakInit = vi.fn().mockResolvedValue(false);
    mockKeycloakLogin = vi.fn().mockResolvedValue(undefined);
    mockKeycloakLogout = vi.fn().mockResolvedValue(undefined);
    mockKeycloakUpdateToken = vi.fn().mockResolvedValue(false);

    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false, media: query, onchange: null,
      addListener: vi.fn(), removeListener: vi.fn(),
      addEventListener: vi.fn(), removeEventListener: vi.fn(), dispatchEvent: vi.fn(),
    })));
  });

  it('deve iniciar no estado "initializing"', async () => {
    // Init nunca resolve para testar o estado transitório
    mockKeycloakInit = vi.fn().mockImplementation(() => new Promise((resolve) => setTimeout(() => resolve(false), 5000)));
    renderWithAuth();
    expect(screen.getByRole('status', { name: 'Carregando página' })).not.toBeNull();
  });

  it('deve ir para "unauthenticated" quando Keycloak retorna false', async () => {
    mockKeycloakInit = vi.fn().mockResolvedValue(false);
    renderWithAuth();
    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('unauthenticated')
    );
    expect(screen.getByTestId('isAuthenticated').textContent).toBe('false');
    expect(screen.getByTestId('user').textContent).toBe('null');
  });

  it('deve ir para "authenticated" com usuário populado quando Keycloak autentica', async () => {
    mockAuthenticated = true;
    mockTokenParsed = {
      sub: 'user-123',
      preferred_username: 'dev.admin',
      name: 'Dev Admin',
      email: 'dev.admin@local.test',
      realm_access: { roles: [APP_ROLES.ADMIN] },
    };
    mockKeycloakInit = vi.fn().mockResolvedValue(true);

    renderWithAuth();
    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('authenticated')
    );
    expect(screen.getByTestId('isAuthenticated').textContent).toBe('true');
    expect(screen.getByTestId('user').textContent).toBe('dev.admin');
    expect(screen.getByTestId('roles').textContent).toBe(APP_ROLES.ADMIN);
    expect(mockSetTokenProvider).toHaveBeenLastCalledWith(expect.any(Function));
  });

  it('deve expor roles do usuário autenticado', async () => {
    mockAuthenticated = true;
    mockTokenParsed = {
      sub: 'user-456',
      preferred_username: 'gestor',
      name: 'Gestor Teste',
      email: 'gestor@local.test',
      realm_access: { roles: [APP_ROLES.GESTOR, APP_ROLES.OPERADOR] },
    };
    mockKeycloakInit = vi.fn().mockResolvedValue(true);

    renderWithAuth();
    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('authenticated')
    );
    expect(screen.getByTestId('roles').textContent).toBe(`${APP_ROLES.GESTOR},${APP_ROLES.OPERADOR}`);
  });

  it('deve chamar kc.login ao invocar login()', async () => {
    mockKeycloakInit = vi.fn().mockResolvedValue(false);
    renderWithAuth();
    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('unauthenticated')
    );

    await act(async () => {
      screen.getByTestId('btn-login').click();
    });
    expect(mockKeycloakLogin).toHaveBeenCalledTimes(1);
  });

  it('deve chamar kc.logout ao invocar logout()', async () => {
    mockAuthenticated = true;
    mockTokenParsed = {
      sub: 'user-123', preferred_username: 'dev.admin',
      name: 'Dev Admin', email: '', realm_access: { roles: [] },
    };
    mockKeycloakInit = vi.fn().mockResolvedValue(true);
    renderWithAuth();
    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('authenticated')
    );

    await act(async () => {
      screen.getByTestId('btn-logout').click();
    });
    expect(mockKeycloakLogout).toHaveBeenCalledTimes(1);
  });

  it('deve ir para "error" quando Keycloak falha na inicialização', async () => {
    mockKeycloakInit = vi.fn().mockRejectedValue(new Error('Keycloak connection refused'));
    renderWithAuth();
    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('error')
    );
  });

  it('deve retornar false quando refreshToken falha e encerrar sessão', async () => {
    mockAuthenticated = true;
    mockTokenParsed = {
      sub: 'user-123', preferred_username: 'dev.admin',
      name: 'Dev Admin', email: '', realm_access: { roles: [APP_ROLES.ADMIN] },
    };
    mockKeycloakInit = vi.fn().mockResolvedValue(true);
    mockKeycloakUpdateToken = vi.fn().mockRejectedValue(new Error('Token expired'));

    const RefreshConsumer: React.FC = () => {
      const { refreshToken, status } = useAuth();
      return (
        <div>
          <span data-testid="status">{status}</span>
          <button data-testid="btn-refresh" onClick={() => refreshToken()}>Refresh</button>
        </div>
      );
    };

    render(<AuthProvider><RefreshConsumer /></AuthProvider>);
    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('authenticated')
    );

    await act(async () => {
      screen.getByTestId('btn-refresh').click();
    });

    await waitFor(() =>
      expect(screen.getByTestId('status').textContent).toBe('unauthenticated')
    );
  });
});
