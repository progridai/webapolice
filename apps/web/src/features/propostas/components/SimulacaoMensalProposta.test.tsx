import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { formatarMoedaDecimal } from '../../../shared/utils/formatters';
import {
  obterRamosSimulacao,
  simularValorMensal,
} from '../api/simulacaoMensal.api';
import type { SimulacaoMensalResultado } from '../types/simulacaoMensal.types';
import { SimulacaoMensalProposta } from './SimulacaoMensalProposta';
import { detalhe } from './propostas.fixtures';
vi.mock('../api/simulacaoMensal.api');
const ramo = {
  publicId: 'ramo-a',
  nome: 'Vida em Grupo',
  codigo: '01',
  iofPercentual: 2.5,
};
const resultado: SimulacaoMensalResultado = {
  revisaoPreenchimento: 0,
  incluiConjuge: false,
  ramo,
  calculadaEm: '2026-11-18T12:00:00Z',
  coberturas: [
    {
      publicId: 'c',
      nome: 'Morte',
      premioAjustado: true,
      premioTitular: '5.00',
      premioConjuge: '0.00',
      liquido: '5.00',
    },
  ],
  liquidoTitular: '5.00',
  liquidoConjuge: '0.00',
  liquidoTotal: '5.00',
  iof: '0.13',
  totalMensal: '5.13',
};
beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(obterRamosSimulacao).mockReset();
  vi.mocked(simularValorMensal).mockReset();
  localStorage.clear();
  vi.mocked(obterRamosSimulacao).mockResolvedValue([ramo]);
  vi.mocked(simularValorMensal).mockResolvedValue(resultado);
});
function abrir() {
  fireEvent.click(screen.getByRole('button', { name: 'Simular valor mensal' }));
}
async function calcular() {
  const select = await screen.findByLabelText('Ramo para o IOF', {
    exact: false,
  });
  expect(select).toHaveProperty('value', '');
  fireEvent.change(select, { target: { value: ramo.publicId } });
  fireEvent.click(screen.getByRole('button', { name: 'Calcular simulação' }));
}
it.each(['claro', 'escuro', 'sistema'])(
  'exige escolha e mostra valor calculado no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    render(
      <ThemeProvider>
        <SimulacaoMensalProposta proposta={detalhe} disabled={false} />
      </ThemeProvider>,
    );
    abrir();
    await calcular();
    expect(await screen.findByText('R$ 5,13')).toBeTruthy();
    expect(screen.getByText('R$ 0,13')).toBeTruthy();
    expect(screen.getByText(/IOF utilizado: 2.5%/)).toBeTruthy();
    expect(screen.getByText('Morte (prêmio ajustado)')).toBeTruthy();
    expect(simularValorMensal).toHaveBeenCalledWith(
      detalhe.publicId,
      0,
      ramo.publicId,
      expect.any(AbortSignal),
    );
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      tema === 'escuro' ? 'dark' : 'light',
    );
  },
);
it('cancela consulta pendente ao fechar e descarta resposta tardia', async () => {
  let concluir: (value: SimulacaoMensalResultado) => void = () => {};
  vi.mocked(simularValorMensal).mockImplementation(
    () =>
      new Promise((resolve) => {
        concluir = resolve;
      }),
  );
  render(<SimulacaoMensalProposta proposta={detalhe} disabled={false} />);
  abrir();
  await calcular();
  await waitFor(() => expect(simularValorMensal).toHaveBeenCalledOnce());
  const signal = vi.mocked(simularValorMensal).mock.calls[0][3];
  fireEvent.click(screen.getByRole('button', { name: 'Fechar simulação' }));
  expect(signal.aborted).toBe(true);
  await act(async () => concluir(resultado));
  expect(screen.queryByText('R$ 5,13')).toBeNull();
});
it('descarta valor anterior quando muda o Ramo e ao revisar proposta', async () => {
  const { rerender } = render(
    <SimulacaoMensalProposta proposta={detalhe} disabled={false} />,
  );
  abrir();
  await calcular();
  await screen.findByText('R$ 5,13');
  fireEvent.change(screen.getByLabelText('Ramo para o IOF', { exact: false }), {
    target: { value: '' },
  });
  expect(screen.queryByText('R$ 5,13')).toBeNull();
  fireEvent.change(screen.getByLabelText('Ramo para o IOF', { exact: false }), {
    target: { value: ramo.publicId },
  });
  fireEvent.click(screen.getByRole('button', { name: 'Calcular simulação' }));
  await screen.findByText('R$ 5,13');
  rerender(
    <SimulacaoMensalProposta
      proposta={{ ...detalhe, revisaoPreenchimento: 1 }}
      disabled={false}
    />,
  );
  expect(screen.queryByText('R$ 5,13')).toBeNull();
});
it('apresenta erro de cálculo e permite tentar novamente', async () => {
  vi.mocked(simularValorMensal).mockRejectedValueOnce(
    new Error('IOF não informado'),
  );
  render(<SimulacaoMensalProposta proposta={detalhe} disabled={false} />);
  abrir();
  await calcular();
  expect(await screen.findByText('IOF não informado')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(await screen.findByText('R$ 5,13')).toBeTruthy();
});
it('mostra estado vazio e permite recuperar erro das opções', async () => {
  vi.mocked(obterRamosSimulacao)
    .mockRejectedValueOnce(new Error('Falha das opções'))
    .mockResolvedValueOnce([]);
  render(<SimulacaoMensalProposta proposta={detalhe} disabled={false} />);
  abrir();
  expect(await screen.findByText('Falha das opções')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(await screen.findByText('Nenhum Ramo disponível')).toBeTruthy();
});
it('requer contratação explícita e respeita bloqueio da edição', () => {
  const { rerender } = render(
    <SimulacaoMensalProposta
      proposta={{ ...detalhe, incluiConjuge: null }}
      disabled={false}
    />,
  );
  expect(screen.getByText('Informe a contratação de cônjuge')).toBeTruthy();
  expect(
    screen.queryByRole('button', { name: 'Simular valor mensal' }),
  ).toBeNull();
  rerender(<SimulacaoMensalProposta proposta={detalhe} disabled />);
  expect(
    screen.getByRole('button', { name: 'Simular valor mensal' }),
  ).toHaveProperty('disabled', true);
});
it('formata decimais textuais sem perder centavos grandes', () => {
  expect(formatarMoedaDecimal('9999999999999999.99')).toBe(
    'R$ 9.999.999.999.999.999,99',
  );
  expect(formatarMoedaDecimal('0.00')).toBe('R$ 0,00');
  expect(formatarMoedaDecimal('5e2')).toBe('Valor inválido');
});
