import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes, useNavigate } from 'react-router-dom';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { seguradorasApi } from '../../seguradoras/api/seguradoras.api';
import type {
  PagedResult,
  SeguradoraListItem,
} from '../../seguradoras/types/seguradora.types';
import * as api from '../api/estipulantes.api';
import { EstipulantesTable } from '../components/EstipulantesTable';
import { EstipulantesMobileList } from '../components/EstipulantesMobileList';
import { CadastrarEstipulantePage } from './CadastrarEstipulantePage';
import { EditarEstipulantePage } from './EditarEstipulantePage';
import { EstipulanteDetalhePage } from './EstipulanteDetalhePage';

vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../api/estipulantes.api');
vi.mock('../../seguradoras/api/seguradoras.api');
vi.mock('../../clientes/api/localidadesApi', () => ({
  buscarCidadesPorUf: vi.fn().mockResolvedValue([]),
}));

const seguradoraId = '7c9c5c49-b5c0-48b4-839b-b5ada399d167';
const resposta: PagedResult<SeguradoraListItem> = {
  itens: [
    {
      id: 10,
      publicId: seguradoraId,
      nome: 'Seguradora de Proteção',
      ativo: true,
      createdAt: '2026-11-24',
    },
  ],
  paginaAtual: 1,
  tamanhoPagina: 20,
  totalItens: 1,
  totalPaginas: 1,
};
const estipulante = {
  publicId: 'estip-1',
  razaoSocial: 'Empresa Teste',
  cnpj: '61198164000160',
  ativo: true,
  grupoId: 19,
  seguradoraPublicId: seguradoraId,
  seguradoraNome: 'Seguradora Anterior',
};
beforeEach(() => {
  localStorage.clear();
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({ permissoes: ['seguradoras.visualizar'] }),
  );
  vi.mocked(seguradorasApi.listar).mockReset().mockResolvedValue(resposta);
  vi.mocked(api.obterEstipulante).mockReset().mockResolvedValue(estipulante);
  vi.mocked(api.obterConfiguracao)
    .mockReset()
    .mockResolvedValue({ dataInicioVigencia: '2026-01-01' });
  vi.mocked(api.cadastrarEstipulante)
    .mockReset()
    .mockResolvedValue({ publicId: 'estip-novo' });
  vi.mocked(api.alterarEstipulante).mockReset().mockResolvedValue(undefined);
});
function TrocarEstipulante() {
  const navigate = useNavigate();
  return (
    <button onClick={() => navigate('/estipulantes/outro/editar')}>
      Trocar estipulante
    </button>
  );
}
function abrir(tela: 'novo' | 'editar' | 'detalhes' = 'editar') {
  return render(
    <ThemeProvider>
      <MemoryRouter initialEntries={[`/estipulantes/estip-1/${tela}`]}>
        <TrocarEstipulante />
        <Routes>
          <Route
            path="/estipulantes/estip-1/novo"
            element={<CadastrarEstipulantePage />}
          />
          <Route
            path="/estipulantes/:publicId/editar"
            element={<EditarEstipulantePage />}
          />
          <Route
            path="/estipulantes/:publicId/detalhes"
            element={<EstipulanteDetalhePage />}
          />
          <Route path="/estipulantes" element={<p>Listagem</p>} />
        </Routes>
      </MemoryRouter>
    </ThemeProvider>,
  );
}
async function buscar() {
  const input = await screen.findByLabelText<HTMLInputElement>('Seguradora');
  fireEvent.focus(input);
  fireEvent.change(input, { target: { value: 'Proteção' } });
  return input;
}
it.each(['claro', 'escuro', 'sistema'])(
  'seleciona e envia a seguradora na criação no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    abrir('novo');
    expect(screen.queryByLabelText('Grupo')).toBeNull();
    fireEvent.change(screen.getByLabelText(/^Razão Social/), {
      target: { value: 'Empresa Nova' },
    });
    fireEvent.input(screen.getByLabelText(/^CNPJ/), {
      target: { value: '61198164000160' },
    });
    fireEvent.change(screen.getByLabelText(/^Início de Vigência/), {
      target: { value: '2026-01-01' },
    });
    await buscar();
    fireEvent.click(
      await screen.findByRole('button', { name: 'Seguradora de Proteção' }),
    );
    fireEvent.click(screen.getByRole('button', { name: 'Salvar Estipulante' }));
    await waitFor(() =>
      expect(api.cadastrarEstipulante).toHaveBeenCalledWith(
        expect.objectContaining({ seguradoraPublicId: seguradoraId }),
      ),
    );
    const enviado = vi.mocked(api.cadastrarEstipulante).mock.calls[0][0];
    expect(enviado).not.toHaveProperty('grupoPublicId');
    expect(enviado).not.toHaveProperty('grupoId');
    expect(seguradorasApi.listar).toHaveBeenCalledWith(
      { pagina: 1, tamanhoPagina: 20, busca: 'Proteção', ativo: true },
      expect.any(AbortSignal),
    );
  },
);
it('preserva a seguradora e o grupo legado ao salvar sem permissão de consulta', async () => {
  vi.mocked(useAuthorization).mockReturnValue(makeAuthorization());
  abrir();
  const input = await screen.findByLabelText<HTMLInputElement>('Seguradora');
  expect(input.value).toBe('Seguradora Anterior');
  expect(input.disabled).toBe(true);
  expect(screen.queryByLabelText('Grupo')).toBeNull();
  fireEvent.click(screen.getByRole('button', { name: 'Salvar Estipulante' }));
  await waitFor(() =>
    expect(api.alterarEstipulante).toHaveBeenCalledWith(
      'estip-1',
      expect.objectContaining({
        grupoId: 19,
        seguradoraPublicId: seguradoraId,
      }),
    ),
  );
  expect(seguradorasApi.listar).not.toHaveBeenCalled();
});
it('permite salvar sem seguradora', async () => {
  vi.mocked(api.obterEstipulante).mockResolvedValue({
    ...estipulante,
    seguradoraPublicId: undefined,
    seguradoraNome: undefined,
  });
  abrir();
  await screen.findByLabelText('Seguradora');
  fireEvent.click(screen.getByRole('button', { name: 'Salvar Estipulante' }));
  await waitFor(() =>
    expect(api.alterarEstipulante).toHaveBeenCalledWith(
      'estip-1',
      expect.objectContaining({ seguradoraPublicId: undefined, grupoId: 19 }),
    ),
  );
});

it('remove o vínculo somente ao salvar a remoção solicitada', async () => {
  abrir();
  fireEvent.click(
    await screen.findByRole('button', { name: 'Remover seguradora' }),
  );
  expect(api.alterarEstipulante).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Salvar Estipulante' }));
  await waitFor(() =>
    expect(api.alterarEstipulante).toHaveBeenCalledWith(
      'estip-1',
      expect.objectContaining({ grupoId: 19, seguradoraPublicId: undefined }),
    ),
  );
});

it('cancela a carga anterior sem misturar os vínculos de outro estipulante', async () => {
  let resolver: ((valor: typeof estipulante) => void) | undefined;
  const pendente = new Promise<typeof estipulante>((resolve) => {
    resolver = resolve;
  });
  vi.mocked(api.obterEstipulante)
    .mockReturnValueOnce(pendente)
    .mockResolvedValueOnce({
      ...estipulante,
      publicId: 'outro',
      grupoId: 27,
      seguradoraNome: 'Seguradora Atual',
    });
  abrir();
  const signal = vi.mocked(api.obterEstipulante).mock.calls[0][1];
  fireEvent.click(screen.getByRole('button', { name: 'Trocar estipulante' }));
  expect(await screen.findByDisplayValue('Seguradora Atual')).toBeTruthy();
  expect(signal?.aborted).toBe(true);
  await act(async () => {
    resolver?.(estipulante);
    await pendente;
  });
  expect(screen.queryByDisplayValue('Seguradora Anterior')).toBeNull();
  fireEvent.click(screen.getByRole('button', { name: 'Salvar Estipulante' }));
  await waitFor(() =>
    expect(api.alterarEstipulante).toHaveBeenCalledWith(
      'outro',
      expect.objectContaining({ grupoId: 27 }),
    ),
  );
});
it('troca a seguradora sem alterar o grupo legado', async () => {
  const novoId = '1d2cd217-684a-44cb-a04b-8318a7db9746';
  vi.mocked(seguradorasApi.listar).mockResolvedValue({
    ...resposta,
    itens: [{ ...resposta.itens[0], publicId: novoId }],
  });
  abrir();
  await buscar();
  fireEvent.click(
    await screen.findByRole('button', { name: 'Seguradora de Proteção' }),
  );
  fireEvent.click(screen.getByRole('button', { name: 'Salvar Estipulante' }));
  await waitFor(() =>
    expect(api.alterarEstipulante).toHaveBeenCalledWith(
      'estip-1',
      expect.objectContaining({ grupoId: 19, seguradoraPublicId: novoId }),
    ),
  );
});
it('mostra nome nos detalhes e retira Grupo da visualização', async () => {
  abrir('detalhes');
  expect(await screen.findByText('Seguradora Anterior')).toBeTruthy();
  expect(screen.queryByText(seguradoraId)).toBeNull();
  expect(screen.queryByText('Grupo:')).toBeNull();
});
it.each(['claro', 'escuro', 'sistema'])(
  'preserva o dia das datas civis de vigência no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    vi.mocked(api.obterConfiguracao).mockResolvedValue({
      dataInicioVigencia: '2026-01-01',
      dataFimVigencia: '2027-12-31',
    });
    abrir('detalhes');
    expect(await screen.findByText('01/01/2026')).toBeTruthy();
    expect(screen.getByText('31/12/2027')).toBeTruthy();
    expect(screen.queryByText('31/12/2025')).toBeNull();
    expect(screen.queryByText('30/12/2027')).toBeNull();
  },
);
it.each(['claro', 'escuro', 'sistema'])(
  'omite Grupo nas consultas desktop e móvel no tema %s, preservando o registro',
  (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const registro = {
      ...estipulante,
      id: 1,
      grupo: 'Grupo Histórico',
      dataCadastro: '2026-01-01',
    };
    render(
      <ThemeProvider>
        <EstipulantesTable
          estipulantes={[registro]}
          isLoading={false}
          onSort={vi.fn()}
        />
        <EstipulantesMobileList estipulantes={[registro]} />
      </ThemeProvider>,
    );
    expect(screen.queryByRole('columnheader', { name: 'Grupo' })).toBeNull();
    expect(screen.queryByText(/Grupo Histórico/)).toBeNull();
    expect(screen.getAllByText('Empresa Teste')).toHaveLength(2);
    expect(registro.grupo).toBe('Grupo Histórico');
  },
);
it('trata busca vazia e permite tentar novamente após falha', async () => {
  vi.mocked(seguradorasApi.listar)
    .mockRejectedValueOnce(new Error('Falha de rede'))
    .mockResolvedValueOnce({ ...resposta, itens: [], totalItens: 0 });
  abrir();
  await buscar();
  fireEvent.click(
    await screen.findByRole('button', { name: 'Tentar novamente' }),
  );
  expect(
    await screen.findByText(
      'Nenhuma seguradora ativa encontrada. Refine a busca.',
    ),
  ).toBeTruthy();
});
it('cancela a busca pendente ao sair e descarta sua resposta', async () => {
  let resolver: ((valor: PagedResult<SeguradoraListItem>) => void) | undefined;
  const pendente = new Promise<PagedResult<SeguradoraListItem>>((resolve) => {
    resolver = resolve;
  });
  vi.mocked(seguradorasApi.listar).mockReturnValueOnce(pendente);
  const view = abrir();
  await buscar();
  await waitFor(() => expect(seguradorasApi.listar).toHaveBeenCalledOnce());
  const signal = vi.mocked(seguradorasApi.listar).mock.calls[0][1];
  view.unmount();
  expect(signal?.aborted).toBe(true);
  await act(async () => {
    resolver?.(resposta);
    await pendente;
  });
  expect(screen.queryByText('Seguradora de Proteção')).toBeNull();
});
