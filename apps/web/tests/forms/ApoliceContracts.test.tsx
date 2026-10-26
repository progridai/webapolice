import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ApoliceForm } from '../../src/features/apolices/components/forms/ApoliceForm';
import { ApoliceFormPage } from '../../src/features/apolices/pages/ApoliceFormPage';
import { listarEstipulantes } from '../../src/features/estipulantes/api/estipulantes.api';
import { seguradorasApi } from '../../src/features/seguradoras/api/seguradoras.api';
import { corretorasApi } from '../../src/features/corretoras/api/corretoras.api';
import {
  obterApolice,
  alterarApolice,
} from '../../src/features/apolices/api/apolices.api';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';
import { HttpApiError } from '../../src/services/http/httpError';

vi.mock('../../src/features/estipulantes/api/estipulantes.api', () => ({
  listarEstipulantes: vi.fn(),
}));
vi.mock('../../src/features/seguradoras/api/seguradoras.api', () => ({
  seguradorasApi: { listar: vi.fn() },
}));
vi.mock('../../src/features/corretoras/api/corretoras.api', () => ({
  corretorasApi: { listar: vi.fn() },
}));
vi.mock('../../src/features/apolices/api/apolices.api', () => ({
  obterApolice: vi.fn(),
  alterarApolice: vi.fn(),
  criarApolice: vi.fn(),
}));

const labels = {
  _estipulanteNome: 'Empresa de Teste',
  _seguradoraNome: 'Seguradora de Teste',
  _corretoraNome: 'Corretora de Teste',
};
const initialData = {
  nome: 'Contrato Teste',
  estipulanteId: '101',
  seguradoraId: '202',
  corretoraId: '303',
  dataInicioVigencia: '2026-01-01',
  ...labels,
};

beforeEach(() => {
  vi.mocked(listarEstipulantes).mockResolvedValue({
    itens: [
      {
        id: 101,
        publicId: 'estipulante-uuid',
        razaoSocial: labels._estipulanteNome,
        cnpj: '11222333000181',
        ativo: true,
        dataCadastro: '2026-01-01',
      },
    ],
    paginaAtual: 1,
    tamanhoPagina: 20,
    totalItens: 1,
    totalPaginas: 1,
  });
  vi.mocked(seguradorasApi.listar).mockResolvedValue({
    itens: [
      {
        id: 202,
        publicId: 'seguradora-uuid',
        nome: labels._seguradoraNome,
        ativo: true,
        createdAt: '2026-01-01',
      },
    ],
    paginaAtual: 1,
    tamanhoPagina: 20,
    totalItens: 1,
    totalPaginas: 1,
  });
  vi.mocked(corretorasApi.listar).mockResolvedValue({
    itens: [
      {
        id: 303,
        publicId: 'corretora-uuid',
        nome: labels._corretoraNome,
        ativo: true,
        createdAt: '2026-01-01',
      },
    ],
    paginaAtual: 1,
    tamanhoPagina: 20,
    totalItens: 1,
    totalPaginas: 1,
  });
  vi.stubGlobal('alert', vi.fn());
});
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

function submit() {
  fireEvent.submit(document.querySelector('form')!);
}
async function selectEntity(label: string, option: string) {
  fireEvent.focus(screen.getByLabelText(label));
  fireEvent.mouseDown(await screen.findByText(option));
}

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Seletores de Apólice no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it('usa os DTOs reais e conserva os IDs numéricos exigidos pela gravação', async () => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const onSubmit = vi.fn().mockResolvedValue(undefined);
      render(
        <ThemeProvider>
          <MemoryRouter>
            <ApoliceForm
              initialData={{
                nome: 'Contrato Teste',
                dataInicioVigencia: '2026-01-01',
              }}
              onSubmit={onSubmit}
            />
          </MemoryRouter>
        </ThemeProvider>,
      );
      await selectEntity('Estipulante', labels._estipulanteNome);
      await selectEntity('Seguradora', labels._seguradoraNome);
      await selectEntity('Corretora (Opcional)', labels._corretoraNome);
      expect(listarEstipulantes).toHaveBeenCalledWith(
        {
          nome: '',
          page: 1,
          pageSize: 20,
        },
        expect.any(AbortSignal),
      );
      expect(seguradorasApi.listar).toHaveBeenCalledWith(
        {
          busca: '',
          pagina: 1,
          tamanhoPagina: 20,
        },
        expect.any(AbortSignal),
      );
      expect(corretorasApi.listar).toHaveBeenCalledWith(
        {
          busca: '',
          pagina: 1,
          tamanhoPagina: 20,
        },
        expect.any(AbortSignal),
      );
      submit();
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toMatchObject({
        estipulanteId: '101',
        seguradoraId: '202',
        corretoraId: '303',
      });
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
  },
);

describe('Edição e validação de Apólices', () => {
  it('mantém os rótulos iniciais fora do resultado validado sem consultar as listas', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(
      <MemoryRouter>
        <ApoliceForm initialData={initialData} onSubmit={onSubmit} />
      </MemoryRouter>,
    );
    expect(screen.getByLabelText<HTMLInputElement>('Estipulante').value).toBe(
      labels._estipulanteNome,
    );
    expect(screen.getByLabelText<HTMLInputElement>('Seguradora').value).toBe(
      labels._seguradoraNome,
    );
    expect(
      screen.getByLabelText<HTMLInputElement>('Corretora (Opcional)').value,
    ).toBe(labels._corretoraNome);
    submit();
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit.mock.calls[0][0]).not.toHaveProperty('_estipulanteNome');
    expect(listarEstipulantes).not.toHaveBeenCalled();
    expect(seguradorasApi.listar).not.toHaveBeenCalled();
    expect(corretorasApi.listar).not.toHaveBeenCalled();
  });

  it('associa os erros aos seletores obrigatórios', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(
      <MemoryRouter>
        <ApoliceForm
          initialData={{
            nome: 'Contrato Teste',
            dataInicioVigencia: '2026-01-01',
          }}
          onSubmit={onSubmit}
        />
      </MemoryRouter>,
    );
    submit();
    for (const name of ['Estipulante', 'Seguradora']) {
      const field = screen.getByLabelText(name);
      await waitFor(() =>
        expect(field.getAttribute('aria-invalid')).toBe('true'),
      );
      expect(
        document
          .getElementById(field.getAttribute('aria-describedby')!)
          ?.getAttribute('role'),
      ).toBe('alert');
    }
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('desabilita os seletores durante a gravação', () => {
    render(
      <MemoryRouter>
        <ApoliceForm initialData={initialData} onSubmit={vi.fn()} isLoading />
      </MemoryRouter>,
    );
    for (const label of ['Estipulante', 'Seguradora', 'Corretora (Opcional)'])
      expect(screen.getByLabelText<HTMLInputElement>(label).disabled).toBe(
        true,
      );
  });

  it('preserva o texto digitado ao substituir uma seleção anterior', async () => {
    render(
      <MemoryRouter>
        <ApoliceForm initialData={initialData} onSubmit={vi.fn()} />
      </MemoryRouter>,
    );
    const field = screen.getByLabelText<HTMLInputElement>('Estipulante');
    fireEvent.focus(field);
    fireEvent.change(field, { target: { value: 'Nova Empresa' } });
    expect(field.value).toBe('Nova Empresa');
    await waitFor(() =>
      expect(listarEstipulantes).toHaveBeenCalledWith(
        {
          nome: 'Nova Empresa',
          page: 1,
          pageSize: 20,
        },
        expect.any(AbortSignal),
      ),
    );
  });

  it('exibe a falha de consulta e permite tentar novamente sem submeter o formulário', async () => {
    vi.mocked(listarEstipulantes).mockRejectedValueOnce(
      new Error('Falha temporária'),
    );
    const onSubmit = vi.fn();
    render(
      <MemoryRouter>
        <ApoliceForm onSubmit={onSubmit} />
      </MemoryRouter>,
    );
    fireEvent.focus(screen.getByLabelText('Estipulante'));
    const alert = await screen.findByRole('alert');
    expect(alert.textContent).toContain('Não foi possível carregar as opções.');
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
    expect(await screen.findByText(labels._estipulanteNome)).toBeTruthy();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('descarta opções de uma busca antiga que termina após a busca atual', async () => {
    let resolveOld!: (
      value: Awaited<ReturnType<typeof listarEstipulantes>>,
    ) => void;
    vi.mocked(listarEstipulantes).mockReturnValueOnce(
      new Promise((resolve) => {
        resolveOld = resolve;
      }),
    );
    const current = {
      itens: [
        {
          id: 404,
          publicId: 'outro-uuid',
          razaoSocial: 'Empresa Atual',
          cnpj: '11222333000181',
          ativo: true,
          dataCadastro: '2026-01-01',
        },
      ],
      paginaAtual: 1,
      tamanhoPagina: 20,
      totalItens: 1,
      totalPaginas: 1,
    };
    vi.mocked(listarEstipulantes).mockResolvedValueOnce(current);
    render(
      <MemoryRouter>
        <ApoliceForm onSubmit={vi.fn()} />
      </MemoryRouter>,
    );
    const field = screen.getByLabelText('Estipulante');
    fireEvent.focus(field);
    await waitFor(() => expect(listarEstipulantes).toHaveBeenCalledOnce());
    fireEvent.change(field, { target: { value: 'Atual' } });
    expect(await screen.findByText('Empresa Atual')).toBeTruthy();
    await act(async () =>
      resolveOld({
        ...current,
        itens: [{ ...current.itens[0], razaoSocial: 'Empresa Antiga' }],
      }),
    );
    expect(screen.queryByText('Empresa Antiga')).toBeNull();
    expect(screen.getByText('Empresa Atual')).toBeTruthy();
  });

  function renderPage() {
    render(
      <MemoryRouter initialEntries={['/apolices/apolice-1/editar']}>
        <Routes>
          <Route
            path="/apolices/:publicId/editar"
            element={<ApoliceFormPage />}
          />
          <Route
            path="/apolices/apolice-1"
            element={<div>Apólice Salva</div>}
          />
        </Routes>
      </MemoryRouter>,
    );
  }
  beforeEach(() => {
    vi.mocked(obterApolice).mockResolvedValue({
      publicId: 'apolice-1',
      nome: 'Contrato Teste',
      estipulanteId: 101,
      estipulanteNome: labels._estipulanteNome,
      seguradoraId: 202,
      seguradoraNome: labels._seguradoraNome,
      corretoraId: 303,
      corretoraNome: labels._corretoraNome,
      dataInicioVigencia: '2026-01-01',
      ativo: true,
      status: 'Ativa',
      ramos: [],
    });
    vi.mocked(alterarApolice).mockResolvedValue(undefined);
  });
  it('carrega o detalhe na página real e salva com os IDs e campos previstos', async () => {
    renderPage();
    await screen.findByLabelText('Estipulante');
    submit();
    expect(await screen.findByText('Apólice Salva')).toBeTruthy();
    expect(alterarApolice).toHaveBeenCalledExactlyOnceWith('apolice-1', {
      nome: 'Contrato Teste',
      estipulanteId: '101',
      seguradoraId: '202',
      corretoraId: '303',
      dataInicioVigencia: '2026-01-01',
      dataFimVigencia: '',
      dataAniversario: '',
      observacao: '',
    });
  });
  it('apresenta a mensagem do erro normalizado e conserva o formulário para correção', async () => {
    vi.mocked(alterarApolice).mockRejectedValue(
      new HttpApiError({
        status: 422,
        message: 'Revise os dados do contrato.',
      }),
    );
    renderPage();
    await screen.findByLabelText('Estipulante');
    submit();
    expect(await screen.findByRole('alert')).toHaveProperty(
      'textContent',
      expect.stringContaining('Revise os dados do contrato.'),
    );
    expect(
      screen.getByRole('heading', { name: 'Editar Apólice' }),
    ).toBeTruthy();
    expect(
      screen.getByLabelText<HTMLInputElement>('Nome (Denominação)').value,
    ).toBe('Contrato Teste');
  });
});
