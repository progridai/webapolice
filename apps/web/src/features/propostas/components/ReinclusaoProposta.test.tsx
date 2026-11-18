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
  obterProposta,
  prepararReinclusao,
  confirmarProposta,
  reagendarProposta,
} from '../api/propostas.api';
import type { PropostaDetalhe } from '../types/proposta.types';
import { ReinclusaoProposta } from './ReinclusaoProposta';
import { ConfirmacaoProposta } from './ConfirmacaoProposta';
import { GestaoAgendamentoProposta } from './GestaoAgendamentoProposta';
import { PropostaDetalhes } from './PropostaDetalhes';
import { detalhe } from './propostas.fixtures';

vi.mock('../api/propostas.api');
vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: () => ({ possuiPermissao: () => true }),
}));
const encerrada: PropostaDetalhe = {
  ...detalhe,
  podeEditarRascunho: false,
  podePrepararReinclusao: true,
  situacaoVigenciaCodigo: 'CANCELADA',
  dataFimVigencia: '2026-11-10',
  revisaoPreenchimento: 3,
};
const rascunho: PropostaDetalhe = {
  ...detalhe,
  reinclusao: true,
  dataFimVigenciaOrigem: '2026-11-10',
  podeConfirmar: true,
};
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(prepararReinclusao)
    .mockReset()
    .mockResolvedValue('nova-reinclusao');
  vi.mocked(obterProposta).mockResolvedValue(encerrada);
  vi.mocked(confirmarProposta).mockResolvedValue({
    vigente: false,
    agendada: true,
    revisaoPreenchimento: 1,
  });
  vi.mocked(reagendarProposta).mockResolvedValue({ revisaoPreenchimento: 2 });
});
function montar() {
  const props = {
    proposta: encerrada,
    podeInserir: true,
    disabled: false,
    onBusy: vi.fn(),
    onCriada: vi.fn(),
    onRecarregar: vi.fn(),
  };
  return { ...render(<ReinclusaoProposta {...props} />), props };
}

it.each(['claro', 'escuro', 'sistema'])(
  'oferece escolha explícita entre as duas fontes no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    for (const escolha of ['ANTERIOR', 'MODULO']) {
      const criada = vi.fn();
      const busy = vi.fn();
      const { unmount } = render(
        <ThemeProvider>
          <ReinclusaoProposta
            proposta={encerrada}
            podeInserir
            disabled={false}
            onBusy={busy}
            onCriada={criada}
            onRecarregar={vi.fn()}
          />
        </ThemeProvider>,
      );
      fireEvent.click(
        screen.getByRole('button', { name: 'Preparar reinclusão' }),
      );
      expect(busy).toHaveBeenLastCalledWith(true);
      const select = screen.getByLabelText('Origem das condições', {
        exact: false,
      });
      expect(select).toHaveProperty('value', '');
      expect(
        screen.getByText(/Eventual intervalo permanecerá sem cobertura/),
      ).toBeTruthy();
      expect(prepararReinclusao).not.toHaveBeenCalled();
      fireEvent.click(
        screen.getByRole('button', {
          name: 'Criar pré proposta de reinclusão',
        }),
      );
      expect(
        await screen.findByText('Escolha a origem das condições.'),
      ).toBeTruthy();
      expect(select.getAttribute('aria-invalid')).toBe('true');
      fireEvent.change(select, { target: { value: escolha } });
      fireEvent.click(
        screen.getByRole('button', {
          name: 'Criar pré proposta de reinclusão',
        }),
      );
      await waitFor(() =>
        expect(criada).toHaveBeenCalledWith('nova-reinclusao'),
      );
      expect(prepararReinclusao).toHaveBeenCalledWith(
        encerrada.publicId,
        3,
        escolha,
      );
      await waitFor(() => expect(busy).toHaveBeenLastCalledWith(false));
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        tema === 'escuro' ? 'dark' : 'light',
      );
      unmount();
      vi.clearAllMocks();
    }
  },
);

it('volta sem criar e respeita permissão, elegibilidade e bloqueio externo', () => {
  const { props, rerender } = montar();
  rerender(<ReinclusaoProposta {...props} podeInserir={false} />);
  expect(screen.queryByRole('button')).toBeNull();
  rerender(
    <ReinclusaoProposta
      {...props}
      proposta={{ ...encerrada, podePrepararReinclusao: false }}
    />,
  );
  expect(screen.queryByRole('button')).toBeNull();
  rerender(<ReinclusaoProposta {...props} disabled />);
  expect(screen.getByRole('button')).toHaveProperty('disabled', true);
  rerender(<ReinclusaoProposta {...props} />);
  fireEvent.click(screen.getByRole('button', { name: 'Preparar reinclusão' }));
  fireEvent.change(
    screen.getByLabelText('Origem das condições', { exact: false }),
    { target: { value: 'ANTERIOR' } },
  );
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem criar' }));
  expect(prepararReinclusao).not.toHaveBeenCalled();
  expect(props.onBusy).toHaveBeenLastCalledWith(false);
});

it('preserva escolha após erro de revisão e oferece recarga', async () => {
  vi.mocked(prepararReinclusao).mockRejectedValue(
    new Error('A origem foi alterada. Recarregue.'),
  );
  const { props } = montar();
  fireEvent.click(screen.getByRole('button', { name: 'Preparar reinclusão' }));
  const select = screen.getByLabelText('Origem das condições', {
    exact: false,
  });
  fireEvent.change(select, { target: { value: 'MODULO' } });
  fireEvent.click(
    screen.getByRole('button', { name: 'Criar pré proposta de reinclusão' }),
  );
  expect(
    await screen.findByText('A origem foi alterada. Recarregue.'),
  ).toBeTruthy();
  expect(select).toHaveProperty('value', 'MODULO');
  expect(props.onCriada).not.toHaveBeenCalled();
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Recarregar proposta e descartar escolha',
    }),
  );
  expect(props.onRecarregar).toHaveBeenCalledOnce();
});

it('impede duplicidade durante validação e envio, bloqueando escolha e saída', async () => {
  let resolver: (id: string) => void = () => {};
  vi.mocked(prepararReinclusao).mockReturnValue(
    new Promise<string>((resolve) => {
      resolver = resolve;
    }),
  );
  montar();
  fireEvent.click(screen.getByRole('button', { name: 'Preparar reinclusão' }));
  fireEvent.change(
    screen.getByLabelText('Origem das condições', { exact: false }),
    { target: { value: 'ANTERIOR' } },
  );
  const form = screen.getByRole('form');
  fireEvent.submit(form);
  fireEvent.submit(form);
  await waitFor(() => expect(prepararReinclusao).toHaveBeenCalledOnce());
  expect(
    screen.getByLabelText('Origem das condições', { exact: false }),
  ).toHaveProperty('disabled', true);
  expect(
    screen.getByRole('button', { name: 'Voltar sem criar' }),
  ).toHaveProperty('disabled', true);
  fireEvent.submit(form);
  expect(prepararReinclusao).toHaveBeenCalledOnce();
  await act(async () => resolver('nova'));
});

it('integra a preparação aos detalhes e bloqueia navegação até voltar', async () => {
  const busy = vi.fn();
  render(
    <PropostaDetalhes
      publicId={encerrada.publicId}
      onVoltar={vi.fn()}
      onNovaVersao={vi.fn()}
      onBusy={busy}
    />,
  );
  fireEvent.click(
    await screen.findByRole('button', { name: 'Preparar reinclusão' }),
  );
  await waitFor(() =>
    expect(
      screen.getByRole('button', { name: 'Voltar às propostas' }),
    ).toHaveProperty('disabled', true),
  );
  expect(busy).toHaveBeenLastCalledWith(true);
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem criar' }));
  await waitFor(() =>
    expect(
      screen.getByRole('button', { name: 'Voltar às propostas' }),
    ).toHaveProperty('disabled', false),
  );
});

it.each([false, true])(
  'confirma reinclusão explicando origem cancelada, agendada=%s',
  async (agendada) => {
    vi.mocked(confirmarProposta).mockResolvedValue({
      vigente: !agendada,
      agendada,
      revisaoPreenchimento: 1,
    });
    const concluida = vi.fn();
    render(
      <ConfirmacaoProposta
        proposta={rascunho}
        podeAlterar
        disabled={false}
        onBusy={vi.fn()}
        onConcluida={concluida}
      />,
    );
    fireEvent.click(screen.getByRole('button', { name: 'Confirmar proposta' }));
    expect(
      screen.getByText(/Informe início igual ou posterior a 10\/11\/2026/),
    ).toBeTruthy();
    expect(
      screen.queryByText(
        /Para início futuro, a versão anterior continua vigente/,
      ),
    ).toBeNull();
    fireEvent.change(
      screen.getByLabelText('Data de início da vigência', { exact: false }),
      { target: { value: agendada ? '2026-11-13' : '2026-11-11' } },
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Confirmar com esta data' }),
    );
    await waitFor(() =>
      expect(concluida).toHaveBeenCalledWith(
        expect.stringContaining('origem permanece cancelada'),
      ),
    );
  },
);

it('reagenda reinclusão sem anunciar cobertura na origem', async () => {
  const concluida = vi.fn();
  render(
    <GestaoAgendamentoProposta
      proposta={{
        ...rascunho,
        podeReagendar: true,
        dataInicioVigencia: '2026-11-13',
      }}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={concluida}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Reagendar início' }));
  expect(
    screen.getByText(/a origem permanece cancelada e o intervalo/),
  ).toBeTruthy();
  fireEvent.change(
    screen.getByLabelText('Novo início da vigência', { exact: false }),
    { target: { value: '2026-11-15' } },
  );
  fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
    target: { value: 'Adiamento' },
  });
  fireEvent.click(screen.getByRole('button', { name: 'Salvar nova data' }));
  await waitFor(() =>
    expect(concluida).toHaveBeenCalledWith(
      expect.stringContaining('origem permanece cancelada'),
    ),
  );
});
