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
  alterarCancelamentoVigencia,
  retirarCancelamentoVigencia,
} from '../api/propostas.api';
import type {
  PropostaDetalhe,
  CancelamentoVigenciaResultado,
} from '../types/proposta.types';
import { EncerramentoProposta } from './EncerramentoProposta';
import { ConfirmacaoProposta } from './ConfirmacaoProposta';
import { detalhe } from './propostas.fixtures';

vi.mock('../api/propostas.api');
const pedido: PropostaDetalhe = {
  ...detalhe,
  vigente: true,
  podeEditarRascunho: false,
  revisaoPreenchimento: 3,
  cancelamentoSolicitadoEm: '2026-11-12T12:00:00Z',
  dataCancelamentoVigencia: '2026-11-13',
  situacaoVigenciaCodigo: 'CANCELAMENTO_PENDENTE',
  podeAlterarCancelamento: true,
  podeRetirarCancelamento: true,
  podeEfetivarCancelamento: true,
};
const acoes = [
  {
    acao: 'alterar',
    abrir: 'Alterar pedido de cancelamento',
    confirmar: 'Confirmar alteração do pedido',
  },
  {
    acao: 'retirar',
    abrir: 'Retirar pedido de cancelamento',
    confirmar: 'Confirmar retirada do pedido',
  },
] as const;
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(alterarCancelamentoVigencia).mockReset().mockResolvedValue({
    cancelada: false,
    agendada: true,
    revisaoPreenchimento: 4,
    pendenciaVigencia: null,
  });
  vi.mocked(retirarCancelamentoVigencia)
    .mockReset()
    .mockResolvedValue({ revisaoPreenchimento: 4 });
});
function montar(proposta = pedido) {
  const onConcluida = vi.fn();
  const onBusy = vi.fn();
  const props = {
    proposta,
    podeAlterar: true,
    disabled: false,
    onConcluida,
    onBusy,
  };
  return {
    ...render(<EncerramentoProposta {...props} />),
    props,
    onConcluida,
    onBusy,
  };
}
function preencher(acao: 'alterar' | 'retirar', data = '2026-11-15') {
  if (acao === 'alterar')
    fireEvent.change(
      screen.getByLabelText('Data de cancelamento da vigência', {
        exact: false,
      }),
      { target: { value: data } },
    );
  fireEvent.change(screen.getByLabelText('Justificativa', { exact: false }), {
    target: { value: '  Revisão do Pedido  ' },
  });
}

it.each(['claro', 'escuro', 'sistema'])(
  'altera e retira pedido pendente com justificativa no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    for (const { acao, abrir, confirmar } of acoes) {
      const concluida = vi.fn();
      const busy = vi.fn();
      const { unmount } = render(
        <ThemeProvider>
          <EncerramentoProposta
            proposta={pedido}
            podeAlterar
            disabled={false}
            onBusy={busy}
            onConcluida={concluida}
          />
        </ThemeProvider>,
      );
      fireEvent.click(screen.getByRole('button', { name: abrir }));
      expect(busy).toHaveBeenLastCalledWith(true);
      expect(
        screen.getByText(/Data registrada no pedido: 13\/11\/2026/),
      ).toBeTruthy();
      expect(screen.getByText(/mesmo com a data alcançada/)).toBeTruthy();
      expect(
        screen.queryByRole('button', { name: 'Efetivar cancelamento agora' }),
      ).toBeNull();
      expect(alterarCancelamentoVigencia).not.toHaveBeenCalled();
      expect(retirarCancelamentoVigencia).not.toHaveBeenCalled();
      preencher(acao);
      fireEvent.click(screen.getByRole('button', { name: confirmar }));
      await waitFor(() =>
        expect(concluida).toHaveBeenCalledWith(
          expect.stringContaining(
            acao === 'alterar'
              ? 'Pedido de cancelamento alterado'
              : 'Pedido de cancelamento retirado',
          ),
        ),
      );
      if (acao === 'alterar')
        expect(alterarCancelamentoVigencia).toHaveBeenCalledWith(
          pedido.publicId,
          3,
          '2026-11-15',
          'Revisão do Pedido',
        );
      else
        expect(retirarCancelamentoVigencia).toHaveBeenCalledWith(
          pedido.publicId,
          3,
          'Revisão do Pedido',
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

it.each(['2026-11-10', '2026-11-12'])(
  'comunica cancelamento imediato ao alterar para %s conforme o servidor',
  async (data) => {
    vi.mocked(alterarCancelamentoVigencia).mockResolvedValue({
      cancelada: true,
      agendada: false,
      revisaoPreenchimento: 4,
      pendenciaVigencia: null,
    });
    const { onConcluida } = montar();
    fireEvent.click(screen.getByRole('button', { name: acoes[0].abrir }));
    preencher('alterar', data);
    fireEvent.click(screen.getByRole('button', { name: acoes[0].confirmar }));
    await waitFor(() =>
      expect(onConcluida).toHaveBeenCalledWith(
        expect.stringContaining('Vigência cancelada'),
      ),
    );
    expect(alterarCancelamentoVigencia).toHaveBeenCalledWith(
      pedido.publicId,
      3,
      data,
      'Revisão do Pedido',
    );
  },
);

it.each(acoes)(
  'valida $acao e volta sem gravar',
  async ({ acao, abrir, confirmar }) => {
    const { onBusy } = montar();
    fireEvent.click(screen.getByRole('button', { name: abrir }));
    fireEvent.click(screen.getByRole('button', { name: confirmar }));
    expect(await screen.findByText('Informe a justificativa.')).toBeTruthy();
    if (acao === 'alterar')
      expect(
        screen.getByText('Informe uma data de cancelamento válida.'),
      ).toBeTruthy();
    else
      expect(
        screen.queryByLabelText('Data de cancelamento da vigência', {
          exact: false,
        }),
      ).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Voltar sem alterar' }));
    expect(onBusy).toHaveBeenLastCalledWith(false);
    expect(alterarCancelamentoVigencia).not.toHaveBeenCalled();
    expect(retirarCancelamentoVigencia).not.toHaveBeenCalled();
  },
);

it.each(acoes)(
  'preserva entradas após conflito em $acao e oferece recarga',
  async ({ acao, abrir, confirmar }) => {
    vi.mocked(alterarCancelamentoVigencia).mockRejectedValue(
      new Error('A proposta foi alterada. Recarregue.'),
    );
    vi.mocked(retirarCancelamentoVigencia).mockRejectedValue(
      new Error('A proposta foi alterada. Recarregue.'),
    );
    const { onConcluida } = montar();
    fireEvent.click(screen.getByRole('button', { name: abrir }));
    preencher(acao);
    fireEvent.click(screen.getByRole('button', { name: confirmar }));
    expect(
      await screen.findByText('A proposta foi alterada. Recarregue.'),
    ).toBeTruthy();
    expect(
      screen.getByLabelText('Justificativa', { exact: false }),
    ).toHaveProperty('value', '  Revisão do Pedido  ');
    if (acao === 'alterar')
      expect(
        screen.getByLabelText('Data de cancelamento da vigência', {
          exact: false,
        }),
      ).toHaveProperty('value', '2026-11-15');
    expect(onConcluida).not.toHaveBeenCalled();
    fireEvent.click(
      screen.getByRole('button', {
        name: 'Recarregar proposta e descartar alterações',
      }),
    );
    expect(onConcluida).toHaveBeenCalledWith(null);
  },
);

it.each(acoes)(
  'bloqueia envios duplicados na validação e requisição de $acao',
  async ({ acao, abrir }) => {
    let resolver: (valor: CancelamentoVigenciaResultado) => void = () => {};
    const pendente = new Promise<CancelamentoVigenciaResultado>((resolve) => {
      resolver = resolve;
    });
    vi.mocked(alterarCancelamentoVigencia).mockReturnValue(pendente);
    vi.mocked(retirarCancelamentoVigencia).mockReturnValue(pendente);
    montar();
    fireEvent.click(screen.getByRole('button', { name: abrir }));
    preencher(acao);
    const form = screen.getByRole('form');
    fireEvent.submit(form);
    fireEvent.submit(form);
    const chamada =
      acao === 'alterar'
        ? alterarCancelamentoVigencia
        : retirarCancelamentoVigencia;
    await waitFor(() => expect(chamada).toHaveBeenCalledOnce());
    expect(
      screen.getByLabelText('Justificativa', { exact: false }),
    ).toHaveProperty('disabled', true);
    expect(
      screen.getByRole('button', { name: 'Voltar sem alterar' }),
    ).toHaveProperty('disabled', true);
    if (acao === 'alterar')
      expect(
        screen.getByLabelText('Data de cancelamento da vigência', {
          exact: false,
        }),
      ).toHaveProperty('disabled', true);
    fireEvent.submit(form);
    expect(chamada).toHaveBeenCalledOnce();
    await act(async () =>
      resolver({
        cancelada: false,
        agendada: true,
        revisaoPreenchimento: 4,
        pendenciaVigencia: null,
      }),
    );
  },
);

it('respeita permissão, bloqueio externo e flags do servidor', () => {
  const { props, rerender } = montar();
  rerender(<EncerramentoProposta {...props} podeAlterar={false} />);
  expect(screen.queryByRole('button')).toBeNull();
  rerender(<EncerramentoProposta {...props} disabled />);
  for (const { abrir } of acoes)
    expect(screen.getByRole('button', { name: abrir })).toHaveProperty(
      'disabled',
      true,
    );
  rerender(
    <EncerramentoProposta
      {...props}
      proposta={{ ...pedido, podeAlterarCancelamento: false }}
    />,
  );
  expect(screen.queryByRole('button', { name: acoes[0].abrir })).toBeNull();
  expect(screen.getByRole('button', { name: acoes[1].abrir })).toBeTruthy();
  rerender(
    <EncerramentoProposta
      {...props}
      proposta={{
        ...pedido,
        vigente: false,
        podeAlterarCancelamento: false,
        podeRetirarCancelamento: false,
        podeEfetivarCancelamento: false,
        situacaoVigenciaCodigo: 'CANCELADA',
      }}
    />,
  );
  expect(screen.queryByRole('button')).toBeNull();
});

it('propaga bloqueio de navegação e libera ao sair da gestão', async () => {
  const busy = vi.fn();
  render(
    <ConfirmacaoProposta
      proposta={pedido}
      podeAlterar
      disabled={false}
      onBusy={busy}
      onConcluida={vi.fn()}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: acoes[1].abrir }));
  await waitFor(() => expect(busy).toHaveBeenLastCalledWith(true));
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem alterar' }));
  await waitFor(() => expect(busy).toHaveBeenLastCalledWith(false));
});
