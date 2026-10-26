import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import {
  ClienteForm,
  type ClienteFormData,
} from '../../src/features/clientes/components/ClienteForm';
import { CooperadoForm } from '../../src/features/cooperados/components/CooperadoForm';
import type { CooperadoFormData } from '../../src/features/cooperados/types/cooperados.types';
import { buscarCidadesPorUf } from '../../src/features/clientes/api/localidadesApi';
import { listarCoordenadoresAtivos } from '../../src/features/cooperados/api/cooperadosApi';
import { consultarCep } from '../../src/shared/api/enderecosApi';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/auth/AuthorizationProvider', () => ({
  useAuthorization: () => ({ possuiRecurso: () => false }),
}));
vi.mock('../../src/features/clientes/api/localidadesApi');
vi.mock('../../src/features/cooperados/api/cooperadosApi');
vi.mock('../../src/shared/api/enderecosApi');

const cliente: Partial<ClienteFormData> = {
  tipoPessoa: 1,
  nome: 'Cliente Teste',
  documento: '52998224725',
  dataNascimento: '1990-01-01',
  contatos: [
    { tipoContato: 'EMAIL', valor: 'contato@example.com', principal: true },
  ],
  enderecos: [],
};
const cooperado: CooperadoFormData = {
  tipo: 2,
  nome: 'Cooperado Teste',
  cpf: '52998224725',
};

beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(buscarCidadesPorUf).mockResolvedValue([]);
  vi.mocked(listarCoordenadoresAtivos).mockResolvedValue([
    {
      publicId: 'coord-1',
      nome: 'Coordenador Teste',
      tipo: 2,
      cpfMascarado: '***.***.***-25',
      desativado: false,
      dataCadastroUtc: '2026-01-01T00:00:00Z',
    },
  ]);
});
afterEach(() => {
  vi.restoreAllMocks();
  vi.unstubAllGlobals();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

const contactTypes = () =>
  screen.getAllByLabelText<HTMLSelectElement>('Tipo de Contato', {
    exact: false,
  });
const contactValues = () =>
  screen.getAllByLabelText<HTMLInputElement>('Contato', { exact: true });
function inputDocument(value: string) {
  const input = screen.getByLabelText<HTMLInputElement>(
    /Documento \(CPF\/CNPJ\)/,
  );
  fireEvent.input(input, { target: { value: '' } });
  fireEvent.input(input, { target: { value } });
  return input;
}

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Campos condicionais em $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    beforeEach(() => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
    });

    it('atualiza CPF/CNPJ e óbito de Cliente e entrega os valores atuais', async () => {
      const onSubmit = vi.fn<(data: ClienteFormData) => void>();
      render(
        <ThemeProvider>
          <ClienteForm
            initialData={cliente}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      expect(screen.queryByLabelText(/Data de Óbito/)).toBeNull();
      fireEvent.click(
        screen.getByRole('checkbox', { name: 'Cliente Falecido/Extinto' }),
      );
      fireEvent.change(screen.getByLabelText(/Data de Óbito/), {
        target: { value: '2020-01-01' },
      });
      fireEvent.click(
        screen.getByRole('checkbox', { name: 'Cliente Falecido/Extinto' }),
      );
      expect(screen.queryByLabelText(/Data de Óbito/)).toBeNull();
      fireEvent.click(
        screen.getByRole('checkbox', { name: 'Cliente Falecido/Extinto' }),
      );
      expect(
        screen.getByLabelText<HTMLInputElement>(/Data de Óbito/).value,
      ).toBe('2020-01-01');
      fireEvent.change(screen.getByLabelText(/Tipo de Pessoa/), {
        target: { value: '2' },
      });
      expect(inputDocument('11222333000181').value).toBe('11.222.333/0001-81');
      fireEvent.change(screen.getByLabelText(/Tipo de Pessoa/), {
        target: { value: '1' },
      });
      expect(inputDocument('52998224725').value).toBe('529.982.247-25');
      fireEvent.change(screen.getByLabelText(/Tipo de Pessoa/), {
        target: { value: '2' },
      });
      inputDocument('11222333000181');
      fireEvent.click(screen.getByRole('button', { name: 'Salvar Cliente' }));
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toMatchObject({
        tipoPessoa: 2,
        documento: '11222333000181',
        falecido: true,
        dataObito: '2020-01-01',
      });
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });

    it('alterna Cooperado/Coordenador e mantém a exigência de coordenador responsável', async () => {
      vi.spyOn(console, 'error').mockImplementation(() => {});
      const onSubmit = vi
        .fn<(data: CooperadoFormData) => Promise<void>>()
        .mockResolvedValue(undefined);
      render(
        <ThemeProvider>
          <CooperadoForm
            initialData={cooperado}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      expect(screen.queryByLabelText(/Coordenador responsável/)).toBeNull();
      fireEvent.change(screen.getByLabelText('Tipo', { exact: false }), {
        target: { value: '1' },
      });
      fireEvent.click(screen.getByRole('button', { name: /^Salvar$/ }));
      expect(
        await screen.findByText(
          'Coordenador responsável é obrigatório para Cooperados',
        ),
      ).toBeTruthy();
      expect(onSubmit).not.toHaveBeenCalled();
      const coordinator = screen.getByLabelText<HTMLSelectElement>(
        /Coordenador responsável/,
      );
      expect(coordinator.getAttribute('aria-invalid')).toBe('true');
      await within(coordinator).findByRole('option', {
        name: 'Coordenador Teste',
      });
      fireEvent.change(coordinator, { target: { value: 'coord-1' } });
      fireEvent.click(screen.getByRole('button', { name: /^Salvar$/ }));
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toMatchObject({
        tipo: 1,
        coordenadorId: 'coord-1',
        cpf: '52998224725',
      });
      fireEvent.change(screen.getByLabelText('Tipo', { exact: false }), {
        target: { value: '2' },
      });
      expect(screen.queryByLabelText(/Coordenador responsável/)).toBeNull();
      fireEvent.click(screen.getByRole('button', { name: /^Salvar$/ }));
      await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(2));
      expect(onSubmit.mock.calls[1][0].tipo).toBe(2);
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
  },
);

describe('Assinaturas de campos de Cliente e Cooperado', () => {
  it('mantém os tipos de contato sincronizados ao adicionar, remover e mudar de tipo', async () => {
    const onSubmit = vi.fn<(data: ClienteFormData) => void>();
    render(
      <ClienteForm
        initialData={cliente}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    expect(contactValues()[0].type).toBe('email');
    fireEvent.change(contactTypes()[0], { target: { value: 'CELULAR' } });
    expect(contactValues()[0].type).toBe('text');
    fireEvent.input(contactValues()[0], { target: { value: '51999861341' } });
    fireEvent.click(screen.getByRole('button', { name: 'Adicionar Contato' }));
    expect(contactValues()[1].type).toBe('email');
    fireEvent.change(contactValues()[1], {
      target: { value: 'dois@example.com' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Adicionar Contato' }));
    fireEvent.change(contactTypes()[2], { target: { value: 'TELEFONE' } });
    fireEvent.input(contactValues()[2], { target: { value: '5132419416' } });
    fireEvent.click(screen.getAllByRole('button', { name: /^Remover$/ })[0]);
    expect(contactTypes().map((field) => field.value)).toEqual([
      'EMAIL',
      'TELEFONE',
    ]);
    expect(contactValues().map((field) => field.type)).toEqual([
      'email',
      'text',
    ]);
    fireEvent.change(contactTypes()[1], { target: { value: 'EMAIL' } });
    expect(contactValues()[1].type).toBe('email');
    fireEvent.change(contactValues()[1], {
      target: { value: 'tres@example.com' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar Cliente' }));
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(
      onSubmit.mock.calls[0][0].contatos.map(({ tipoContato, valor }) => ({
        tipoContato,
        valor,
      })),
    ).toEqual([
      { tipoContato: 'EMAIL', valor: 'dois@example.com' },
      { tipoContato: 'EMAIL', valor: 'tres@example.com' },
    ]);
  });

  it('aplica reset de initialData aos campos condicionais e à lista de contatos', async () => {
    const onSubmit = vi.fn<(data: ClienteFormData) => void>();
    const view = render(
      <ClienteForm
        initialData={cliente}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    const next: Partial<ClienteFormData> = {
      ...cliente,
      nome: 'Empresa Teste',
      tipoPessoa: 2,
      documento: '11222333000181',
      falecido: true,
      dataObito: '2020-01-01',
      contatos: [
        { tipoContato: 'CELULAR', valor: '51999861341', principal: true },
      ],
    };
    view.rerender(
      <ClienteForm initialData={next} onSubmit={onSubmit} onCancel={vi.fn()} />,
    );
    expect(screen.getByLabelText<HTMLInputElement>(/Data de Óbito/).value).toBe(
      '2020-01-01',
    );
    expect(
      screen.getByLabelText<HTMLSelectElement>(/Tipo de Pessoa/).value,
    ).toBe('2');
    expect(contactValues()[0].type).toBe('text');
    fireEvent.click(screen.getByRole('button', { name: 'Salvar Cliente' }));
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit.mock.calls[0][0]).toMatchObject({
      nome: 'Empresa Teste',
      tipoPessoa: 2,
      documento: '11222333000181',
      contatos: [{ tipoContato: 'CELULAR', valor: '51999861341' }],
    });
    view.rerender(
      <ClienteForm
        initialData={cliente}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    expect(screen.queryByLabelText(/Data de Óbito/)).toBeNull();
    expect(contactValues()[0].type).toBe('email');
  });

  it('consulta cidades ao mudar UF e desabilita a cidade ao limpar o estado', async () => {
    vi.mocked(buscarCidadesPorUf).mockImplementation(async (uf) =>
      uf === 'SP'
        ? [{ id: 35, nome: 'Campinas' }]
        : [{ id: 43, nome: 'Porto Alegre' }],
    );
    render(
      <CooperadoForm
        initialData={cooperado}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />,
    );
    const city = screen.getByLabelText<HTMLSelectElement>('Cidade');
    expect(city.disabled).toBe(true);
    fireEvent.change(screen.getByLabelText('Estado (UF)'), {
      target: { value: 'SP' },
    });
    await within(city).findByRole('option', { name: 'Campinas' });
    expect(buscarCidadesPorUf).toHaveBeenCalledWith(
      'SP',
      expect.any(AbortSignal),
    );
    expect(city.disabled).toBe(false);
    fireEvent.change(screen.getByLabelText('Estado (UF)'), {
      target: { value: 'RS' },
    });
    await within(city).findByRole('option', { name: 'Porto Alegre' });
    expect(buscarCidadesPorUf).toHaveBeenCalledWith(
      'RS',
      expect.any(AbortSignal),
    );
    expect(within(city).queryByRole('option', { name: 'Campinas' })).toBeNull();
    fireEvent.change(screen.getByLabelText('Estado (UF)'), {
      target: { value: '' },
    });
    expect(city.disabled).toBe(true);
    expect(
      within(city).queryByRole('option', { name: 'Porto Alegre' }),
    ).toBeNull();
  });

  it('reage ao CEP, atualiza UF por setValue e permite selecionar a cidade', async () => {
    vi.mocked(consultarCep).mockResolvedValue({
      cep: '12345678',
      logradouro: 'Rua Teste',
      bairro: 'Centro',
      uf: 'SP',
      cidade: 'Campinas',
      cidadeId: null,
      estadoId: 1,
    });
    vi.mocked(buscarCidadesPorUf).mockResolvedValue([
      { id: 35, nome: 'Campinas' },
    ]);
    render(
      <CooperadoForm
        initialData={cooperado}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />,
    );
    fireEvent.input(screen.getByLabelText(/CEP/), {
      target: { value: '12345678' },
    });
    const city = screen.getByLabelText<HTMLSelectElement>('Cidade');
    await within(city).findByRole('option', { name: 'Campinas' });
    expect(city.disabled).toBe(false);
    fireEvent.change(city, { target: { value: '35' } });
    expect(city.value).toBe('35');
    expect(consultarCep).toHaveBeenCalledWith(
      '12345678',
      expect.any(AbortSignal),
    );
    expect(screen.getByLabelText<HTMLInputElement>('Logradouro').value).toBe(
      'Rua Teste',
    );
    expect(screen.getByLabelText<HTMLSelectElement>('Estado (UF)').value).toBe(
      'SP',
    );
    expect(buscarCidadesPorUf).toHaveBeenCalledWith(
      'SP',
      expect.any(AbortSignal),
    );
    fireEvent.input(screen.getByLabelText(/CEP/), { target: { value: '' } });
    expect(screen.getByLabelText<HTMLInputElement>('Logradouro').value).toBe(
      '',
    );
    expect(screen.getByLabelText<HTMLSelectElement>('Estado (UF)').value).toBe(
      '',
    );
    expect(screen.getByLabelText<HTMLSelectElement>('Cidade').disabled).toBe(
      true,
    );
  });
});
