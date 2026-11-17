import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { HomePage } from './HomePage';
import { AuthContext } from '../../auth/AuthContext';
import type { AuthContextValue } from '../../auth/auth.types';
import { useAuthorization } from '../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../tests/helpers/authorization';
import { ThemeProvider } from '../../shared/theme/ThemeContext';
import {
  obterAtalhosHome,
  salvarAtalhosHome,
  type PreferenciasHome,
} from './atalhosHome.api';

vi.mock('../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));
vi.mock('./atalhosHome.api', () => ({
  obterAtalhosHome: vi.fn(),
  salvarAtalhosHome: vi.fn(),
}));
vi.mock('../../app/config/env', () => ({
  ENV: { ENABLE_DESIGN_SYSTEM: true },
}));
const navigateMock = vi.fn();
vi.mock('react-router-dom', async () => ({
  ...(await vi.importActual<typeof import('react-router-dom')>(
    'react-router-dom',
  )),
  useNavigate: () => navigateMock,
}));

function authValue(id: string): AuthContextValue {
  return {
    status: 'authenticated',
    isLoading: false,
    isAuthenticated: true,
    user: {
      id,
      username: id,
      name: 'Maria Silva',
      email: 'maria@local.test',
      roles: ['admin'],
    },
    roles: ['admin'],
    login: vi.fn(),
    logout: vi.fn(),
    refreshToken: vi.fn().mockResolvedValue(true),
    hasRole: () => true,
    hasAnyRole: () => true,
    hasAllRoles: () => true,
  };
}

function tree(userId = 'user-1') {
  return (
    <ThemeProvider>
      <MemoryRouter>
        <AuthContext.Provider value={authValue(userId)}>
          <HomePage />
        </AuthContext.Provider>
      </MemoryRouter>
    </ThemeProvider>
  );
}

async function openConfiguration() {
  const button = screen.getByRole('button', { name: 'Personalizar atalhos' });
  await waitFor(() => expect(button).toHaveProperty('disabled', false));
  fireEvent.click(button);
  return within(screen.getByRole('dialog', { name: 'Personalizar atalhos' }));
}

function deferred() {
  let resolve!: (data: PreferenciasHome) => void;
  const promise = new Promise<PreferenciasHome>((done) => {
    resolve = done;
  });
  return { promise, resolve };
}

describe('Atalhos pessoais da Home', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    vi.mocked(obterAtalhosHome)
      .mockReset()
      .mockResolvedValue({ atalhosHome: null });
    vi.mocked(salvarAtalhosHome)
      .mockReset()
      .mockImplementation(async (paths) => ({ atalhosHome: paths }));
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        acessoTotal: true,
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: ['PROPOSTAS'],
      }),
    );
  });

  it.each(['claro', 'escuro', 'sistema'])(
    'mostra os atalhos iniciais da operação e permite navegar no tema %s',
    async (theme) => {
      localStorage.setItem('webapolice-tema', theme);
      render(tree());
      expect(
        await screen.findByRole('button', { name: 'Abrir Clientes' }),
      ).toBeTruthy();
      expect(
        screen
          .getAllByRole('heading', { level: 3 })
          .map((heading) => heading.textContent),
      ).toEqual(['Clientes', 'Apólices', 'Propostas']);
      fireEvent.click(screen.getByRole('button', { name: 'Abrir Apólices' }));
      expect(navigateMock).toHaveBeenCalledWith('/apolices');
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        theme === 'escuro' ? 'dark' : 'light',
      );
    },
  );

  it('seleciona telas agrupadas, salva na conta e atualiza os cartões na ordem do menu', async () => {
    render(tree());
    const dialog = await openConfiguration();
    expect(
      dialog
        .getAllByRole('group')
        .map((group) => group.querySelector('legend')?.textContent),
    ).toEqual([
      'Operação',
      'Parceiros e Estrutura',
      'Configurações do Seguro',
      'Segurança',
    ]);
    fireEvent.click(dialog.getByRole('checkbox', { name: 'Apólices' }));
    fireEvent.click(dialog.getByRole('checkbox', { name: 'Seguradoras' }));
    fireEvent.click(dialog.getByRole('button', { name: 'Salvar atalhos' }));
    await screen.findByText('Seus atalhos foram atualizados.');
    expect(salvarAtalhosHome).toHaveBeenCalledWith(
      ['/clientes', '/propostas', '/seguradoras'],
      expect.any(AbortSignal),
    );
    expect(screen.queryByRole('dialog')).toBeNull();
    expect(screen.queryByRole('button', { name: 'Abrir Apólices' })).toBeNull();
    expect(
      screen.getByRole('button', { name: 'Abrir Seguradoras' }),
    ).toBeTruthy();
  });

  it.each(['Cancelar', 'Fechar Modal', 'Escape'])(
    'descarta a edição ao usar %s',
    async (action) => {
      render(tree());
      const dialog = await openConfiguration();
      fireEvent.click(dialog.getByRole('checkbox', { name: 'Apólices' }));
      if (action === 'Escape') fireEvent.keyDown(window, { key: 'Escape' });
      else fireEvent.click(dialog.getByRole('button', { name: action }));
      expect(screen.queryByRole('dialog')).toBeNull();
      expect(salvarAtalhosHome).not.toHaveBeenCalled();
      const reopened = await openConfiguration();
      expect(
        reopened.getByRole('checkbox', { name: 'Apólices' }),
      ).toHaveProperty('checked', true);
    },
  );

  it('permite salvar nenhum atalho e preserva a seleção vazia após voltar à Home', async () => {
    const view = render(tree());
    const dialog = await openConfiguration();
    for (const name of ['Clientes', 'Apólices', 'Propostas'])
      fireEvent.click(dialog.getByRole('checkbox', { name }));
    fireEvent.click(dialog.getByRole('button', { name: 'Salvar atalhos' }));
    await screen.findByText('Seus atalhos foram atualizados.');
    expect(salvarAtalhosHome).toHaveBeenCalledWith([], expect.any(AbortSignal));
    expect(screen.queryByRole('button', { name: /Abrir / })).toBeNull();
    view.unmount();
    vi.mocked(obterAtalhosHome).mockResolvedValue({ atalhosHome: [] });
    render(tree());
    await screen.findByText('Nenhum atalho na sua página inicial');
    expect(screen.queryByRole('button', { name: 'Abrir Clientes' })).toBeNull();
  });

  it('usa as permissões atuais do menu e não as roles antigas da Home', async () => {
    vi.mocked(useAuthorization).mockReturnValue(makeAuthorization());
    vi.mocked(obterAtalhosHome).mockResolvedValue({
      atalhosHome: ['/clientes', '/seguranca/modulos'],
    });
    render(tree());
    const dialog = await openConfiguration();
    expect(dialog.queryAllByRole('checkbox')).toHaveLength(0);
    expect(screen.queryByRole('button', { name: /Abrir / })).toBeNull();
  });

  it('oculta destinos desconhecidos e atalhos sem módulo ou recurso, inclusive com Acesso Total', async () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({ acessoTotal: true }),
    );
    vi.mocked(obterAtalhosHome).mockResolvedValue({
      atalhosHome: [
        '/clientes',
        '/propostas',
        '/seguranca/modulos',
        'https://exemplo.test',
      ],
    });
    render(tree());
    await screen.findByRole('button', { name: 'Abrir Clientes' });
    expect(
      screen.getAllByRole('heading', { level: 3 }).map((h) => h.textContent),
    ).toEqual(['Clientes']);
    const dialog = await openConfiguration();
    expect(dialog.queryByRole('checkbox', { name: 'Propostas' })).toBeNull();
    expect(
      dialog.queryByRole('checkbox', { name: 'Módulos do Sistema' }),
    ).toBeNull();
  });

  it('mostra carregamento e cancela a consulta ao sair da página', () => {
    const pending = deferred();
    vi.mocked(obterAtalhosHome).mockReturnValue(pending.promise);
    const view = render(tree());
    expect(
      screen.getByRole('status', { name: 'Carregando seus atalhos' }),
    ).toBeTruthy();
    expect(
      screen.getByRole('button', { name: 'Personalizar atalhos' }),
    ).toHaveProperty('disabled', true);
    const signal = vi.mocked(obterAtalhosHome).mock.calls[0][0];
    expect(signal.aborted).toBe(false);
    view.unmount();
    expect(signal.aborted).toBe(true);
  });

  it('oferece nova tentativa após erro ao consultar sem assumir uma seleção vazia', async () => {
    vi.mocked(obterAtalhosHome)
      .mockRejectedValueOnce(new Error('Falha temporária'))
      .mockResolvedValueOnce({ atalhosHome: ['/seguradoras'] });
    render(tree());
    await screen.findByText('Falha temporária');
    expect(
      screen.getByRole('button', { name: 'Personalizar atalhos' }),
    ).toHaveProperty('disabled', true);
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
    await screen.findByRole('button', { name: 'Abrir Seguradoras' });
    expect(screen.queryByRole('button', { name: 'Abrir Clientes' })).toBeNull();
  });

  it('preserva a edição após erro de salvamento e permite nova tentativa', async () => {
    vi.mocked(salvarAtalhosHome).mockRejectedValueOnce(
      new Error('Não foi possível salvar agora.'),
    );
    render(tree());
    const dialog = await openConfiguration();
    fireEvent.click(dialog.getByRole('checkbox', { name: 'Corretoras' }));
    fireEvent.click(dialog.getByRole('button', { name: 'Salvar atalhos' }));
    await screen.findByText('Não foi possível salvar agora.');
    expect(dialog.getByRole('checkbox', { name: 'Corretoras' })).toHaveProperty(
      'checked',
      true,
    );
    fireEvent.click(dialog.getByRole('button', { name: 'Salvar atalhos' }));
    await screen.findByRole('button', { name: 'Abrir Corretoras' });
    expect(salvarAtalhosHome).toHaveBeenCalledTimes(2);
  });

  it('bloqueia fechamento e duplicidade durante o envio e cancela ao desmontar', async () => {
    const pending = deferred();
    vi.mocked(salvarAtalhosHome).mockReturnValue(pending.promise);
    const view = render(tree());
    const dialog = await openConfiguration();
    fireEvent.click(dialog.getByRole('button', { name: 'Salvar atalhos' }));
    fireEvent.click(dialog.getByRole('button', { name: 'Salvar atalhos' }));
    fireEvent.keyDown(window, { key: 'Escape' });
    fireEvent.click(dialog.getByRole('button', { name: 'Fechar Modal' }));
    expect(screen.getByRole('dialog')).toBeTruthy();
    expect(salvarAtalhosHome).toHaveBeenCalledTimes(1);
    const signal = vi.mocked(salvarAtalhosHome).mock.calls[0][1];
    view.unmount();
    expect(signal.aborted).toBe(true);
    await act(async () => pending.resolve({ atalhosHome: ['/clientes'] }));
    expect(screen.queryByText('Seus atalhos foram atualizados.')).toBeNull();
  });

  it('cancela a sessão anterior e descarta resposta atrasada ao trocar de usuário', async () => {
    const first = deferred();
    const second = deferred();
    vi.mocked(obterAtalhosHome)
      .mockReturnValueOnce(first.promise)
      .mockReturnValueOnce(second.promise);
    const view = render(tree('user-1'));
    const firstSignal = vi.mocked(obterAtalhosHome).mock.calls[0][0];
    view.rerender(tree('user-2'));
    expect(firstSignal.aborted).toBe(true);
    await act(async () => second.resolve({ atalhosHome: ['/corretoras'] }));
    await screen.findByRole('button', { name: 'Abrir Corretoras' });
    await act(async () => first.resolve({ atalhosHome: ['/clientes'] }));
    expect(screen.queryByRole('button', { name: 'Abrir Clientes' })).toBeNull();
  });
});
