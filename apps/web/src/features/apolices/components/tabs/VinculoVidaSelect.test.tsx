import { fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, expect, it, vi } from 'vitest';
import { VinculoVidaSelect } from './VinculoVidaSelect';
import { useVinculosVida } from '../../hooks/useVinculosVida';
import { ThemeProvider } from '../../../../shared/theme/ThemeContext';
vi.mock('../../hooks/useVinculosVida');
const onChange = vi.fn();
const refetch = vi.fn().mockResolvedValue(undefined);
const props = {
  apoliceId: 'apolice',
  clienteId: 'cliente',
  value: '',
  onChange,
  bloqueado: false,
  disabled: false,
  required: true,
  novaMatricula: null,
  onNovaMatriculaChange: vi.fn(),
  podeCadastrar: true,
};
beforeEach(() => {
  vi.mocked(useVinculosVida).mockReturnValue({
    data: {
      items: [
        { publicId: 'v1', matricula: 'ABC-01', estipulante: 'Empresa A' },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
    },
    error: null,
    isLoading: false,
    refetch,
  });
});
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});
it.each(['claro', 'escuro', 'sistema'] as const)(
  'seleciona a matrícula no tema %s',
  (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    render(
      <ThemeProvider>
        <VinculoVidaSelect {...props} />
      </ThemeProvider>,
    );
    fireEvent.change(screen.getByLabelText(/Vínculo cadastral/), {
      target: { value: 'v1' },
    });
    expect(onChange).toHaveBeenCalledWith('v1', false);
  },
);
it('preserva vínculo atual inativo que não aparece entre os disponíveis', () => {
  render(
    <VinculoVidaSelect
      {...props}
      value="antigo"
      atual={{
        publicId: 'antigo',
        matricula: 'HIST-01',
        estipulante: 'Empresa A',
      }}
    />,
  );
  expect(
    (screen.getByLabelText(/Vínculo cadastral/) as HTMLSelectElement).value,
  ).toBe('antigo');
  expect(screen.getByRole('option', { name: /HIST-01/ })).toBeTruthy();
});
it('bloqueia troca quando há propostas sem buscar opções', () => {
  render(
    <VinculoVidaSelect
      {...props}
      bloqueado
      atual={{ publicId: 'v1', matricula: 'ABC-01', estipulante: 'Empresa A' }}
    />,
  );
  expect(screen.queryByRole('combobox')).toBeNull();
  expect(screen.getByText(/Esta participação possui propostas/)).toBeTruthy();
  expect(useVinculosVida).toHaveBeenLastCalledWith('apolice', '', 1);
});
it('oferece recuperação após falha da consulta', () => {
  vi.mocked(useVinculosVida).mockReturnValue({
    data: null,
    error: new Error('Falha da consulta'),
    isLoading: false,
    refetch,
  });
  render(<VinculoVidaSelect {...props} />);
  expect((screen.getByRole('combobox') as HTMLSelectElement).disabled).toBe(
    true,
  );
  fireEvent.click(screen.getByText('Tentar novamente'));
  expect(refetch).toHaveBeenCalled();
});
it('orienta cadastrar vínculo quando não há compatíveis', () => {
  vi.mocked(useVinculosVida).mockReturnValue({
    data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
    error: null,
    isLoading: false,
    refetch,
  });
  render(<VinculoVidaSelect {...props} />);
  expect(screen.getByText(/Nenhum vínculo ativo compatível/)).toBeTruthy();
});

it('identifica matrícula sem estipulante e informa a associação antes de salvar', () => {
  vi.mocked(useVinculosVida).mockReturnValue({
    data: {
      items: [
        {
          publicId: 'pendente',
          matricula: '123',
          estipulante: null,
          requerEstipulante: true,
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
    },
    error: null,
    isLoading: false,
    refetch,
  });
  const { rerender } = render(
    <VinculoVidaSelect {...props} estipulanteNome="Empresa A" />,
  );
  expect(
    screen.getByRole('option', { name: /123.*Sem estipulante/ }),
  ).toBeTruthy();
  fireEvent.change(screen.getByLabelText(/Vínculo cadastral/), {
    target: { value: 'pendente' },
  });
  expect(onChange).toHaveBeenCalledWith('pendente', true);
  rerender(
    <VinculoVidaSelect
      {...props}
      value="pendente"
      estipulanteNome="Empresa A"
    />,
  );
  expect(
    screen.getByText(/A matrícula 123 será vinculada a Empresa A/),
  ).toBeTruthy();
});

it('desabilita completar matrícula sem permissão para alterar clientes', () => {
  vi.mocked(useVinculosVida).mockReturnValue({
    data: {
      items: [
        {
          publicId: 'pendente',
          matricula: '123',
          estipulante: null,
          requerEstipulante: true,
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
    },
    error: null,
    isLoading: false,
    refetch,
  });
  render(<VinculoVidaSelect {...props} podeCadastrar={false} />);
  expect(
    (
      screen.getByRole('option', {
        name: /123.*Sem estipulante/,
      }) as HTMLOptionElement
    ).disabled,
  ).toBe(true);
  expect(
    screen.getByText(/Para cadastrar uma matrícula ou completar o estipulante/),
  ).toBeTruthy();
});
