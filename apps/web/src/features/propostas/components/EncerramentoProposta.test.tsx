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
  descartarRascunho,
  efetivarCancelamentoVigencia,
  solicitarCancelamentoVigencia,
} from '../api/propostas.api';
import { EncerramentoProposta } from './EncerramentoProposta';
import { ConfirmacaoProposta } from './ConfirmacaoProposta';
import { detalhe } from './propostas.fixtures';
import type { CancelamentoVigenciaResultado } from '../types/proposta.types';

vi.mock('../api/propostas.api');
const vigente = {
  ...detalhe,
  podeEditarRascunho: false,
  podeCancelarVigencia: true,
  vigente: true,
  revisaoPreenchimento: 2,
  situacaoVigenciaCodigo: 'VIGENTE' as const,
};
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(descartarRascunho)
    .mockReset()
    .mockResolvedValue({ revisaoPreenchimento: 3 });
  vi.mocked(solicitarCancelamentoVigencia).mockReset().mockResolvedValue({
    cancelada: false,
    agendada: true,
    revisaoPreenchimento: 3,
    pendenciaVigencia: null,
  });
  vi.mocked(efetivarCancelamentoVigencia).mockReset().mockResolvedValue({
    cancelada: true,
    agendada: false,
    revisaoPreenchimento: 4,
    pendenciaVigencia: null,
  });
});

it.each(['claro', 'escuro', 'sistema'])(
  'solicita cancelamento futuro com data informada e justificativa no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const concluida = vi.fn();
    const busy = vi.fn();
    render(
      <ThemeProvider>
        <EncerramentoProposta
          proposta={vigente}
          podeAlterar
          disabled={false}
          onBusy={busy}
          onConcluida={concluida}
        />
      </ThemeProvider>,
    );
    fireEvent.click(screen.getByRole('button', { name: 'Cancelar vigência' }));
    expect(busy).toHaveBeenLastCalledWith(true);
    expect(
      screen.getByText(/Informe o primeiro dia sem vigência/),
    ).toBeTruthy();
    const data = screen.getByLabelText('Data de cancelamento da vigência', {
      exact: false,
    });
    expect(data).toHaveProperty('value', '');
    fireEvent.change(data, { target: { value: '2026-11-13' } });
    fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
      target: { value: 'Encerramento da Adesão' },
    });
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Confirmar cancelamento da vigência',
      }),
    );
    await waitFor(() =>
      expect(concluida).toHaveBeenCalledWith(
        expect.stringContaining('Cancelamento agendado'),
      ),
    );
    expect(solicitarCancelamentoVigencia).toHaveBeenCalledWith(
      vigente.publicId,
      2,
      '2026-11-13',
      'Encerramento da Adesão',
    );
    await waitFor(() => expect(busy).toHaveBeenLastCalledWith(false));
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      tema === 'escuro' ? 'dark' : 'light',
    );
  },
);

it.each(['claro', 'escuro', 'sistema'])(
  'descarta pré proposta somente após justificar no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const concluida = vi.fn();
    render(
      <ThemeProvider>
        <EncerramentoProposta
          proposta={{ ...detalhe, podeDescartarRascunho: true }}
          podeAlterar
          disabled={false}
          onBusy={vi.fn()}
          onConcluida={concluida}
        />
      </ThemeProvider>,
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Descartar pré proposta' }),
    );
    expect(descartarRascunho).not.toHaveBeenCalled();
    expect(
      screen.queryByLabelText('Data de cancelamento da vigência', {
        exact: false,
      }),
    ).toBeNull();
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Confirmar descarte da pré proposta',
      }),
    );
    expect(await screen.findByText('Informe a justificativa.')).toBeTruthy();
    fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
      target: { value: '  Condições Não Aprovadas  ' },
    });
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Confirmar descarte da pré proposta',
      }),
    );
    await waitFor(() =>
      expect(concluida).toHaveBeenCalledWith(
        expect.stringContaining('Pré proposta descartada'),
      ),
    );
    expect(descartarRascunho).toHaveBeenCalledWith(
      detalhe.publicId,
      0,
      'Condições Não Aprovadas',
    );
  },
);

it.each(['2026-11-10', '2026-11-12'])(
  'aceita data %s para cancelamento imediato conforme resultado do servidor',
  async (data) => {
    vi.mocked(solicitarCancelamentoVigencia).mockResolvedValue({
      cancelada: true,
      agendada: false,
      revisaoPreenchimento: 3,
      pendenciaVigencia: null,
    });
    const concluida = vi.fn();
    render(
      <EncerramentoProposta
        proposta={vigente}
        podeAlterar
        disabled={false}
        onBusy={vi.fn()}
        onConcluida={concluida}
      />,
    );
    fireEvent.click(screen.getByRole('button', { name: 'Cancelar vigência' }));
    fireEvent.change(
      screen.getByLabelText('Data de cancelamento da vigência', {
        exact: false,
      }),
      { target: { value: data } },
    );
    fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
      target: { value: 'Encerramento' },
    });
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Confirmar cancelamento da vigência',
      }),
    );
    await waitFor(() =>
      expect(concluida).toHaveBeenCalledWith(
        expect.stringContaining('Vigência cancelada'),
      ),
    );
    expect(solicitarCancelamentoVigencia).toHaveBeenCalledWith(
      vigente.publicId,
      2,
      data,
      'Encerramento',
    );
  },
);

it('exige data e justificativa e volta sem gravar', async () => {
  const busy = vi.fn();
  render(
    <EncerramentoProposta
      proposta={vigente}
      podeAlterar
      disabled={false}
      onBusy={busy}
      onConcluida={vi.fn()}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Cancelar vigência' }));
  fireEvent.click(
    screen.getByRole('button', { name: 'Confirmar cancelamento da vigência' }),
  );
  expect(
    await screen.findByText('Informe uma data de cancelamento válida.'),
  ).toBeTruthy();
  expect(screen.getByText('Informe a justificativa.')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem alterar' }));
  expect(solicitarCancelamentoVigencia).not.toHaveBeenCalled();
  expect(busy).toHaveBeenLastCalledWith(false);
});

it('preserva dados após bloqueio por sucessora e oferece recarga', async () => {
  vi.mocked(solicitarCancelamentoVigencia).mockRejectedValue(
    new Error('Resolva a versão sucessora primeiro.'),
  );
  const concluida = vi.fn();
  render(
    <EncerramentoProposta
      proposta={vigente}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={concluida}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Cancelar vigência' }));
  const data = screen.getByLabelText('Data de cancelamento da vigência', {
    exact: false,
  });
  const texto = screen.getByLabelText('Justificativa', { exact: false });
  fireEvent.change(data, { target: { value: '2026-11-13' } });
  fireEvent.change(texto, { target: { value: 'Encerramento' } });
  fireEvent.click(
    screen.getByRole('button', { name: 'Confirmar cancelamento da vigência' }),
  );
  expect(
    await screen.findByText('Resolva a versão sucessora primeiro.'),
  ).toBeTruthy();
  expect(data).toHaveProperty('value', '2026-11-13');
  expect(texto).toHaveProperty('value', 'Encerramento');
  expect(concluida).not.toHaveBeenCalled();
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Recarregar proposta e descartar alterações',
    }),
  );
  expect(concluida).toHaveBeenCalledWith(null);
});

it.each(['cancelar', 'descartar'])(
  'bloqueia envio duplicado e alterações enquanto %s está pendente',
  async (acao) => {
    let resolver: (resultado: CancelamentoVigenciaResultado) => void = () => {};
    const pendente = new Promise<CancelamentoVigenciaResultado>((resolve) => {
      resolver = resolve;
    });
    vi.mocked(solicitarCancelamentoVigencia).mockReturnValue(pendente);
    vi.mocked(descartarRascunho).mockReturnValue(pendente);
    render(
      <EncerramentoProposta
        proposta={
          acao === 'cancelar'
            ? vigente
            : { ...detalhe, podeDescartarRascunho: true }
        }
        podeAlterar
        disabled={false}
        onBusy={vi.fn()}
        onConcluida={vi.fn()}
      />,
    );
    fireEvent.click(
      screen.getByRole('button', {
        name:
          acao === 'cancelar' ? 'Cancelar vigência' : 'Descartar pré proposta',
      }),
    );
    if (acao === 'cancelar')
      fireEvent.change(
        screen.getByLabelText('Data de cancelamento da vigência', {
          exact: false,
        }),
        { target: { value: '2026-11-13' } },
      );
    fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
      target: { value: 'Encerramento' },
    });
    const form = screen.getByRole('form');
    fireEvent.submit(form);
    fireEvent.submit(form);
    const chamada =
      acao === 'cancelar' ? solicitarCancelamentoVigencia : descartarRascunho;
    await waitFor(() => expect(chamada).toHaveBeenCalledOnce());
    expect(
      screen.getByLabelText('Justificativa', { exact: false }),
    ).toHaveProperty('disabled', true);
    expect(
      screen.getByRole('button', { name: 'Voltar sem alterar' }),
    ).toHaveProperty('disabled', true);
    fireEvent.submit(form);
    expect(chamada).toHaveBeenCalledOnce();
    await act(async () =>
      resolver({
        cancelada: true,
        agendada: false,
        revisaoPreenchimento: 3,
        pendenciaVigencia: null,
      }),
    );
  },
);

it('respeita permissão e elegibilidade, mantendo agendamento para consulta', () => {
  const props = {
    proposta: vigente,
    podeAlterar: false,
    disabled: false,
    onBusy: vi.fn(),
    onConcluida: vi.fn(),
  };
  const { rerender } = render(<EncerramentoProposta {...props} />);
  expect(screen.queryByRole('button')).toBeNull();
  rerender(<EncerramentoProposta {...props} podeAlterar disabled />);
  expect(
    screen.getByRole('button', { name: 'Cancelar vigência' }),
  ).toHaveProperty('disabled', true);
  rerender(
    <EncerramentoProposta
      {...props}
      podeAlterar
      proposta={{
        ...vigente,
        podeCancelarVigencia: false,
        cancelamentoSolicitadoEm: '2026-11-12T12:00:00Z',
        dataCancelamentoVigencia: '2026-11-13',
        situacaoVigenciaCodigo: 'CANCELAMENTO_AGENDADO',
      }}
    />,
  );
  expect(screen.getByText('Cancelamento de vigência agendado')).toBeTruthy();
  expect(screen.queryByRole('button')).toBeNull();
});

it('recarrega pendência de cancelamento sem comunicar sucesso e permite nova tentativa após erro', async () => {
  vi.mocked(efetivarCancelamentoVigencia)
    .mockRejectedValueOnce(new Error('Falha de rede'))
    .mockResolvedValue({
      cancelada: false,
      agendada: false,
      revisaoPreenchimento: 4,
      pendenciaVigencia: 'Revisar Situação',
    });
  const concluida = vi.fn();
  render(
    <EncerramentoProposta
      proposta={{
        ...vigente,
        podeCancelarVigencia: false,
        podeEfetivarCancelamento: true,
        situacaoVigenciaCodigo: 'CANCELAMENTO_PENDENTE',
      }}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={concluida}
    />,
  );
  fireEvent.click(
    screen.getByRole('button', { name: 'Efetivar cancelamento agora' }),
  );
  expect(await screen.findByText('Falha de rede')).toBeTruthy();
  expect(concluida).not.toHaveBeenCalled();
  fireEvent.click(
    screen.getByRole('button', { name: 'Efetivar cancelamento agora' }),
  );
  await waitFor(() => expect(concluida).toHaveBeenCalledWith(null));
  expect(efetivarCancelamentoVigencia).toHaveBeenCalledTimes(2);
});

it('impede confirmar uma pré proposta enquanto o descarte está em aberto', async () => {
  render(
    <ConfirmacaoProposta
      proposta={{
        ...detalhe,
        podeConfirmar: true,
        podeDescartarRascunho: true,
      }}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={vi.fn()}
    />,
  );
  fireEvent.click(
    screen.getByRole('button', { name: 'Descartar pré proposta' }),
  );
  await waitFor(() =>
    expect(
      screen.getByRole('button', { name: 'Confirmar proposta' }),
    ).toHaveProperty('disabled', true),
  );
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem alterar' }));
  expect(
    screen.getByRole('button', { name: 'Confirmar proposta' }),
  ).toHaveProperty('disabled', false);
});
