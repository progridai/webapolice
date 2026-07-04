/**
 * tests/layouts/AuthenticatedLayout.test.tsx
 *
 * Testa o layout autenticado: navegação, usuário, logout, mobile e teclado.
 */
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { AuthContext } from '../../src/auth/AuthContext';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';
import { AuthenticatedLayout } from '../../src/layouts/AuthenticatedLayout/AuthenticatedLayout';
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

const mockLogout = vi.fn();

const authenticatedAuth: AuthContextValue = {
  status: 'authenticated',
  isLoading: false,
  isAuthenticated: true,
  user: {
    id: 'user-1',
    username: 'dev.admin',
    name: 'Dev Admin',
    email: 'dev.admin@local.test',
    roles: [APP_ROLES.ADMIN],
  },
  roles: [APP_ROLES.ADMIN],
  login: vi.fn(),
  logout: mockLogout,
  refreshToken: vi.fn().mockResolvedValue(true),
  hasRole: (role: string) => [APP_ROLES.ADMIN].includes(role),
  hasAnyRole: (roles: string[]) => roles.some((r) => [APP_ROLES.ADMIN].includes(r)),
  hasAllRoles: (roles: string[]) => roles.every((r) => [APP_ROLES.ADMIN].includes(r)),
};

const renderLayout = () =>
  render(
    <ThemeProvider>
      <MemoryRouter initialEntries={['/app']}>
        <AuthContext.Provider value={authenticatedAuth}>
          <AuthenticatedLayout />
        </AuthContext.Provider>
      </MemoryRouter>
    </ThemeProvider>
  );

describe('AuthenticatedLayout', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false, media: query, onchange: null,
      addListener: vi.fn(), removeListener: vi.fn(),
      addEventListener: vi.fn(), removeEventListener: vi.fn(), dispatchEvent: vi.fn(),
    })));
  });

  it('deve renderizar a navegação principal', () => {
    renderLayout();
    expect(screen.getByRole('navigation', { name: /navegação principal/i })).not.toBeNull();
    expect(screen.getByText('Início')).not.toBeNull();
  });

  it('deve renderizar o nome do usuário autenticado', () => {
    renderLayout();
    expect(screen.getByText('Dev Admin')).not.toBeNull();
  });

  it('deve chamar logout ao clicar em "Sair da conta"', async () => {
    renderLayout();

    // Abre o menu do usuário
    const menuBtn = screen.getByRole('button', { name: /menu do usuário/i });
    fireEvent.click(menuBtn);

    // Clica em "Sair da conta"
    const logoutBtn = await screen.findByRole('menuitem', { name: /sair da conta/i });
    fireEvent.click(logoutBtn);

    await waitFor(() => expect(mockLogout).toHaveBeenCalledTimes(1));
  });

  it('deve abrir e fechar o menu mobile', () => {
    renderLayout();

    // Inicialmente o painel mobile não deve estar visível
    expect(screen.queryByRole('dialog')).toBeNull();

    // Abre o menu (botão hambúrguer no header)
    const menuBtn = screen.getByRole('button', { name: /abrir menu de navegação/i });
    fireEvent.click(menuBtn);

    expect(screen.getByRole('dialog')).not.toBeNull();

    // Fecha com o botão de fechar que está dentro do dialog
    const dialog = screen.getByRole('dialog');
    const closeBtn = dialog.querySelector('button[aria-label="Fechar menu de navegação"]');
    expect(closeBtn).not.toBeNull();
    fireEvent.click(closeBtn!);

    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it('deve fechar o menu mobile ao pressionar Escape', () => {
    renderLayout();

    const menuBtn = screen.getByRole('button', { name: /abrir menu de navegação/i });
    fireEvent.click(menuBtn);
    const dialog = screen.getByRole('dialog');
    expect(dialog).not.toBeNull();

    fireEvent.keyDown(dialog, { key: 'Escape' });
    expect(screen.queryByRole('dialog')).toBeNull();
  });
});
