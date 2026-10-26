import { Profiler } from 'react';
import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CooperadoForm } from '../../src/features/cooperados/components/CooperadoForm';
import type { CooperadoFormData } from '../../src/features/cooperados/types/cooperados.types';
import {
  buscarCidadesPorUf,
  type CidadeResponse,
} from '../../src/features/clientes/api/localidadesApi';
import { listarCoordenadoresAtivos } from '../../src/features/cooperados/api/cooperadosApi';
import {
  consultarCep,
  type EnderecoConsultaResult,
} from '../../src/shared/api/enderecosApi';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/features/clientes/api/localidadesApi');
vi.mock('../../src/features/cooperados/api/cooperadosApi');
vi.mock('../../src/shared/api/enderecosApi');

const initial: CooperadoFormData = {
  tipo: 2,
  nome: 'Cooperado Teste',
  cpf: '52998224725',
};
const sp = [
  { id: 35, nome: 'Campinas' },
  { id: 36, nome: 'São Paulo' },
];
const rs = [{ id: 43, nome: 'Porto Alegre' }];
const address: EnderecoConsultaResult = {
  cep: '12345678',
  logradouro: 'Rua Teste',
  bairro: 'Centro',
  uf: 'SP',
  cidade: 'Campinas',
  cidadeId: 35,
  estadoId: 1,
};
function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (reason: unknown) => void;
  const promise = new Promise<T>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}
function setup(data = initial, strict = false) {
  const onSubmit = vi
    .fn<(data: CooperadoFormData) => Promise<void>>()
    .mockResolvedValue(undefined);
  const view = render(
    <ThemeProvider>
      <CooperadoForm
        initialData={data}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />
    </ThemeProvider>,
    { reactStrictMode: strict },
  );
  return { ...view, onSubmit };
}
function submitOnCitiesCommit(optionCount: number) {
  const onSubmit = vi
    .fn<(data: CooperadoFormData) => Promise<void>>()
    .mockResolvedValue(undefined);
  let submitted = false;
  render(
    <Profiler
      id="cidade-pronta"
      onRender={() => {
        const select = document.querySelector<HTMLSelectElement>(
          'select[name="cidadeId"]',
        );
        const state =
          document.querySelector<HTMLSelectElement>('select[name="uf"]');
        const button = document.querySelector<HTMLButtonElement>(
          'button[type="submit"]',
        );
        const form = select?.form;
        if (
          !submitted &&
          form &&
          state?.value === 'SP' &&
          select &&
          !select.disabled &&
          button &&
          !button.disabled &&
          select.options.length === optionCount + 1
        ) {
          submitted = true;
          // Envia no commit que disponibiliza as opções, antes dos efeitos passivos.
          form.dispatchEvent(
            new Event('submit', { bubbles: true, cancelable: true }),
          );
        }
      }}
    >
      <ThemeProvider>
        <CooperadoForm
          initialData={initial}
          onSubmit={onSubmit}
          onCancel={vi.fn()}
        />
      </ThemeProvider>
    </Profiler>,
  );
  return { onSubmit, wasSubmitted: () => submitted };
}
const city = () => screen.getByLabelText<HTMLSelectElement>('Cidade');
const uf = (value: string) =>
  fireEvent.change(screen.getByLabelText('Estado (UF)'), { target: { value } });
const cep = (value = '12345678') =>
  fireEvent.input(screen.getByLabelText(/CEP/), { target: { value } });
const save = () =>
  screen.getByRole<HTMLButtonElement>('button', { name: /^Salvar$/ });
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
  vi.mocked(listarCoordenadoresAtivos).mockResolvedValue([]);
  vi.mocked(buscarCidadesPorUf).mockResolvedValue(sp);
  vi.mocked(consultarCep).mockResolvedValue(address);
});
afterEach(() => {
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Cidade por CEP em $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it.each([
      {
        scenario: 'lista vazia',
        options: [],
        received: 35,
        expected: undefined,
      },
      {
        scenario: 'ID fora da lista',
        options: sp,
        received: 999,
        expected: undefined,
      },
      { scenario: 'ID válido', options: sp, received: 35, expected: 35 },
    ])(
      'confere $scenario ao enviar no primeiro commit disponível',
      async ({ options, received, expected }) => {
        localStorage.setItem('webapolice-tema', theme);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        vi.mocked(buscarCidadesPorUf).mockResolvedValue(options);
        vi.mocked(consultarCep).mockResolvedValue({
          ...address,
          cidadeId: received,
        });
        const { onSubmit, wasSubmitted } = submitOnCitiesCommit(options.length);
        cep();
        await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
        expect(wasSubmitted()).toBe(true);
        const payload = onSubmit.mock.calls[0][0];
        expect(payload).toMatchObject({ cep: '12345678', uf: 'SP' });
        if (expected === undefined)
          expect(payload).not.toHaveProperty('cidadeId');
        else expect(payload.cidadeId).toBe(expected);
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      },
    );

    it('seleciona a cidade quando as opções chegam e envia o ID numérico correto', async () => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const lookup = deferred<CidadeResponse[]>();
      vi.mocked(buscarCidadesPorUf).mockReturnValue(lookup.promise);
      const { onSubmit } = setup();
      cep();
      await waitFor(() => expect(buscarCidadesPorUf).toHaveBeenCalled());
      expect(city().disabled).toBe(true);
      await act(async () => lookup.resolve(sp));
      await waitFor(() => expect(city().value).toBe('35'));
      expect(city().disabled).toBe(false);
      expect(
        within(city()).getByRole<HTMLOptionElement>('option', {
          name: 'Campinas',
        }).selected,
      ).toBe(true);
      fireEvent.click(save());
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toMatchObject({
        cep: '12345678',
        uf: 'SP',
        cidadeId: 35,
      });
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });

    it('exibe erro acessível e permite tentar novamente sem reaproveitar cidade inválida', async () => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      vi.mocked(buscarCidadesPorUf)
        .mockRejectedValueOnce(new Error('Detalhe técnico privado'))
        .mockResolvedValueOnce(sp);
      const { onSubmit } = setup({ ...initial, uf: 'SP', cidadeId: 35 });
      expect(await screen.findByRole('alert')).toBeTruthy();
      expect(city().getAttribute('aria-invalid')).toBe('true');
      expect(
        document.getElementById(city().getAttribute('aria-describedby')!)
          ?.textContent,
      ).toContain('Não foi possível carregar as cidades.');
      expect(screen.queryByText('Detalhe técnico privado')).toBeNull();
      expect(city().value).toBe('');
      fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
      await within(city()).findByRole('option', { name: 'Campinas' });
      expect(screen.queryByRole('alert')).toBeNull();
      expect(city().getAttribute('aria-invalid')).toBeNull();
      fireEvent.change(city(), { target: { value: '35' } });
      fireEvent.click(save());
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0].cidadeId).toBe(35);
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
  },
);

describe('Ciclo da cidade de Cooperados', () => {
  it.each(['CEP', 'cidades'])(
    'ignora envio direto com %s pendente e aceita uma tentativa após a conclusão',
    async (operation) => {
      const cities = deferred<CidadeResponse[]>();
      const lookup = deferred<EnderecoConsultaResult>();
      if (operation === 'CEP')
        vi.mocked(consultarCep).mockReturnValue(lookup.promise);
      else vi.mocked(buscarCidadesPorUf).mockReturnValue(cities.promise);
      const { onSubmit } = setup(
        operation === 'CEP' ? initial : { ...initial, uf: 'SP', cidadeId: 35 },
      );
      if (operation === 'CEP') cep();
      expect(save().disabled).toBe(true);
      const form = city().form;
      expect(form).toBeTruthy();
      await act(async () => {
        form?.dispatchEvent(
          new Event('submit', { bubbles: true, cancelable: true }),
        );
      });
      expect(onSubmit).not.toHaveBeenCalled();
      await act(async () => {
        if (operation === 'CEP') lookup.resolve(address);
        else cities.resolve(sp);
      });
      await waitFor(() => expect(save().disabled).toBe(false));
      fireEvent.click(save());
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0].cidadeId).toBe(35);
    },
  );

  it('preserva cidade da edição após carregar opções e não consulta novamente o CEP inicial', async () => {
    const { onSubmit } = setup({
      ...initial,
      uf: 'SP',
      cidadeId: 35,
      cep: '12345678',
    });
    await waitFor(() => expect(city().value).toBe('35'));
    await waitFor(() => expect(city().disabled).toBe(false));
    expect(consultarCep).not.toHaveBeenCalled();
    fireEvent.click(save());
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit.mock.calls[0][0]).toMatchObject({ cidadeId: 35, uf: 'SP' });
  });

  it('usa opções já carregadas para CEP da mesma UF e conserva mudança manual de cidade', async () => {
    setup({ ...initial, uf: 'SP' });
    await within(city()).findByRole('option', { name: 'Campinas' });
    cep();
    await waitFor(() => expect(city().value).toBe('35'));
    fireEvent.change(city(), { target: { value: '36' } });
    fireEvent.change(screen.getByLabelText(/^Nome/), {
      target: { value: 'Nome Alterado' },
    });
    expect(city().value).toBe('36');
    cep('87654321');
    await waitFor(() => expect(consultarCep).toHaveBeenCalledTimes(2));
    await waitFor(() => expect(city().value).toBe('35'));
    expect(buscarCidadesPorUf).toHaveBeenCalledOnce();
  });

  it.each([null, 999])(
    'permite escolha manual quando o CEP retorna cidade %s fora das opções',
    async (cidadeId) => {
      vi.mocked(consultarCep).mockResolvedValue({ ...address, cidadeId });
      const { onSubmit } = setup();
      cep();
      await within(city()).findByRole('option', { name: 'Campinas' });
      await waitFor(() => expect(city().value).toBe(''));
      fireEvent.click(save());
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).not.toHaveProperty('cidadeId');
      fireEvent.change(city(), { target: { value: '36' } });
      fireEvent.click(save());
      await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(2));
      expect(onSubmit.mock.calls[1][0].cidadeId).toBe(36);
    },
  );

  it('mantém o estado vazio sem cidade inexistente quando a UF não retorna opções', async () => {
    vi.mocked(buscarCidadesPorUf).mockResolvedValue([]);
    const { onSubmit } = setup();
    cep();
    await waitFor(() => expect(city().disabled).toBe(false));
    expect(city().options).toHaveLength(1);
    expect(city().value).toBe('');
    fireEvent.click(save());
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit.mock.calls[0][0]).not.toHaveProperty('cidadeId');
  });

  it('limpa a cidade ao mudar de UF, mesmo se o mesmo ID aparecer na nova lista', async () => {
    vi.mocked(buscarCidadesPorUf)
      .mockResolvedValueOnce(sp)
      .mockResolvedValueOnce([{ id: 35, nome: 'Outra Cidade' }]);
    const { onSubmit } = setup({ ...initial, uf: 'SP', cidadeId: 35 });
    await within(city()).findByRole('option', { name: 'Campinas' });
    uf('RS');
    await within(city()).findByRole('option', { name: 'Outra Cidade' });
    expect(city().value).toBe('');
    fireEvent.click(save());
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit.mock.calls[0][0]).toMatchObject({ uf: 'RS' });
    expect(onSubmit.mock.calls[0][0]).not.toHaveProperty('cidadeId');
  });

  it.each(['sucesso', 'falha'])(
    'descarta %s da UF anterior sem encerrar o carregamento atual',
    async (outcome) => {
      const old = deferred<CidadeResponse[]>();
      const current = deferred<CidadeResponse[]>();
      vi.mocked(buscarCidadesPorUf)
        .mockReturnValueOnce(old.promise)
        .mockReturnValueOnce(current.promise);
      setup({ ...initial, uf: 'SP' });
      uf('RS');
      expect(vi.mocked(buscarCidadesPorUf).mock.calls[0][1]?.aborted).toBe(
        true,
      );
      await act(async () => {
        if (outcome === 'sucesso') old.resolve(sp);
        else old.reject(new Error('Falha antiga'));
      });
      expect(city().disabled).toBe(true);
      expect(save().disabled).toBe(true);
      expect(
        within(city()).queryByRole('option', { name: 'Campinas' }),
      ).toBeNull();
      expect(screen.queryByRole('alert')).toBeNull();
      await act(async () => current.resolve(rs));
      expect(city().disabled).toBe(false);
      expect(
        within(city()).getByRole('option', { name: 'Porto Alegre' }),
      ).toBeTruthy();
    },
  );

  it.each(['sucesso', 'falha'])(
    'ignora %s antigo depois de limpar a UF',
    async (outcome) => {
      const lookup = deferred<CidadeResponse[]>();
      vi.mocked(buscarCidadesPorUf).mockReturnValue(lookup.promise);
      setup({ ...initial, uf: 'SP', cidadeId: 35 });
      uf('');
      expect(vi.mocked(buscarCidadesPorUf).mock.calls[0][1]?.aborted).toBe(
        true,
      );
      await act(async () => {
        if (outcome === 'sucesso') lookup.resolve(sp);
        else lookup.reject(null);
      });
      expect(city().value).toBe('');
      expect(city().disabled).toBe(true);
      expect(city().options).toHaveLength(1);
      expect(screen.queryByRole('alert')).toBeNull();
    },
  );

  it('mantém a cidade do CEP mais recente ao receber a lista da UF anterior depois', async () => {
    const old = deferred<CidadeResponse[]>();
    vi.mocked(buscarCidadesPorUf)
      .mockReturnValueOnce(old.promise)
      .mockResolvedValueOnce(rs);
    vi.mocked(consultarCep)
      .mockResolvedValueOnce(address)
      .mockResolvedValueOnce({
        ...address,
        cep: '87654321',
        uf: 'RS',
        cidadeId: 43,
      });
    const { onSubmit } = setup();
    cep();
    await waitFor(() => expect(buscarCidadesPorUf).toHaveBeenCalledOnce());
    cep('87654321');
    await waitFor(() => expect(city().value).toBe('43'));
    await act(async () => old.resolve(sp));
    expect(city().value).toBe('43');
    expect(
      within(city()).queryByRole('option', { name: 'Campinas' }),
    ).toBeNull();
    fireEvent.click(save());
    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit.mock.calls[0][0]).toMatchObject({
      cep: '87654321',
      uf: 'RS',
      cidadeId: 43,
    });
  });

  it('aplica o ID do CEP mais recente da mesma UF quando a lista pendente chega', async () => {
    const lookup = deferred<CidadeResponse[]>();
    vi.mocked(buscarCidadesPorUf).mockReturnValue(lookup.promise);
    vi.mocked(consultarCep)
      .mockResolvedValueOnce(address)
      .mockResolvedValueOnce({ ...address, cep: '87654321', cidadeId: 36 });
    setup();
    cep();
    await waitFor(() => expect(buscarCidadesPorUf).toHaveBeenCalledOnce());
    cep('87654321');
    await waitFor(() => expect(consultarCep).toHaveBeenCalledTimes(2));
    await act(async () => lookup.resolve(sp));
    expect(city().value).toBe('36');
    expect(buscarCidadesPorUf).toHaveBeenCalledOnce();
  });

  it('limpa o CEP enquanto a cidade carrega e não restaura a seleção com resposta tardia', async () => {
    const lookup = deferred<CidadeResponse[]>();
    vi.mocked(buscarCidadesPorUf).mockReturnValue(lookup.promise);
    setup();
    cep();
    await waitFor(() => expect(buscarCidadesPorUf).toHaveBeenCalledOnce());
    cep('');
    expect(vi.mocked(buscarCidadesPorUf).mock.calls[0][1]?.aborted).toBe(true);
    await act(async () => lookup.resolve(sp));
    expect(city().value).toBe('');
    expect(city().disabled).toBe(true);
    expect(screen.getByLabelText<HTMLSelectElement>('Estado (UF)').value).toBe(
      '',
    );
  });

  it('preserva UF manual após cancelar uma consulta de CEP ainda pendente', async () => {
    const lookup = deferred<EnderecoConsultaResult>();
    vi.mocked(consultarCep).mockReturnValue(lookup.promise);
    vi.mocked(buscarCidadesPorUf).mockResolvedValue(rs);
    setup();
    cep();
    expect(save().disabled).toBe(true);
    uf('RS');
    expect(vi.mocked(consultarCep).mock.calls[0][1]?.aborted).toBe(true);
    await within(city()).findByRole('option', { name: 'Porto Alegre' });
    fireEvent.change(city(), { target: { value: '43' } });
    await act(async () => lookup.resolve(address));
    expect(city().value).toBe('43');
    expect(screen.getByLabelText<HTMLSelectElement>('Estado (UF)').value).toBe(
      'RS',
    );
    expect(screen.queryByText(/CEP \(Buscando/)).toBeNull();
  });

  it('aborta consultas de cidades e CEP ao desmontar', async () => {
    const cities = deferred<CidadeResponse[]>();
    const lookup = deferred<EnderecoConsultaResult>();
    vi.mocked(buscarCidadesPorUf).mockReturnValue(cities.promise);
    vi.mocked(consultarCep).mockReturnValue(lookup.promise);
    const view = setup({ ...initial, uf: 'SP' });
    cep();
    view.unmount();
    expect(vi.mocked(buscarCidadesPorUf).mock.calls[0][1]?.aborted).toBe(true);
    expect(vi.mocked(consultarCep).mock.calls[0][1]?.aborted).toBe(true);
    await act(async () => {
      cities.resolve(sp);
      lookup.resolve(address);
    });
    expect(screen.queryByLabelText('Cidade')).toBeNull();
  });

  it('descarta montagem anterior de StrictMode e preserva cidade inicial', async () => {
    const old = deferred<CidadeResponse[]>();
    vi.mocked(buscarCidadesPorUf)
      .mockReturnValueOnce(old.promise)
      .mockResolvedValueOnce(sp);
    setup({ ...initial, uf: 'SP', cidadeId: 35 }, true);
    await waitFor(() => expect(city().disabled).toBe(false));
    expect(city().value).toBe('35');
    expect(vi.mocked(buscarCidadesPorUf).mock.calls[0][1]?.aborted).toBe(true);
    await act(async () => old.resolve(rs));
    expect(city().value).toBe('35');
    expect(
      within(city()).queryByRole('option', { name: 'Porto Alegre' }),
    ).toBeNull();
  });
});
