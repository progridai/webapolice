import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { cancelarAgendamento, reagendarProposta } from '../api/propostas.api';
import { GestaoAgendamentoProposta } from './GestaoAgendamentoProposta';
import { ConfirmacaoProposta } from './ConfirmacaoProposta';
import { detalhe } from './propostas.fixtures';
import type { GestaoAgendamentoResultado } from '../types/proposta.types';

vi.mock('../api/propostas.api');
const agendada = {
  ...detalhe,
  podeEditarRascunho: false,
  podeReagendar: true,
  podeCancelarAgendamento: true,
  revisaoPreenchimento: 2,
  dataInicioVigencia: '2026-12-01',
  confirmadaEm: '2026-11-12T12:00:00Z',
  situacaoVigenciaCodigo: 'AGENDADA' as const,
};
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(cancelarAgendamento)
    .mockReset()
    .mockResolvedValue({ revisaoPreenchimento: 3 });
  vi.mocked(reagendarProposta)
    .mockReset()
    .mockResolvedValue({ revisaoPreenchimento: 3 });
});

it.each(['claro', 'escuro', 'sistema'])(
  'reagenda com justificativa no tema %s e libera o bloqueio',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const concluida = vi.fn();
    const busy = vi.fn();
    render(
      <ThemeProvider>
        <GestaoAgendamentoProposta
          proposta={agendada}
          podeAlterar
          disabled={false}
          onBusy={busy}
          onConcluida={concluida}
        />
      </ThemeProvider>,
    );
    fireEvent.click(screen.getByRole('button', { name: 'Reagendar início' }));
    expect(busy).toHaveBeenLastCalledWith(true);
    expect(screen.getByText(/Informe outra data futura/)).toBeTruthy();
    fireEvent.change(
      screen.getByLabelText('Novo início da vigência', { exact: false }),
      { target: { value: '2026-12-10' } },
    );
    fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
      target: { value: '  Ajuste de Calendário  ' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar nova data' }));
    await waitFor(() =>
      expect(concluida).toHaveBeenCalledWith(
        expect.stringContaining('Início reagendado'),
      ),
    );
    expect(reagendarProposta).toHaveBeenCalledWith(
      agendada.publicId,
      2,
      '2026-12-10',
      'Ajuste de Calendário',
    );
    expect(cancelarAgendamento).not.toHaveBeenCalled();
    await waitFor(() => expect(busy).toHaveBeenLastCalledWith(false));
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      tema === 'escuro' ? 'dark' : 'light',
    );
  },
);

it.each(['claro', 'escuro', 'sistema'])(
  'cancela definitivamente somente após justificar no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const concluida = vi.fn();
    render(
      <ThemeProvider>
        <GestaoAgendamentoProposta
          proposta={agendada}
          podeAlterar
          disabled={false}
          onBusy={vi.fn()}
          onConcluida={concluida}
        />
      </ThemeProvider>,
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Cancelar agendamento' }),
    );
    expect(cancelarAgendamento).not.toHaveBeenCalled();
    expect(
      screen.getByText('Encerrar esta versão como Cancelada'),
    ).toBeTruthy();
    expect(
      screen.queryByLabelText('Novo início da vigência', { exact: false }),
    ).toBeNull();
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Confirmar cancelamento do agendamento',
      }),
    );
    expect(await screen.findByText('Informe a justificativa.')).toBeTruthy();
    expect(cancelarAgendamento).not.toHaveBeenCalled();
    fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
      target: { value: 'Desistência da Adesão' },
    });
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Confirmar cancelamento do agendamento',
      }),
    );
    await waitFor(() =>
      expect(concluida).toHaveBeenCalledWith(
        expect.stringContaining('Agendamento cancelado'),
      ),
    );
    expect(cancelarAgendamento).toHaveBeenCalledWith(
      agendada.publicId,
      2,
      'Desistência da Adesão',
    );
    expect(reagendarProposta).not.toHaveBeenCalled();
  },
);

it('exige data válida e justificativa e volta sem gravar', async () => {
  const busy = vi.fn();
  render(
    <GestaoAgendamentoProposta
      proposta={agendada}
      podeAlterar
      disabled={false}
      onBusy={busy}
      onConcluida={vi.fn()}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Reagendar início' }));
  fireEvent.click(screen.getByRole('button', { name: 'Salvar nova data' }));
  expect(
    await screen.findByText('Informe uma data de início válida.'),
  ).toBeTruthy();
  expect(screen.getByText('Informe a justificativa.')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem alterar' }));
  expect(busy).toHaveBeenLastCalledWith(false);
  expect(reagendarProposta).not.toHaveBeenCalled();
  expect(cancelarAgendamento).not.toHaveBeenCalled();
});

it('preserva dados após conflito e permite recarregar descartando as alterações', async () => {
  vi.mocked(reagendarProposta).mockRejectedValue(
    new Error('A proposta foi ativada em outra operação. Recarregue.'),
  );
  const concluida = vi.fn();
  render(
    <GestaoAgendamentoProposta
      proposta={agendada}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={concluida}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Reagendar início' }));
  const data = screen.getByLabelText('Novo início da vigência', {
    exact: false,
  });
  const texto = screen.getByLabelText('Justificativa', { exact: false });
  fireEvent.change(data, { target: { value: '2026-12-10' } });
  fireEvent.change(texto, { target: { value: 'Ajuste de Data' } });
  fireEvent.click(screen.getByRole('button', { name: 'Salvar nova data' }));
  expect(await screen.findByText(/A proposta foi ativada/)).toBeTruthy();
  expect(data).toHaveProperty('value', '2026-12-10');
  expect(texto).toHaveProperty('value', 'Ajuste de Data');
  expect(concluida).not.toHaveBeenCalled();
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Recarregar proposta e descartar alterações',
    }),
  );
  expect(concluida).toHaveBeenCalledWith(null);
});

it.each(['reagendar', 'cancelar'])(
  'bloqueia duplicidade durante validação e envio de %s',
  async (acao) => {
    let resolver: (resultado: GestaoAgendamentoResultado) => void = () => {};
    const pendente = new Promise<GestaoAgendamentoResultado>((resolve) => {
      resolver = resolve;
    });
    vi.mocked(reagendarProposta).mockReturnValue(pendente);
    vi.mocked(cancelarAgendamento).mockReturnValue(pendente);
    render(
      <GestaoAgendamentoProposta
        proposta={agendada}
        podeAlterar
        disabled={false}
        onBusy={vi.fn()}
        onConcluida={vi.fn()}
      />,
    );
    fireEvent.click(
      screen.getByRole('button', {
        name:
          acao === 'reagendar' ? 'Reagendar início' : 'Cancelar agendamento',
      }),
    );
    if (acao === 'reagendar')
      fireEvent.change(
        screen.getByLabelText('Novo início da vigência', { exact: false }),
        { target: { value: '2026-12-10' } },
      );
    fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
      target: { value: 'Ajuste' },
    });
    const formulario = screen.getByRole('form');
    fireEvent.submit(formulario);
    fireEvent.submit(formulario);
    const chamada =
      acao === 'reagendar' ? reagendarProposta : cancelarAgendamento;
    await waitFor(() => expect(chamada).toHaveBeenCalledOnce());
    expect(
      screen.getByLabelText('Justificativa', { exact: false }),
    ).toHaveProperty('disabled', true);
    expect(
      screen.getByRole('button', { name: 'Voltar sem alterar' }),
    ).toHaveProperty('disabled', true);
    fireEvent.submit(formulario);
    expect(chamada).toHaveBeenCalledOnce();
    await act(async () => resolver({ revisaoPreenchimento: 3 }));
  },
);

it('preserva justificativa no erro de cancelamento e permite tentar novamente', async () => {
  vi.mocked(cancelarAgendamento).mockRejectedValueOnce(
    new Error('Falha de rede'),
  );
  const concluida = vi.fn();
  render(
    <GestaoAgendamentoProposta
      proposta={agendada}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={concluida}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Cancelar agendamento' }));
  fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
    target: { value: 'Desistência' },
  });
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Confirmar cancelamento do agendamento',
    }),
  );
  expect(await screen.findByText('Falha de rede')).toBeTruthy();
  expect(concluida).not.toHaveBeenCalled();
  expect(
    screen.getByLabelText('Justificativa', { exact: false }),
  ).toHaveProperty('value', 'Desistência');
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Confirmar cancelamento do agendamento',
    }),
  );
  await waitFor(() => expect(concluida).toHaveBeenCalledOnce());
  expect(cancelarAgendamento).toHaveBeenCalledTimes(2);
});

it('respeita permissão, elegibilidade do servidor e bloqueio de outra operação', () => {
  const props = {
    proposta: agendada,
    podeAlterar: false,
    disabled: false,
    onBusy: vi.fn(),
    onConcluida: vi.fn(),
  };
  const { rerender } = render(<GestaoAgendamentoProposta {...props} />);
  expect(screen.queryByRole('button')).toBeNull();
  rerender(
    <GestaoAgendamentoProposta
      {...props}
      podeAlterar
      proposta={{
        ...agendada,
        podeReagendar: false,
        podeCancelarAgendamento: false,
      }}
    />,
  );
  expect(screen.queryByRole('button')).toBeNull();
  rerender(<GestaoAgendamentoProposta {...props} podeAlterar disabled />);
  expect(
    screen.getByRole('button', { name: 'Reagendar início' }),
  ).toHaveProperty('disabled', true);
  expect(
    screen.getByRole('button', { name: 'Cancelar agendamento' }),
  ).toHaveProperty('disabled', true);
});

it('bloqueia efetivação enquanto o cancelamento está em aberto e exibe cancelada sem ativação pendente', async () => {
  const props = {
    proposta: {
      ...agendada,
      situacaoVigenciaCodigo: 'ATIVACAO_PENDENTE' as const,
      podeEfetivarVigencia: true,
    },
    podeAlterar: true,
    disabled: false,
    onBusy: vi.fn(),
    onConcluida: vi.fn(),
  };
  const { rerender } = render(<ConfirmacaoProposta {...props} />);
  fireEvent.click(screen.getByRole('button', { name: 'Cancelar agendamento' }));
  await waitFor(() =>
    expect(
      screen.getByRole('button', { name: 'Efetivar vigência agora' }),
    ).toHaveProperty('disabled', true),
  );
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem alterar' }));
  rerender(
    <ConfirmacaoProposta
      {...props}
      proposta={{
        ...agendada,
        situacaoVigenciaCodigo: 'CANCELADA',
        canceladaEm: '2026-11-12T12:00:00Z',
        podeReagendar: false,
        podeCancelarAgendamento: false,
      }}
    />,
  );
  expect(screen.getByText('Agendamento cancelado')).toBeTruthy();
  expect(screen.queryByText('Ativação da vigência pendente')).toBeNull();
  expect(screen.queryByRole('button')).toBeNull();
});
