import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import type { PagedResult } from '../../../shared/types/pagination';
import { formatarMoedaDecimal } from '../../../shared/utils/formatters';
import { listarParcelas } from '../api/parcelas.api';
import type { ParcelaHistorico } from '../types/parcelaHistorico.types';
import { HistoricoParcelasProposta } from './HistoricoParcelasProposta';
vi.mock('../api/parcelas.api');
const parcela: ParcelaHistorico = {
  posicao: 1,
  publicId: 'parcela-a',
  propostaPublicId: 'origem-a',
  propostaNumero: 'aBc-123',
  versao: 1,
  parcela: 1,
  competenciaInt: 202611,
  ano: 2026,
  mes: 11,
  dataVencimento: '2026-11-20',
  dataLancamento: '2026-11-01',
  dataPagamento: null,
  classificacao: 'MENSALIDADE',
  legado: false,
  premioLiquido: '5.00',
  iof: '0.13',
  premioTotal: '5.13',
  valorPago: null,
  observacao: null,
};
const pagina: PagedResult<ParcelaHistorico> = {
  items: [parcela],
  page: 1,
  pageSize: 20,
  totalCount: 1,
};
beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(listarParcelas).mockReset().mockResolvedValue(pagina);
  localStorage.clear();
});
function abrir() {
  fireEvent.click(
    screen.getByRole('button', { name: 'Consultar histórico de parcelas' }),
  );
}
it.each(['claro', 'escuro', 'sistema'])(
  'consulta versões com valores registrados no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    render(
      <ThemeProvider>
        <HistoricoParcelasProposta publicId="atual-a" disabled={false} />
      </ThemeProvider>,
    );
    expect(listarParcelas).not.toHaveBeenCalled();
    abrir();
    expect(await screen.findByText('R$ 5,13')).toBeTruthy();
    expect(screen.getByText('aBc-123 / 1')).toBeTruthy();
    expect(listarParcelas).toHaveBeenCalledWith(
      'atual-a',
      1,
      expect.any(AbortSignal),
    );
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      tema === 'escuro' ? 'dark' : 'light',
    );
  },
);
it('navega por todas as páginas do histórico sem repetir a primeira', async () => {
  vi.mocked(listarParcelas)
    .mockResolvedValueOnce({ ...pagina, totalCount: 21 })
    .mockResolvedValueOnce({
      ...pagina,
      page: 2,
      totalCount: 21,
      items: [
        {
          ...parcela,
          publicId: 'parcela-b',
          parcela: 21,
          versao: 2,
          premioTotal: '9.99',
        },
      ],
    });
  render(<HistoricoParcelasProposta publicId="atual-a" disabled={false} />);
  abrir();
  await screen.findByText('R$ 5,13');
  fireEvent.click(
    screen.getByRole('button', { name: 'Ir para a próxima página' }),
  );
  expect(await screen.findByText('R$ 9,99')).toBeTruthy();
  expect(screen.queryByText('R$ 5,13')).toBeNull();
  expect(listarParcelas).toHaveBeenLastCalledWith(
    'atual-a',
    2,
    expect.any(AbortSignal),
  );
});
it('cancela consulta pendente ao fechar e descarta resposta tardia', async () => {
  let concluir: (valor: PagedResult<ParcelaHistorico>) => void = () => {};
  vi.mocked(listarParcelas).mockImplementation(
    () =>
      new Promise((resolve) => {
        concluir = resolve;
      }),
  );
  render(<HistoricoParcelasProposta publicId="atual-a" disabled={false} />);
  abrir();
  await waitFor(() => expect(listarParcelas).toHaveBeenCalledOnce());
  const signal = vi.mocked(listarParcelas).mock.calls[0][2];
  fireEvent.click(
    screen.getByRole('button', { name: 'Fechar histórico de parcelas' }),
  );
  expect(signal.aborted).toBe(true);
  await act(async () => concluir(pagina));
  expect(screen.queryByText('R$ 5,13')).toBeNull();
});
it('descarta consulta da proposta anterior ao trocar o contexto', async () => {
  const { rerender } = render(
    <HistoricoParcelasProposta publicId="atual-a" disabled={false} />,
  );
  abrir();
  await screen.findByText('R$ 5,13');
  vi.mocked(listarParcelas).mockResolvedValueOnce({ ...pagina, items: [] });
  rerender(<HistoricoParcelasProposta publicId="atual-b" disabled={false} />);
  expect(screen.queryByText('R$ 5,13')).toBeNull();
  expect(await screen.findByText('Nenhum movimento registrado')).toBeTruthy();
  expect(listarParcelas).toHaveBeenLastCalledWith(
    'atual-b',
    1,
    expect.any(AbortSignal),
  );
});
it('apresenta erro, nova tentativa e vazio sem inventar parcelas', async () => {
  vi.mocked(listarParcelas)
    .mockRejectedValueOnce(new Error('Falha de consulta'))
    .mockResolvedValueOnce({ ...pagina, items: [], totalCount: 0 });
  render(<HistoricoParcelasProposta publicId="atual-a" disabled={false} />);
  abrir();
  expect(await screen.findByText('Falha de consulta')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(await screen.findByText('Nenhum movimento registrado')).toBeTruthy();
});
it('preserva campos desconhecidos do legado e centavos grandes', async () => {
  vi.mocked(listarParcelas).mockResolvedValue({
    ...pagina,
    items: [
      {
        ...parcela,
        publicId: null,
        parcela: null,
        competenciaInt: null,
        legado: true,
        premioLiquido: null,
        iof: null,
        premioTotal: '9999999999999999.99',
      },
    ],
  });
  render(<HistoricoParcelasProposta publicId="atual-a" disabled={false} />);
  abrir();
  expect(await screen.findByText('R$ 9.999.999.999.999.999,99')).toBeTruthy();
  expect(screen.getByText('Legado')).toBeTruthy();
  expect(screen.getAllByText('Não informada').length).toBe(2);
  expect(formatarMoedaDecimal('-9999999999999999.99')).toBe(
    '-R$ 9.999.999.999.999.999,99',
  );
});
it('respeita bloqueio das ações incompatíveis', () => {
  render(<HistoricoParcelasProposta publicId="atual-a" disabled />);
  expect(
    screen.getByRole('button', { name: 'Consultar histórico de parcelas' }),
  ).toHaveProperty('disabled', true);
  expect(listarParcelas).not.toHaveBeenCalled();
});
