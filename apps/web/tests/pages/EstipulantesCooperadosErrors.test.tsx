import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import * as estip from '../../src/features/estipulantes/api/estipulantes.api';
import * as coop from '../../src/features/cooperados/api/cooperadosApi';
import { CadastrarEstipulantePage } from '../../src/features/estipulantes/pages/CadastrarEstipulantePage';
import { EditarEstipulantePage } from '../../src/features/estipulantes/pages/EditarEstipulantePage';
import { EstipulanteDetalhePage } from '../../src/features/estipulantes/pages/EstipulanteDetalhePage';
import { NovoCooperadoPage } from '../../src/features/cooperados/pages/NovoCooperadoPage';
import { EditarCooperadoPage } from '../../src/features/cooperados/pages/EditarCooperadoPage';
import { CooperadoDetalhePage } from '../../src/features/cooperados/pages/CooperadoDetalhePage';
import { buscarCidadesPorUf } from '../../src/features/clientes/api/localidadesApi';
import { useAuthorization } from '../../src/auth/AuthorizationProvider';
import { makeAuthorization } from '../helpers/authorization';
import { HttpApiError } from '../../src/services/http/httpError';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/features/estipulantes/api/estipulantes.api');
vi.mock('../../src/features/cooperados/api/cooperadosApi');
vi.mock('../../src/features/clientes/api/localidadesApi');
vi.mock('../../src/auth/AuthorizationProvider');

const empresa: Awaited<ReturnType<typeof estip.obterEstipulante>> = {
  publicId: 'estip-1',
  razaoSocial: 'Empresa Teste',
  cnpj: '61198164000160',
  ativo: true,
  contatos: [
    { tipoContato: 'EMAIL', valor: 'empresa@example.com', principal: true },
  ],
  contatosInstitucionais: [
    {
      nome: 'Contato Teste',
      departamento: 'Atendimento',
      email: 'contato@example.com',
    },
  ],
};
const cooperado: Awaited<ReturnType<typeof coop.obterCooperadoDetalhe>> = {
  publicId: 'coop-1',
  nome: 'Cooperado Teste',
  cpf: '52998224725',
  tipo: 2,
  desativado: false,
};

beforeEach(() => {
  vi.resetAllMocks();
  vi.stubGlobal(
    'matchMedia',
    vi.fn().mockReturnValue({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    }),
  );
  vi.spyOn(console, 'error').mockImplementation(() => {});
  vi.mocked(useAuthorization).mockReturnValue(makeAuthorization());
  vi.mocked(buscarCidadesPorUf).mockResolvedValue([]);
  vi.mocked(coop.listarCoordenadoresAtivos).mockResolvedValue([]);
  vi.mocked(coop.obterCooperadoDetalhe).mockResolvedValue(cooperado);
  vi.mocked(coop.cadastrarCooperado).mockResolvedValue({ publicId: 'coop-1' });
  vi.mocked(coop.alterarCooperado).mockResolvedValue(undefined);
  vi.mocked(estip.obterEstipulante).mockResolvedValue(empresa);
  vi.mocked(estip.obterConfiguracao).mockResolvedValue({
    dataInicioVigencia: '2026-01-01',
  });
  vi.mocked(estip.cadastrarEstipulante).mockResolvedValue({
    publicId: 'estip-1',
  });
  vi.mocked(estip.alterarEstipulante).mockResolvedValue(undefined);
});
afterEach(() => {
  vi.restoreAllMocks();
  vi.unstubAllGlobals();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

function renderPage(
  domain: 'estipulantes' | 'cooperados',
  mode: 'novo' | 'editar' | 'detalhe',
) {
  const id = domain === 'estipulantes' ? 'estip-1' : 'coop-1';
  const path = `/${domain}/${mode === 'novo' ? 'novo' : id + (mode === 'editar' ? '/editar' : '')}`;
  render(
    <ThemeProvider>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route
            path="/estipulantes/novo"
            element={<CadastrarEstipulantePage />}
          />
          <Route
            path="/estipulantes/:publicId/editar"
            element={<EditarEstipulantePage />}
          />
          <Route
            path="/estipulantes/:publicId"
            element={<EstipulanteDetalhePage />}
          />
          <Route path="/cooperados/novo" element={<NovoCooperadoPage />} />
          <Route
            path="/cooperados/:id/editar"
            element={<EditarCooperadoPage />}
          />
          <Route path="/cooperados/:id" element={<CooperadoDetalhePage />} />
          <Route path={`/${domain}`} element={<div>Lista de Cadastros</div>} />
        </Routes>
      </MemoryRouter>
    </ThemeProvider>,
  );
}

async function saveAndRecover(
  domain: 'estipulantes' | 'cooperados',
  edit: boolean,
  error: unknown,
  expected: string,
) {
  if (domain === 'estipulantes') {
    if (edit) vi.mocked(estip.alterarEstipulante).mockRejectedValueOnce(error);
    else vi.mocked(estip.cadastrarEstipulante).mockRejectedValueOnce(error);
  } else {
    if (edit) vi.mocked(coop.alterarCooperado).mockRejectedValueOnce(error);
    else vi.mocked(coop.cadastrarCooperado).mockRejectedValueOnce(error);
  }
  renderPage(domain, edit ? 'editar' : 'novo');
  const name = await screen.findByLabelText<HTMLInputElement>(
    domain === 'estipulantes' ? /Razão Social/ : /^Nome/,
  );
  fireEvent.change(name, { target: { value: 'Cadastro Revisado' } });
  if (!edit) {
    if (domain === 'estipulantes') {
      fireEvent.input(screen.getByLabelText(/CNPJ/), {
        target: { value: '61.198.164/0001-60' },
      });
      fireEvent.change(screen.getByLabelText(/Início de Vigência/), {
        target: { value: '2026-01-01' },
      });
    } else {
      fireEvent.change(screen.getByLabelText('Tipo'), {
        target: { value: '2' },
      });
      fireEvent.input(screen.getByLabelText(/^CPF/), {
        target: { value: '529.982.247-25' },
      });
    }
  }
  const saveName = domain === 'estipulantes' ? 'Salvar Estipulante' : 'Salvar';
  fireEvent.click(screen.getByRole('button', { name: saveName }));
  expect((await screen.findByRole('alert')).textContent).toContain(expected);
  expect(name.value).toBe('Cadastro Revisado');
  expect(screen.queryByText('Lista de Cadastros')).toBeNull();
  const button = screen.getByRole<HTMLButtonElement>('button', {
    name: saveName,
  });
  expect(button.disabled).toBe(false);
  fireEvent.click(button);
  expect(await screen.findByText('Lista de Cadastros')).toBeTruthy();
  if (domain === 'estipulantes') {
    if (edit) {
      expect(estip.alterarEstipulante).toHaveBeenCalledTimes(2);
      expect(estip.alterarEstipulante).toHaveBeenLastCalledWith(
        'estip-1',
        expect.objectContaining({
          razaoSocial: 'Cadastro Revisado',
          configuracao: {
            dataInicioVigencia: '2026-01-01',
            dataFimVigencia: undefined,
          },
          contatos: empresa.contatos,
          contatosInstitucionais: [
            {
              nome: 'Contato Teste',
              departamento: 'Atendimento',
              email: 'contato@example.com',
              telefone: undefined,
              ramal: '',
            },
          ],
        }),
      );
    } else {
      expect(estip.cadastrarEstipulante).toHaveBeenCalledTimes(2);
      expect(estip.cadastrarEstipulante).toHaveBeenLastCalledWith(
        expect.objectContaining({
          razaoSocial: 'Cadastro Revisado',
          cnpj: '61198164000160',
        }),
      );
    }
  } else {
    const payload = {
      nome: 'Cadastro Revisado',
      cpf: '52998224725',
      tipo: 2,
      credenciado: false,
    };
    if (edit) {
      expect(coop.alterarCooperado).toHaveBeenCalledTimes(2);
      expect(coop.alterarCooperado).toHaveBeenLastCalledWith('coop-1', payload);
    } else {
      expect(coop.cadastrarCooperado).toHaveBeenCalledTimes(2);
      expect(coop.cadastrarCooperado).toHaveBeenLastCalledWith(payload);
    }
  }
}

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Erros nos cadastros no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it.each([
      { domain: 'estipulantes' as const, edit: false },
      { domain: 'estipulantes' as const, edit: true },
      { domain: 'cooperados' as const, edit: false },
      { domain: 'cooperados' as const, edit: true },
    ])(
      'mantém o conflito e permite nova tentativa: $domain (edição: $edit)',
      async ({ domain, edit }) => {
        localStorage.setItem('webapolice-tema', theme);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        await saveAndRecover(
          domain,
          edit,
          new HttpApiError({ status: 409, message: 'Conflito de cadastro.' }),
          'Conflito de cadastro.',
        );
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      },
    );
  },
);

it.each([
  {
    domain: 'estipulantes' as const,
    edit: false,
    fallback: 'Ocorreu um erro ao cadastrar o estipulante.',
  },
  {
    domain: 'estipulantes' as const,
    edit: true,
    fallback: 'Ocorreu um erro ao salvar o estipulante.',
  },
  {
    domain: 'cooperados' as const,
    edit: false,
    fallback: 'Ocorreu um erro ao cadastrar o cooperado/coordenador.',
  },
  {
    domain: 'cooperados' as const,
    edit: true,
    fallback: 'Ocorreu um erro ao alterar o cooperado/coordenador.',
  },
])(
  'trata rejeição null sem perder dados: $domain (edição: $edit)',
  async ({ domain, edit, fallback }) => {
    await saveAndRecover(domain, edit, null, fallback);
  },
);

it.each([false, true])(
  'preserva a contingência de conflito sem mensagem de Estipulante (edição: %s)',
  async (edit) => {
    await saveAndRecover(
      'estipulantes',
      edit,
      new HttpApiError({ status: 409, message: '' }),
      edit
        ? 'A Razão Social informada diverge'
        : 'Já existe um cadastro com este CNPJ.',
    );
  },
);

it.each([400, 422])(
  'apresenta os erros de validação %s na criação do Cooperado',
  async (status) => {
    await saveAndRecover(
      'cooperados',
      false,
      new HttpApiError({
        status,
        message: 'Dados inválidos.',
        details: { errors: { Cpf: ['CPF já utilizado.'] } },
      }),
      'Erro de validação: Cpf: CPF já utilizado.',
    );
  },
);

it('preserva a mensagem da validação sem erros de campo', async () => {
  await saveAndRecover(
    'cooperados',
    false,
    new HttpApiError({ status: 422, message: 'Revise o cadastro.' }),
    'Erro de validação: Revise o cadastro.',
  );
});

it.each(['editar', 'detalhe'] as const)(
  'trata 404 do Estipulante na página de %s',
  async (mode) => {
    vi.mocked(estip.obterEstipulante).mockRejectedValueOnce(
      new HttpApiError({ status: 404, message: 'Registro ausente.' }),
    );
    renderPage('estipulantes', mode);
    expect(await screen.findByText('Estipulante não encontrado')).toBeTruthy();
  },
);

it.each(['editar', 'detalhe'] as const)(
  'tolera configuração 404 sem ocultar o Estipulante: %s',
  async (mode) => {
    vi.mocked(estip.obterConfiguracao).mockRejectedValueOnce(
      new HttpApiError({ status: 404, message: 'Configuração ausente.' }),
    );
    renderPage('estipulantes', mode);
    if (mode === 'editar')
      expect(await screen.findByDisplayValue('Empresa Teste')).toBeTruthy();
    else
      expect(
        await screen.findByRole('heading', { name: 'Empresa Teste' }),
      ).toBeTruthy();
    expect(screen.queryByText('Estipulante não encontrado')).toBeNull();
  },
);

it('apresenta acesso negado na consulta do Estipulante', async () => {
  vi.mocked(estip.obterEstipulante).mockRejectedValueOnce(
    new HttpApiError({ status: 403, message: 'Acesso negado.' }),
  );
  renderPage('estipulantes', 'detalhe');
  expect((await screen.findByRole('alert')).textContent).toContain(
    'Você não tem permissão',
  );
});

it('permite tentar novamente após falha da consulta do Estipulante', async () => {
  vi.mocked(estip.obterEstipulante).mockRejectedValueOnce(null);
  renderPage('estipulantes', 'detalhe');
  expect((await screen.findByRole('alert')).textContent).toContain(
    'Não foi possível carregar',
  );
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(
    await screen.findByRole('heading', { name: 'Empresa Teste' }),
  ).toBeTruthy();
});

it.each(['editar', 'detalhe'] as const)(
  'encerra o carregamento do Cooperado com rejeição null: %s',
  async (mode) => {
    vi.mocked(coop.obterCooperadoDetalhe).mockRejectedValueOnce(null);
    renderPage('cooperados', mode);
    expect((await screen.findByRole('alert')).textContent).toContain(
      'Não foi possível carregar',
    );
    if (mode === 'editar')
      await waitFor(() =>
        expect(screen.queryByRole('button', { name: 'Salvar' })).toBeNull(),
      );
  },
);
