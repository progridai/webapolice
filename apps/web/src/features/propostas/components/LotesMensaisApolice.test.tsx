import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import {
  conferirLote,
  gerarLote,
  fecharLote,
  baixarParcela,
} from '../api/financeiro.api';
import type { LoteMensal } from '../types/financeiro.types';
import { LotesMensaisApolice } from './LotesMensaisApolice';
vi.mock('../api/financeiro.api');
const lote: LoteMensal = {
  apolicePublicId: 'apolice-A',
  competencia: 202612,
  conferencia: 'TOKEN',
  geradaEm: null,
  fechadaEm: null,
  podeGerar: true,
  podeFechar: false,
  impedimentos: [],
  vidas: [
    {
      clienteNome: 'Maria da Silva',
      matricula: 'aBc-001',
      vidaPublicId: 'vida-A',
      propostaPublicId: 'prop-A',
      numero: 'P1',
      versao: 2,
      situacao: 'INCLUIR',
      motivo: null,
      novaMensalidade: '10.25',
      saldoAnterior: '10.25',
      totalAcumulado: '20.50',
      parcelas: [
        {
          publicId: 'parcela-A',
          parcela: 1,
          competencia: 202611,
          vencimento: '2026-11-30',
          total: '10.25',
          aberto: '10.25',
          revisaoBaixa: 0,
          dataPagamento: null,
        },
      ],
    },
  ],
};
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(conferirLote).mockResolvedValue(lote);
  vi.mocked(gerarLote).mockResolvedValue(lote);
  vi.mocked(fecharLote).mockResolvedValue(lote);
  vi.mocked(baixarParcela).mockResolvedValue(true);
});
function consultar() {
  fireEvent.change(
    screen.getByLabelText('Competência do lote', { exact: false }),
    { target: { value: '2026-12' } },
  );
  fireEvent.click(screen.getByRole('button', { name: 'Conferir competência' }));
}
it.each(['claro', 'escuro', 'sistema'])(
  'mostra nova mensalidade e dívida anterior e gera com conferência no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    render(
      <ThemeProvider>
        <LotesMensaisApolice
          publicId="apolice-A"
          podeAlterar
          onBusy={vi.fn()}
        />
      </ThemeProvider>,
    );
    expect(conferirLote).not.toHaveBeenCalled();
    consultar();
    expect(await screen.findByText('R$ 20,50')).toBeTruthy();
    expect(screen.getByText('Inclusão')).toBeTruthy();
    fireEvent.click(
      screen.getByRole('button', { name: 'Confirmar geração do lote' }),
    );
    await waitFor(() =>
      expect(gerarLote).toHaveBeenCalledWith('apolice-A', 202612, 'TOKEN'),
    );
    expect(fecharLote).not.toHaveBeenCalled();
  },
);
it('registra baixa integral e conserva a distinção entre baixa e fechamento', async () => {
  vi.mocked(conferirLote).mockResolvedValue({
    ...lote,
    geradaEm: '2026-12-01',
    podeGerar: false,
    podeFechar: true,
  });
  render(
    <LotesMensaisApolice publicId="apolice-A" podeAlterar onBusy={vi.fn()} />,
  );
  consultar();
  await screen.findByText('R$ 20,50');
  fireEvent.click(screen.getByRole('button', { name: 'Exibir detalhes' }));
  fireEvent.change(
    screen.getByLabelText('Data do pagamento da parcela 1', { exact: false }),
    { target: { value: '2026-12-10' } },
  );
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Confirmar baixa integral da parcela 1',
    }),
  );
  await waitFor(() =>
    expect(baixarParcela).toHaveBeenCalledWith(
      'apolice-A',
      'parcela-A',
      0,
      '2026-12-10',
    ),
  );
  await waitFor(() =>
    expect(
      screen.getByRole('button', {
        name: 'Confirmar fechamento, preservando abertas',
      }),
    ).toHaveProperty('disabled', false),
  );
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Confirmar fechamento, preservando abertas',
    }),
  );
  await waitFor(() => expect(fecharLote).toHaveBeenCalledOnce());
});
it('bloqueia duplo envio e edição enquanto grava', async () => {
  let concluir: ((r: LoteMensal) => void) | undefined;
  vi.mocked(gerarLote).mockImplementation(
    () =>
      new Promise((resolve) => {
        concluir = resolve;
      }),
  );
  render(
    <LotesMensaisApolice publicId="apolice-A" podeAlterar onBusy={vi.fn()} />,
  );
  consultar();
  await screen.findByText('R$ 20,50');
  const b = screen.getByRole('button', { name: 'Confirmar geração do lote' });
  fireEvent.click(b);
  fireEvent.click(b);
  expect(gerarLote).toHaveBeenCalledOnce();
  expect(
    screen.getByLabelText('Competência do lote', { exact: false }),
  ).toHaveProperty('disabled', true);
  await act(async () => concluir?.(lote));
});
it('cancela consulta antiga ao trocar competência e não exibe resultado atrasado', async () => {
  let concluir: ((r: LoteMensal) => void) | undefined;
  vi.mocked(conferirLote).mockImplementation(
    () =>
      new Promise((resolve) => {
        concluir = resolve;
      }),
  );
  render(
    <LotesMensaisApolice publicId="apolice-A" podeAlterar onBusy={vi.fn()} />,
  );
  consultar();
  await waitFor(() => expect(conferirLote).toHaveBeenCalledOnce());
  const signal = vi.mocked(conferirLote).mock.calls[0][2];
  fireEvent.change(
    screen.getByLabelText('Competência do lote', { exact: false }),
    { target: { value: '2027-01' } },
  );
  expect(signal.aborted).toBe(true);
  await act(async () => concluir?.(lote));
  expect(screen.queryByText('R$ 20,50')).toBeNull();
});
it('apresenta erro, nova tentativa, vazio e oculta escritas para consulta', async () => {
  vi.mocked(conferirLote)
    .mockRejectedValueOnce(new Error('Falha de rede'))
    .mockResolvedValueOnce({ ...lote, vidas: [] });
  render(
    <LotesMensaisApolice
      publicId="apolice-A"
      podeAlterar={false}
      onBusy={vi.fn()}
    />,
  );
  consultar();
  expect(await screen.findByText('Falha de rede')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(await screen.findByText('Nenhuma Vida nesta Apólice')).toBeTruthy();
  expect(
    screen.queryByRole('button', { name: 'Confirmar geração do lote' }),
  ).toBeNull();
});
