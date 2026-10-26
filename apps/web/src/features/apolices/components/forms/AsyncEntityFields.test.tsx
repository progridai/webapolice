import { StrictMode, useState } from 'react';
import { act, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { EntitySearchField, type AsyncEntityOption } from './AsyncEntityField';
import { ClienteAsyncSelect } from '../tabs/ClienteAsyncSelect';
import { listarClientes } from '../../../clientes/api/clientesApi';
import { ThemeProvider } from '../../../../shared/theme/ThemeContext';

vi.mock('../../../clientes/api/clientesApi', () => ({
  listarClientes: vi.fn(),
}));
type Searcher = (
  term: string,
  signal?: AbortSignal,
) => Promise<AsyncEntityOption[]>;
const option = { value: 'registro-atual', label: 'Registro Atual' };
const oldOption = { value: 'registro-antigo', label: 'Registro Antigo' };
const kinds = [
  { kind: 'Entidade', delay: 350 },
  { kind: 'Cliente', delay: 500 },
] as const;
type Kind = (typeof kinds)[number]['kind'];

function deferred<T>() {
  let resolve!: (data: T) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<T>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}

interface FieldProps {
  kind: Kind;
  searcher: Searcher;
  onChange: (value: string) => void;
  externalValue?: string;
  initialLabel?: string;
  disabled?: boolean;
}
function Field({
  kind,
  searcher,
  onChange,
  externalValue,
  initialLabel,
  disabled,
}: FieldProps) {
  const [value, setValue] = useState('');
  const common = {
    id: 'registro',
    value: externalValue ?? value,
    disabled,
    onChange: (next: string) => {
      setValue(next);
      onChange(next);
    },
    'aria-describedby': 'registro-ajuda',
    'aria-invalid': 'true' as const,
  };
  return (
    <>
      <label htmlFor="registro">Registro</label>
      <span id="registro-ajuda">Escolha um registro</span>
      {kind === 'Entidade' ? (
        <EntitySearchField
          {...common}
          initialLabel={initialLabel}
          searcher={searcher}
        />
      ) : (
        <ClienteAsyncSelect {...common} initialNome={initialLabel} />
      )}
    </>
  );
}
function prepare(kind: Kind, extras: Partial<FieldProps> = {}) {
  const searcher = vi.fn<Searcher>().mockResolvedValue([option]);
  vi.mocked(listarClientes).mockImplementation(async (query, signal) => ({
    itens: (await searcher(query.nome || '', signal)).map(
      ({ value, label }) => ({
        id: value,
        nome: label,
        documentoMascarado: '***',
        status: 'ativo',
        dataCadastroUtc: '2026-10-22',
      }),
    ),
    paginaAtual: 1,
    tamanhoPagina: 50,
    totalItens: 1,
    totalPaginas: 1,
  }));
  const props: FieldProps = { kind, searcher, onChange: vi.fn(), ...extras };
  return { searcher, props };
}
const input = () => screen.getByLabelText<HTMLInputElement>('Registro');
async function tick(ms: number) {
  await act(async () => {
    await vi.advanceTimersByTimeAsync(ms);
  });
}

beforeEach(() => {
  vi.useFakeTimers();
  vi.mocked(listarClientes).mockReset();
});
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe.each(kinds)('Busca de $kind', ({ kind, delay }) => {
  it('agrupa a digitação, respeita o intervalo e encaminha sinal e filtros', async () => {
    const { props, searcher } = prepare(kind);
    render(<Field {...props} />);
    expect(searcher).not.toHaveBeenCalled();
    fireEvent.focus(input());
    fireEvent.change(input(), { target: { value: 'JO' } });
    await tick(delay - 1);
    fireEvent.change(input(), { target: { value: 'JOÃO' } });
    await tick(delay - 1);
    expect(searcher).not.toHaveBeenCalled();
    expect(screen.queryByText(option.label)).toBeNull();
    await tick(1);
    expect(searcher).toHaveBeenCalledExactlyOnceWith(
      'JOÃO',
      expect.any(AbortSignal),
    );
    expect(screen.getByText(option.label)).toBeTruthy();
    if (kind === 'Cliente')
      expect(listarClientes).toHaveBeenCalledWith(
        { page: 1, pageSize: 50, nome: 'JOÃO', status: 1 },
        expect.any(AbortSignal),
      );
  });

  it.each([false, true])(
    'descarta resultado antigo enquanto aguarda a busca nova (falha: %s)',
    async (failure) => {
      const { props, searcher } = prepare(kind);
      const old = deferred<AsyncEntityOption[]>();
      const current = deferred<AsyncEntityOption[]>();
      searcher
        .mockReturnValueOnce(old.promise)
        .mockReturnValueOnce(current.promise);
      render(<Field {...props} />);
      fireEvent.focus(input());
      await tick(delay);
      const signal = searcher.mock.calls[0][1];
      fireEvent.change(input(), { target: { value: 'Atual' } });
      expect(signal?.aborted).toBe(true);
      await act(async () => {
        if (failure) old.reject(new Error('Falha antiga'));
        else old.resolve([oldOption]);
      });
      expect(screen.queryByText(oldOption.label)).toBeNull();
      expect(screen.queryByRole('alert')).toBeNull();
      expect(screen.getByRole('status')).toBeTruthy();
      await tick(delay);
      await act(async () => current.resolve([option]));
      expect(screen.getByText(option.label)).toBeTruthy();
      expect(screen.queryByRole('status')).toBeNull();
    },
  );

  it('oculta as opções anteriores imediatamente ao digitar outro termo', async () => {
    const { props } = prepare(kind);
    render(<Field {...props} />);
    fireEvent.focus(input());
    await tick(delay);
    expect(screen.getByText(option.label)).toBeTruthy();
    fireEvent.change(input(), { target: { value: 'Outra' } });
    expect(screen.queryByText(option.label)).toBeNull();
    expect(screen.getByRole('status')).toBeTruthy();
  });

  it.each(['mouse', 'teclado'] as const)(
    'seleciona por %s e limpa o identificador ao editar sem apagar a digitação',
    async (method) => {
      const { props, searcher } = prepare(kind);
      render(<Field {...props} />);
      fireEvent.focus(input());
      await tick(delay);
      const button = screen.getByRole('button', { name: option.label });
      if (method === 'mouse') fireEvent.mouseDown(button);
      else fireEvent.click(button, { detail: 0 }); // Ativação de botão por teclado.
      expect(props.onChange).toHaveBeenLastCalledWith(option.value);
      expect(input().value).toBe(option.label);
      expect(searcher.mock.calls[0][1]?.aborted).toBe(true);
      fireEvent.focus(input());
      fireEvent.change(input(), { target: { value: 'Nova Pessoa' } });
      expect(props.onChange).toHaveBeenLastCalledWith('');
      expect(input().value).toBe('Nova Pessoa');
      await tick(delay);
      expect(searcher).toHaveBeenLastCalledWith(
        'Nova Pessoa',
        expect.any(AbortSignal),
      );
    },
  );

  it('acompanha rótulo tardio, troca e reset externos sem ressuscitar a busca', async () => {
    const { props, searcher } = prepare(kind, { externalValue: 'A' });
    const { rerender } = render(<Field {...props} />);
    rerender(<Field {...props} initialLabel="Entidade A" />);
    expect(input().value).toBe('Entidade A');
    fireEvent.focus(input());
    await tick(delay);
    const signal = searcher.mock.calls[0][1];
    rerender(<Field {...props} externalValue="B" initialLabel="Entidade B" />);
    expect(input().value).toBe('Entidade B');
    expect(signal?.aborted).toBe(true);
    expect(screen.queryByText(option.label)).toBeNull();
    rerender(<Field {...props} externalValue="A" initialLabel="Entidade A" />);
    await tick(delay);
    expect(searcher).toHaveBeenCalledTimes(1);
    expect(input().value).toBe('Entidade A');
    rerender(<Field {...props} externalValue="" initialLabel="Entidade A" />);
    expect(input().value).toBe('');
    expect(props.onChange).not.toHaveBeenCalled();
  });

  it.each(['fora', 'Escape', 'blur', 'desabilitar', 'desmontar'] as const)(
    'cancela chamada e timer ao fechar: %s',
    async (action) => {
      const { props, searcher } = prepare(kind, {
        externalValue: 'A',
        initialLabel: 'Seleção Inicial',
      });
      const pending = deferred<AsyncEntityOption[]>();
      searcher.mockReturnValueOnce(pending.promise);
      const { rerender, unmount } = render(<Field {...props} />);
      fireEvent.focus(input());
      await tick(delay);
      const signal = searcher.mock.calls[0][1];
      if (action === 'fora') fireEvent.mouseDown(document.body);
      else if (action === 'Escape')
        fireEvent.keyDown(input(), { key: 'Escape' });
      else if (action === 'blur') fireEvent.blur(input());
      else if (action === 'desabilitar')
        rerender(<Field {...props} disabled />);
      else unmount();
      expect(signal?.aborted).toBe(true);
      await act(async () => pending.resolve([oldOption]));
      expect(screen.queryByText(oldOption.label)).toBeNull();
      if (action !== 'desmontar') expect(input().value).toBe('Seleção Inicial');
      if (action === 'desabilitar') {
        rerender(<Field {...props} />);
        await tick(delay);
        expect(searcher).toHaveBeenCalledTimes(1);
      }
    },
  );

  it('mantém a lista ao mover o foco para opções e fecha ao sair pelo teclado', async () => {
    const { props } = prepare(kind);
    render(<Field {...props} />);
    fireEvent.focus(input());
    await tick(delay);
    const button = screen.getByRole('button', { name: option.label });
    fireEvent.blur(input(), { relatedTarget: button });
    expect(screen.getByRole('button', { name: option.label })).toBeTruthy();
    fireEvent.blur(button, { relatedTarget: document.body });
    expect(screen.queryByRole('button', { name: option.label })).toBeNull();
  });

  it('cancela o debounce antes de iniciar chamada e mantém o campo desabilitado', async () => {
    const { props, searcher } = prepare(kind);
    const { rerender } = render(<Field {...props} />);
    fireEvent.focus(input());
    await tick(delay - 1);
    fireEvent.mouseDown(document.body);
    await tick(delay);
    expect(searcher).not.toHaveBeenCalled();
    rerender(<Field {...props} disabled />);
    expect(input().disabled).toBe(true);
    fireEvent.focus(input());
    await tick(delay);
    expect(searcher).not.toHaveBeenCalled();
  });

  it('expõe erro distinto do vazio e refaz a busca sem enviar o formulário', async () => {
    const { props, searcher } = prepare(kind);
    searcher.mockRejectedValueOnce(null).mockResolvedValueOnce([]);
    const onSubmit = vi.fn((event: React.FormEvent) => event.preventDefault());
    render(
      <form onSubmit={onSubmit}>
        <Field {...props} />
      </form>,
    );
    fireEvent.focus(input());
    fireEvent.change(input(), { target: { value: 'Busca Atual' } });
    await tick(delay);
    expect(screen.getByRole('alert').textContent).toContain(
      'Não foi possível carregar',
    );
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
    await tick(delay);
    expect(searcher).toHaveBeenLastCalledWith(
      'Busca Atual',
      expect.any(AbortSignal),
    );
    expect(screen.queryByRole('alert')).toBeNull();
    expect(
      screen.getByText(
        kind === 'Cliente'
          ? 'Nenhum cliente ativo encontrado.'
          : 'Nenhum resultado encontrado.',
      ),
    ).toBeTruthy();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('aborta a nova tentativa ao desmontar', async () => {
    const { props, searcher } = prepare(kind);
    const pending = deferred<AsyncEntityOption[]>();
    searcher
      .mockRejectedValueOnce(new Error('Falha'))
      .mockReturnValueOnce(pending.promise);
    const { unmount } = render(<Field {...props} />);
    fireEvent.focus(input());
    await tick(delay);
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
    await tick(delay);
    unmount();
    expect(searcher.mock.calls[1][1]?.aborted).toBe(true);
    await act(async () => pending.reject(new Error('Falha tardia')));
  });

  it('preserva atributos acessíveis, não repete busca em rerender e suporta StrictMode', async () => {
    const { props, searcher } = prepare(kind);
    const { rerender } = render(
      <StrictMode>
        <Field {...props} />
      </StrictMode>,
    );
    expect(input().getAttribute('aria-describedby')).toBe('registro-ajuda');
    expect(input().getAttribute('aria-invalid')).toBe('true');
    fireEvent.focus(input());
    await tick(delay);
    rerender(
      <StrictMode>
        <Field {...props} />
      </StrictMode>,
    );
    await tick(delay);
    expect(searcher).toHaveBeenCalledTimes(1);
  });

  it.each([
    { theme: 'claro', dark: false, resolved: 'light' },
    { theme: 'escuro', dark: false, resolved: 'dark' },
    { theme: 'sistema', dark: false, resolved: 'light' },
    { theme: 'sistema', dark: true, resolved: 'dark' },
  ])(
    'seleciona, carrega e apresenta erro no tema $theme / $resolved',
    async ({ theme, dark, resolved }) => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const { props, searcher } = prepare(kind);
      render(
        <ThemeProvider>
          <Field {...props} />
        </ThemeProvider>,
      );
      fireEvent.focus(input());
      expect(screen.getByRole('status')).toBeTruthy();
      await tick(delay);
      fireEvent.mouseDown(screen.getByRole('button', { name: option.label }));
      expect(input().value).toBe(option.label);
      searcher.mockRejectedValueOnce(new Error('Falha'));
      fireEvent.focus(input());
      await tick(delay);
      expect(screen.getByRole('alert')).toBeTruthy();
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    },
  );
});
