import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SeguradoraForm } from '../../src/features/seguradoras/components/SeguradoraForm';
import { SubestipulanteForm } from '../../src/features/subestipulantes/components/SubestipulanteForm';
import { EstipulanteForm } from '../../src/features/estipulantes/components/EstipulanteForm';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/features/clientes/api/localidadesApi', () => ({
  buscarCidadesPorUf: vi.fn().mockResolvedValue([]),
}));

afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe('Acessibilidade dos formulários controlados', () => {
  it.each([
    ['claro', false, 'light'],
    ['escuro', false, 'dark'],
    ['sistema', false, 'light'],
    ['sistema', true, 'dark'],
  ])(
    'associa rótulos aos campos no tema %s (sistema escuro: %s)',
    (tema, dark, resolved) => {
      localStorage.setItem('webapolice-tema', String(tema));
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
      render(
        <ThemeProvider>
          <SeguradoraForm onSubmit={vi.fn()} onCancel={vi.fn()} />
          <SubestipulanteForm onSubmit={vi.fn()} onCancel={vi.fn()} />
          <EstipulanteForm onSubmit={vi.fn()} onCancel={vi.fn()} />
        </ThemeProvider>,
      );

      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
      const cnpjs = screen.getAllByLabelText<HTMLInputElement>(/^CNPJ/);
      expect(cnpjs).toHaveLength(3);
      for (const input of cnpjs) expect(input.tagName).toBe('INPUT');
      expect(screen.getByLabelText('CEP').tagName).toBe('INPUT');
      expect(screen.getByLabelText('Cidade').tagName).toBe('SELECT');
      expect(
        screen.getByLabelText(/^Início de Vigência/).getAttribute('type'),
      ).toBe('date');
      expect(
        screen.getByLabelText('Fim de Vigência').getAttribute('type'),
      ).toBe('date');

      fireEvent.click(
        screen.getByRole('button', { name: 'Adicionar Contato Institucional' }),
      );
      expect(screen.getByLabelText('Telefone').tagName).toBe('INPUT');
    },
  );

  it.each([
    ['seguradora', SeguradoraForm],
    ['subestipulante', SubestipulanteForm],
  ] as const)(
    'associa a mensagem de CNPJ inválido ao campo de %s',
    async (_, Form) => {
      const onSubmit = vi.fn();
      const { container } = render(
        <Form onSubmit={onSubmit} onCancel={vi.fn()} />,
      );
      fireEvent.change(screen.getByLabelText('Nome / Razão Social'), {
        target: { value: 'Empresa Teste' },
      });
      const input = screen.getByLabelText('CNPJ');
      fireEvent.input(input, { target: { value: '11.111.111/1111-11' } });
      fireEvent.submit(container.querySelector('form')!);

      await waitFor(() =>
        expect(input.getAttribute('aria-invalid')).toBe('true'),
      );
      const errorId = input.getAttribute('aria-describedby');
      expect(errorId).toBeTruthy();
      expect(document.getElementById(errorId!)?.textContent).toContain(
        'CNPJ inválido.',
      );
      expect(onSubmit).not.toHaveBeenCalled();
    },
  );
});
