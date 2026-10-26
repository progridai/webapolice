import { StrictMode } from 'react';
import { act, fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { CorretorasListPage } from '../../src/features/corretoras/pages/CorretorasListPage';
import { ModulosListPage } from '../../src/features/modulos/pages/ModulosListPage';
import { RamosListPage } from '../../src/features/ramos/pages/RamosListPage';
import { SeguradorasListPage } from '../../src/features/seguradoras/pages/SeguradorasListPage';
import { corretorasApi } from '../../src/features/corretoras/api/corretoras.api';
import { modulosApi } from '../../src/features/modulos/api/modulos.api';
import { ramosApi } from '../../src/features/ramos/api/ramos.api';
import { seguradorasApi } from '../../src/features/seguradoras/api/seguradoras.api';
import { useAuthorization } from '../../src/auth/AuthorizationProvider';
import { makeAuthorization } from '../helpers/authorization';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/features/corretoras/api/corretoras.api');
vi.mock('../../src/features/modulos/api/modulos.api');
vi.mock('../../src/features/ramos/api/ramos.api');
vi.mock('../../src/features/seguradoras/api/seguradoras.api');
vi.mock('../../src/auth/AuthorizationProvider');
const pages = {
  corretoras: CorretorasListPage,
  modulos: ModulosListPage,
  ramos: RamosListPage,
  seguradoras: SeguradorasListPage,
};
type Kind = keyof typeof pages;
interface Query {
  pagina?: number;
  tamanhoPagina?: number;
  busca?: string;
  ativo?: boolean;
}
function response(name = 'Registro Atual', ativo = true, total = 25) {
  return {
    rows: [
      {
        id: 1,
        publicId: 'registro-1',
        nome: name,
        codigo: 'COD-1',
        ativo,
        createdAt: '2026-01-01',
        updatedAt: '2026-01-01',
      },
    ],
    total,
  };
}
type Result = ReturnType<typeof response>;
function deferred<T>() {
  let resolve!: (data: T) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<T>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}
function prepare(kind: Kind, permitted = true) {
  const load = vi
    .fn<(query: Query | undefined, signal?: AbortSignal) => Promise<Result>>()
    .mockResolvedValue(response());
  const mutation = vi
    .fn<(id: string, operation: 'inativar' | 'reativar') => Promise<void>>()
    .mockResolvedValue(undefined);
  vi.mocked(corretorasApi.listar).mockImplementation(async (query, signal) => {
    const data = await load(query, signal);
    return {
      itens: data.rows,
      paginaAtual: query?.pagina || 1,
      tamanhoPagina: 10,
      totalItens: data.total,
      totalPaginas: Math.ceil(data.total / 10),
    };
  });
  vi.mocked(seguradorasApi.listar).mockImplementation(async (query, signal) => {
    const data = await load(query, signal);
    return {
      itens: data.rows,
      paginaAtual: query?.pagina || 1,
      tamanhoPagina: 10,
      totalItens: data.total,
      totalPaginas: Math.ceil(data.total / 10),
    };
  });
  vi.mocked(modulosApi.listar).mockImplementation(async (query, signal) => {
    const data = await load(query, signal);
    return { items: data.rows, totalCount: data.total };
  });
  vi.mocked(ramosApi.listar).mockImplementation(async (query, signal) => {
    const data = await load(query, signal);
    return { items: data.rows, totalCount: data.total };
  });
  for (const api of [corretorasApi, modulosApi, ramosApi, seguradorasApi]) {
    vi.mocked(api.inativar).mockImplementation((id) =>
      mutation(id, 'inativar'),
    );
  }
  for (const api of [corretorasApi, ramosApi, seguradorasApi]) {
    vi.mocked(api.reativar).mockImplementation((id) =>
      mutation(id, 'reativar'),
    );
  }
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      permissoes: permitted
        ? ['inserir', 'alterar', 'inativar', 'reativar'].map(
            (action) => `${kind}.${action}`,
          )
        : [],
    }),
  );
  return { load, mutation };
}
function Page({ kind }: { kind: Kind }) {
  const Component = pages[kind];
  return (
    <MemoryRouter>
      <Component />
    </MemoryRouter>
  );
}
async function flush() {
  await act(async () => {});
}
async function tick(ms = 500) {
  await act(async () => {
    await vi.advanceTimersByTimeAsync(ms);
  });
}
const status = () => screen.getByRole('combobox', { name: 'Status' });
const search = () => screen.getByRole<HTMLInputElement>('textbox');
const busy = () => document.querySelector('[aria-busy="true"]');
async function action(
  kind: Kind,
  operation: 'Inativar' | 'Reativar' = 'Inativar',
) {
  fireEvent.click(screen.getByRole('button', { name: 'Mais ações' }));
  fireEvent.click(screen.getByRole('menuitem', { name: operation }));
  if (kind === 'corretoras' || kind === 'seguradoras') {
    fireEvent.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: operation === 'Inativar' ? 'Sim, inativar' : 'Sim, reativar',
      }),
    );
  }
  await flush();
}
beforeEach(() => {
  vi.resetAllMocks();
  vi.useFakeTimers();
  vi.stubGlobal('confirm', vi.fn().mockReturnValue(true));
  vi.stubGlobal('alert', vi.fn());
});
afterEach(() => {
  vi.restoreAllMocks();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe.each(['corretoras', 'modulos', 'ramos', 'seguradoras'] as const)(
  'Consultas de %s',
  (kind) => {
    it('inicia carregando, envia filtros e sinal, exibe total e registros', async () => {
      const { load } = prepare(kind);
      const pending = deferred<Result>();
      load.mockReturnValueOnce(pending.promise);
      render(<Page kind={kind} />);
      expect(busy()).toBeTruthy();
      expect(load).toHaveBeenCalledExactlyOnceWith(
        { pagina: 1, tamanhoPagina: 10, busca: undefined, ativo: undefined },
        expect.any(AbortSignal),
      );
      await act(async () => pending.resolve(response()));
      expect(screen.getByText('Registro Atual')).toBeTruthy();
      expect(screen.getByText('Exibindo 1–10 de 25 registros')).toBeTruthy();
      expect(busy()).toBeNull();
    });

    it.each([false, true])(
      'ignora término antigo após trocar filtros e mantém carregamento atual (falha: %s)',
      async (failure) => {
        const { load } = prepare(kind);
        const old = deferred<Result>();
        const current = deferred<Result>();
        load
          .mockReturnValueOnce(old.promise)
          .mockReturnValueOnce(current.promise);
        render(<Page kind={kind} />);
        fireEvent.change(status(), { target: { value: 'false' } });
        expect(load.mock.calls[0][1]?.aborted).toBe(true);
        expect(load.mock.calls[1][0]?.ativo).toBe(false);
        await act(async () => {
          if (failure) old.reject(new Error('Falha antiga'));
          else old.resolve(response('Registro Antigo'));
        });
        expect(screen.queryByText('Registro Antigo')).toBeNull();
        expect(screen.queryByRole('alert')).toBeNull();
        expect(busy()).toBeTruthy();
        await act(async () =>
          current.resolve(response('Registro Novo', false, 10)),
        );
        expect(screen.getByText('Registro Novo')).toBeTruthy();
        expect(screen.getByText('Exibindo 1–10 de 10 registros')).toBeTruthy();
        expect(busy()).toBeNull();
      },
    );

    it('oculta resultados anteriores na paginação e reinicia a página ao alterar o status', async () => {
      const { load } = prepare(kind);
      render(<Page kind={kind} />);
      await flush();
      const pending = deferred<Result>();
      load.mockReturnValueOnce(pending.promise);
      fireEvent.click(
        screen.getByRole('button', { name: 'Ir para a página 2' }),
      );
      expect(load.mock.calls[1][0]?.pagina).toBe(2);
      expect(screen.queryByText('Registro Atual')).toBeNull();
      expect(busy()).toBeTruthy();
      await act(async () => pending.resolve(response('Segunda Página')));
      expect(screen.getByText('Exibindo 11–20 de 25 registros')).toBeTruthy();
      fireEvent.change(status(), { target: { value: 'true' } });
      await flush();
      expect(load.mock.calls[2][0]).toMatchObject({ pagina: 1, ativo: true });
    });

    it('preserva debounce, busca e regras existentes de espaços, e limpa filtros', async () => {
      const { load } = prepare(kind);
      render(<Page kind={kind} />);
      await flush();
      fireEvent.click(
        screen.getByRole('button', { name: 'Ir para a página 2' }),
      );
      await flush();
      fireEvent.change(search(), { target: { value: 'JO' } });
      await tick(499);
      expect(load).toHaveBeenCalledTimes(2);
      fireEvent.change(search(), { target: { value: '  JOÃO  ' } });
      await tick(499);
      expect(load).toHaveBeenCalledTimes(2);
      await tick(1);
      expect(load.mock.calls[2][0]).toMatchObject({
        pagina: 1,
        busca:
          kind === 'corretoras' || kind === 'seguradoras' ? 'JOÃO' : '  JOÃO  ',
      });
      fireEvent.change(status(), { target: { value: 'false' } });
      await flush();
      fireEvent.click(screen.getByRole('button', { name: 'Limpar' }));
      await flush();
      expect(search().value).toBe('');
      expect(load.mock.calls.at(-1)?.[0]).toEqual({
        pagina: 1,
        tamanhoPagina: 10,
        busca: undefined,
        ativo: undefined,
      });
      expect(
        screen.getByRole<HTMLButtonElement>('button', {
          name: 'Limpar',
        }).disabled,
      ).toBe(true);
    });

    it('mantém false, true e ausência de status sem repetir em rerender equivalente', async () => {
      const { load } = prepare(kind);
      const { rerender } = render(<Page kind={kind} />);
      await flush();
      for (const [input, expected] of [
        ['false', false],
        ['true', true],
        ['', undefined],
      ] as const) {
        fireEvent.change(status(), { target: { value: input } });
        await flush();
        expect(load.mock.calls.at(-1)?.[0]?.ativo).toBe(expected);
      }
      rerender(<Page kind={kind} />);
      await flush();
      expect(load).toHaveBeenCalledTimes(4);
    });

    it('mostra erro, limpa erro ao trocar filtro e tenta novamente com os filtros atuais', async () => {
      const { load } = prepare(kind);
      load
        .mockRejectedValueOnce(null)
        .mockRejectedValueOnce(new Error('Falha Atual'));
      render(<Page kind={kind} />);
      await flush();
      expect(screen.getByRole('alert')).toBeTruthy();
      expect(screen.queryByRole('table')).toBeNull();
      fireEvent.change(status(), { target: { value: 'false' } });
      expect(screen.queryByRole('alert')).toBeNull();
      await flush();
      fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
      await flush();
      expect(screen.queryByRole('alert')).toBeNull();
      expect(screen.getByText('Registro Atual')).toBeTruthy();
      expect(load).toHaveBeenCalledTimes(3);
      expect(load.mock.calls[2][0]?.ativo).toBe(false);
      expect(load.mock.calls[1][1]?.aborted).toBe(true);
    });

    it('apresenta vazio sem alerta quando a consulta retorna sem registros', async () => {
      const { load } = prepare(kind);
      load.mockResolvedValueOnce({ rows: [], total: 0 });
      render(<Page kind={kind} />);
      await flush();
      expect(
        screen.getByText(
          kind === 'modulos'
            ? 'Nenhum módulo encontrado'
            : kind === 'ramos'
              ? 'Nenhum ramo encontrado'
              : kind === 'corretoras'
                ? 'Nenhuma corretora encontrada'
                : 'Nenhuma seguradora encontrada',
        ),
      ).toBeTruthy();
      expect(screen.queryByRole('alert')).toBeNull();
      expect(
        screen.queryByRole('button', { name: 'Ir para a próxima página' }),
      ).toBeNull();
    });

    it.each([false, true])(
      'aborta ao desmontar e ignora conclusão tardia (falha: %s)',
      async (failure) => {
        const { load } = prepare(kind);
        const pending = deferred<Result>();
        load.mockReturnValueOnce(pending.promise);
        const { unmount } = render(<Page kind={kind} />);
        unmount();
        expect(load.mock.calls[0][1]?.aborted).toBe(true);
        await act(async () => {
          if (failure) pending.reject(new Error('Falha tardia'));
          else pending.resolve(response());
        });
        expect(load).toHaveBeenCalledTimes(1);
        expect(screen.queryByRole('alert')).toBeNull();
      },
    );

    it('atualiza os filtros atuais após inativação iniciada antes de mudar o filtro', async () => {
      const { load, mutation } = prepare(kind);
      const pending = deferred<void>();
      mutation.mockReturnValueOnce(pending.promise);
      render(<Page kind={kind} />);
      await flush();
      await action(kind);
      expect(mutation).toHaveBeenCalledExactlyOnceWith(
        'registro-1',
        'inativar',
      );
      fireEvent.change(status(), { target: { value: 'false' } });
      await flush();
      expect(load).toHaveBeenCalledTimes(2);
      await act(async () => pending.resolve());
      expect(load).toHaveBeenCalledTimes(3);
      expect(load.mock.calls[2][0]).toMatchObject({ pagina: 1, ativo: false });
      expect(load.mock.calls[1][1]?.aborted).toBe(true);
    });

    it('preserva dados em atualização e permite recuperar falha da consulta após inativar', async () => {
      const { load } = prepare(kind);
      render(<Page kind={kind} />);
      await flush();
      const pending = deferred<Result>();
      load.mockReturnValueOnce(pending.promise);
      await action(kind);
      expect(screen.getByText('Registro Atual')).toBeTruthy();
      await act(async () => pending.reject(new Error('Falha de atualização')));
      expect(screen.getByRole('alert')).toBeTruthy();
      expect(screen.getByText('Registro Atual')).toBeTruthy();
      fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
      await flush();
      expect(screen.queryByRole('alert')).toBeNull();
      expect(load).toHaveBeenCalledTimes(3);
    });

    it('mantém falha da alteração de status separada da consulta e não recarrega ao falhar', async () => {
      vi.spyOn(console, 'error').mockImplementation(() => {});
      const { load, mutation } = prepare(kind);
      mutation.mockRejectedValueOnce(new Error('Falha'));
      render(<Page kind={kind} />);
      await flush();
      await action(kind);
      expect(load).toHaveBeenCalledTimes(1);
      if (kind === 'corretoras' || kind === 'seguradoras') {
        expect(screen.getByRole('alert').textContent).toContain(
          'Falha na operação',
        );
        expect(
          screen.queryByRole('button', { name: 'Tentar novamente' }),
        ).toBeNull();
        expect(screen.getByRole('dialog')).toBeTruthy();
      } else expect(window.alert).toHaveBeenCalledOnce();
    });

    if (kind !== 'modulos')
      it('preserva reativação e recarrega mantendo o filtro de inativos', async () => {
        const { load, mutation } = prepare(kind);
        load.mockResolvedValue(response('Registro Atual', false));
        render(<Page kind={kind} />);
        await flush();
        fireEvent.change(status(), { target: { value: 'false' } });
        await flush();
        await action(kind, 'Reativar');
        expect(mutation).toHaveBeenCalledExactlyOnceWith(
          'registro-1',
          'reativar',
        );
        expect(load.mock.calls.at(-1)?.[0]?.ativo).toBe(false);
        expect(load).toHaveBeenCalledTimes(3);
      });

    it('conserva as permissões para criação e alterações de status', async () => {
      prepare(kind, false);
      render(<Page kind={kind} />);
      await flush();
      expect(screen.queryByRole('button', { name: /^Nov[oa] / })).toBeNull();
      expect(screen.queryByRole('button', { name: 'Mais ações' })).toBeNull();
    });

    it('cancela a primeira consulta no StrictMode e aceita só a atual', async () => {
      const { load } = prepare(kind);
      const pending = deferred<Result>();
      load.mockReturnValueOnce(pending.promise);
      render(
        <StrictMode>
          <Page kind={kind} />
        </StrictMode>,
      );
      await flush();
      expect(load).toHaveBeenCalledTimes(2);
      expect(load.mock.calls[0][1]?.aborted).toBe(true);
      await act(async () => pending.resolve(response('Registro Antigo')));
      expect(screen.queryByText('Registro Antigo')).toBeNull();
      expect(screen.getByText('Registro Atual')).toBeTruthy();
    });

    it.each([
      { theme: 'claro', dark: false, resolved: 'light' },
      { theme: 'escuro', dark: false, resolved: 'dark' },
      { theme: 'sistema', dark: false, resolved: 'light' },
      { theme: 'sistema', dark: true, resolved: 'dark' },
    ])(
      'carrega, apresenta falha e recupera no tema $theme / $resolved',
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
        const { load } = prepare(kind);
        load.mockRejectedValueOnce(new Error('Falha'));
        render(
          <ThemeProvider>
            <Page kind={kind} />
          </ThemeProvider>,
        );
        await flush();
        expect(screen.getByRole('alert')).toBeTruthy();
        fireEvent.click(
          screen.getByRole('button', { name: 'Tentar novamente' }),
        );
        await flush();
        expect(screen.getByText('Registro Atual')).toBeTruthy();
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      },
    );
  },
);
