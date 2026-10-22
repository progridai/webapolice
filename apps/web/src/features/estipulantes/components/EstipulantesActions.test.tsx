import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { EstipulantesTable } from './EstipulantesTable';
import { EstipulantesMobileList } from './EstipulantesMobileList';
import type { EstipulanteListItem } from '../types/estipulante.types';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';

const themes = [
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
];
const cases = themes.flatMap((theme) =>
  ['tabela', 'mobile'].flatMap((view) =>
    [true, false].map((active) => ({ ...theme, view, active })),
  ),
);

afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

function renderList(view: string, active: boolean, allowed = true) {
  const onInativar = vi.fn();
  const onReativar = vi.fn();
  const estipulante: EstipulanteListItem = {
    id: 1,
    publicId: 'estip-1',
    razaoSocial: 'Empresa Teste',
    cnpj: '61198164000160',
    ativo: active,
    dataCadastro: '2026-01-01',
  };
  const props = {
    estipulantes: [estipulante],
    podeInativar: allowed,
    podeReativar: allowed,
    onInativar,
    onReativar,
  };
  render(
    <ThemeProvider>
      {view === 'tabela' ? (
        <EstipulantesTable {...props} isLoading={false} onSort={vi.fn()} />
      ) : (
        <EstipulantesMobileList {...props} />
      )}
    </ThemeProvider>,
  );
  return { onInativar, onReativar };
}

describe('Confirmação das ações de Estipulantes', () => {
  it.each(cases)(
    '$view, $theme (sistema escuro: $dark), ativo: $active',
    ({ theme, dark, resolved, view, active }) => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const { onInativar, onReativar } = renderList(view, active);
      const action = active ? 'Inativar' : 'Reativar';
      const callback = active ? onInativar : onReativar;
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );

      const openConfirmation = () => {
        fireEvent.click(screen.getByRole('button', { name: 'Mais ações' }));
        const menuItem = screen.getByRole('menuitem', { name: action });
        if (active)
          expect(menuItem.className).toContain('row-actions-item-danger');
        fireEvent.click(menuItem);
      };
      openConfirmation();
      expect(callback).not.toHaveBeenCalled();
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Cancelar',
        }),
      );
      expect(screen.queryByRole('dialog')).toBeNull();
      expect(callback).not.toHaveBeenCalled();

      openConfirmation();
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: action,
        }),
      );
      expect(callback).toHaveBeenCalledExactlyOnceWith('estip-1');
      expect(active ? onReativar : onInativar).not.toHaveBeenCalled();
      expect(screen.queryByRole('dialog')).toBeNull();
    },
  );

  it.each(['tabela', 'mobile'])('oculta as ações sem permissão: %s', (view) => {
    renderList(view, true, false);
    expect(screen.queryByRole('button', { name: 'Mais ações' })).toBeNull();
    expect(screen.queryByRole('dialog')).toBeNull();
  });
});
