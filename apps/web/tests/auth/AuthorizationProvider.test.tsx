import type { PropsWithChildren } from 'react';
import {
  act,
  fireEvent,
  render,
  renderHook,
  screen,
  waitFor,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import {
  AuthorizationProvider,
  useAuthorization,
  type UsuarioAutenticadoResponse,
} from '../../src/auth/AuthorizationProvider';
import { AuthContext } from '../../src/auth/AuthContext';
import type { AuthContextValue } from '../../src/auth/auth.types';
import { httpClient } from '../../src/services/http/httpClient';
import type { HttpResponse } from '../../src/services/http/http.types';
import { HttpApiError } from '../../src/services/http/httpError';
import { PermissionProtectedRoute } from '../../src/app/routes/PermissionProtectedRoute';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/services/http/httpClient', () => ({
  httpClient: { get: vi.fn() },
}));
function auth(
  id: string | null = 'A',
  overrides: Partial<AuthContextValue> = {},
): AuthContextValue {
  return {
    status: id ? 'authenticated' : 'unauthenticated',
    isAuthenticated: !!id,
    isLoading: false,
    user: id
      ? { id, username: 'Conta de Teste', name: 'Pessoa', email: '', roles: [] }
      : null,
    roles: [],
    login: vi.fn().mockResolvedValue(undefined),
    logout: vi.fn().mockResolvedValue(undefined),
    refreshToken: vi.fn().mockResolvedValue(true),
    hasRole: () => false,
    hasAnyRole: () => false,
    hasAllRoles: () => false,
    ...overrides,
  };
}
function me(
  overrides: Partial<UsuarioAutenticadoResponse> = {},
): UsuarioAutenticadoResponse {
  return {
    id: 'A',
    usuario: 'Conta de Teste',
    usuarioEncontrado: true,
    usuarioAtivo: true,
    acessoTotal: false,
    operadorSistema: false,
    modulosHabilitados: ['CLIENTES'],
    recursosHabilitados: ['clientes.contatos'],
    permissoes: ['clientes.visualizar'],
    ...overrides,
  };
}
const response = (data = me()): HttpResponse<UsuarioAutenticadoResponse> => ({
  data,
  status: 200,
});
function deferred() {
  let resolve!: (data: HttpResponse<UsuarioAutenticadoResponse>) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<HttpResponse<UsuarioAutenticadoResponse>>(
    (ok, fail) => {
      resolve = ok;
      reject = fail;
    },
  );
  return { promise, resolve, reject };
}
function setup(initial = auth(), strict = false) {
  let session = initial;
  function Wrapper({ children }: PropsWithChildren) {
    return (
      <AuthContext.Provider value={session}>
        <AuthorizationProvider>{children}</AuthorizationProvider>
      </AuthContext.Provider>
    );
  }
  const hook = renderHook(() => useAuthorization(), {
    wrapper: Wrapper,
    reactStrictMode: strict,
  });
  return {
    ...hook,
    setSession(next: AuthContextValue) {
      session = next;
      hook.rerender();
    },
  };
}
function expectDenied(value: ReturnType<typeof useAuthorization>) {
  expect(value.usuarioEncontrado).toBe(false);
  expect(value.usuarioAtivo).toBe(false);
  expect(value.acessoTotal).toBe(false);
  expect(value.operadorSistema).toBe(false);
  expect(value.modulosHabilitados).toEqual([]);
  expect(value.recursosHabilitados).toEqual([]);
  expect(value.permissoes).toEqual([]);
  expect(value.possuiModulo('CLIENTES')).toBe(false);
  expect(value.possuiRecurso('clientes.contatos')).toBe(false);
  expect(value.possuiPermissao('clientes.visualizar')).toBe(false);
  expect(value.possuiAcessoTotal()).toBe(false);
  expect(value.ehOperadorSistema()).toBe(false);
}
const signal = (index: number) =>
  vi.mocked(httpClient.get).mock.calls[index][1]?.signal;
beforeEach(() => {
  vi.mocked(httpClient.get).mockReset();
  vi.mocked(httpClient.get).mockResolvedValue(response());
});
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe('Ciclo do AuthorizationProvider', () => {
  it('não consulta desautenticado e nega todas as verificações, inclusive refresh', async () => {
    const { result } = setup(auth(null));
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBe(false);
    expectDenied(result.current);
    await act(async () => result.current.refresh());
    expect(httpClient.get).not.toHaveBeenCalled();
  });

  it('aguarda autenticação antes de consultar e deriva carregamento sem conceder acesso', async () => {
    const { result, setSession } = setup(
      auth(null, { status: 'initializing', isLoading: true }),
    );
    expect(result.current.isLoading).toBe(true);
    expectDenied(result.current);
    expect(httpClient.get).not.toHaveBeenCalled();
    setSession(auth());
    expect(result.current.isLoading).toBe(true);
    expectDenied(result.current);
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(httpClient.get).toHaveBeenCalledExactlyOnceWith(
      '/api/seguranca/me',
      { signal: expect.any(AbortSignal) },
    );
    expect(result.current.possuiPermissao('clientes.visualizar')).toBe(true);
  });

  it('preserva correspondência exata de módulos, recursos, permissões e função de operador', async () => {
    vi.mocked(httpClient.get).mockResolvedValueOnce(
      response(me({ operadorSistema: true })),
    );
    const { result } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.usuarioEncontrado).toBe(true);
    expect(result.current.usuarioAtivo).toBe(true);
    expect(result.current.possuiModulo('CLIENTES')).toBe(true);
    expect(result.current.possuiModulo('clientes')).toBe(false);
    expect(result.current.possuiRecurso('clientes.contatos')).toBe(true);
    expect(result.current.possuiRecurso('CLIENTES.CONTATOS')).toBe(false);
    expect(result.current.possuiPermissao('clientes.visualizar')).toBe(true);
    expect(result.current.possuiPermissao('clientes.alterar')).toBe(false);
    expect(result.current.ehOperadorSistema()).toBe(true);
    expect(result.current.possuiAcessoTotal()).toBe(false);
  });

  it('preserva acesso total como atalho de permissão, sem habilitar módulos ou recursos ausentes', async () => {
    vi.mocked(httpClient.get).mockResolvedValueOnce(
      response(
        me({
          acessoTotal: true,
          permissoes: [],
          modulosHabilitados: [],
          recursosHabilitados: [],
        }),
      ),
    );
    const { result } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.possuiPermissao('clientes.alterar')).toBe(true);
    expect(result.current.possuiAcessoTotal()).toBe(true);
    expect(result.current.possuiModulo('CLIENTES')).toBe(false);
    expect(result.current.possuiRecurso('clientes.contatos')).toBe(false);
    expect(result.current.ehOperadorSistema()).toBe(false);
  });

  it('não refaz consulta quando o objeto de usuário muda com o mesmo identificador', async () => {
    const { result, setSession } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const refresh = result.current.refresh;
    setSession(
      auth('A', {
        user: { ...auth().user!, name: 'Nome Atualizado', roles: ['operador'] },
      }),
    );
    expect(httpClient.get).toHaveBeenCalledTimes(1);
    expect(result.current.refresh).toBe(refresh);
    expect(result.current.ehOperadorSistema()).toBe(false); // O resultado da API continua sendo a fonte da autorização.
  });

  it.each([false, true])(
    'troca de usuário descarta conclusão antiga antes da atual (falha: %s)',
    async (failure) => {
      const old = deferred();
      const current = deferred();
      vi.mocked(httpClient.get)
        .mockReturnValueOnce(old.promise)
        .mockReturnValueOnce(current.promise);
      const { result, setSession } = setup();
      const oldRefresh = result.current.refresh;
      setSession(auth('B'));
      expect(signal(0)?.aborted).toBe(true);
      expectDenied(result.current);
      await act(async () => oldRefresh());
      expect(httpClient.get).toHaveBeenCalledTimes(2);
      await act(async () => {
        if (failure) old.reject(new Error('Falha Antiga'));
        else old.resolve(response(me({ acessoTotal: true })));
      });
      expect(result.current.isLoading).toBe(true);
      expectDenied(result.current);
      expect(screen.queryByRole('alert')).toBeNull();
      await act(async () =>
        current.resolve(
          response(me({ id: 'B', permissoes: ['clientes.alterar'] })),
        ),
      );
      expect(result.current.possuiPermissao('clientes.visualizar')).toBe(false);
      expect(result.current.possuiPermissao('clientes.alterar')).toBe(true);
    },
  );

  it.each([false, true])(
    'não restaura acesso antigo quando a nova sessão já respondeu (falha: %s)',
    async (failure) => {
      const old = deferred();
      const current = deferred();
      vi.mocked(httpClient.get)
        .mockReturnValueOnce(old.promise)
        .mockReturnValueOnce(current.promise);
      const { result, setSession } = setup();
      setSession(auth('B'));
      await act(async () =>
        current.resolve(response(me({ id: 'B', permissoes: [] }))),
      );
      await act(async () => {
        if (failure) old.reject(new Error('Falha'));
        else old.resolve(response(me({ acessoTotal: true })));
      });
      expect(result.current.isLoading).toBe(false);
      expect(result.current.acessoTotal).toBe(false);
      expect(result.current.possuiPermissao('clientes.visualizar')).toBe(false);
      expect(screen.queryByRole('alert')).toBeNull();
    },
  );

  it('remove permissões imediatamente no logout e exige nova consulta ao entrar com o mesmo usuário', async () => {
    const { result, setSession } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.possuiPermissao('clientes.visualizar')).toBe(true);
    const next = deferred();
    vi.mocked(httpClient.get).mockReturnValueOnce(next.promise);
    setSession(auth(null));
    expectDenied(result.current);
    expect(result.current.isLoading).toBe(false);
    setSession(auth());
    expect(result.current.isLoading).toBe(true);
    expectDenied(result.current);
    await act(async () => next.resolve(response(me({ permissoes: [] }))));
    expect(result.current.possuiPermissao('clientes.visualizar')).toBe(false);
    expect(httpClient.get).toHaveBeenCalledTimes(2);
  });

  it('aborta atualização ao sair da sessão e não aceita seu sucesso tardio', async () => {
    const { result, setSession } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const pending = deferred();
    vi.mocked(httpClient.get).mockReturnValueOnce(pending.promise);
    let completion!: Promise<void>;
    act(() => {
      completion = result.current.refresh();
    });
    expectDenied(result.current);
    setSession(auth(null));
    expect(signal(1)?.aborted).toBe(true);
    expectDenied(result.current);
    await act(async () => {
      pending.resolve(response(me({ acessoTotal: true })));
      await completion;
    });
    expectDenied(result.current);
    expect(result.current.isLoading).toBe(false);
  });

  it('refresh não reutiliza concessões anteriores e só libera o resultado atualizado após a Promise', async () => {
    const { result } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const pending = deferred();
    vi.mocked(httpClient.get).mockReturnValueOnce(pending.promise);
    let completion!: Promise<void>;
    act(() => {
      completion = result.current.refresh();
    });
    expect(result.current.isLoading).toBe(true);
    expectDenied(result.current);
    await act(async () => {
      pending.resolve(response(me({ permissoes: [] })));
      await completion;
    });
    expect(result.current.isLoading).toBe(false);
    expect(result.current.possuiPermissao('clientes.visualizar')).toBe(false);
  });

  it('atualizações concorrentes aceitam somente a mais recente', async () => {
    const { result } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const old = deferred();
    const current = deferred();
    vi.mocked(httpClient.get)
      .mockReturnValueOnce(old.promise)
      .mockReturnValueOnce(current.promise);
    let first!: Promise<void>;
    let second!: Promise<void>;
    act(() => {
      first = result.current.refresh();
      second = result.current.refresh();
    });
    expect(signal(1)?.aborted).toBe(true);
    expectDenied(result.current);
    await act(async () => {
      current.resolve(response(me({ permissoes: [] })));
      await second;
    });
    await act(async () => {
      old.resolve(response(me({ acessoTotal: true })));
      await first;
    });
    expect(result.current.possuiAcessoTotal()).toBe(false);
    expect(result.current.possuiPermissao('clientes.visualizar')).toBe(false);
  });

  it('desmontagem aborta refresh e callback guardado não inicia novas chamadas', async () => {
    const { result, unmount } = setup();
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const pending = deferred();
    vi.mocked(httpClient.get).mockReturnValueOnce(pending.promise);
    const refresh = result.current.refresh;
    let completion!: Promise<void>;
    act(() => {
      completion = refresh();
    });
    unmount();
    expect(signal(1)?.aborted).toBe(true);
    await act(async () => {
      pending.reject(new Error('Falha Tardia'));
      await completion;
      await refresh();
    });
    expect(httpClient.get).toHaveBeenCalledTimes(2);
    expect(screen.queryByRole('alert')).toBeNull();
  });

  it.each([
    new HttpApiError({ status: 401, message: 'Detalhe Técnico' }),
    new HttpApiError({ status: 403, message: 'Detalhe Técnico' }),
    new HttpApiError({ status: 500, message: 'Detalhe Técnico' }),
    null,
    undefined,
    { message: 'Detalhe Técnico' },
  ])(
    'exibe erro genérico sem detalhes e permite recuperar uma falha: %j',
    async (error) => {
      vi.mocked(httpClient.get).mockRejectedValueOnce(error);
      setup();
      const alert = await screen.findByRole('alert');
      expect(alert.textContent).toContain(
        'Não foi possível carregar as permissões',
      );
      expect(alert.textContent).not.toContain('Detalhe Técnico');
      expect(alert.textContent).not.toContain('/api/');
      expect(alert.textContent).not.toContain('F12');
      fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
      await waitFor(() => expect(screen.queryByRole('alert')).toBeNull());
      expect(httpClient.get).toHaveBeenCalledTimes(2);
    },
  );

  it.each([
    new DOMException('Cancelada', 'AbortError'),
    Object.assign(new Error('Cancelada'), { name: 'AbortError' }),
    Object.assign(new Error('Cancelada'), { name: 'CanceledError' }),
  ])(
    'cancelamento da chamada atual sem sinal abortado não revalida concessões ($name)',
    async (error) => {
      const { result } = setup();
      await waitFor(() => expect(result.current.isLoading).toBe(false));
      vi.mocked(httpClient.get).mockRejectedValueOnce(error);
      let completion!: Promise<void>;
      act(() => {
        completion = result.current.refresh();
      });
      expectDenied(result.current);
      await act(async () => completion);
      expect(screen.getByRole('alert')).toBeTruthy();
    },
  );

  it('uma falha da sessão anterior não bloqueia a próxima sessão', async () => {
    vi.mocked(httpClient.get).mockRejectedValueOnce(new Error('Falha'));
    const { result, setSession } = setup();
    await screen.findByRole('alert');
    setSession(auth('B'));
    expect(screen.queryByRole('alert')).toBeNull();
    expect(result.current.isLoading).toBe(true);
    expectDenied(result.current);
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.possuiPermissao('clientes.visualizar')).toBe(true);
  });

  it('suporta StrictMode e ignora a resposta da primeira montagem cancelada', async () => {
    const old = deferred();
    const current = deferred();
    vi.mocked(httpClient.get)
      .mockReturnValueOnce(old.promise)
      .mockReturnValueOnce(current.promise);
    const { result, unmount } = setup(auth(), true);
    expect(signal(0)?.aborted).toBe(true);
    expect(httpClient.get).toHaveBeenCalledTimes(2);
    await act(async () => current.resolve(response(me({ permissoes: [] }))));
    await act(async () => old.resolve(response(me({ acessoTotal: true }))));
    expect(result.current.possuiAcessoTotal()).toBe(false);
    unmount();
    expect(signal(1)?.aborted).toBe(true);
  });
});

function RefreshButton() {
  const { refresh } = useAuthorization();
  return <button onClick={() => void refresh()}>Atualizar permissões</button>;
}
function guardedPage() {
  return (
    <AuthContext.Provider value={auth()}>
      <AuthorizationProvider>
        <MemoryRouter initialEntries={['/clientes']}>
          <RefreshButton />
          <Routes>
            <Route
              path="/clientes"
              element={
                <PermissionProtectedRoute
                  moduloCodigo="CLIENTES"
                  permissaoCodigo="clientes.visualizar"
                >
                  <div>Conteúdo protegido</div>
                </PermissionProtectedRoute>
              }
            />
            <Route path="/forbidden" element={<div>Acesso negado</div>} />
            <Route
              path="/unauthorized"
              element={<div>Usuário não autorizado</div>}
            />
          </Routes>
        </MemoryRouter>
      </AuthorizationProvider>
    </AuthContext.Provider>
  );
}

describe('Guard com AuthorizationProvider real', () => {
  it.each([
    {
      name: 'sem permissão',
      data: me({ permissoes: [] }),
      destination: 'Acesso negado',
    },
    {
      name: 'módulo desligado com acesso total',
      data: me({ acessoTotal: true, modulosHabilitados: [] }),
      destination: 'Acesso negado',
    },
    {
      name: 'usuário inativo',
      data: me({ usuarioAtivo: false }),
      destination: 'Usuário não autorizado',
    },
    {
      name: 'usuário não encontrado',
      data: me({ usuarioEncontrado: false }),
      destination: 'Usuário não autorizado',
    },
  ])('preserva a negativa para $name', async ({ data, destination }) => {
    vi.mocked(httpClient.get).mockResolvedValueOnce(response(data));
    render(guardedPage());
    expect(await screen.findByText(destination)).toBeTruthy();
    expect(screen.queryByText('Conteúdo protegido')).toBeNull();
  });

  it.each([
    { theme: 'claro', dark: false, resolved: 'light' },
    { theme: 'escuro', dark: false, resolved: 'dark' },
    { theme: 'sistema', dark: false, resolved: 'light' },
    { theme: 'sistema', dark: true, resolved: 'dark' },
  ])(
    'bloqueia durante atualização, recupera erro e aplica revogação no tema $theme / $resolved',
    async ({ theme, dark, resolved }) => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      render(<ThemeProvider>{guardedPage()}</ThemeProvider>);
      await screen.findByText('Conteúdo protegido');
      const pending = deferred();
      vi.mocked(httpClient.get).mockReturnValueOnce(pending.promise);
      fireEvent.click(
        screen.getByRole('button', { name: 'Atualizar permissões' }),
      );
      expect(screen.queryByText('Conteúdo protegido')).toBeNull();
      expect(screen.getByText('Carregando...')).toBeTruthy();
      await act(async () => pending.reject(new Error('Diagnóstico interno')));
      expect(screen.getByRole('alert')).toBeTruthy();
      expect(screen.queryByText('Conteúdo protegido')).toBeNull();
      expect(screen.queryByText('Diagnóstico interno')).toBeNull();
      vi.mocked(httpClient.get).mockResolvedValueOnce(
        response(me({ permissoes: [] })),
      );
      fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
      expect(await screen.findByText('Acesso negado')).toBeTruthy();
      expect(screen.queryByRole('alert')).toBeNull();
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    },
  );
});
