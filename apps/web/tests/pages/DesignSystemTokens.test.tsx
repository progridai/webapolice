import { fireEvent, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { DesignSystemPage } from '../../src/pages/DesignSystem/DesignSystemPage';
import { AsyncSelect } from '../../src/components/ui/Select/AsyncSelect';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';
import {
  IdentidadeVisualService,
  obterCorContraste,
} from '../../src/shared/identidade/IdentidadeVisualService';

// Cálculo independente do resultado do serviço para conferir a razão de contraste.
function luminance(hex: string) {
  const components = hex
    .slice(1)
    .match(/../g)!
    .map((part) => {
      const value = parseInt(part, 16) / 255;
      return value <= 0.04045
        ? value / 12.92
        : ((value + 0.055) / 1.055) ** 2.4;
    });
  return (
    components[0] * 0.2126 + components[1] * 0.7152 + components[2] * 0.0722
  );
}
function contrast(background: string, foreground: string) {
  const values = [luminance(background), luminance(foreground)].sort(
    (a, b) => b - a,
  );
  return (values[0] + 0.05) / (values[1] + 0.05);
}
const samples = [
  { name: 'Azul Escuro', color: '#0055FF' },
  { name: 'Verde Neon', color: '#00FF00' },
  { name: 'Vermelho Vivo', color: '#FF0000' },
  { name: 'Roxo Escuro', color: '#6B21A8' },
  { name: 'Preto Absoluto', color: '#000000' },
  { name: 'Branco Puro', color: '#FFFFFF' },
];
afterEach(() => {
  IdentidadeVisualService.restaurarIdentidadePadrao();
  for (const { color } of samples)
    localStorage.removeItem(
      IdentidadeVisualService.getStorageKey('teste-' + color.slice(1)),
    );
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
  'Tokens no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    function setTheme() {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
    }
    it('mantém as seis simulações de marca com controles secundários e restaura o dourado', () => {
      setTheme();
      render(
        <ThemeProvider>
          <DesignSystemPage />
        </ThemeProvider>,
      );
      for (const { name, color } of samples) {
        const button = screen.getByRole<HTMLButtonElement>('button', { name });
        expect(button.classList.contains('btn-secondary')).toBe(true);
        expect(button.hasAttribute('style')).toBe(false);
        fireEvent.click(button);
        const style = document.documentElement.style;
        expect(style.getPropertyValue('--cor-marca-principal')).toBe(color);
        expect(
          contrast(
            color,
            style.getPropertyValue('--cor-sobre-marca-principal'),
          ),
        ).toBeGreaterThanOrEqual(4.5);
        expect(localStorage.getItem('webapolice-identidade-atual')).toBe(
          'teste-' + color.slice(1),
        );
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      }
      fireEvent.click(screen.getByRole('button', { name: 'Dourado (Padrão)' }));
      expect(
        document.documentElement.style.getPropertyValue(
          '--cor-marca-principal',
        ),
      ).toBe('');
      expect(localStorage.getItem('webapolice-identidade-atual')).toBeNull();
      expect(
        IdentidadeVisualService.obterIdentidadeAtual().marcaPrincipal,
      ).toBe('#D4AF37');
    });

    it('usa tokens oficiais para opção selecionada, foco, menu e erro do seletor', async () => {
      setTheme();
      const option = { value: '1', label: 'Opção Selecionada' };
      const view = render(
        <ThemeProvider>
          <AsyncSelect
            id="token-select"
            value={option}
            defaultOptions={[option]}
            loadOptions={async () => [option]}
          />
        </ThemeProvider>,
      );
      const input = screen.getByRole('combobox');
      fireEvent.keyDown(input, { key: 'ArrowDown' });
      const selected = await screen.findByRole('option', {
        name: option.label,
        selected: true,
      });
      expect(getComputedStyle(selected).color).toBe(
        'var(--cor-sobre-marca-principal)',
      );
      expect(getComputedStyle(selected).backgroundColor).toBe(
        'var(--cor-marca-principal)',
      );
      const control = document.querySelector('.react-select__control')!;
      expect(getComputedStyle(control).backgroundColor).toBe(
        'var(--cor-fundo-superficie)',
      );
      expect(
        getComputedStyle(document.querySelector('.react-select__menu')!)
          .boxShadow,
      ).toBe('var(--sombra-card)');
      view.rerender(
        <ThemeProvider>
          <AsyncSelect
            id="token-select"
            error
            value={option}
            defaultOptions={[option]}
            loadOptions={async () => [option]}
          />
        </ThemeProvider>,
      );
      expect(getComputedStyle(control).borderColor).toBe('var(--cor-erro)');
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
  },
);

it('garante contraste AA na escolha automática para a escala cinza e as marcas de demonstração', () => {
  const grayscale = Array.from(
    { length: 256 },
    (_, value) => '#' + value.toString(16).padStart(2, '0').repeat(3),
  );
  for (const color of [
    '#D4AF37',
    ...samples.map((sample) => sample.color),
    ...grayscale,
  ]) {
    expect(
      contrast(color, obterCorContraste(color)),
      color,
    ).toBeGreaterThanOrEqual(4.5);
  }
});

it('usa preto puro quando branco e preto suave falham no vermelho intermediário', () => {
  expect(contrast('#FF0000', '#1A1A1A')).toBeLessThan(4.5);
  expect(contrast('#FF0000', '#FFFFFF')).toBeLessThan(4.5);
  expect(obterCorContraste('#FF0000')).toBe('#000000');
});
