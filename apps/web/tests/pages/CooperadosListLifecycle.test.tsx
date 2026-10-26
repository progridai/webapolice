import { act, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, useNavigate } from 'react-router-dom';
import { CooperadosListPage } from '../../src/features/cooperados/pages/CooperadosListPage';
import { listarCooperados } from '../../src/features/cooperados/api/cooperadosApi';
import { useAuthorization } from '../../src/auth/AuthorizationProvider';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';
import { makeAuthorization } from '../helpers/authorization';

vi.mock('../../src/features/cooperados/api/cooperadosApi');
vi.mock('../../src/auth/AuthorizationProvider');
type Result = Awaited<ReturnType<typeof listarCooperados>>;
const page: Result = {
  itens: [
    {
      publicId: 'cooperado-1',
      nome: 'Cooperado Atual',
      cpfMascarado: '***.123.***-**',
      tipo: 1,
      desativado: false,
      dataCadastroUtc: '2026-10-22',
    },
  ],
  totalItens: 25,
  totalPaginas: 2,
  paginaAtual: 1,
  tamanhoPagina: 20,
};
const originalWidth = window.innerWidth;
function width(value: number) {
  Object.defineProperty(window, 'innerWidth', { configurable: true, value });
}
function deferred() {
  let resolve!: (data: Result) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<Result>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}
function NavigateQuery() {
  const navigate = useNavigate();
  return (
    <button onClick={() => navigate('/cooperados?nome=Nova&status=2')}>
      Mudar consulta pela URL
    </button>
  );
}
function Page({
  url = '/cooperados',
  navigation = false,
}: {
  url?: string;
  navigation?: boolean;
}) {
  return (
    <MemoryRouter initialEntries={[url]}>
      {navigation && <NavigateQuery />}
      <CooperadosListPage />
    </MemoryRouter>
  );
}
async function flush() {
  await act(async () => {});
}
async function tick() {
  await act(async () => {
    await vi.advanceTimersByTimeAsync(500);
  });
}
const search = () =>
  screen.getByPlaceholderText<HTMLInputElement>('Nome, Código ou CPF');

beforeEach(() => {
  vi.resetAllMocks();
  vi.useFakeTimers();
  width(1024);
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      permissoes: ['cooperados.inserir', 'cooperados.alterar'],
    }),
  );
  vi.mocked(listarCooperados).mockImplementation(async (query) => ({
    ...page,
    paginaAtual: query.page,
    tamanhoPagina: query.limit,
    totalPaginas: Math.ceil(page.totalItens / query.limit),
  }));
});
afterEach(() => {
  width(originalWidth);
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe('Página de Cooperados com hook real', () => {
  it('usa filtros da URL e preserva ordenação e paginação', async () => {
    render(
      <Page url="/cooperados?page=2&limit=10&nome=Ana&cpf=123&status=1&sortBy=nome&direction=desc" />,
    );
    await flush();
    expect(listarCooperados).toHaveBeenCalledWith(
      {
        page: 2,
        limit: 10,
        nome: 'Ana',
        cpf: '123',
        status: '1',
        sortBy: 'nome',
        direction: 'desc',
      },
      expect.any(AbortSignal),
    );
    expect(screen.getByText('Exibindo 11–20 de 25 registros')).toBeTruthy();
    fireEvent.click(screen.getByRole('button', { name: 'Nome' }));
    await flush();
    expect(vi.mocked(listarCooperados).mock.calls.at(-1)?.[0]).toMatchObject({
      page: 2,
      sortBy: 'nome',
      direction: 'asc',
    });
    fireEvent.click(screen.getByRole('button', { name: 'Ir para a página 3' }));
    await flush();
    expect(vi.mocked(listarCooperados).mock.calls.at(-1)?.[0].page).toBe(3);
    expect(screen.getByText('Exibindo 21–25 de 25 registros')).toBeTruthy();
  });

  it('mantém busca por nome/CPF, status e limpeza usando os componentes reais', async () => {
    render(<Page />);
    await flush();
    fireEvent.change(search(), { target: { value: '  Maria da SILVA  ' } });
    expect(listarCooperados).toHaveBeenCalledTimes(1);
    await tick();
    expect(vi.mocked(listarCooperados).mock.calls.at(-1)?.[0]).toMatchObject({
      page: 1,
      nome: 'Maria da SILVA',
      cpf: '',
    });
    fireEvent.change(search(), { target: { value: '123.456.789-09' } });
    await tick();
    expect(vi.mocked(listarCooperados).mock.calls.at(-1)?.[0]).toMatchObject({
      page: 1,
      nome: '',
      cpf: '123.456.789-09',
    });
    fireEvent.change(screen.getByRole('combobox', { name: 'Situação' }), {
      target: { value: '2' },
    });
    await flush();
    expect(vi.mocked(listarCooperados).mock.calls.at(-1)?.[0].status).toBe('2');
    fireEvent.click(screen.getByRole('button', { name: 'Limpar filtros' }));
    await flush();
    expect(search().value).toBe('');
    expect(vi.mocked(listarCooperados).mock.calls.at(-1)?.[0]).toMatchObject({
      page: 1,
      limit: 20,
      nome: '',
      cpf: '',
      status: '',
    });
  });

  it('não exibe resposta cancelada após navegação para outros filtros', async () => {
    const old = deferred();
    const current = deferred();
    vi.mocked(listarCooperados)
      .mockReturnValueOnce(old.promise)
      .mockReturnValueOnce(current.promise);
    render(<Page navigation />);
    fireEvent.click(
      screen.getByRole('button', { name: 'Mudar consulta pela URL' }),
    );
    expect(vi.mocked(listarCooperados).mock.calls[0][1]?.aborted).toBe(true);
    await act(async () =>
      old.resolve({
        ...page,
        itens: [{ ...page.itens[0], nome: 'Cooperado Antigo' }],
      }),
    );
    expect(screen.queryByText('Cooperado Antigo')).toBeNull();
    expect(document.querySelector('[aria-busy="true"]')).toBeTruthy();
    await act(async () => current.resolve(page));
    expect(screen.getByText('Cooperado Atual')).toBeTruthy();
    expect(vi.mocked(listarCooperados).mock.calls[1][0]).toMatchObject({
      nome: 'Nova',
      status: '2',
    });
  });

  it('não oferece criação/edição sem permissão e conserva leitura', async () => {
    vi.mocked(useAuthorization).mockReturnValue(makeAuthorization());
    render(<Page />);
    await flush();
    expect(screen.getByText('Cooperado Atual')).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Novo Cooperado' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Mais ações' })).toBeNull();
    expect(screen.getByRole('button', { name: 'Detalhes' })).toBeTruthy();
  });

  it('aborta a nova tentativa pendente ao sair da página', async () => {
    const pending = deferred();
    vi.mocked(listarCooperados)
      .mockRejectedValueOnce(new Error('Falha'))
      .mockReturnValueOnce(pending.promise);
    const { unmount } = render(<Page />);
    await flush();
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
    unmount();
    expect(vi.mocked(listarCooperados).mock.calls[1][1]?.aborted).toBe(true);
    await act(async () => pending.reject(new Error('Falha Tardia')));
    expect(screen.queryByRole('alert')).toBeNull();
  });

  it.each([1024, 390])(
    'exibe vazio sem erro na largura %s',
    async (viewport) => {
      width(viewport);
      vi.mocked(listarCooperados).mockResolvedValueOnce({
        ...page,
        itens: [],
        totalItens: 0,
        totalPaginas: 0,
      });
      render(<Page />);
      await flush();
      expect(screen.getByText('Nenhum cooperado cadastrado')).toBeTruthy();
      expect(screen.queryByRole('alert')).toBeNull();
    },
  );
});

describe.each([1024, 390])('Página de Cooperados na largura %s', (viewport) => {
  it.each([
    { theme: 'claro', dark: false, resolved: 'light' },
    { theme: 'escuro', dark: false, resolved: 'dark' },
    { theme: 'sistema', dark: false, resolved: 'light' },
    { theme: 'sistema', dark: true, resolved: 'dark' },
  ])(
    'carrega, apresenta falha e recupera no tema $theme / $resolved',
    async ({ theme, dark, resolved }) => {
      width(viewport);
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const pending = deferred();
      vi.mocked(listarCooperados).mockReturnValueOnce(pending.promise);
      render(
        <ThemeProvider>
          <Page url="/cooperados?nome=Pessoa&status=1" />
        </ThemeProvider>,
      );
      expect(document.querySelector('[aria-busy="true"]')).toBeTruthy();
      await act(async () => pending.reject(new Error('Falha Temporária')));
      expect(screen.getByRole('alert').textContent).toContain(
        'Falha Temporária',
      );
      fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
      await flush();
      expect(screen.getByText('Cooperado Atual')).toBeTruthy();
      expect(screen.queryByRole('alert')).toBeNull();
      expect(vi.mocked(listarCooperados).mock.calls[1][0]).toMatchObject({
        nome: 'Pessoa',
        status: '1',
      });
      expect(
        screen.getByRole('button', { name: 'Novo Cooperado' }),
      ).toBeTruthy();
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
      if (viewport === 390)
        expect(
          screen.getByRole('heading', { name: 'Cooperado Atual' }),
        ).toBeTruthy();
      else
        expect(
          screen.getByRole('table', { name: 'Lista de cooperados' }),
        ).toBeTruthy();
    },
  );
});
