/**
 * tests/routes/RoleProtectedRoute.test.tsx
 *
 * Testa o componente RoleProtectedRoute.
 */
import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { RoleProtectedRoute } from '../../src/app/routes/RoleProtectedRoute';
import { AuthContext } from '../../src/auth/AuthContext';
import type { AuthContextValue } from '../../src/auth/auth.types';
import { APP_ROLES } from '../../src/auth/roles';

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

const makeAuthValue = (roles: string[]): AuthContextValue => ({
  status: 'authenticated',
  isLoading: false,
  isAuthenticated: true,
  user: { id: '1', username: 'test', name: 'Test', email: 'test@test.com', roles },
  roles,
  login: vi.fn(),
  logout: vi.fn(),
  refreshToken: vi.fn().mockResolvedValue(true),
  hasRole: (role: string) => roles.includes(role),
  hasAnyRole: (r: string[]) => r.some((role) => roles.includes(role)),
  hasAllRoles: (r: string[]) => r.every((role) => roles.includes(role)),
});

const renderWithRole = (userRoles: string[], allowedRoles: string[]) =>
  render(
    <MemoryRouter initialEntries={['/admin']}>
      <AuthContext.Provider value={makeAuthValue(userRoles)}>
        <Routes>
          <Route
            element={<RoleProtectedRoute allowedRoles={allowedRoles} />}
          >
            <Route path="/admin" element={<div>Área Admin</div>} />
          </Route>
          <Route path="/forbidden" element={<div>Acesso Negado</div>} />
          <Route path="/unauthorized" element={<div>Não Autorizado</div>} />
        </Routes>
      </AuthContext.Provider>
    </MemoryRouter>
  );

describe('RoleProtectedRoute', () => {
  beforeEach(() => {
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false, media: query, onchange: null,
      addListener: vi.fn(), removeListener: vi.fn(),
      addEventListener: vi.fn(), removeEventListener: vi.fn(), dispatchEvent: vi.fn(),
    })));
  });

  it('deve permitir acesso quando o usuário possui a role requerida', async () => {
    renderWithRole([APP_ROLES.ADMIN], [APP_ROLES.ADMIN]);

    await waitFor(() =>
      expect(screen.getByText('Área Admin')).not.toBeNull()
    );
  });

  it('deve redirecionar para /forbidden quando role não é satisfeita', async () => {
    renderWithRole([APP_ROLES.OPERADOR], [APP_ROLES.ADMIN]);

    await waitFor(() =>
      expect(screen.getByText('Acesso Negado')).not.toBeNull()
    );
  });

  it('deve permitir acesso quando possui ao menos uma das múltiplas roles permitidas', async () => {
    renderWithRole([APP_ROLES.GESTOR], [APP_ROLES.ADMIN, APP_ROLES.GESTOR]);

    await waitFor(() =>
      expect(screen.getByText('Área Admin')).not.toBeNull()
    );
  });

  it('deve redirecionar para /forbidden quando não possui nenhuma das roles permitidas', async () => {
    renderWithRole([APP_ROLES.OPERADOR], [APP_ROLES.ADMIN, APP_ROLES.GESTOR]);

    await waitFor(() =>
      expect(screen.getByText('Acesso Negado')).not.toBeNull()
    );
  });
});
