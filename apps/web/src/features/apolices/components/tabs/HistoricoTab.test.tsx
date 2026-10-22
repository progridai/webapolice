import { fireEvent, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { HistoricoTab } from './HistoricoTab';
import { useApoliceHistorico } from '../../hooks/useApoliceHistorico';
import type { ApoliceHistoricoResult } from '../../types/apolice.types';
import { ThemeProvider } from '../../../../shared/theme/ThemeContext';

vi.mock('../../hooks/useApoliceHistorico', () => ({
  useApoliceHistorico: vi.fn(),
}));

const records: ApoliceHistoricoResult[] = [
  {
    acao: 'ALTEROU_STATUS',
    descricao: 'Registro ORIGINAL Um',
    dataAcao: '2026-10-21T12:00:00Z',
  },
  {
    acao: 'ALTEROU_STATUS',
    descricao: 'Registro ORIGINAL Dois',
    dataAcao: '2026-10-21T12:00:00Z',
  },
];
records.forEach(Object.freeze);
Object.freeze(records);

afterEach(() => {
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
  'Histórico no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it('mantém eventos com data/hora iguais sem chaves duplicadas e preserva a evidência na paginação', () => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      const consoleError = vi
        .spyOn(console, 'error')
        .mockImplementation(() => {});
      const original = JSON.stringify(records);
      vi.mocked(useApoliceHistorico).mockImplementation(
        (_id, page = 1, pageSize = 15) => ({
          data: { items: records, totalCount: 30, page, pageSize },
          isLoading: false,
          error: null,
        }),
      );
      render(
        <ThemeProvider>
          <HistoricoTab publicId="apolice-1" />
        </ThemeProvider>,
      );
      expect(screen.getAllByText('ALTEROU_STATUS')).toHaveLength(2);
      expect(screen.getByText('Registro ORIGINAL Um')).toBeTruthy();
      expect(screen.getByText('Registro ORIGINAL Dois')).toBeTruthy();
      fireEvent.click(
        screen.getByRole('button', { name: 'Ir para a próxima página' }),
      );
      expect(useApoliceHistorico).toHaveBeenLastCalledWith('apolice-1', 2, 15);
      expect(screen.getAllByText('ALTEROU_STATUS')).toHaveLength(2);
      expect(consoleError).not.toHaveBeenCalled();
      expect(JSON.stringify(records)).toBe(original);
      expect(records.every((record) => !('chaveLinha' in record))).toBe(true);
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
  },
);
