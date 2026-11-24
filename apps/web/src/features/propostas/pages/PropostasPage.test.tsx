import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import {
  listarApolices,
  listarApoliceVidas,
} from '../../apolices/api/apolices.api';
import type {
  ApoliceListItem,
  ApoliceVidaListItem,
} from '../../apolices/types/apolice.types';
import { criarPropostaDaVida } from '../api/propostas.api';
import { PropostasPage } from './PropostasPage';
vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../../apolices/api/apolices.api');
vi.mock('../api/propostas.api');
vi.mock('../components/PropostasConsulta', () => ({
  PropostasConsulta: ({ initialPublicId }: { initialPublicId?: string }) => (
    <p>Consulta {initialPublicId ?? 'geral'}</p>
  ),
}));
const apolice: ApoliceListItem = {
  publicId: 'apolice-A',
  numeroPrincipal: 'aBc-101',
  estipulanteNome: 'Empresa A',
  seguradoraNome: 'Seguradora A',
  dataInicioVigencia: '2026-01-01',
  dataFimVigencia: '2027-12-31',
  status: 'ativa',
  ativo: true,
  quantidadeRamos: 1,
  resumoRamos: 'Vida',
};
const vida: ApoliceVidaListItem = {
  apoliceVidaPublicId: 'vida-A',
  clientePublicId: 'cliente-A',
  clienteNome: 'Maria da Silva',
  clienteVinculoPublicId: 'vinculo-A',
  vinculoMatricula: 'aBc-X009',
  apoliceModuloPublicId: 'modulo-A',
  moduloNome: 'Módulo A',
  subgrupoNome: 'Subgrupo A',
  ativo: true,
  status: 'ativa',
  possuiPropostas: false,
};
const apPage = { items: [apolice], totalCount: 1, page: 1, pageSize: 20 };
const vidaPage = { items: [vida], totalCount: 1, page: 1, pageSize: 20 };
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      modulosHabilitados: ['APOLICES'],
      recursosHabilitados: ['PROPOSTAS'],
      permissoes: [
        'propostas.visualizar',
        'propostas.inserir',
        'apolices.visualizar',
      ],
    }),
  );
  vi.mocked(listarApolices).mockReset().mockResolvedValue(apPage);
  vi.mocked(listarApoliceVidas).mockReset().mockResolvedValue(vidaPage);
  vi.mocked(criarPropostaDaVida).mockReset().mockResolvedValue('nova-UUID');
});
function abrir() {
  fireEvent.click(screen.getByRole('button', { name: 'Nova pré proposta' }));
}
async function selecionarApolice() {
  fireEvent.click(
    await screen.findByRole('button', {
      name: 'Selecionar apólice aBc-101 de Empresa A',
    }),
  );
}
async function selecionarVida() {
  fireEvent.click(
    await screen.findByRole('button', {
      name: 'Selecionar Vida de Maria da Silva, matrícula aBc-X009',
    }),
  );
}
function confirmar() {
  fireEvent.click(
    within(screen.getByRole('dialog')).getByRole('button', {
      name: 'Criar pré proposta',
    }),
  );
}
it.each(['claro', 'escuro', 'sistema'])(
  'cria pela página e abre o UUID retornado no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    render(
      <ThemeProvider>
        <PropostasPage />
      </ThemeProvider>,
    );
    expect(listarApolices).not.toHaveBeenCalled();
    abrir();
    await selecionarApolice();
    await selecionarVida();
    expect(
      within(screen.getByRole('dialog')).getByText('aBc-X009'),
    ).toBeTruthy();
    confirmar();
    expect(await screen.findByText('Consulta nova-UUID')).toBeTruthy();
    expect(criarPropostaDaVida).toHaveBeenCalledWith(
      'apolice-A',
      'vida-A',
      undefined,
    );
    expect(listarApolices).toHaveBeenCalledWith(
      { page: 1, pageSize: 20, busca: undefined, ativo: true },
      expect.any(AbortSignal),
    );
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      tema === 'escuro' ? 'dark' : 'light',
    );
  },
);
it.each(['permissao', 'modulo', 'recurso'])(
  'oculta criação sem %s e não consulta seleções',
  (caso) => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        modulosHabilitados: caso === 'modulo' ? [] : ['APOLICES'],
        recursosHabilitados: caso === 'recurso' ? [] : ['PROPOSTAS'],
        permissoes: [
          'propostas.visualizar',
          ...(caso === 'permissao' ? [] : ['propostas.inserir']),
        ],
      }),
    );
    render(<PropostasPage />);
    expect(
      screen.queryByRole('button', { name: 'Nova pré proposta' }),
    ).toBeNull();
    expect(listarApolices).not.toHaveBeenCalled();
  },
);
it('explica falta da consulta de Apólices sem mudar permissões nem chamar a API', async () => {
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      modulosHabilitados: ['APOLICES'],
      recursosHabilitados: ['PROPOSTAS'],
      permissoes: ['propostas.visualizar', 'propostas.inserir'],
    }),
  );
  render(<PropostasPage />);
  abrir();
  expect(
    await screen.findByText('Consulta de Apólices necessária'),
  ).toBeTruthy();
  expect(listarApolices).not.toHaveBeenCalled();
  fireEvent.click(
    screen.getByRole('button', { name: 'Cancelar nova pré proposta' }),
  );
  expect(screen.getByText('Consulta geral')).toBeTruthy();
});
it('não permite primeira proposta para Vida inativa, sem vínculo ou com histórico', async () => {
  vi.mocked(listarApoliceVidas).mockResolvedValue({
    ...vidaPage,
    totalCount: 3,
    items: [
      {
        ...vida,
        apoliceVidaPublicId: 'inativa',
        clienteNome: 'Inativa',
        ativo: false,
      },
      {
        ...vida,
        apoliceVidaPublicId: 'sem-vinculo',
        clienteNome: 'Sem Vínculo',
        clienteVinculoPublicId: null,
      },
      {
        ...vida,
        apoliceVidaPublicId: 'com-proposta',
        clienteNome: 'Com Proposta',
        possuiPropostas: true,
      },
    ],
  });
  render(<PropostasPage />);
  abrir();
  await selecionarApolice();
  await screen.findByText('Vida inativa');
  for (const button of screen.getAllByRole('button', {
    name: /Selecionar Vida de/,
  }))
    expect(button).toHaveProperty('disabled', true);
  expect(screen.getByText(/Já possui proposta; consulte/)).toBeTruthy();
  expect(criarPropostaDaVida).not.toHaveBeenCalled();
});
it('busca e pagina Apólices e Vidas antes de criar com os UUIDs selecionados', async () => {
  const segunda = { ...apolice, publicId: 'apolice-B', numeroPrincipal: 'B2' };
  vi.mocked(listarApolices)
    .mockResolvedValueOnce({ ...apPage, totalCount: 21 })
    .mockResolvedValueOnce({
      ...apPage,
      page: 2,
      totalCount: 21,
      items: [segunda],
    });
  vi.mocked(listarApoliceVidas)
    .mockResolvedValueOnce({ ...vidaPage, totalCount: 21 })
    .mockResolvedValueOnce({
      ...vidaPage,
      page: 2,
      totalCount: 21,
      items: [{ ...vida, apoliceVidaPublicId: 'vida-B', clienteNome: 'João' }],
    });
  render(<PropostasPage />);
  abrir();
  await screen.findByText('aBc-101');
  fireEvent.click(
    screen.getByRole('button', { name: 'Ir para a próxima página' }),
  );
  fireEvent.click(
    await screen.findByRole('button', {
      name: 'Selecionar apólice B2 de Empresa A',
    }),
  );
  await screen.findByText('Maria da Silva');
  fireEvent.click(
    screen.getByRole('button', { name: 'Ir para a próxima página' }),
  );
  fireEvent.click(
    await screen.findByRole('button', {
      name: 'Selecionar Vida de João, matrícula aBc-X009',
    }),
  );
  confirmar();
  await waitFor(() =>
    expect(criarPropostaDaVida).toHaveBeenCalledWith(
      'apolice-B',
      'vida-B',
      undefined,
    ),
  );
  expect(listarApoliceVidas).toHaveBeenLastCalledWith(
    'apolice-B',
    { page: 2, pageSize: 20, busca: undefined },
    expect.any(AbortSignal),
  );
});
it('consulta por texto e permite trocar a Apólice descartando consulta de Vidas pendente', async () => {
  let concluir: ((value: typeof vidaPage) => void) | undefined;
  vi.mocked(listarApoliceVidas).mockImplementation(
    () =>
      new Promise((resolve) => {
        concluir = resolve;
      }),
  );
  render(<PropostasPage />);
  abrir();
  await screen.findByText('aBc-101');
  fireEvent.change(screen.getByLabelText('Buscar Apólice'), {
    target: { value: ' Empresa ' },
  });
  fireEvent.click(screen.getByRole('button', { name: 'Pesquisar Apólices' }));
  await waitFor(() =>
    expect(listarApolices).toHaveBeenLastCalledWith(
      { page: 1, pageSize: 20, busca: 'Empresa', ativo: true },
      expect.any(AbortSignal),
    ),
  );
  await selecionarApolice();
  await waitFor(() => expect(listarApoliceVidas).toHaveBeenCalledOnce());
  const signal = vi.mocked(listarApoliceVidas).mock.calls[0][2];
  fireEvent.click(screen.getByRole('button', { name: 'Trocar Apólice' }));
  expect(signal?.aborted).toBe(true);
  await act(async () => concluir?.(vidaPage));
  expect(screen.queryByText('Maria da Silva')).toBeNull();
});
it('cancela a consulta pendente ao sair da criação e ignora resposta atrasada', async () => {
  let concluir: ((value: typeof apPage) => void) | undefined;
  vi.mocked(listarApolices).mockImplementation(
    () =>
      new Promise((resolve) => {
        concluir = resolve;
      }),
  );
  render(<PropostasPage />);
  abrir();
  await waitFor(() => expect(listarApolices).toHaveBeenCalledOnce());
  const signal = vi.mocked(listarApolices).mock.calls[0][1];
  fireEvent.click(
    screen.getByRole('button', { name: 'Cancelar nova pré proposta' }),
  );
  expect(signal?.aborted).toBe(true);
  await act(async () => concluir?.(apPage));
  expect(screen.queryByText('aBc-101')).toBeNull();
});
it('mostra erro, nova tentativa e estados vazios nas duas etapas', async () => {
  vi.mocked(listarApolices)
    .mockRejectedValueOnce(new Error('Sem conexão'))
    .mockResolvedValueOnce({ ...apPage, items: [], totalCount: 0 })
    .mockResolvedValueOnce(apPage);
  vi.mocked(listarApoliceVidas)
    .mockRejectedValueOnce(new Error('Falha nas Vidas'))
    .mockResolvedValueOnce({ ...vidaPage, items: [], totalCount: 0 });
  render(<PropostasPage />);
  abrir();
  expect(await screen.findByText('Sem conexão')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(
    await screen.findByText('Nenhuma Apólice ativa encontrada'),
  ).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Pesquisar Apólices' }));
  await selecionarApolice();
  expect(await screen.findByText('Falha nas Vidas')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(await screen.findByText('Nenhuma Vida encontrada')).toBeTruthy();
});
it('impede reenvio e saída durante a gravação e mantém contexto se o servidor recusar', async () => {
  let rejeitar: ((reason: Error) => void) | undefined;
  vi.mocked(criarPropostaDaVida)
    .mockImplementationOnce(
      () =>
        new Promise((_, reject) => {
          rejeitar = reject;
        }),
    )
    .mockResolvedValueOnce('outra-UUID');
  render(<PropostasPage />);
  abrir();
  await selecionarApolice();
  await selecionarVida();
  const confirmarBotao = within(screen.getByRole('dialog')).getByRole(
    'button',
    { name: 'Criar pré proposta' },
  );
  fireEvent.click(confirmarBotao);
  fireEvent.click(confirmarBotao);
  expect(criarPropostaDaVida).toHaveBeenCalledOnce();
  expect(
    screen.getByRole('button', { name: 'Cancelar nova pré proposta' }),
  ).toHaveProperty('disabled', true);
  fireEvent.keyDown(window, { key: 'Escape' });
  expect(screen.getByRole('dialog')).toBeTruthy();
  await act(async () =>
    rejeitar?.(new Error('A Vida foi alterada. Recarregue antes de criar.')),
  );
  expect(
    await screen.findByText('A Vida foi alterada. Recarregue antes de criar.'),
  ).toBeTruthy();
  expect(
    within(screen.getByRole('dialog')).getByText('Maria da Silva'),
  ).toBeTruthy();
  confirmar();
  expect(await screen.findByText('Consulta outra-UUID')).toBeTruthy();
});
