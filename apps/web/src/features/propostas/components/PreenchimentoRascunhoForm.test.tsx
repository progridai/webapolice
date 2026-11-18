import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { salvarRascunho, obterProposta } from '../api/propostas.api';
import { PreenchimentoRascunhoForm } from './PreenchimentoRascunhoForm';
import { PropostaDetalhes } from './PropostaDetalhes';
import { detalhe } from './propostas.fixtures';
import {
  criarPreenchimentoSchema,
  dadosPreenchimento,
} from '../schemas/preenchimentoRascunho.schema';

vi.mock('../api/propostas.api');
vi.mock('../../../auth/AuthorizationProvider');
const rascunho = { ...detalhe, beneficiarios: [] };

beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(salvarRascunho).mockReset().mockResolvedValue();
  vi.mocked(obterProposta).mockReset().mockResolvedValue(rascunho);
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      permissoes: ['propostas.visualizar', 'propostas.alterar'],
      modulosHabilitados: ['APOLICES'],
      recursosHabilitados: ['PROPOSTAS'],
    }),
  );
});

describe('Preenchimento da pré proposta', () => {
  it('exige escolha explícita de cônjuge e envia a contratação escolhida', async () => {
    const salvo = vi.fn();
    render(
      <PreenchimentoRascunhoForm
        proposta={{ ...rascunho, incluiConjuge: null }}
        onCancelar={vi.fn()}
        onSalvo={salvo}
      />,
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar pré proposta' }),
    );
    expect(
      await screen.findByText('Informe se há contratação de cônjuge.'),
    ).toBeTruthy();
    expect(salvarRascunho).not.toHaveBeenCalled();
    fireEvent.change(
      screen.getByLabelText('Contratação de cônjuge', { exact: false }),
      { target: { value: 'SIM' } },
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar pré proposta' }),
    );
    await waitFor(() => expect(salvo).toHaveBeenCalledOnce());
    expect(salvarRascunho).toHaveBeenCalledWith(
      rascunho.publicId,
      expect.objectContaining({ incluiConjuge: true }),
    );
  });
  it.each(['claro', 'escuro', 'sistema'])(
    'preenche beneficiário e prêmio no tema %s, mantendo revisão e valores nulos',
    async (tema) => {
      localStorage.setItem('webapolice-tema', tema);
      const salvo = vi.fn();
      render(
        <ThemeProvider>
          <PreenchimentoRascunhoForm
            proposta={rascunho}
            onCancelar={vi.fn()}
            onSalvo={salvo}
          />
        </ThemeProvider>,
      );
      fireEvent.click(
        screen.getByRole('button', { name: 'Adicionar beneficiário' }),
      );
      fireEvent.change(
        screen.getByLabelText('Nome do beneficiário 1', { exact: false }),
        { target: { value: 'MARIA d’ÁVILA' } },
      );
      fireEvent.change(
        screen.getByLabelText('Parentesco do beneficiário 1', { exact: false }),
        { target: { value: 'FILHA' } },
      );
      fireEvent.change(
        screen.getByLabelText('Participação (%) do beneficiário 1', {
          exact: false,
        }),
        { target: { value: '25,1234' } },
      );
      fireEvent.change(screen.getByLabelText('Prêmio titular da cobertura 1'), {
        target: { value: '20,50' },
      });
      fireEvent.change(screen.getByLabelText('Prêmio cônjuge da cobertura 1'), {
        target: { value: '' },
      });
      fireEvent.change(
        screen.getByLabelText('Justificativa do ajuste da cobertura 1'),
        { target: { value: 'AJUSTE DE PROTEÇÃO' } },
      );
      fireEvent.click(
        screen.getByRole('button', { name: 'Salvar pré proposta' }),
      );
      await waitFor(() => expect(salvo).toHaveBeenCalledOnce());
      expect(salvarRascunho).toHaveBeenCalledWith(rascunho.publicId, {
        revisaoEsperada: 0,
        incluiConjuge: false,
        observacao: '',
        beneficiarios: [
          {
            publicId: null,
            nome: 'MARIA d’ÁVILA',
            parentescoRegistrado: 'FILHA',
            percentualParticipacao: 25.1234,
          },
        ],
        coberturas: [
          {
            publicId: 'cobertura-a',
            premioTitular: 20.5,
            premioConjuge: null,
            justificativaAjustePremio: 'AJUSTE DE PROTEÇÃO',
          },
        ],
      });
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        tema === 'escuro' ? 'dark' : 'light',
      );
    },
  );

  it('exige justificativa e impede salvar soma maior que 100%, incluindo protegidos', async () => {
    const dados = dadosPreenchimento(detalhe);
    dados.beneficiarios.push({
      publicId: null,
      nome: 'Maria',
      parentescoRegistrado: 'Filha',
      percentualParticipacao: '1',
    });
    expect(criarPreenchimentoSchema(detalhe).safeParse(dados).success).toBe(
      false,
    );
    render(
      <PreenchimentoRascunhoForm
        proposta={rascunho}
        onCancelar={vi.fn()}
        onSalvo={vi.fn()}
      />,
    );
    fireEvent.change(screen.getByLabelText('Prêmio titular da cobertura 1'), {
      target: { value: '22' },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar pré proposta' }),
    );
    expect(
      await screen.findByText('Justifique este ajuste de prêmio.'),
    ).toBeTruthy();
    expect(salvarRascunho).not.toHaveBeenCalled();
  });

  it('permite corrigir erro, preserva edição e bloqueia envio duplicado enquanto pendente', async () => {
    vi.mocked(salvarRascunho).mockRejectedValueOnce(
      new Error(
        'A pré proposta foi alterada em outra tela. Recarregue a proposta antes de salvar.',
      ),
    );
    const salvo = vi.fn();
    const busy = vi.fn();
    render(
      <PreenchimentoRascunhoForm
        proposta={rascunho}
        onCancelar={vi.fn()}
        onSalvo={salvo}
        onBusy={busy}
      />,
    );
    fireEvent.change(screen.getByLabelText('Observação da pré proposta'), {
      target: { value: 'MINHA OBSERVAÇÃO' },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar pré proposta' }),
    );
    expect(
      await screen.findByText(/A pré proposta foi alterada em outra tela/),
    ).toBeTruthy();
    expect(screen.getByDisplayValue('MINHA OBSERVAÇÃO')).toBeTruthy();
    expect(salvo).not.toHaveBeenCalled();
    let concluir: () => void = () => {};
    vi.mocked(salvarRascunho).mockImplementationOnce(
      () =>
        new Promise<void>((resolve) => {
          concluir = resolve;
        }),
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar pré proposta' }),
    );
    await waitFor(() => expect(salvarRascunho).toHaveBeenCalledTimes(2));
    fireEvent.submit(
      screen.getByRole('form', { name: 'Preenchimento da pré proposta' }),
    );
    expect(salvarRascunho).toHaveBeenCalledTimes(2);
    expect(
      screen
        .getByRole('button', { name: 'Cancelar edição' })
        .hasAttribute('disabled'),
    ).toBe(true);
    await act(async () => concluir());
    await waitFor(() => expect(salvo).toHaveBeenCalledOnce());
    expect(busy).toHaveBeenLastCalledWith(false);
  });

  it('remove somente no formulário e pede descarte sem enviar a API', async () => {
    const cancelar = vi.fn();
    render(
      <PreenchimentoRascunhoForm
        proposta={{
          ...rascunho,
          beneficiarios: [
            {
              publicId: 'beneficiario-a',
              podeAlterar: true,
              nome: 'Maria',
              parentesco: 'Filha',
              percentualParticipacao: 50,
            },
          ],
        }}
        onCancelar={cancelar}
        onSalvo={vi.fn()}
      />,
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Remover beneficiário 1' }),
    );
    expect(
      screen.queryByLabelText('Nome do beneficiário 1', { exact: false }),
    ).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Cancelar edição' }));
    expect(await screen.findByText('Descartar alterações?')).toBeTruthy();
    expect(cancelar).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole('button', { name: 'Continuar editando' }));
    fireEvent.click(screen.getByRole('button', { name: 'Cancelar edição' }));
    fireEvent.click(
      await screen.findByRole('button', { name: 'Descartar alterações' }),
    );
    expect(cancelar).toHaveBeenCalledOnce();
    expect(salvarRascunho).not.toHaveBeenCalled();
  });

  it('não oferece edição sem permissão ou em proposta somente para consulta', async () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        permissoes: ['propostas.visualizar'],
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: ['PROPOSTAS'],
      }),
    );
    const { unmount } = render(
      <PropostaDetalhes
        publicId={rascunho.publicId}
        onVoltar={vi.fn()}
        onNovaVersao={vi.fn()}
      />,
    );
    await screen.findByText('Proposta aBc-123');
    expect(
      screen.queryByRole('button', { name: 'Editar pré proposta' }),
    ).toBeNull();
    unmount();
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        permissoes: ['propostas.visualizar', 'propostas.alterar'],
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: ['PROPOSTAS'],
      }),
    );
    vi.mocked(obterProposta).mockResolvedValue({
      ...rascunho,
      podeEditarRascunho: false,
    });
    render(
      <PropostaDetalhes
        publicId={rascunho.publicId}
        onVoltar={vi.fn()}
        onNovaVersao={vi.fn()}
      />,
    );
    await screen.findByText('Proposta aBc-123');
    expect(
      screen.queryByRole('button', { name: 'Editar pré proposta' }),
    ).toBeNull();
  });

  it('recarrega valores persistidos após salvar e bloqueia a navegação durante a edição', async () => {
    const busy = vi.fn();
    render(
      <PropostaDetalhes
        publicId={rascunho.publicId}
        onVoltar={vi.fn()}
        onNovaVersao={vi.fn()}
        onBusy={busy}
      />,
    );
    fireEvent.click(
      await screen.findByRole('button', { name: 'Editar pré proposta' }),
    );
    expect(busy).toHaveBeenLastCalledWith(true);
    expect(
      screen
        .getByRole('button', { name: 'Voltar às propostas' })
        .hasAttribute('disabled'),
    ).toBe(true);
    fireEvent.change(screen.getByLabelText('Observação da pré proposta'), {
      target: { value: 'AÇÃO DE PROTEÇÃO' },
    });
    vi.mocked(obterProposta).mockResolvedValue({
      ...rascunho,
      revisaoPreenchimento: 1,
      observacao: 'Ação de Proteção',
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar pré proposta' }),
    );
    expect(await screen.findByText('Ação de Proteção')).toBeTruthy();
    expect(await screen.findByText('Pré proposta salva')).toBeTruthy();
    expect(busy).toHaveBeenLastCalledWith(false);
  });

  it('oferece recarga com descarte após conflito sem repetir a gravação', async () => {
    vi.mocked(salvarRascunho).mockRejectedValue(
      new Error('Recarregue a proposta antes de salvar.'),
    );
    const recarregar = vi.fn();
    render(
      <PreenchimentoRascunhoForm
        proposta={rascunho}
        onCancelar={vi.fn()}
        onSalvo={vi.fn()}
        onRecarregar={recarregar}
      />,
    );
    fireEvent.change(screen.getByLabelText('Observação da pré proposta'), {
      target: { value: 'Alteração Local' },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar pré proposta' }),
    );
    fireEvent.click(
      await screen.findByRole('button', { name: 'Recarregar proposta' }),
    );
    expect(recarregar).not.toHaveBeenCalled();
    fireEvent.click(
      screen.getByRole('button', { name: 'Descartar alterações' }),
    );
    expect(recarregar).toHaveBeenCalledOnce();
    expect(salvarRascunho).toHaveBeenCalledOnce();
  });

  it.each(['-1', '1.001', '90071992547409.91', '1e2'])(
    'recusa prêmio %s sem arredondar silenciosamente',
    (valor) => {
      const dados = dadosPreenchimento(rascunho);
      dados.coberturas[0].premioTitular = valor;
      dados.coberturas[0].justificativaAjustePremio = 'Motivo';
      expect(criarPreenchimentoSchema(rascunho).safeParse(dados).success).toBe(
        false,
      );
    },
  );
});
