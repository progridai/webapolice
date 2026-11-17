import { fireEvent, render, screen, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ROUTES } from '../../src/app/routes/routePaths';
import { useAuthorization } from '../../src/auth/AuthorizationProvider';
import { AppNavigation } from '../../src/layouts/AuthenticatedLayout/AppNavigation';
import {
  ThemeProvider,
  type TemaPreferido,
} from '../../src/shared/theme/ThemeContext';
import { makeAuthorization } from '../helpers/authorization';

vi.mock('../../src/auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));

const mockEnv = vi.hoisted(() => ({ ENABLE_DESIGN_SYSTEM: true }));

vi.mock('../../src/app/config/env', () => ({ ENV: mockEnv }));

const expectedGroups = [
  ['Início', ['Início']],
  ['Operação', ['Clientes', 'Apólices', 'Propostas']],
  [
    'Parceiros e Estrutura',
    [
      'Seguradoras',
      'Corretoras',
      'Estipulantes',
      'Subestipulantes',
      'Cooperados',
    ],
  ],
  [
    'Configurações do Seguro',
    ['Ramos', 'Coberturas', 'Módulos Globais', 'Convênios de Cobrança'],
  ],
  ['Segurança', ['Usuários', 'Perfis', 'Catálogo de Permissões', 'Auditoria']],
  ['Sistema', ['Módulos do Sistema', 'Design System']],
] as const;

function renderNavigation(path: string = ROUTES.APP, onNavigate = vi.fn()) {
  return render(
    <ThemeProvider>
      <MemoryRouter initialEntries={[path]}>
        <AppNavigation onNavigate={onNavigate} />
      </MemoryRouter>
    </ThemeProvider>,
  );
}

describe('AppNavigation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    mockEnv.ENABLE_DESIGN_SYSTEM = true;
    vi.mocked(useAuthorization).mockReturnValue(makeAuthorization());
    vi.stubGlobal(
      'matchMedia',
      vi.fn().mockImplementation((query: string) => ({
        matches: true,
        media: query,
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
      })),
    );
  });

  it.each<TemaPreferido>(['claro', 'escuro', 'sistema'])(
    'mantém os grupos e a ordem operacional no tema %s',
    (theme) => {
      localStorage.setItem('webapolice-tema', theme);
      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({
          acessoTotal: true,
          operadorSistema: true,
          modulosHabilitados: ['APOLICES'],
          recursosHabilitados: ['PROPOSTAS'],
        }),
      );

      renderNavigation();

      expect(document.documentElement.getAttribute('data-theme')).toBe(
        theme === 'claro' ? 'light' : 'dark',
      );
      expect(screen.getAllByRole('list').map((list) => list.ariaLabel)).toEqual(
        expectedGroups.map(([label]) => label),
      );
      expect(
        screen.getAllByRole('heading', { level: 2 }).map((h) => h.textContent),
      ).toEqual(expectedGroups.slice(1).map(([label]) => label));
      for (const [label, links] of expectedGroups) {
        expect(
          within(screen.getByRole('list', { name: label }))
            .getAllByRole('link')
            .map((link) => link.textContent),
        ).toEqual(links);
      }
    },
  );

  it('exibe somente Início e oculta títulos e grupos vazios sem concessões', () => {
    renderNavigation();

    expect(screen.getAllByRole('link').map((link) => link.textContent)).toEqual(
      ['Início'],
    );
    expect(screen.queryAllByRole('heading')).toHaveLength(0);
    expect(screen.getAllByRole('list')).toHaveLength(1);
  });

  it('filtra cada item por módulo e permissão antes de montar os grupos', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        modulosHabilitados: ['APOLICES', 'CADASTRO'],
        permissoes: [
          'apolices.visualizar',
          'coberturas.visualizar',
          'seguradoras.visualizar',
          'cooperados.visualizar',
          'seguranca.usuarios.visualizar',
        ],
      }),
    );

    renderNavigation();

    expect(screen.getAllByRole('link').map((link) => link.textContent)).toEqual(
      ['Início', 'Apólices', 'Seguradoras', 'Coberturas'],
    );
    expect(screen.queryByRole('list', { name: 'Segurança' })).toBeNull();
    expect(screen.queryByRole('list', { name: 'Sistema' })).toBeNull();
  });

  it.each([
    { acessoTotal: false, modulo: true, recurso: false, permissao: true },
    { acessoTotal: false, modulo: false, recurso: true, permissao: true },
    { acessoTotal: false, modulo: true, recurso: true, permissao: false },
    { acessoTotal: true, modulo: false, recurso: true, permissao: false },
    { acessoTotal: true, modulo: true, recurso: false, permissao: false },
  ])('preserva as barreiras de Propostas: %j', (scenario) => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        acessoTotal: scenario.acessoTotal,
        modulosHabilitados: scenario.modulo ? ['APOLICES'] : [],
        recursosHabilitados: scenario.recurso ? ['PROPOSTAS'] : [],
        permissoes: scenario.permissao ? ['propostas.visualizar'] : [],
      }),
    );

    renderNavigation();

    expect(screen.queryByRole('link', { name: 'Propostas' })).toBeNull();
  });

  it('permite consultar Propostas com módulo, recurso e permissão disponíveis', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: ['PROPOSTAS'],
        permissoes: ['propostas.visualizar'],
      }),
    );

    renderNavigation();

    expect(
      screen.getByRole('link', { name: 'Propostas' }).getAttribute('href'),
    ).toBe(ROUTES.PROPOSTAS);
  });

  it('mantém Sistema exclusivo do operador, inclusive para Acesso Total', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({ acessoTotal: true }),
    );

    renderNavigation();

    expect(screen.queryByRole('list', { name: 'Sistema' })).toBeNull();
  });

  it('mantém Módulos do Sistema disponível ao operador com Design System desligado', () => {
    mockEnv.ENABLE_DESIGN_SYSTEM = false;
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({ operadorSistema: true }),
    );

    renderNavigation();

    expect(
      within(screen.getByRole('list', { name: 'Sistema' }))
        .getAllByRole('link')
        .map((link) => link.textContent),
    ).toEqual(['Módulos do Sistema']);
  });

  it('preserva a rota ativa e o retorno de navegação usado pelo menu mobile', () => {
    const onNavigate = vi.fn();
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({ acessoTotal: true }),
    );

    renderNavigation(ROUTES.APOLICES, onNavigate);

    const active = screen.getByRole('link', { name: 'Apólices' });
    expect(active.getAttribute('aria-current')).toBe('page');
    expect(active.classList.contains('app-nav-item--active')).toBe(true);
    fireEvent.click(screen.getByRole('link', { name: 'Clientes' }));
    expect(onNavigate).toHaveBeenCalledTimes(1);
    expect(
      screen
        .getByRole('link', { name: 'Clientes' })
        .getAttribute('aria-current'),
    ).toBe('page');
  });
});
