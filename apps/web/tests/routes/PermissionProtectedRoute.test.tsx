import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { PermissionProtectedRoute } from '../../src/app/routes/PermissionProtectedRoute';
import { useAuthorization } from '../../src/auth/AuthorizationProvider';
import { makeAuthorization } from '../helpers/authorization';

vi.mock('../../src/auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));

function Destination({ name }: { name: string }) {
  const location = useLocation();
  return (
    <div>
      {name}: {location.state?.from?.pathname}
    </div>
  );
}

function renderRoute(somenteOperador = false) {
  return render(
    <MemoryRouter initialEntries={['/clientes']}>
      <Routes>
        <Route
          path="/clientes"
          element={
            <PermissionProtectedRoute
              moduloCodigo="CLIENTES"
              permissaoCodigo="clientes.visualizar"
              somenteOperador={somenteOperador}
            >
              <div>Clientes autorizados</div>
            </PermissionProtectedRoute>
          }
        />
        <Route
          path="/forbidden"
          element={<Destination name="Acesso negado" />}
        />
        <Route
          path="/unauthorized"
          element={<Destination name="Usuário não autorizado" />}
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe('PermissionProtectedRoute', () => {
  beforeEach(() => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        modulosHabilitados: ['CLIENTES'],
        permissoes: ['clientes.visualizar'],
      }),
    );
  });

  it('permite acesso com módulo habilitado e permissão', () => {
    renderRoute();
    expect(screen.getByText('Clientes autorizados')).toBeTruthy();
  });

  it('aguarda o carregamento sem mostrar conteúdo protegido', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({ isLoading: true }),
    );
    renderRoute();
    expect(screen.getByText('Carregando...')).toBeTruthy();
    expect(screen.queryByText('Clientes autorizados')).toBeNull();
  });

  it.each([
    { error: true },
    { usuarioEncontrado: false },
    { usuarioAtivo: false },
  ])('bloqueia erro ou usuário indisponível: %j', (state) => {
    vi.mocked(useAuthorization).mockReturnValue(makeAuthorization(state));
    renderRoute();
    expect(screen.getByText('Usuário não autorizado: /clientes')).toBeTruthy();
    expect(screen.queryByText('Clientes autorizados')).toBeNull();
  });

  it('bloqueia a falta de permissão e preserva a rota de origem', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({ modulosHabilitados: ['CLIENTES'] }),
    );
    renderRoute();
    expect(screen.getByText('Acesso negado: /clientes')).toBeTruthy();
  });

  it.each([false, true])(
    'bloqueia módulo desabilitado, inclusive com acesso total: %s',
    (acessoTotal) => {
      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({ acessoTotal, permissoes: ['clientes.visualizar'] }),
      );
      renderRoute();
      expect(screen.getByText('Acesso negado: /clientes')).toBeTruthy();
    },
  );

  it('permite acesso total somente com o módulo habilitado', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        acessoTotal: true,
        modulosHabilitados: ['CLIENTES'],
      }),
    );
    renderRoute();
    expect(screen.getByText('Clientes autorizados')).toBeTruthy();
  });

  it('bloqueia função de operador para usuário comum com permissão', () => {
    renderRoute(true);
    expect(screen.getByText('Acesso negado: /clientes')).toBeTruthy();
  });

  it('permite operador com módulo habilitado e permissão', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        operadorSistema: true,
        modulosHabilitados: ['CLIENTES'],
        permissoes: ['clientes.visualizar'],
      }),
    );
    renderRoute(true);
    expect(screen.getByText('Clientes autorizados')).toBeTruthy();
  });
});
