/**
 * tests/pages/ErrorPages.test.tsx
 *
 * Testa as páginas de erro: conteúdo, ações e acessibilidade.
 */
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { AuthContext } from '../../src/auth/AuthContext';
import type { AuthContextValue } from '../../src/auth/auth.types';
import { UnauthorizedPage } from '../../src/pages/Unauthorized/UnauthorizedPage';
import { ForbiddenPage } from '../../src/pages/Forbidden/ForbiddenPage';
import { NotFoundPage } from '../../src/pages/NotFound/NotFoundPage';
import { UnexpectedErrorPage } from '../../src/pages/UnexpectedError/UnexpectedErrorPage';

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

const mockAuth: AuthContextValue = {
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
};

const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <MemoryRouter>
    <AuthContext.Provider value={mockAuth}>
      {children}
    </AuthContext.Provider>
  </MemoryRouter>
);

describe('Páginas de Erro', () => {
  beforeEach(() => {
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false, media: query, onchange: null,
      addListener: vi.fn(), removeListener: vi.fn(),
      addEventListener: vi.fn(), removeEventListener: vi.fn(), dispatchEvent: vi.fn(),
    })));
  });

  describe('UnauthorizedPage', () => {
    it('deve exibir mensagem de sessão necessária', () => {
      render(<UnauthorizedPage />, { wrapper: Wrapper });
      expect(screen.getByText(/sessão necessária/i)).not.toBeNull();
    });

    it('deve exibir botão "Entrar"', () => {
      render(<UnauthorizedPage />, { wrapper: Wrapper });
      expect(screen.getByRole('button', { name: /entrar/i })).not.toBeNull();
    });

    it('deve ter role="main" para acessibilidade', () => {
      render(<UnauthorizedPage />, { wrapper: Wrapper });
      expect(screen.getByRole('main')).not.toBeNull();
    });
  });

  describe('ForbiddenPage', () => {
    it('deve exibir mensagem de acesso negado', () => {
      render(<ForbiddenPage />, { wrapper: Wrapper });
      expect(screen.getByText(/acesso negado/i)).not.toBeNull();
    });

    it('deve exibir botão "Voltar ao início"', () => {
      render(<ForbiddenPage />, { wrapper: Wrapper });
      expect(screen.getByRole('button', { name: /voltar ao início/i })).not.toBeNull();
    });

    it('deve exibir botão "Sair da conta"', () => {
      render(<ForbiddenPage />, { wrapper: Wrapper });
      expect(screen.getByRole('button', { name: /sair da conta/i })).not.toBeNull();
    });
  });

  describe('NotFoundPage', () => {
    it('deve exibir mensagem de página não encontrada', () => {
      render(<NotFoundPage />, { wrapper: Wrapper });
      expect(screen.getByText(/página não encontrada/i)).not.toBeNull();
    });

    it('deve exibir botão "Voltar ao início"', () => {
      render(<NotFoundPage />, { wrapper: Wrapper });
      expect(screen.getByRole('button', { name: /voltar ao início/i })).not.toBeNull();
    });

    it('deve atualizar o document.title', () => {
      render(<NotFoundPage />, { wrapper: Wrapper });
      expect(document.title).toContain('Página não encontrada');
    });
  });

  describe('UnexpectedErrorPage', () => {
    it('deve exibir mensagem de erro inesperado', () => {
      render(<UnexpectedErrorPage />, { wrapper: Wrapper });
      expect(screen.getByText(/algo deu errado/i)).not.toBeNull();
    });

    it('deve exibir botão "Tentar novamente"', () => {
      render(<UnexpectedErrorPage />, { wrapper: Wrapper });
      expect(screen.getByRole('button', { name: /tentar novamente/i })).not.toBeNull();
    });

    it('deve não exibir detalhes técnicos', () => {
      render(<UnexpectedErrorPage />, { wrapper: Wrapper });
      expect(screen.queryByText(/stack/i)).toBeNull();
      expect(screen.queryByText(/exception/i)).toBeNull();
    });
  });
});
