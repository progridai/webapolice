import { act, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import {
  MemoryRouter,
  Outlet,
  Route,
  Routes,
  useLocation,
  useParams,
} from 'react-router-dom';
import { CooperadosRoutes } from '../../src/features/cooperados/routes/cooperados.routes';
import { useAuthorization } from '../../src/auth/AuthorizationProvider';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';
import { makeAuthorization } from '../helpers/authorization';

const modules = vi.hoisted(() => {
  function pendingModule() {
    let release!: () => void;
    const ready = new Promise<void>((resolve) => {
      release = resolve;
    });
    return { ready, release, loaded: vi.fn() };
  }
  return {
    list: pendingModule(),
    create: pendingModule(),
    detail: pendingModule(),
    edit: pendingModule(),
  };
});
vi.mock('../../src/auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));
vi.mock('../../src/layouts/AuthenticatedLayout', () => ({
  AuthenticatedLayout: () => (
    <>
      <header>Área autenticada</header>
      <Outlet />
    </>
  ),
}));
vi.mock('../../src/features/cooperados/pages/CooperadosListPage', async () => {
  modules.list.loaded();
  await modules.list.ready;
  return {
    CooperadosListPage: () => <LoadedPage name="Lista de Cooperados" />,
  };
});
vi.mock('../../src/features/cooperados/pages/NovoCooperadoPage', async () => {
  modules.create.loaded();
  await modules.create.ready;
  return { NovoCooperadoPage: () => <LoadedPage name="Novo Cooperado" /> };
});
vi.mock(
  '../../src/features/cooperados/pages/CooperadoDetalhePage',
  async () => {
    modules.detail.loaded();
    await modules.detail.ready;
    return {
      CooperadoDetalhePage: () => <LoadedPage name="Detalhes do Cooperado" />,
    };
  },
);
vi.mock('../../src/features/cooperados/pages/EditarCooperadoPage', async () => {
  modules.edit.loaded();
  await modules.edit.ready;
  return { EditarCooperadoPage: () => <LoadedPage name="Editar Cooperado" /> };
});
function LoadedPage({ name }: { name: string }) {
  const { id } = useParams();
  const location = useLocation();
  return (
    <>
      <h1>{name}</h1>
      <p data-testid="route-id">{id || 'sem id'}</p>
      <p data-testid="route-search">{location.search}</p>
    </>
  );
}
function renderPath(path: string) {
  return render(
    <ThemeProvider>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          {CooperadosRoutes}
          <Route path="/forbidden" element={<div>Acesso negado</div>} />
          <Route
            path="/unauthorized"
            element={<div>Usuário não autorizado</div>}
          />
        </Routes>
      </MemoryRouter>
    </ThemeProvider>,
  );
}
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

const cases = [
  {
    path: '/cooperados',
    name: 'Lista de Cooperados',
    permission: 'cooperados.visualizar',
    wrongPermission: 'cooperados.inserir',
    module: modules.list,
    id: 'sem id',
    theme: 'claro',
    dark: false,
    resolved: 'light',
  },
  {
    path: '/cooperados/novo',
    name: 'Novo Cooperado',
    permission: 'cooperados.inserir',
    wrongPermission: 'cooperados.visualizar',
    module: modules.create,
    id: 'sem id',
    theme: 'escuro',
    dark: false,
    resolved: 'dark',
  },
  {
    path: '/cooperados/coop-1',
    name: 'Detalhes do Cooperado',
    permission: 'cooperados.visualizar',
    wrongPermission: 'cooperados.alterar',
    module: modules.detail,
    id: 'coop-1',
    theme: 'sistema',
    dark: false,
    resolved: 'light',
  },
  {
    path: '/cooperados/coop-1/editar',
    name: 'Editar Cooperado',
    permission: 'cooperados.alterar',
    wrongPermission: 'cooperados.visualizar',
    module: modules.edit,
    id: 'coop-1',
    theme: 'sistema',
    dark: true,
    resolved: 'dark',
  },
];

describe('Carregamento e permissões das rotas de Cooperados', () => {
  it.each(cases)(
    'protege $path e mostra carregamento local até disponibilizar a página',
    async (scenario) => {
      localStorage.setItem('webapolice-tema', scenario.theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: scenario.dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const path = `${scenario.path}?status=2&nome=Pessoa`;

      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({ isLoading: true }),
      );
      const waitingAuthorization = renderPath(path);
      expect(screen.getByText('Carregando...')).toBeTruthy();
      expect(scenario.module.loaded).not.toHaveBeenCalled();
      waitingAuthorization.unmount();

      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({ usuarioAtivo: false }),
      );
      const unauthorized = renderPath(path);
      expect(await screen.findByText('Usuário não autorizado')).toBeTruthy();
      expect(scenario.module.loaded).not.toHaveBeenCalled();
      unauthorized.unmount();

      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({ permissoes: [scenario.wrongPermission] }),
      );
      const denied = renderPath(path);
      expect(await screen.findByText('Acesso negado')).toBeTruthy();
      expect(scenario.module.loaded).not.toHaveBeenCalled();
      expect(screen.queryByRole('heading', { name: scenario.name })).toBeNull();
      denied.unmount();

      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({ permissoes: [scenario.permission] }),
      );
      // Cada rota mantém seu fallback nos quatro estados de tema, sem iniciar outro import.
      for (const theme of [
        { preference: 'claro', dark: false, resolved: 'light' },
        { preference: 'escuro', dark: false, resolved: 'dark' },
        { preference: 'sistema', dark: false, resolved: 'light' },
        { preference: 'sistema', dark: true, resolved: 'dark' },
      ]) {
        localStorage.setItem('webapolice-tema', theme.preference);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: theme.dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        const pendingPage = renderPath(path);
        expect(
          screen.getByRole('status', { name: 'Carregando página' }),
        ).toBeTruthy();
        expect(screen.getByText('Área autenticada')).toBeTruthy();
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          theme.resolved,
        );
        await waitFor(() =>
          expect(scenario.module.loaded).toHaveBeenCalledOnce(),
        );
        pendingPage.unmount();
      }
      localStorage.setItem('webapolice-tema', scenario.theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: scenario.dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const allowed = renderPath(path); // Sem Suspense no teste: o fallback deve pertencer à rota.
      expect(
        screen.getByRole('status', { name: 'Carregando página' }),
      ).toBeTruthy();
      expect(screen.getByText('Área autenticada')).toBeTruthy();
      await waitFor(() =>
        expect(scenario.module.loaded).toHaveBeenCalledOnce(),
      );
      await act(async () => scenario.module.release());
      expect(
        await screen.findByRole('heading', { name: scenario.name }),
      ).toBeTruthy();
      expect(
        screen.queryByRole('status', { name: 'Carregando página' }),
      ).toBeNull();
      expect(screen.getByTestId('route-id').textContent).toBe(scenario.id);
      expect(screen.getByTestId('route-search').textContent).toBe(
        '?status=2&nome=Pessoa',
      );
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        scenario.resolved,
      );
      allowed.unmount();

      renderPath(path);
      expect(
        await screen.findByRole('heading', { name: scenario.name }),
      ).toBeTruthy();
      expect(scenario.module.loaded).toHaveBeenCalledOnce();
    },
  );
});
