import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { obterFinanceiro, conferirFinanceiro } from '../api/financeiro.api';
import { obterRamosSimulacao } from '../api/simulacaoMensal.api';
import type { ConferenciaFinanceiraResultado } from '../types/financeiro.types';
import { FinanceiroPropostaForm } from './FinanceiroPropostaForm';
import { detalhe } from './propostas.fixtures';
vi.mock('../api/financeiro.api');
vi.mock('../api/simulacaoMensal.api');
const ramo = {
  publicId: 'ramo-A',
  codigo: '01',
  nome: 'Vida',
  iofPercentual: 2.5,
};
const resultado: ConferenciaFinanceiraResultado = {
  conferencia: 'ABCDEF',
  competenciaInicial: 202611,
  vencimentoInicial: '2026-11-30',
  competenciaReferencia: 202611,
  vencimentoReferencia: '2026-11-30',
  valores: {
    revisaoPreenchimento: 0,
    incluiConjuge: false,
    ramo,
    calculadaEm: '2026-11-18',
    coberturas: [],
    liquidoTitular: '5.00',
    liquidoConjuge: '0.00',
    liquidoTotal: '5.00',
    iof: '0.13',
    totalMensal: '5.13',
  },
};
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(obterRamosSimulacao).mockResolvedValue([ramo]);
  vi.mocked(obterFinanceiro).mockResolvedValue({
    ramoPublicId: null,
    competenciaInicial: null,
    vencimentoInicial: null,
    competenciaReferencia: null,
    vencimentoReferencia: null,
    premioLiquido: null,
    iofPercentual: null,
    iof: null,
    totalMensal: null,
    conferenciaAtual: false,
  });
  vi.mocked(conferirFinanceiro).mockResolvedValue(resultado);
});
async function preencher() {
  await screen.findByLabelText('Ramo para o IOF', { exact: false });
  fireEvent.change(
    screen.getByLabelText('Data de início da vigência', { exact: false }),
    { target: { value: '2026-11-01' } },
  );
  fireEvent.change(
    screen.getByLabelText('Competência inicial', { exact: false }),
    { target: { value: '2026-11' } },
  );
  fireEvent.change(
    screen.getByLabelText('Primeiro vencimento', { exact: false }),
    { target: { value: '2026-11-30' } },
  );
  fireEvent.change(screen.getByLabelText('Ramo para o IOF', { exact: false }), {
    target: { value: ramo.publicId },
  });
  fireEvent.click(
    screen.getByRole('button', { name: 'Conferir primeira parcela' }),
  );
}
it.each(['claro', 'escuro', 'sistema'])(
  'confere antes de salvar no tema %s e invalida ao editar',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const salvar = vi.fn().mockResolvedValue(undefined);
    render(
      <ThemeProvider>
        <FinanceiroPropostaForm
          proposta={detalhe}
          onCancelar={vi.fn()}
          onSalvar={salvar}
        />
      </ThemeProvider>,
    );
    await preencher();
    expect(await screen.findByText('R$ 5,13')).toBeTruthy();
    fireEvent.click(
      screen.getByRole('button', { name: 'Confirmar proposta e valores' }),
    );
    await waitFor(() =>
      expect(salvar).toHaveBeenCalledWith(
        {
          competenciaInicial: 202611,
          vencimentoInicial: '2026-11-30',
          ramoPublicId: 'ramo-A',
          conferencia: 'ABCDEF',
        },
        '2026-11-01',
      ),
    );
    fireEvent.change(
      screen.getByLabelText('Primeiro vencimento', { exact: false }),
      { target: { value: '2026-12-01' } },
    );
    expect(screen.queryByText('R$ 5,13')).toBeNull();
    expect(
      screen.getByRole('button', { name: 'Confirmar proposta e valores' }),
    ).toHaveProperty('disabled', true);
  },
);
it('cancela consulta pendente ao desmontar e ignora resposta antiga', async () => {
  let concluir: ((r: ConferenciaFinanceiraResultado) => void) | undefined;
  vi.mocked(conferirFinanceiro).mockImplementation(
    () =>
      new Promise((resolve) => {
        concluir = resolve;
      }),
  );
  const view = render(
    <FinanceiroPropostaForm
      proposta={detalhe}
      onCancelar={vi.fn()}
      onSalvar={vi.fn()}
    />,
  );
  await preencher();
  await waitFor(() => expect(conferirFinanceiro).toHaveBeenCalledOnce());
  const signal = vi.mocked(conferirFinanceiro).mock.calls[0][2];
  view.unmount();
  expect(signal.aborted).toBe(true);
  await act(async () => concluir?.(resultado));
});
it('impede duplo envio e mostra falha exigindo conferir novamente', async () => {
  let rejeitar: ((e: Error) => void) | undefined;
  const salvar = vi.fn<() => Promise<void>>().mockImplementation(
    () =>
      new Promise((_, reject) => {
        rejeitar = reject;
      }),
  );
  render(
    <FinanceiroPropostaForm
      proposta={detalhe}
      onCancelar={vi.fn()}
      onSalvar={salvar}
    />,
  );
  await preencher();
  await screen.findByText('R$ 5,13');
  const botao = screen.getByRole('button', {
    name: 'Confirmar proposta e valores',
  });
  fireEvent.click(botao);
  fireEvent.click(botao);
  expect(salvar).toHaveBeenCalledOnce();
  expect(
    screen.getByRole('button', { name: 'Cancelar conferência financeira' }),
  ).toHaveProperty('disabled', true);
  await act(async () => rejeitar?.(new Error('Conferência desatualizada')));
  expect(await screen.findByText('Conferência desatualizada')).toBeTruthy();
  expect(botao).toHaveProperty('disabled', true);
});
it('permite tentar novamente e mostra ausência de Ramos', async () => {
  vi.mocked(obterRamosSimulacao)
    .mockRejectedValueOnce(new Error('Sem conexão'))
    .mockResolvedValueOnce([]);
  render(
    <FinanceiroPropostaForm
      proposta={detalhe}
      onCancelar={vi.fn()}
      onSalvar={vi.fn()}
    />,
  );
  expect(await screen.findByText('Sem conexão')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(await screen.findByText('Nenhum Ramo disponível')).toBeTruthy();
});
