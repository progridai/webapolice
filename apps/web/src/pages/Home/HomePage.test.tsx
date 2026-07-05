import { render, screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { HomePage } from './HomePage';
import { AuthContext } from '../../auth/AuthContext';
import { APP_ROLES } from '../../auth/roles';
import type { AuthContextValue } from '../../auth/auth.types';

const navigateMock = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

vi.mock('../../app/config/env', () => ({
  ENV: {
    API_BASE_URL: 'http://127.0.0.1:5007',
    KEYCLOAK_URL: 'http://127.0.0.1:8080',
    KEYCLOAK_REALM: 'webapolice',
    KEYCLOAK_CLIENT_ID: 'webapolice-web',
    ENABLE_DESIGN_SYSTEM: true,
    MODE: 'test',
    IS_DEV: false,
    APP_VERSION: '0.1.0',
  },
}));

function authValue(roles: string[]): AuthContextValue {
  return {
    status: 'authenticated',
    isLoading: false,
    isAuthenticated: true,
    user: {
      id: 'user-1',
      username: 'dev.admin',
      name: 'Dev Admin',
      email: 'dev.admin@local.test',
      roles,
    },
    roles,
    login: vi.fn(),
    logout: vi.fn(),
    refreshToken: vi.fn().mockResolvedValue(true),
    hasRole: (role: string) => roles.includes(role),
    hasAnyRole: (rolesToCheck: string[]) => rolesToCheck.some((role) => roles.includes(role)),
    hasAllRoles: (rolesToCheck: string[]) => rolesToCheck.every((role) => roles.includes(role)),
  };
}

function renderHome(roles: string[]) {
  return render(
    <MemoryRouter>
      <AuthContext.Provider value={authValue(roles)}>
        <HomePage />
      </AuthContext.Provider>
    </MemoryRouter>
  );
}

describe('HomePage', () => {
  it('shows Clientes as available and navigates to the centralized route', () => {
    navigateMock.mockClear();
    renderHome([APP_ROLES.ADMIN]);

    expect(screen.getByText('Clientes')).not.toBeNull();
    expect(screen.queryByText(/em breve/i)).toBeNull();

    fireEvent.click(screen.getByRole('button', { name: /acessar clientes/i }));

    expect(navigateMock).toHaveBeenCalledWith('/clientes');
  });

  it('does not show Clientes to users without the required role', () => {
    renderHome([]);

    expect(screen.queryByText('Clientes')).toBeNull();
  });
});
