import type { ReactElement } from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ModuloFormPage } from '../../src/features/modulos/pages/ModuloFormPage';
import { RamoFormPage } from '../../src/features/ramos/pages/RamoFormPage';
import { SeguradoraFormPage } from '../../src/features/seguradoras/pages/SeguradoraFormPage';
import { SubestipulanteFormPage } from '../../src/features/subestipulantes/pages/SubestipulanteFormPage';
import { modulosApi } from '../../src/features/modulos/api/modulos.api';
import { ramosApi } from '../../src/features/ramos/api/ramos.api';
import { seguradorasApi } from '../../src/features/seguradoras/api/seguradoras.api';
import { subestipulantesApi } from '../../src/features/subestipulantes/api/subestipulantes.api';
import { HttpApiError } from '../../src/services/http/httpError';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/features/modulos/api/modulos.api');
vi.mock('../../src/features/ramos/api/ramos.api');
vi.mock('../../src/features/seguradoras/api/seguradoras.api');
vi.mock('../../src/features/subestipulantes/api/subestipulantes.api');

const modulo: Awaited<ReturnType<typeof modulosApi.obter>> = {
  publicId: 'registro-1',
  nome: 'Cadastro Existente',
  descricao: 'Descrição Atual',
  ativo: false,
  createdAt: '2026-01-01',
  updatedAt: '2026-01-02',
};
const ramo: Awaited<ReturnType<typeof ramosApi.obter>> = {
  publicId: 'registro-1',
  codigo: 'VG',
  nome: 'Cadastro Existente',
  descricao: 'Descrição Atual',
  ativo: true,
  createdAt: '2026-01-01',
  updatedAt: '2026-01-02',
};
const seguradora: Awaited<ReturnType<typeof seguradorasApi.obter>> = {
  publicId: 'registro-1',
  nome: 'Cadastro Existente',
  observacao: 'Descrição Atual',
  ativo: true,
  createdAt: '2026-01-01',
  updatedAt: '2026-01-02',
};
const subestipulante: Awaited<ReturnType<typeof subestipulantesApi.obter>> = {
  ...seguradora,
};

interface Scenario {
  name: string;
  path: string;
  page: ReactElement;
  saveButton: string;
  textField: RegExp;
  setup: () => void;
  rejectSave: (edit: boolean, error: unknown) => void;
  rejectLoad: (error: unknown) => void;
  expectRequest: (edit: boolean) => void;
  saveFallback: string;
  loadFallback: string;
  showsLoadMessage: boolean;
}

const scenarios: Scenario[] = [
  {
    name: 'Módulos',
    path: '/modulos',
    page: <ModuloFormPage />,
    saveButton: 'Salvar',
    textField: /Descrição/,
    setup: () => {
      vi.mocked(modulosApi.obter).mockResolvedValue(modulo);
      vi.mocked(modulosApi.criar).mockResolvedValue(modulo);
      vi.mocked(modulosApi.alterar).mockResolvedValue(modulo);
    },
    rejectSave: (edit, error) => {
      if (edit) vi.mocked(modulosApi.alterar).mockRejectedValueOnce(error);
      else vi.mocked(modulosApi.criar).mockRejectedValueOnce(error);
    },
    rejectLoad: (error) =>
      vi.mocked(modulosApi.obter).mockRejectedValueOnce(error),
    expectRequest: (edit) => {
      const payload = {
        nome: 'Cadastro Revisado',
        descricao: 'Texto Revisado',
      };
      if (edit)
        expect(modulosApi.alterar).toHaveBeenNthCalledWith(2, 'registro-1', {
          ...payload,
          ativo: false,
        });
      else expect(modulosApi.criar).toHaveBeenNthCalledWith(2, payload);
    },
    saveFallback: 'Ocorreu um erro ao salvar o módulo.',
    loadFallback: 'Não foi possível carregar os dados do módulo.',
    showsLoadMessage: false,
  },
  {
    name: 'Ramos',
    path: '/ramos',
    page: <RamoFormPage />,
    saveButton: 'Salvar',
    textField: /Descrição/,
    setup: () => {
      vi.mocked(ramosApi.obter).mockResolvedValue(ramo);
      vi.mocked(ramosApi.criar).mockResolvedValue(ramo);
      vi.mocked(ramosApi.alterar).mockResolvedValue(undefined);
    },
    rejectSave: (edit, error) => {
      if (edit) vi.mocked(ramosApi.alterar).mockRejectedValueOnce(error);
      else vi.mocked(ramosApi.criar).mockRejectedValueOnce(error);
    },
    rejectLoad: (error) =>
      vi.mocked(ramosApi.obter).mockRejectedValueOnce(error),
    expectRequest: (edit) => {
      const payload = {
        nome: 'Cadastro Revisado',
        descricao: 'Texto Revisado',
      };
      if (edit)
        expect(ramosApi.alterar).toHaveBeenNthCalledWith(
          2,
          'registro-1',
          payload,
        );
      else
        expect(ramosApi.criar).toHaveBeenNthCalledWith(2, {
          ...payload,
          codigo: 'VG',
        });
    },
    saveFallback: 'Ocorreu um erro ao salvar o Ramo.',
    loadFallback: 'Não foi possível carregar os dados do Ramo.',
    showsLoadMessage: false,
  },
  {
    name: 'Seguradoras',
    path: '/seguradoras',
    page: <SeguradoraFormPage />,
    saveButton: 'Salvar',
    textField: /Observação/,
    setup: () => {
      vi.mocked(seguradorasApi.obter).mockResolvedValue(seguradora);
      vi.mocked(seguradorasApi.criar).mockResolvedValue({
        publicId: 'registro-1',
      });
      vi.mocked(seguradorasApi.alterar).mockResolvedValue(undefined);
    },
    rejectSave: (edit, error) => {
      if (edit) vi.mocked(seguradorasApi.alterar).mockRejectedValueOnce(error);
      else vi.mocked(seguradorasApi.criar).mockRejectedValueOnce(error);
    },
    rejectLoad: (error) =>
      vi.mocked(seguradorasApi.obter).mockRejectedValueOnce(error),
    expectRequest: (edit) => {
      const payload = {
        nome: 'Cadastro Revisado',
        codigo: undefined,
        susep: undefined,
        cnpj: undefined,
        observacao: 'Texto Revisado',
      };
      if (edit)
        expect(seguradorasApi.alterar).toHaveBeenNthCalledWith(
          2,
          'registro-1',
          { ...payload, publicId: 'registro-1' },
        );
      else expect(seguradorasApi.criar).toHaveBeenNthCalledWith(2, payload);
    },
    saveFallback:
      'Ocorreu um erro ao salvar a seguradora. Verifique os dados informados.',
    loadFallback: 'Não foi possível carregar os dados da seguradora.',
    showsLoadMessage: true,
  },
  {
    name: 'Subestipulantes',
    path: '/subestipulantes',
    page: <SubestipulanteFormPage />,
    saveButton: 'Salvar Subestipulante',
    textField: /Observações Internas/,
    setup: () => {
      vi.mocked(subestipulantesApi.obter).mockResolvedValue(subestipulante);
      vi.mocked(subestipulantesApi.criar).mockResolvedValue({
        publicId: 'registro-1',
      });
      vi.mocked(subestipulantesApi.alterar).mockResolvedValue(undefined);
    },
    rejectSave: (edit, error) => {
      if (edit)
        vi.mocked(subestipulantesApi.alterar).mockRejectedValueOnce(error);
      else vi.mocked(subestipulantesApi.criar).mockRejectedValueOnce(error);
    },
    rejectLoad: (error) =>
      vi.mocked(subestipulantesApi.obter).mockRejectedValueOnce(error),
    expectRequest: (edit) => {
      const payload = {
        nome: 'Cadastro Revisado',
        codigo: undefined,
        cnpj: undefined,
        observacao: 'Texto Revisado',
      };
      if (edit)
        expect(subestipulantesApi.alterar).toHaveBeenNthCalledWith(
          2,
          'registro-1',
          { ...payload, publicId: 'registro-1' },
        );
      else expect(subestipulantesApi.criar).toHaveBeenNthCalledWith(2, payload);
    },
    saveFallback:
      'Ocorreu um erro ao salvar o subestipulante. Verifique os dados informados.',
    loadFallback: 'Não foi possível carregar os dados do subestipulante.',
    showsLoadMessage: true,
  },
];

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
});
afterEach(() => {
  vi.restoreAllMocks();
  vi.unstubAllGlobals();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

function renderPage(scenario: Scenario, edit: boolean) {
  render(
    <ThemeProvider>
      <MemoryRouter
        initialEntries={[
          `${scenario.path}/${edit ? 'registro-1/editar' : 'novo'}`,
        ]}
      >
        <Routes>
          <Route path={`${scenario.path}/novo`} element={scenario.page} />
          <Route
            path={`${scenario.path}/:publicId/editar`}
            element={scenario.page}
          />
          <Route path={scenario.path} element={<div>Lista de Cadastros</div>} />
        </Routes>
      </MemoryRouter>
    </ThemeProvider>,
  );
}

async function checkSaveFailure(
  scenario: Scenario,
  edit: boolean,
  error: unknown,
  message: string,
) {
  scenario.setup();
  scenario.rejectSave(edit, error);
  renderPage(scenario, edit);
  const nome = await screen.findByLabelText<HTMLInputElement>(/Nome/);
  fireEvent.change(nome, { target: { value: 'Cadastro Revisado' } });
  fireEvent.change(screen.getByLabelText(scenario.textField), {
    target: { value: 'Texto Revisado' },
  });
  if (scenario.name === 'Ramos' && !edit)
    fireEvent.change(screen.getByLabelText(/Código/), {
      target: { value: 'VG' },
    });
  fireEvent.click(screen.getByRole('button', { name: scenario.saveButton }));
  const alert = await screen.findByRole('alert');
  expect(alert.textContent).toContain(message);
  expect(alert.className).toContain('alert-error');
  expect(nome.value).toBe('Cadastro Revisado');
  expect(
    screen.getByLabelText<HTMLTextAreaElement>(scenario.textField).value,
  ).toBe('Texto Revisado');
  expect(screen.queryByText('Lista de Cadastros')).toBeNull();
  const save = screen.getByRole<HTMLButtonElement>('button', {
    name: scenario.saveButton,
  });
  expect(save.disabled).toBe(false);
  fireEvent.click(save);
  expect(await screen.findByText('Lista de Cadastros')).toBeTruthy();
  scenario.expectRequest(edit);
}

describe.each(scenarios)('Erros de formulário: $name', (scenario) => {
  describe.each([
    { theme: 'claro', dark: false, resolved: 'light' },
    { theme: 'escuro', dark: false, resolved: 'dark' },
    { theme: 'sistema', dark: false, resolved: 'light' },
    { theme: 'sistema', dark: true, resolved: 'dark' },
  ])('tema $theme (sistema escuro: $dark)', ({ theme, dark, resolved }) => {
    it.each([false, true])(
      'preserva a mensagem HTTP e permite nova tentativa (edição: %s)',
      async (edit) => {
        localStorage.setItem('webapolice-tema', theme);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        await checkSaveFailure(
          scenario,
          edit,
          new HttpApiError({ status: 409, message: 'Cadastro em conflito.' }),
          'Cadastro em conflito.',
        );
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      },
    );
  });

  describe.each([false, true])(
    'mensagens de contingência (edição: %s)',
    (edit) => {
      it.each([
        { name: 'null', error: null },
        { name: 'mensagem vazia', error: new Error('') },
        {
          name: 'objeto fora do contrato',
          error: { response: { data: { message: 'Formato antigo' } } },
        },
      ])('trata $name sem perder os dados', async ({ error }) => {
        await checkSaveFailure(scenario, edit, error, scenario.saveFallback);
      });
    },
  );

  it.each([
    {
      name: 'HTTP',
      error: new HttpApiError({
        status: 404,
        message: 'Registro indisponível.',
      }),
    },
    { name: 'null', error: null },
  ])('encerra o carregamento e apresenta a falha $name', async ({ error }) => {
    scenario.setup();
    scenario.rejectLoad(error);
    renderPage(scenario, true);
    const alert = await screen.findByRole('alert');
    expect(alert.textContent).toContain(
      scenario.showsLoadMessage && error instanceof Error
        ? error.message
        : scenario.loadFallback,
    );
    await waitFor(() =>
      expect(
        screen.getByRole('button', { name: scenario.saveButton }),
      ).toBeTruthy(),
    );
  });
});
