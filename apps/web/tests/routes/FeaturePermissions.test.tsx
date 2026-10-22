import { Suspense } from 'react';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Outlet, Route, Routes } from 'react-router-dom';
import { useAuthorization } from '../../src/auth/AuthorizationProvider';
import { RamosRoutes } from '../../src/features/ramos/routes/ramos.routes';
import { SeguradorasRoutes } from '../../src/features/seguradoras/routes/seguradoras.routes';
import { SubestipulantesRoutes } from '../../src/features/subestipulantes/routes/subestipulantes.routes';
import { CooperadosRoutes } from '../../src/features/cooperados/routes/cooperados.routes';
import { makeAuthorization } from '../helpers/authorization';

vi.mock('../../src/auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));
vi.mock('../../src/layouts/AuthenticatedLayout', () => ({
  AuthenticatedLayout: () => <Outlet />,
}));
vi.mock('../../src/features/ramos/pages/RamosListPage', () => ({
  RamosListPage: () => <div>Lista de Ramos</div>,
}));
vi.mock('../../src/features/ramos/pages/RamoFormPage', () => ({
  RamoFormPage: () => <div>Formulário de Ramo</div>,
}));
vi.mock('../../src/features/seguradoras/pages/SeguradorasListPage', () => ({
  SeguradorasListPage: () => <div>Lista de Seguradoras</div>,
}));
vi.mock('../../src/features/seguradoras/pages/SeguradoraFormPage', () => ({
  SeguradoraFormPage: () => <div>Formulário de Seguradora</div>,
}));
vi.mock(
  '../../src/features/subestipulantes/pages/SubestipulantesListPage',
  () => ({
    SubestipulantesListPage: () => <div>Lista de Subestipulantes</div>,
  }),
);
vi.mock(
  '../../src/features/subestipulantes/pages/SubestipulanteFormPage',
  () => ({
    SubestipulanteFormPage: () => <div>Formulário de Subestipulante</div>,
  }),
);
vi.mock('../../src/features/cooperados/pages/CooperadosListPage', () => ({
  CooperadosListPage: () => <div>Lista de Cooperados</div>,
}));
vi.mock('../../src/features/cooperados/pages/NovoCooperadoPage', () => ({
  NovoCooperadoPage: () => <div>Novo Cooperado</div>,
}));
vi.mock('../../src/features/cooperados/pages/EditarCooperadoPage', () => ({
  EditarCooperadoPage: () => <div>Editar Cooperado</div>,
}));
vi.mock('../../src/features/cooperados/pages/CooperadoDetalhePage', () => ({
  CooperadoDetalhePage: () => <div>Detalhes do Cooperado</div>,
}));

const cases = [
  { path: '/ramos', page: 'Lista de Ramos', permissions: ['ramos.visualizar'] },
  {
    path: '/ramos/novo',
    page: 'Formulário de Ramo',
    permissions: ['ramos.visualizar'],
  },
  {
    path: '/ramos/ramo-1/editar',
    page: 'Formulário de Ramo',
    permissions: ['ramos.visualizar'],
  },
  {
    path: '/seguradoras',
    page: 'Lista de Seguradoras',
    permissions: ['seguradoras.visualizar'],
  },
  {
    path: '/seguradoras/nova',
    page: 'Formulário de Seguradora',
    permissions: ['seguradoras.visualizar', 'seguradoras.inserir'],
  },
  {
    path: '/seguradoras/seg-1/editar',
    page: 'Formulário de Seguradora',
    permissions: ['seguradoras.visualizar', 'seguradoras.alterar'],
  },
  {
    path: '/subestipulantes',
    page: 'Lista de Subestipulantes',
    permissions: ['subestipulantes.visualizar'],
  },
  {
    path: '/subestipulantes/novo',
    page: 'Formulário de Subestipulante',
    permissions: ['subestipulantes.visualizar', 'subestipulantes.inserir'],
  },
  {
    path: '/subestipulantes/sub-1/editar',
    page: 'Formulário de Subestipulante',
    permissions: ['subestipulantes.visualizar', 'subestipulantes.alterar'],
  },
  {
    path: '/cooperados',
    page: 'Lista de Cooperados',
    permissions: ['cooperados.visualizar'],
  },
  {
    path: '/cooperados/novo',
    page: 'Novo Cooperado',
    permissions: ['cooperados.inserir'],
  },
  {
    path: '/cooperados/coop-1',
    page: 'Detalhes do Cooperado',
    permissions: ['cooperados.visualizar'],
  },
  {
    path: '/cooperados/coop-1/editar',
    page: 'Editar Cooperado',
    permissions: ['cooperados.alterar'],
  },
];

function renderPath(path: string, permissions: string[]) {
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({ permissoes: permissions }),
  );
  render(
    <MemoryRouter initialEntries={[path]}>
      <Suspense fallback={<div>Carregando rota</div>}>
        <Routes>
          {RamosRoutes}
          {SeguradorasRoutes}
          {SubestipulantesRoutes}
          {CooperadosRoutes}
          <Route path="/forbidden" element={<div>Acesso negado</div>} />
          <Route
            path="/unauthorized"
            element={<div>Usuário não autorizado</div>}
          />
        </Routes>
      </Suspense>
    </MemoryRouter>,
  );
}

describe('Permissões nas rotas reais dos cadastros', () => {
  it.each(cases)(
    'permite $path com as permissões exigidas',
    async ({ path, page, permissions }) => {
      renderPath(path, permissions);
      expect(await screen.findByText(page)).toBeTruthy();
    },
  );

  it.each(cases)('bloqueia $path sem permissão', async ({ path, page }) => {
    renderPath(path, []);
    expect(await screen.findByText('Acesso negado')).toBeTruthy();
    expect(screen.queryByText(page)).toBeNull();
  });

  it.each(cases.filter((c) => c.permissions.length > 1))(
    'bloqueia $path com somente visualização',
    async ({ path, page, permissions }) => {
      renderPath(path, [permissions[0]]);
      expect(await screen.findByText('Acesso negado')).toBeTruthy();
      expect(screen.queryByText(page)).toBeNull();
    },
  );
});
