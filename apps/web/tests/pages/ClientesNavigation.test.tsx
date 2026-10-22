import { fireEvent, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, useLocation } from 'react-router-dom';
import { ClientesTable } from '../../src/features/clientes/components/ClientesTable';
import { ClientesMobileList } from '../../src/features/clientes/components/ClientesMobileList';
import { ClientesFilters } from '../../src/features/clientes/components/ClientesFilters';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';
import type { ClienteListItem } from '../../src/features/clientes/types/cliente.types';

const cliente: ClienteListItem = {
  id: 'afbe6a15-1841-45c6-8324-70aff26ce507',
  nome: 'João da Silva',
  documentoMascarado: '529.***.***-25',
  status: 'ativo',
  dataCadastroUtc: '2026-01-01T00:00:00Z',
};
const search = '?page=3&nome=Jo%C3%A3o&status=1';
function Location() {
  const location = useLocation();
  return (
    <output data-testid="location">
      {JSON.stringify({ pathname: location.pathname, state: location.state })}
    </output>
  );
}
function renderList(view: string, podeAlterar = true) {
  render(
    <ThemeProvider>
      <MemoryRouter initialEntries={[`/clientes${search}`]}>
        {view === 'tabela' ? (
          <ClientesTable
            clientes={[cliente]}
            isLoading={false}
            onSort={vi.fn()}
            podeAlterar={podeAlterar}
          />
        ) : (
          <ClientesMobileList clientes={[cliente]} podeAlterar={podeAlterar} />
        )}
        <Location />
      </MemoryRouter>
    </ThemeProvider>,
  );
}
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Navegação de Clientes no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it.each(['tabela', 'mobile'])(
      'preserva o UUID nas ações e os filtros ao abrir detalhes: %s',
      (view) => {
        localStorage.setItem('webapolice-tema', theme);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        renderList(view);
        fireEvent.click(screen.getByRole('button', { name: 'Detalhes' }));
        expect(JSON.parse(screen.getByTestId('location').textContent!)).toEqual(
          {
            pathname: `/clientes/${cliente.id}`,
            state: { fromListagem: true, search },
          },
        );
        fireEvent.click(screen.getByRole('button', { name: 'Mais ações' }));
        fireEvent.click(screen.getByRole('menuitem', { name: 'Editar' }));
        expect(
          JSON.parse(screen.getByTestId('location').textContent!).pathname,
        ).toBe(`/clientes/${cliente.id}/editar`);
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      },
    );
  },
);

describe('Permissões e status dos filtros de Clientes', () => {
  it.each(['tabela', 'mobile'])(
    'oferece somente detalhes quando a edição não é permitida: %s',
    (view) => {
      renderList(view, false);
      expect(screen.getByRole('button', { name: 'Detalhes' })).toBeTruthy();
      expect(screen.queryByRole('button', { name: 'Mais ações' })).toBeNull();
    },
  );
  it.each([
    ['1', 1],
    ['2', 2],
    ['', ''],
  ])('envia o status tipado ao selecionar %j', (value, expected) => {
    const onFilterChange = vi.fn();
    render(
      <ClientesFilters
        filters={{ status: '', nome: '', cpf: '' }}
        onFilterChange={onFilterChange}
        onClearFilters={vi.fn()}
      />,
    );
    fireEvent.change(screen.getByLabelText('Status'), { target: { value } });
    expect(onFilterChange).toHaveBeenCalledExactlyOnceWith({
      status: expected,
      page: 1,
    });
  });
});
