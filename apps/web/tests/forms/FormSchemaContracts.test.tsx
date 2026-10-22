import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ClienteForm } from '../../src/features/clientes/components/ClienteForm';
import { EstipulanteForm } from '../../src/features/estipulantes/components/EstipulanteForm';
import { CooperadoForm } from '../../src/features/cooperados/components/CooperadoForm';
import { ModuloForm } from '../../src/features/modulos/components/ModuloForm';
import { ApoliceRamoFormModal } from '../../src/features/apolices/components/tabs/ApoliceRamoFormModal';
import { buscarCidadesPorUf } from '../../src/features/clientes/api/localidadesApi';
import { listarCoordenadoresAtivos } from '../../src/features/cooperados/api/cooperadosApi';
import { ramosApi } from '../../src/features/ramos/api/ramos.api';
import { consultarCep } from '../../src/shared/api/enderecosApi';
import {
  atualizarRamoApolice,
  vincularRamoApolice,
} from '../../src/features/apolices/api/apolices.api';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/auth/AuthorizationProvider', () => ({
  useAuthorization: () => ({ possuiRecurso: () => false }),
}));
vi.mock('../../src/features/clientes/api/localidadesApi', () => ({
  buscarCidadesPorUf: vi.fn(),
}));
vi.mock('../../src/shared/api/enderecosApi', () => ({ consultarCep: vi.fn() }));
vi.mock('../../src/features/cooperados/api/cooperadosApi', () => ({
  listarCoordenadoresAtivos: vi.fn(),
}));
vi.mock('../../src/features/ramos/api/ramos.api', () => ({
  ramosApi: {
    listar: vi.fn(),
  },
}));
vi.mock('../../src/features/apolices/api/apolices.api', () => ({
  atualizarRamoApolice: vi.fn(),
  vincularRamoApolice: vi.fn(),
}));

const cliente = {
  nome: '  Cliente Teste  ',
  tipoPessoa: 1,
  documento: '52998224725',
  dataNascimento: '1990-01-01',
  contatos: [
    { tipoContato: 'EMAIL', valor: '', principal: false },
    { tipoContato: 'CELULAR', valor: '51999861341', principal: true },
  ],
  enderecos: [{ tipoEndereco: 'RESIDENCIAL', uf: 'SP', principal: true }],
};
const estipulante = {
  razaoSocial: '  Empresa Teste  ',
  cnpj: '11222333000181',
  contatos: [
    { tipoContato: 'EMAIL', valor: '', principal: false },
    { tipoContato: 'CELULAR', valor: '51999861341', principal: true },
  ],
  endereco: { uf: 'SP' },
  configuracao: {
    dataInicioVigencia: '2026-01-01',
    dataFimVigencia: '2026-12-31',
  },
};
const cooperado = {
  nome: 'Cooperado Teste',
  cpf: '52998224725',
  tipo: 2 as const,
  credenciado: false,
  uf: 'SP',
};
const ramo = {
  publicId: 'vinculo-1',
  ramoCodigo: 'VG',
  ramoNome: 'Vida em Grupo',
  ativo: true,
  numeroApolice: '1234',
  iofPercentual: 7.38,
};

beforeEach(() => {
  vi.mocked(listarCoordenadoresAtivos).mockResolvedValue([
    {
      publicId: 'coordenador-1',
      nome: 'Coordenador Teste',
      tipo: 2,
      cpfMascarado: '***.***.***-25',
      desativado: false,
      dataCadastroUtc: '2026-01-01T00:00:00Z',
    },
  ]);
  vi.mocked(ramosApi.listar).mockResolvedValue({
    items: [
      {
        publicId: 'ramo-1',
        codigo: 'VG',
        nome: 'Vida em Grupo',
        ativo: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
    totalCount: 1,
  });
  vi.mocked(atualizarRamoApolice).mockResolvedValue(undefined);
  vi.mocked(vincularRamoApolice).mockResolvedValue(undefined);
  vi.mocked(buscarCidadesPorUf).mockResolvedValue([
    { id: 3509502, nome: 'Campinas' },
  ]);
});
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
  vi.restoreAllMocks();
});

async function selectCidade() {
  const cidade = screen.getByLabelText<HTMLSelectElement>('Cidade');
  await waitFor(() =>
    expect(
      within(cidade).getByRole('option', { name: 'Campinas' }),
    ).toBeTruthy(),
  );
  fireEvent.change(cidade, { target: { value: '3509502' } });
}
function submit() {
  const form = document.querySelector('form');
  expect(form).toBeTruthy();
  fireEvent.submit(form!);
}
function expectFieldError(field: HTMLElement) {
  expect(field.getAttribute('aria-invalid')).toBe('true');
  const id = field.getAttribute('aria-describedby');
  expect(id).toBeTruthy();
  expect(document.getElementById(id!)?.getAttribute('role')).toBe('alert');
}

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Contratos dos formulários no tema $theme (sistema escuro: $dark)',
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
    afterEach(() => {
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });

    it('entrega os números validados e filtra contatos vazios de Cliente', async () => {
      const onSubmit = vi.fn();
      render(
        <ThemeProvider>
          <ClienteForm
            initialData={cliente}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      await selectCidade();
      fireEvent.change(screen.getByLabelText('Sexo'), {
        target: { value: '2' },
      });
      submit();
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toMatchObject({
        nome: 'Cliente Teste',
        tipoPessoa: 1,
        documento: '52998224725',
        sexo: 2,
        falecido: false,
        contatos: [
          { tipoContato: 'CELULAR', valor: '51999861341', principal: true },
        ],
        enderecos: [
          { tipoEndereco: 'RESIDENCIAL', cidadeId: 3509502, principal: true },
        ],
      });
    });

    it('entrega endereço numérico, CNPJ e contatos normalizados de Estipulante', async () => {
      const onSubmit = vi.fn();
      render(
        <ThemeProvider>
          <EstipulanteForm
            initialData={estipulante}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      await selectCidade();
      submit();
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toMatchObject({
        razaoSocial: 'Empresa Teste',
        cnpj: '11222333000181',
        endereco: { cidadeId: 3509502 },
        contatos: [
          { tipoContato: 'CELULAR', valor: '51999861341', principal: true },
        ],
      });
    });

    it('converte os seletores e números de Cooperado antes de montar o payload', async () => {
      const onSubmit = vi.fn().mockResolvedValue(undefined);
      render(
        <ThemeProvider>
          <CooperadoForm
            initialData={cooperado}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      await selectCidade();
      fireEvent.change(screen.getByLabelText('Tipo'), {
        target: { value: '1' },
      });
      const coordenador = screen.getByLabelText('Coordenador responsável');
      await waitFor(() =>
        expect(
          within(coordenador).getByRole('option', {
            name: 'Coordenador Teste',
          }),
        ).toBeTruthy(),
      );
      fireEvent.change(coordenador, { target: { value: 'coordenador-1' } });
      fireEvent.change(screen.getByLabelText('Número de Dependentes'), {
        target: { value: '3' },
      });
      fireEvent.change(screen.getByLabelText('Banco'), {
        target: { value: '12' },
      });
      fireEvent.click(
        screen.getByRole('checkbox', { name: 'Cooperado Credenciado' }),
      );
      submit();
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toMatchObject({
        tipo: 1,
        cpf: '52998224725',
        coordenadorId: 'coordenador-1',
        cidadeId: 3509502,
        bancoId: 12,
        numeroDependentes: 3,
        credenciado: true,
      });
      expect(onSubmit.mock.calls[0][0]).not.toHaveProperty('email');
    });

    it('entrega a transformação da descrição e o estado booleano de Módulo', async () => {
      const onSubmit = vi.fn();
      render(
        <ThemeProvider>
          <ModuloForm
            initialData={{ nome: '  Vida  ' }}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      fireEvent.change(screen.getByLabelText('Descrição'), {
        target: { value: '   ' },
      });
      fireEvent.click(screen.getByRole('checkbox', { name: /Módulo Ativo/ }));
      submit();
      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledOnce();
        expect(onSubmit.mock.calls[0][0]).toEqual({
          nome: 'Vida',
          descricao: undefined,
          ativo: false,
        });
      });
    });

    it('envia IOF decimal numérico ao incluir vínculo de Ramo', async () => {
      const onSucesso = vi.fn();
      render(
        <ThemeProvider>
          <ApoliceRamoFormModal
            aberto
            onClose={vi.fn()}
            apolicePublicId="apolice-1"
            onSucesso={onSucesso}
          />
        </ThemeProvider>,
      );
      const select = screen.getByLabelText('Ramo');
      await waitFor(() =>
        expect(
          within(select).getByRole('option', { name: 'VG — Vida em Grupo' }),
        ).toBeTruthy(),
      );
      fireEvent.change(select, { target: { value: 'ramo-1' } });
      fireEvent.change(screen.getByLabelText('IOF (%)'), {
        target: { value: '7.38' },
      });
      submit();
      await waitFor(() => expect(onSucesso).toHaveBeenCalledOnce());
      expect(vincularRamoApolice).toHaveBeenCalledExactlyOnceWith('apolice-1', {
        ramoPublicId: 'ramo-1',
        numeroApolice: '',
        iofPercentual: 7.38,
      });
      expect(atualizarRamoApolice).not.toHaveBeenCalled();
    });
  },
);

describe('Validação preservada e tipos de entrada do formulário', () => {
  it('volta da seleção de pessoa jurídica para física com a máscara de CPF', async () => {
    const onSubmit = vi.fn();
    render(
      <ClienteForm
        initialData={{ ...cliente, tipoPessoa: 2, documento: '11222333000181' }}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    fireEvent.change(screen.getByLabelText('Tipo de Pessoa'), {
      target: { value: '1' },
    });
    const documento = screen.getByLabelText<HTMLInputElement>(
      'Documento (CPF/CNPJ)',
    );
    fireEvent.input(documento, { target: { value: '52998224725' } });
    expect(documento.value).toBe('529.982.247-25');
    submit();
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit.mock.calls[0][0].tipoPessoa).toBe(1);
  });

  it('associa erros de endereço à linha de Cliente e bloqueia CPF inválido', async () => {
    const onSubmit = vi.fn();
    render(
      <ClienteForm
        initialData={cliente}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    const tipo = screen.getByLabelText('Tipo de Endereço');
    const documento = screen.getByLabelText('Documento (CPF/CNPJ)');
    fireEvent.change(tipo, { target: { value: '' } });
    fireEvent.input(documento, { target: { value: '11111111111' } });
    submit();
    await waitFor(() => expectFieldError(tipo));
    expectFieldError(documento);
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('preenche campos vazios quando a consulta de CEP devolve logradouro e bairro nulos', async () => {
    vi.mocked(consultarCep).mockResolvedValue({
      cep: '13000000',
      logradouro: null,
      bairro: null,
      cidade: 'Campinas',
      uf: 'SP',
      cidadeId: 3509502,
      estadoId: 35,
    });
    render(
      <ClienteForm
        initialData={cliente}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />,
    );
    fireEvent.input(screen.getByLabelText('CEP'), {
      target: { value: '13000000' },
    });
    await waitFor(() => expect(consultarCep).toHaveBeenCalledWith('13000000'));
    await waitFor(() =>
      expect(screen.getByLabelText<HTMLSelectElement>('Cidade').value).toBe(
        '3509502',
      ),
    );
    expect(screen.getByLabelText<HTMLInputElement>('Logradouro').value).toBe(
      '',
    );
    expect(screen.getByLabelText<HTMLInputElement>('Bairro').value).toBe('');
  });

  it('bloqueia vigência final anterior à inicial em Estipulante', async () => {
    const onSubmit = vi.fn();
    render(
      <EstipulanteForm
        initialData={estipulante}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    const fim = screen.getByLabelText('Fim de Vigência');
    fireEvent.change(fim, { target: { value: '2025-12-31' } });
    submit();
    await waitFor(() => expectFieldError(fim));
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('exige coordenador quando o tipo selecionado é Cooperado', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(
      <CooperadoForm
        initialData={cooperado}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    fireEvent.change(screen.getByLabelText('Tipo'), { target: { value: '1' } });
    const coordenador = screen.getByLabelText('Coordenador responsável');
    submit();
    await waitFor(() => expectFieldError(coordenador));
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('rejeita tipo de Cooperado fracionário fora das opções do domínio', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(
      <CooperadoForm
        initialData={cooperado}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    const tipo = screen.getByLabelText('Tipo');
    const option = document.createElement('option');
    option.value = '1.5';
    tipo.appendChild(option);
    fireEvent.change(tipo, { target: { value: '1.5' } });
    submit();
    await waitFor(() => expectFieldError(tipo));
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it.each(['0', ''])(
    'preserva IOF zero na edição (entrada: %j)',
    async (iof) => {
      const onSucesso = vi.fn();
      render(
        <ApoliceRamoFormModal
          aberto
          ramoEdicao={ramo}
          onClose={vi.fn()}
          apolicePublicId="apolice-1"
          onSucesso={onSucesso}
        />,
      );
      fireEvent.change(screen.getByLabelText('IOF (%)'), {
        target: { value: iof },
      });
      submit();
      await waitFor(() => expect(onSucesso).toHaveBeenCalledOnce());
      expect(atualizarRamoApolice).toHaveBeenCalledExactlyOnceWith(
        'apolice-1',
        'vinculo-1',
        { numeroApolice: '1234', iofPercentual: 0 },
      );
      expect(vincularRamoApolice).not.toHaveBeenCalled();
    },
  );

  it.each(['-0.01', '100.01'])(
    'rejeita IOF fora dos limites (entrada: %s)',
    async (iof) => {
      render(
        <ApoliceRamoFormModal
          aberto
          ramoEdicao={ramo}
          onClose={vi.fn()}
          apolicePublicId="apolice-1"
          onSucesso={vi.fn()}
        />,
      );
      const field = screen.getByLabelText('IOF (%)');
      fireEvent.change(field, { target: { value: iof } });
      submit();
      await waitFor(() => expectFieldError(field));
      expect(atualizarRamoApolice).not.toHaveBeenCalled();
      expect(vincularRamoApolice).not.toHaveBeenCalled();
    },
  );
});
