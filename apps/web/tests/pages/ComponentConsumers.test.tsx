import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { ClienteDetalhePage } from '../../src/features/clientes/pages/ClienteDetalhePage';
import { EstipulanteDetalhePage } from '../../src/features/estipulantes/pages/EstipulanteDetalhePage';
import { useClienteDetalhe } from '../../src/features/clientes/hooks/useClienteDetalhe';
import { useEstipulanteDetalhe } from '../../src/features/estipulantes/hooks/useEstipulanteDetalhe';
import { ModuloForm } from '../../src/features/modulos/components/ModuloForm';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';
import { makeAuthorization } from '../helpers/authorization';

vi.mock('../../src/features/clientes/hooks/useClienteDetalhe', () => ({
  useClienteDetalhe: vi.fn(),
}));
vi.mock('../../src/features/estipulantes/hooks/useEstipulanteDetalhe', () => ({
  useEstipulanteDetalhe: vi.fn(),
}));
vi.mock('../../src/auth/AuthorizationProvider', () => ({
  useAuthorization: () => makeAuthorization(),
}));

const themes = [
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
];

afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

function setTheme(theme: string, dark: boolean) {
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

describe.each(themes)(
  'Consumidores no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it.each([
      { name: 'Cliente', Page: ClienteDetalhePage },
      { name: 'Estipulante', Page: EstipulanteDetalhePage },
    ])(
      'renderiza a retentativa de $name dentro do alerta e aciona o hook',
      ({ Page }) => {
        setTheme(theme, dark);
        const retry = vi.fn();
        const result = {
          data: null,
          isLoading: false,
          error: new Error('Falha temporária'),
          retry,
        };
        vi.mocked(useClienteDetalhe).mockReturnValue(result);
        vi.mocked(useEstipulanteDetalhe).mockReturnValue(result);
        render(
          <ThemeProvider>
            <MemoryRouter>
              <Page />
            </MemoryRouter>
          </ThemeProvider>,
        );

        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
        const alert = screen.getByRole('alert');
        expect(alert.className).toContain('alert-error');
        expect(within(alert).getByText('Falha temporária')).toBeTruthy();
        fireEvent.click(
          within(alert).getByRole('button', { name: 'Tentar novamente' }),
        );
        expect(retry).toHaveBeenCalledOnce();
      },
    );

    it('bloqueia o envio de Módulo e informa processamento durante gravação', () => {
      setTheme(theme, dark);
      const onSubmit = vi.fn();
      render(
        <ThemeProvider>
          <ModuloForm
            initialData={{ nome: 'Vida' }}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
            isSubmitting
          />
        </ThemeProvider>,
      );
      const save = screen.getByRole<HTMLButtonElement>('button', {
        name: /Salvar/,
      });
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
      expect(save.disabled).toBe(true);
      expect(save.getAttribute('aria-busy')).toBe('true');
      expect(
        within(save).getByRole('status', { name: 'Processando...' }),
      ).toBeTruthy();
      fireEvent.click(save);
      expect(onSubmit).not.toHaveBeenCalled();
    });
  },
);
