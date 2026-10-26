import { createRef, useState } from 'react';
import type { RefObject } from 'react';
import type { SelectInstance } from 'react-select';
import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AsyncSelect } from './AsyncSelect';
import type { AsyncOptionItem, AsyncSelectProps } from './AsyncSelect';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';

const option: AsyncOptionItem = {
  value: 'cliente-uuid',
  label: 'Cliente Teste',
};

function ControlledSelect({
  selectRef,
  loadOptions,
  onChange,
  onBlur,
}: {
  selectRef: RefObject<SelectInstance<AsyncOptionItem, false> | null>;
  loadOptions: AsyncSelectProps['loadOptions'];
  onChange: (value: AsyncOptionItem | null) => void;
  onBlur: () => void;
}) {
  const [value, setValue] = useState<AsyncOptionItem | null>(null);
  return (
    <AsyncSelect
      ref={selectRef}
      id="async-field"
      name="clientePublicId"
      value={value}
      loadOptions={loadOptions}
      onBlur={onBlur}
      onChange={(next) => {
        onChange(next);
        setValue(next);
      }}
    />
  );
}

afterEach(() => {
  vi.unstubAllGlobals();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Seletor assíncrono no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it('busca, seleciona, limpa e encaminha foco e blur com a referência tipada', async () => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const ref = createRef<SelectInstance<AsyncOptionItem, false>>();
      const loadOptions = vi
        .fn<AsyncSelectProps['loadOptions']>()
        .mockResolvedValue([option]);
      const onChange = vi.fn<(value: AsyncOptionItem | null) => void>();
      const onBlur = vi.fn();
      render(
        <ThemeProvider>
          <ControlledSelect
            selectRef={ref}
            loadOptions={loadOptions}
            onChange={onChange}
            onBlur={onBlur}
          />
        </ThemeProvider>,
      );
      const input = screen.getByRole<HTMLInputElement>('combobox');
      act(() => ref.current?.focus());
      expect(document.activeElement).toBe(input);
      expect(input.id).toBe('async-field');
      expect(loadOptions.mock.calls.map(([query]) => query)).toContain('');
      fireEvent.change(input, { target: { value: 'Teste' } });
      expect(await screen.findByText(option.label)).toBeTruthy();
      expect(loadOptions.mock.calls.map(([query]) => query)).toContain('Teste');
      fireEvent.click(screen.getByText(option.label));
      expect(onChange).toHaveBeenLastCalledWith(option);
      expect(
        document.querySelector<HTMLInputElement>(
          'input[name="clientePublicId"]',
        )?.value,
      ).toBe(option.value);
      act(() => ref.current?.clearValue());
      expect(onChange).toHaveBeenLastCalledWith(null);
      fireEvent.blur(input);
      expect(onBlur).toHaveBeenCalled();
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
  },
);

it('mostra carregamento e descarta o resultado de uma busca antiga', async () => {
  let resolveOld!: (options: AsyncOptionItem[]) => void;
  let resolveCurrent!: (options: AsyncOptionItem[]) => void;
  const loadOptions = vi
    .fn<AsyncSelectProps['loadOptions']>()
    .mockReturnValueOnce(
      new Promise((resolve) => {
        resolveOld = resolve;
      }),
    )
    .mockReturnValueOnce(
      new Promise((resolve) => {
        resolveCurrent = resolve;
      }),
    );
  render(<AsyncSelect defaultOptions={false} loadOptions={loadOptions} />);
  const input = screen.getByRole('combobox');
  fireEvent.change(input, { target: { value: 'Antiga' } });
  expect(screen.getByText('Buscando...')).toBeTruthy();
  fireEvent.change(input, { target: { value: 'Atual' } });
  await act(async () => resolveCurrent([option]));
  expect(screen.getByText(option.label)).toBeTruthy();
  await act(async () =>
    resolveOld([{ value: 'antigo', label: 'Cliente Antigo' }]),
  );
  expect(screen.queryByText('Cliente Antigo')).toBeNull();
  expect(screen.getByText(option.label)).toBeTruthy();
  expect(screen.queryByText('Buscando...')).toBeNull();
});

it('apresenta o estado vazio após falha e permite uma nova busca', async () => {
  const loadOptions = vi
    .fn<AsyncSelectProps['loadOptions']>()
    .mockRejectedValueOnce(new Error('Falha temporária'))
    .mockResolvedValueOnce([option]);
  render(<AsyncSelect defaultOptions={false} loadOptions={loadOptions} />);
  const input = screen.getByRole('combobox');
  fireEvent.change(input, { target: { value: 'Primeira' } });
  await waitFor(() =>
    expect(screen.getByText('Nenhum resultado encontrado')).toBeTruthy(),
  );
  fireEvent.change(input, { target: { value: 'Segunda' } });
  expect(await screen.findByText(option.label)).toBeTruthy();
});
