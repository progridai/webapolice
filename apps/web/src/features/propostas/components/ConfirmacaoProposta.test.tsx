import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { confirmarProposta, efetivarVigencia } from '../api/propostas.api';
import {
  ConfirmacaoProposta,
  ConfirmacaoPropostaForm,
} from './ConfirmacaoProposta';
import { detalhe } from './propostas.fixtures';
import type { ConfirmacaoPropostaResultado } from '../types/proposta.types';

vi.mock('../api/propostas.api');
const pronta = { ...detalhe, podeConfirmar: true, revisaoPreenchimento: 7 };
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(confirmarProposta).mockReset().mockResolvedValue({
    vigente: false,
    agendada: true,
    revisaoPreenchimento: 8,
  });
  vi.mocked(efetivarVigencia).mockReset().mockResolvedValue({
    ativada: true,
    jaEfetivada: false,
    revisaoPreenchimento: 9,
    pendenciaVigencia: null,
  });
});

it.each(['claro', 'escuro', 'sistema'])(
  'confirma com data explicitamente informada no tema %s e comunica agendamento',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const concluida = vi.fn();
    const busy = vi.fn();
    render(
      <ThemeProvider>
        <ConfirmacaoProposta
          proposta={pronta}
          podeAlterar
          disabled={false}
          onBusy={busy}
          onConcluida={concluida}
        />
      </ThemeProvider>,
    );
    fireEvent.click(screen.getByRole('button', { name: 'Confirmar proposta' }));
    expect(busy).toHaveBeenLastCalledWith(true);
    expect(
      screen.getByText(
        /Para início futuro, a versão anterior continua vigente/,
      ),
    ).toBeTruthy();
    fireEvent.change(
      screen.getByLabelText('Data de início da vigência', { exact: false }),
      { target: { value: '2026-12-01' } },
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Confirmar com esta data' }),
    );
    await waitFor(() =>
      expect(concluida).toHaveBeenCalledWith(
        expect.stringContaining('início agendado'),
      ),
    );
    expect(confirmarProposta).toHaveBeenCalledWith(
      pronta.publicId,
      7,
      '2026-12-01',
    );
    expect(busy).toHaveBeenLastCalledWith(false);
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      tema === 'escuro' ? 'dark' : 'light',
    );
  },
);

it('exige data e permite cancelar sem gravar', async () => {
  const cancelar = vi.fn();
  render(
    <ConfirmacaoPropostaForm
      proposta={pronta}
      onCancelar={cancelar}
      onConfirmada={vi.fn()}
      onRecarregar={vi.fn()}
    />,
  );
  fireEvent.click(
    screen.getByRole('button', { name: 'Confirmar com esta data' }),
  );
  expect(
    await screen.findByText('Informe uma data de início válida.'),
  ).toBeTruthy();
  expect(confirmarProposta).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Cancelar confirmação' }));
  expect(cancelar).toHaveBeenCalledOnce();
});

it('preserva a data no erro e oferece recarga após conflito de revisão', async () => {
  vi.mocked(confirmarProposta).mockRejectedValue(
    new Error(
      'A proposta foi alterada em outra tela. Recarregue antes de confirmar.',
    ),
  );
  const recarregar = vi.fn();
  const confirmada = vi.fn();
  render(
    <ConfirmacaoPropostaForm
      proposta={pronta}
      onCancelar={vi.fn()}
      onConfirmada={confirmada}
      onRecarregar={recarregar}
    />,
  );
  fireEvent.change(
    screen.getByLabelText('Data de início da vigência', { exact: false }),
    { target: { value: '2026-11-12' } },
  );
  fireEvent.click(
    screen.getByRole('button', { name: 'Confirmar com esta data' }),
  );
  expect(
    await screen.findByText(/A proposta foi alterada em outra tela/),
  ).toBeTruthy();
  expect(screen.getByDisplayValue('2026-11-12')).toBeTruthy();
  expect(confirmada).not.toHaveBeenCalled();
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Recarregar proposta e descartar a data',
    }),
  );
  expect(recarregar).toHaveBeenCalledOnce();
});

it('bloqueia envio duplicado desde a validação assíncrona e cancelamento enquanto salva', async () => {
  let resolver: (resultado: ConfirmacaoPropostaResultado) => void = () => {};
  vi.mocked(confirmarProposta).mockImplementation(
    () =>
      new Promise((resolve) => {
        resolver = resolve;
      }),
  );
  const confirmada = vi.fn();
  render(
    <ConfirmacaoPropostaForm
      proposta={pronta}
      onCancelar={vi.fn()}
      onConfirmada={confirmada}
      onRecarregar={vi.fn()}
    />,
  );
  fireEvent.change(
    screen.getByLabelText('Data de início da vigência', { exact: false }),
    { target: { value: '2026-11-12' } },
  );
  const form = screen.getByRole('form', { name: 'Confirmação da proposta' });
  fireEvent.submit(form);
  fireEvent.submit(form);
  await waitFor(() => expect(confirmarProposta).toHaveBeenCalledOnce());
  expect(
    screen
      .getByRole('button', { name: 'Cancelar confirmação' })
      .hasAttribute('disabled'),
  ).toBe(true);
  fireEvent.submit(form);
  await act(async () =>
    resolver({ vigente: true, agendada: false, revisaoPreenchimento: 8 }),
  );
  await waitFor(() => expect(confirmada).toHaveBeenCalledOnce());
  expect(confirmarProposta).toHaveBeenCalledOnce();
});

it('oculta ações sem alterar ou quando não elegível', () => {
  const { rerender } = render(
    <ConfirmacaoProposta
      proposta={pronta}
      podeAlterar={false}
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={vi.fn()}
    />,
  );
  expect(
    screen.queryByRole('button', { name: 'Confirmar proposta' }),
  ).toBeNull();
  rerender(
    <ConfirmacaoProposta
      proposta={detalhe}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={vi.fn()}
    />,
  );
  expect(
    screen.queryByRole('button', { name: 'Confirmar proposta' }),
  ).toBeNull();
});

it('mostra agenda somente para consulta sem oferecer ativação antes da data', () => {
  render(
    <ConfirmacaoProposta
      proposta={{
        ...detalhe,
        confirmadaEm: '2026-11-12T12:00:00Z',
        dataInicioVigencia: '2026-12-01',
        situacaoVigenciaCodigo: 'AGENDADA',
      }}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={vi.fn()}
    />,
  );
  expect(screen.getByText('Início de vigência agendado')).toBeTruthy();
  expect(
    screen.queryByRole('button', { name: 'Efetivar vigência agora' }),
  ).toBeNull();
  expect(
    screen.queryByRole('button', { name: 'Confirmar proposta' }),
  ).toBeNull();
});

it('revalida a ativação e recarrega a pendência sem anunciar sucesso', async () => {
  vi.mocked(efetivarVigencia).mockResolvedValue({
    ativada: false,
    jaEfetivada: false,
    revisaoPreenchimento: 9,
    pendenciaVigencia: 'Vida Inativa',
  });
  const concluida = vi.fn();
  render(
    <ConfirmacaoProposta
      proposta={{
        ...detalhe,
        podeEfetivarVigencia: true,
        situacaoVigenciaCodigo: 'ATIVACAO_PENDENTE',
        motivoPendenciaVigencia: 'Vida Inativa',
      }}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={concluida}
    />,
  );
  expect(screen.getByText('Vida Inativa')).toBeTruthy();
  fireEvent.click(
    screen.getByRole('button', { name: 'Efetivar vigência agora' }),
  );
  await waitFor(() => expect(concluida).toHaveBeenCalledWith(null));
  expect(efetivarVigencia).toHaveBeenCalledWith(
    detalhe.publicId,
    detalhe.revisaoPreenchimento,
  );
});

it('permite nova tentativa após erro de rede na ativação', async () => {
  vi.mocked(efetivarVigencia).mockRejectedValueOnce(new Error('Erro de rede'));
  const concluida = vi.fn();
  render(
    <ConfirmacaoProposta
      proposta={{ ...detalhe, podeEfetivarVigencia: true }}
      podeAlterar
      disabled={false}
      onBusy={vi.fn()}
      onConcluida={concluida}
    />,
  );
  fireEvent.click(
    screen.getByRole('button', { name: 'Efetivar vigência agora' }),
  );
  expect(await screen.findByText('Erro de rede')).toBeTruthy();
  fireEvent.click(
    screen.getByRole('button', { name: 'Efetivar vigência agora' }),
  );
  await waitFor(() =>
    expect(concluida).toHaveBeenCalledWith(
      expect.stringContaining('Vigência ativada'),
    ),
  );
  expect(efetivarVigencia).toHaveBeenCalledTimes(2);
});
