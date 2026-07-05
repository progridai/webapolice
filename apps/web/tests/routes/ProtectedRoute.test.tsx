import { render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from '../../src/app/routes/ProtectedRoute';
import { AuthContext } from '../../src/auth/AuthContext';
import type { AuthContextValue } from '../../src/auth/auth.types';

function mockAuthValue(overrides: Partial<AuthContextValue>): AuthContextValue {
  return {
    status: 'unauthenticated',
    isLoading: false,
    isAuthenticated: false,
    user: null,
    roles: [],
    login: vi.fn(),
    logout: vi.fn(),
    refreshToken: vi.fn().mockResolvedValue(true),
    hasRole: vi.fn().mockReturnValue(false),
    hasAnyRole: vi.fn().mockReturnValue(false),
    hasAllRoles: vi.fn().mockReturnValue(false),
    ...overrides,
  };
}

function renderWithRouter(authValue: AuthContextValue) {
  return render(
    <MemoryRouter initialEntries={['/protected']}>
      <AuthContext.Provider value={authValue}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/protected" element={<div>Conteudo Protegido</div>} />
          </Route>
          <Route path="/unauthorized" element={<div>Pagina Nao Autorizada</div>} />
        </Routes>
      </AuthContext.Provider>
    </MemoryRouter>
  );
}

describe('ProtectedRoute', () => {
  it('renders protected content when authenticated', async () => {
    renderWithRouter(mockAuthValue({ status: 'authenticated', isAuthenticated: true }));

    await waitFor(() => expect(screen.getByText('Conteudo Protegido')).not.toBeNull());
  });

  it('redirects to unauthorized when unauthenticated', async () => {
    renderWithRouter(mockAuthValue({ status: 'unauthenticated', isAuthenticated: false }));

    await waitFor(() => expect(screen.getByText('Pagina Nao Autorizada')).not.toBeNull());
  });

  it('renders loading while authentication is being resolved', () => {
    renderWithRouter(mockAuthValue({ status: 'initializing', isLoading: true }));

    expect(screen.queryByText('Conteudo Protegido')).toBeNull();
    expect(screen.queryByText('Pagina Nao Autorizada')).toBeNull();
    expect(screen.getAllByRole('status', { name: /Carregando/ }).length).toBeGreaterThan(0);
  });
});
