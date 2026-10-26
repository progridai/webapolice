import { createRef, useState } from 'react';
import type { ChangeEvent, RefObject } from 'react';
import { act, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { CepInput } from './CepInput';
import { CnpjInput } from './CnpjInput';
import { CpfInput } from './CpfInput';
import { PhoneInput } from './PhoneInput';
import { ThemeProvider } from '../../shared/theme/ThemeContext';

const fields = [
  { name: 'CEP', Component: CepInput, raw: '12345678', masked: '12345-678' },
  {
    name: 'CNPJ',
    Component: CnpjInput,
    raw: '11222333000181',
    masked: '11.222.333/0001-81',
  },
  {
    name: 'CPF',
    Component: CpfInput,
    raw: '52998224725',
    masked: '529.982.247-25',
  },
  {
    name: 'Telefone Fixo',
    Component: PhoneInput,
    raw: '1133334444',
    masked: '(11) 3333-4444',
  },
  {
    name: 'Celular',
    Component: PhoneInput,
    raw: '11999998888',
    masked: '(11) 99999-8888',
  },
];

function ControlledField({
  Component,
  inputRef,
  onChange,
  onBlur,
}: {
  Component: typeof CepInput;
  inputRef: RefObject<HTMLInputElement | null>;
  onChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onBlur: () => void;
}) {
  const [value, setValue] = useState('');
  return (
    <Component
      id="masked-field"
      name="documento"
      aria-label="Campo com máscara"
      ref={inputRef}
      value={value}
      error
      className="classe-adicional"
      aria-invalid="true"
      aria-describedby="field-error"
      onBlur={onBlur}
      onChange={(event) => {
        onChange(event);
        setValue(event.target.value);
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
  'Máscaras no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it.each(fields)(
      'mantém máscara, callback, referência e atributos de $name',
      ({ Component, raw, masked }) => {
        localStorage.setItem('webapolice-tema', theme);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        const ref = createRef<HTMLInputElement>();
        const onChange =
          vi.fn<(event: ChangeEvent<HTMLInputElement>) => void>();
        const onBlur = vi.fn();
        render(
          <ThemeProvider>
            <ControlledField
              Component={Component}
              inputRef={ref}
              onChange={onChange}
              onBlur={onBlur}
            />
            <span id="field-error">Revise o campo.</span>
          </ThemeProvider>,
        );
        const input =
          screen.getByLabelText<HTMLInputElement>('Campo com máscara');
        expect(ref.current).toBe(input);
        act(() => ref.current?.focus());
        expect(document.activeElement).toBe(input);
        fireEvent.input(input, { target: { value: raw } });
        expect(input.value).toBe(masked);
        expect(onChange).toHaveBeenLastCalledWith(
          expect.objectContaining({
            target: { name: 'documento', value: masked },
          }),
        );
        fireEvent.blur(input);
        expect(onBlur).toHaveBeenCalledOnce();
        expect(input.classList.contains('form-input')).toBe(true);
        expect(input.classList.contains('input-erro')).toBe(true);
        expect(input.classList.contains('classe-adicional')).toBe(true);
        expect(input.getAttribute('aria-invalid')).toBe('true');
        expect(input.getAttribute('aria-describedby')).toBe('field-error');
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      },
    );
  },
);

it.each(fields)(
  'aceita valor numérico e conserva estados nativos de $name',
  ({ Component, raw, masked }) => {
    const { rerender } = render(
      <Component aria-label="Valor inicial" value={Number(raw)} readOnly />,
    );
    const input = screen.getByLabelText<HTMLInputElement>('Valor inicial');
    expect(input.value).toBe(masked);
    expect(input.readOnly).toBe(true);
    rerender(<Component aria-label="Valor inicial" value="" disabled />);
    expect(input.value).toBe('');
    expect(input.disabled).toBe(true);
  },
);
