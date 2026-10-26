import { StrictMode } from 'react';
import { act, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../../shared/theme/ThemeContext';
import { EntityNameResolver } from './EntityNameResolver';

const idA = '11111111-1111-1111-1111-111111111111';
const idB = '22222222-2222-2222-2222-222222222222';
type Fetcher = NonNullable<
  React.ComponentProps<typeof EntityNameResolver>['fetcher']
>;
type Data = Awaited<ReturnType<Fetcher>>;
function deferred() {
  let resolve!: (value: Data) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<Data>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}

afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe('Nome da entidade de Apólice', () => {
  it.each([undefined, '', '   ', '101', 'identificador-inválido'])(
    'não consulta identificador inválido: %s',
    (idValue) => {
      const fetcher = vi.fn<Fetcher>();
      const { container } = render(
        <EntityNameResolver idValue={idValue} fetcher={fetcher} />,
      );
      expect(container.textContent).toBe('');
      expect(fetcher).not.toHaveBeenCalled();
    },
  );

  it.each([
    [{ razaoSocial: 'Razão', nomeFantasia: 'Fantasia', nome: 'Nome' }, 'Razão'],
    [{ razaoSocial: '', nomeFantasia: 'Fantasia', nome: 'Nome' }, 'Fantasia'],
    [{ nome: 'Nome' }, 'Nome'],
    [{}, 'Nome não especificado'],
  ] as const)('preserva prioridade do nome: %j', async (data, expected) => {
    const fetcher = vi.fn<Fetcher>().mockResolvedValue(data);
    render(<EntityNameResolver idValue={idA} fetcher={fetcher} />);
    expect(await screen.findByText(expected)).toBeTruthy();
  });

  it.each([false, true])(
    'ignora resultado antigo e mantém carregamento atual (falha: %s)',
    async (failure) => {
      const old = deferred();
      const current = deferred();
      const fetcher = vi
        .fn<Fetcher>()
        .mockReturnValueOnce(old.promise)
        .mockReturnValueOnce(current.promise);
      const { rerender } = render(
        <EntityNameResolver idValue={idA} fetcher={fetcher} label="empresa" />,
      );
      expect(screen.getByText('Buscando empresa...')).toBeTruthy();
      rerender(
        <EntityNameResolver idValue={idB} fetcher={fetcher} label="empresa" />,
      );
      expect(fetcher.mock.calls[0][1]?.aborted).toBe(true);
      await act(async () => {
        if (failure) old.reject(new Error('Falha antiga'));
        else old.resolve({ nome: 'Antiga' });
      });
      expect(screen.getByText('Buscando empresa...')).toBeTruthy();
      expect(screen.queryByText('Antiga')).toBeNull();
      await act(async () => current.resolve({ nome: 'Atual' }));
      expect(screen.getByText('Atual')).toBeTruthy();
    },
  );

  it('oculta nome anterior e limpa consulta quando o identificador é removido', async () => {
    const fetcher = vi.fn<Fetcher>().mockResolvedValue({ nome: 'Anterior' });
    const { rerender, container } = render(
      <EntityNameResolver idValue={idA} fetcher={fetcher} />,
    );
    await screen.findByText('Anterior');
    rerender(<EntityNameResolver idValue={idB} fetcher={fetcher} />);
    expect(screen.queryByText('Anterior')).toBeNull();
    expect(screen.getByText('Buscando entidade...')).toBeTruthy();
    rerender(<EntityNameResolver fetcher={fetcher} />);
    expect(container.textContent).toBe('');
    expect(fetcher.mock.calls[1][1]?.aborted).toBe(true);
  });

  it('troca fetcher, mostra erro e recupera na próxima entidade sem repetir em rerender', async () => {
    const fetcher = vi.fn<Fetcher>().mockRejectedValue(null);
    const { rerender } = render(
      <EntityNameResolver idValue={idA} fetcher={fetcher} label="Empresa" />,
    );
    expect(await screen.findByText('Empresa não encontrada')).toBeTruthy();
    const next = vi.fn<Fetcher>().mockResolvedValue({ nome: 'Recuperada' });
    rerender(
      <EntityNameResolver idValue={idA} fetcher={next} label="Empresa" />,
    );
    expect(screen.queryByText('Empresa não encontrada')).toBeNull();
    await screen.findByText('Recuperada');
    rerender(
      <EntityNameResolver idValue={idA} fetcher={next} label="Outra legenda" />,
    );
    expect(next).toHaveBeenCalledTimes(1);
  });

  it('suporta StrictMode e aborta ao desmontar', async () => {
    const old = deferred();
    const current = deferred();
    const fetcher = vi
      .fn<Fetcher>()
      .mockReturnValueOnce(old.promise)
      .mockReturnValueOnce(current.promise);
    const { unmount } = render(
      <StrictMode>
        <EntityNameResolver idValue={idA} fetcher={fetcher} />
      </StrictMode>,
    );
    await waitFor(() => expect(fetcher).toHaveBeenCalledTimes(2));
    expect(fetcher.mock.calls[0][1]?.aborted).toBe(true);
    unmount();
    expect(fetcher.mock.calls[1][1]?.aborted).toBe(true);
    await act(async () => {
      old.resolve({ nome: 'Antiga' });
      current.reject(new Error('Falha'));
    });
  });
});

it.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'resolve nome e erro no tema $theme / $resolved',
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
    const fetcher = vi
      .fn<Fetcher>()
      .mockResolvedValueOnce({ nome: 'Encontrada' })
      .mockRejectedValueOnce(new Error('Falha'));
    const { rerender } = render(
      <ThemeProvider>
        <EntityNameResolver idValue={idA} fetcher={fetcher} />
      </ThemeProvider>,
    );
    expect(screen.getByText('Buscando entidade...').className).toContain(
      'text-texto-terciario',
    );
    const name = await screen.findByText('Encontrada');
    expect(name.className).toContain('text-status-sucesso');
    rerender(
      <ThemeProvider>
        <EntityNameResolver idValue={idB} fetcher={fetcher} />
      </ThemeProvider>,
    );
    const error = await screen.findByText('Entidade não encontrada');
    expect(error.className).toContain('text-status-erro');
    expect(document.documentElement.getAttribute('data-theme')).toBe(resolved);
  },
);
