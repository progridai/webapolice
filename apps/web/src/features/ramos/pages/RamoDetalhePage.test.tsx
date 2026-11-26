import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import {
  MemoryRouter,
  Outlet,
  Route,
  Routes,
  useNavigate,
} from 'react-router-dom';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { HttpApiError } from '../../../services/http/httpError';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { ramosApi, type RamoDto } from '../api/ramos.api';
import { RamosRoutes } from '../routes/ramos.routes';

vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../api/ramos.api');
vi.mock('../../../layouts/AuthenticatedLayout', () => ({
  AuthenticatedLayout: () => <Outlet />,
}));
vi.mock('./RamoFormPage', () => ({
  RamoFormPage: () => <p>Edição do ramo</p>,
}));

const id = '7c9c5c49-b5c0-48b4-839b-b5ada399d167';
const outroId = '1d2cd217-684a-44cb-a04b-8318a7db9746';
const ramo: RamoDto = {
  publicId: id,
  codigo: 'aBc-009',
  nome: 'Seguro de Vida',
  descricao: 'Cobertura do Titular',
  ativo: true,
  createdAt: '2026-11-18T12:00:00Z',
  updatedAt: '2026-11-24T12:00:00Z',
};

beforeEach(() => {
  localStorage.clear();
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      modulosHabilitados: ['APOLICES'],
      permissoes: ['ramos.visualizar'],
    }),
  );
  vi.mocked(ramosApi.obter).mockReset().mockResolvedValue(ramo);
  vi.mocked(ramosApi.listar)
    .mockReset()
    .mockResolvedValue({ items: [ramo], totalCount: 1 });
});

function TrocarRamo() {
  const navigate = useNavigate();
  return (
    <button onClick={() => navigate(`/ramos/${outroId}`)}>Trocar ramo</button>
  );
}

function renderPage(path = `/ramos/${id}`) {
  return render(
    <ThemeProvider>
      <MemoryRouter initialEntries={[path]}>
        <TrocarRamo />
        <Routes>
          {RamosRoutes}
          <Route path="/forbidden" element={<p>Rota bloqueada</p>} />
        </Routes>
      </MemoryRouter>
    </ThemeProvider>,
  );
}

it.each(['claro', 'escuro', 'sistema'])(
  'abre os detalhes a partir de Visualizar no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    renderPage('/ramos');
    fireEvent.click(await screen.findByRole('button', { name: 'Visualizar' }));
    expect(
      await screen.findByRole('heading', { name: ramo.nome }),
    ).toBeTruthy();
    expect(screen.getByText('aBc-009')).toBeTruthy();
    expect(screen.getByText(ramo.descricao ?? '')).toBeTruthy();
    expect(screen.getByText('Ativo')).toBeTruthy();
    expect(screen.getByText('18/11/2026')).toBeTruthy();
    expect(screen.getByText('24/11/2026')).toBeTruthy();
    await waitFor(() =>
      expect(document.activeElement).toBe(
        screen
          .getByRole('heading', { name: ramo.nome })
          .closest('[tabindex="-1"]'),
      ),
    );
    expect(screen.queryByRole('button', { name: 'Editar ramo' })).toBeNull();
    expect(ramosApi.obter).toHaveBeenCalledWith(id, expect.any(AbortSignal));
    fireEvent.click(screen.getByRole('button', { name: 'Voltar para ramos' }));
    expect(
      await screen.findByRole('heading', { name: 'Catálogo de Ramos' }),
    ).toBeTruthy();
  },
);

it('exibe ramo inativo e descrição não informada', async () => {
  vi.mocked(ramosApi.obter).mockResolvedValue({
    ...ramo,
    ativo: false,
    descricao: undefined,
  });
  renderPage();
  expect(await screen.findByText('Inativo')).toBeTruthy();
  expect(screen.getByText('Não informado')).toBeTruthy();
});

it('oculta o código quando repete o nome, sem alterar o valor recebido', async () => {
  vi.mocked(ramosApi.obter).mockResolvedValue({
    ...ramo,
    codigo: 'SEGURO DE VIDA',
  });
  renderPage();
  expect(await screen.findByRole('heading', { name: ramo.nome })).toBeTruthy();
  expect(screen.queryByText('Código:')).toBeNull();
  expect(screen.queryByText('SEGURO DE VIDA')).toBeNull();
});

it.each([false, true])(
  'abre edição com permissão ou acesso total (%s)',
  async (acessoTotal) => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        modulosHabilitados: ['APOLICES'],
        acessoTotal,
        permissoes: [
          'ramos.visualizar',
          ...(acessoTotal ? [] : ['ramos.alterar']),
        ],
      }),
    );
    renderPage();
    fireEvent.click(await screen.findByRole('button', { name: 'Editar ramo' }));
    expect(await screen.findByText('Edição do ramo')).toBeTruthy();
  },
);

it('permite tentar novamente após falha de consulta', async () => {
  vi.mocked(ramosApi.obter).mockRejectedValueOnce(new Error('Falha de rede'));
  renderPage();
  fireEvent.click(
    await screen.findByRole('button', { name: 'Tentar novamente' }),
  );
  expect(await screen.findByRole('heading', { name: ramo.nome })).toBeTruthy();
  expect(ramosApi.obter).toHaveBeenCalledTimes(2);
});

it.each([404, 403])(
  'trata HTTP %s sem oferecer edição ou nova tentativa',
  async (status) => {
    vi.mocked(ramosApi.obter).mockRejectedValue(
      new HttpApiError({ status, message: 'Erro da API' }),
    );
    renderPage();
    expect(
      await screen.findByText(
        status === 404 ? 'Ramo não encontrado' : 'Acesso negado',
      ),
    ).toBeTruthy();
    expect(
      screen.queryByRole('button', { name: 'Tentar novamente' }),
    ).toBeNull();
    expect(screen.queryByRole('button', { name: 'Editar ramo' })).toBeNull();
  },
);

it.each([
  { modulosHabilitados: ['APOLICES'], permissoes: [], acessoTotal: false },
  {
    modulosHabilitados: [],
    permissoes: ['ramos.visualizar'],
    acessoTotal: false,
  },
  { modulosHabilitados: [], permissoes: [], acessoTotal: true },
])(
  'bloqueia a rota antes da consulta sem acesso efetivo (%j)',
  async (acesso) => {
    vi.mocked(useAuthorization).mockReturnValue(makeAuthorization(acesso));
    renderPage();
    expect(await screen.findByText('Rota bloqueada')).toBeTruthy();
    expect(ramosApi.obter).not.toHaveBeenCalled();
  },
);

it('cancela ao trocar de ramo e descarta a resposta antiga', async () => {
  let resolver: ((value: RamoDto) => void) | undefined;
  const pendente = new Promise<RamoDto>((resolve) => {
    resolver = resolve;
  });
  vi.mocked(ramosApi.obter)
    .mockReturnValueOnce(pendente)
    .mockResolvedValueOnce({ ...ramo, publicId: outroId, nome: 'Outro Ramo' });
  const view = renderPage();
  expect(screen.getByRole('status', { name: 'Carregando ramo' })).toBeTruthy();
  const signal = vi.mocked(ramosApi.obter).mock.calls[0][1];
  fireEvent.click(screen.getByRole('button', { name: 'Trocar ramo' }));
  expect(
    await screen.findByRole('heading', { name: 'Outro Ramo' }),
  ).toBeTruthy();
  expect(signal?.aborted).toBe(true);
  await act(async () => {
    resolver?.(ramo);
    await pendente;
  });
  expect(screen.queryByRole('heading', { name: ramo.nome })).toBeNull();
  const atual = vi.mocked(ramosApi.obter).mock.calls[1][1];
  view.unmount();
  expect(atual?.aborted).toBe(true);
});

it('cancela a consulta ainda pendente ao sair da tela', async () => {
  let resolver: ((value: RamoDto) => void) | undefined;
  const pendente = new Promise<RamoDto>((resolve) => {
    resolver = resolve;
  });
  vi.mocked(ramosApi.obter).mockReturnValueOnce(pendente);
  renderPage();
  await waitFor(() => expect(ramosApi.obter).toHaveBeenCalledTimes(1));
  const signal = vi.mocked(ramosApi.obter).mock.calls[0][1];
  fireEvent.click(screen.getByRole('button', { name: 'Voltar para ramos' }));
  expect(signal?.aborted).toBe(true);
  await act(async () => {
    resolver?.(ramo);
    await pendente;
  });
  expect(screen.queryByRole('heading', { name: ramo.nome })).toBeNull();
  expect(
    await screen.findByRole('heading', { name: 'Catálogo de Ramos' }),
  ).toBeTruthy();
});
