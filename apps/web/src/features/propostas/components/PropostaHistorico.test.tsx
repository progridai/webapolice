import { fireEvent, render, screen } from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { obterProposta } from '../api/propostas.api';
import { PropostaDetalhes } from './PropostaDetalhes';
import { detalhe } from './propostas.fixtures';

vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../api/propostas.api');

beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      permissoes: ['propostas.visualizar', 'propostas.alterar'],
      modulosHabilitados: ['APOLICES'],
      recursosHabilitados: ['PROPOSTAS'],
    }),
  );
  vi.mocked(obterProposta)
    .mockReset()
    .mockResolvedValue({
      ...detalhe,
      historico: [
        {
          dataAlteracao: '2026-11-12T12:00:00Z',
          motivo: 'Preparação de Nova Versão',
          observacao: 'Condições da Família',
          propostaAnteriorPublicId: detalhe.publicId,
          versaoAnterior: 1,
          propostaNovaPublicId: 'proposta-nova',
          versaoNova: 2,
        },
      ],
    });
});

it('consulta cancelamento e justificativa sem oferecer edição ou efetivação', async () => {
  vi.mocked(obterProposta).mockResolvedValue({
    ...detalhe,
    status: 'Cancelada',
    statusCodigo: 'CANCELADA',
    situacaoVigenciaCodigo: 'CANCELADA',
    podeEditarRascunho: false,
    canceladaEm: '2026-11-12T12:00:00Z',
    historico: [
      {
        dataAlteracao: '2026-11-12T12:00:00Z',
        motivo: 'Cancelamento de Agendamento',
        observacao: 'Desistência da Adesão',
        propostaAnteriorPublicId: detalhe.publicId,
        propostaNovaPublicId: detalhe.publicId,
        versaoAnterior: 1,
        versaoNova: 1,
      },
    ],
  });
  render(
    <PropostaDetalhes
      publicId={detalhe.publicId}
      onVoltar={vi.fn()}
      onNovaVersao={vi.fn()}
    />,
  );
  expect(await screen.findByText('Cancelada sem vigência')).toBeTruthy();
  expect(screen.getByText('Cancelamento registrado em:')).toBeTruthy();
  expect(screen.getByText('Desistência da Adesão')).toBeTruthy();
  expect(
    screen.queryByRole('button', { name: 'Editar pré proposta' }),
  ).toBeNull();
  expect(
    screen.queryByRole('button', { name: 'Efetivar vigência agora' }),
  ).toBeNull();
  expect(
    screen.queryByRole('button', { name: 'Consultar versão 1' }),
  ).toBeNull();
});

it('mostra cancelamento planejado como vigente e bloqueia preparar sucessora', async () => {
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      permissoes: [
        'propostas.visualizar',
        'propostas.alterar',
        'propostas.inserir',
      ],
      modulosHabilitados: ['APOLICES'],
      recursosHabilitados: ['PROPOSTAS'],
    }),
  );
  vi.mocked(obterProposta).mockResolvedValue({
    ...detalhe,
    vigente: true,
    podeEditarRascunho: false,
    situacaoVigenciaCodigo: 'CANCELAMENTO_AGENDADO',
    cancelamentoSolicitadoEm: '2026-11-12T12:00:00Z',
    dataCancelamentoVigencia: '2026-11-13',
  });
  render(
    <PropostaDetalhes
      publicId={detalhe.publicId}
      onVoltar={vi.fn()}
      onNovaVersao={vi.fn()}
    />,
  );
  expect(
    await screen.findByText('Vigente com cancelamento agendado'),
  ).toBeTruthy();
  expect(screen.getByText('Data solicitada de cancelamento:')).toBeTruthy();
  expect(
    screen.queryByRole('button', { name: 'Preparar nova versão' }),
  ).toBeNull();
});

it.each(['claro', 'escuro', 'sistema'])(
  'consulta histórico e navega para a versão relacionada no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    const consultar = vi.fn();
    render(
      <ThemeProvider>
        <PropostaDetalhes
          publicId={detalhe.publicId}
          onVoltar={vi.fn()}
          onNovaVersao={consultar}
        />
      </ThemeProvider>,
    );
    expect(await screen.findByText('Preparação de Nova Versão')).toBeTruthy();
    expect(screen.getByText('Condições da Família')).toBeTruthy();
    expect(
      screen.queryByRole('button', { name: 'Consultar versão 1' }),
    ).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Consultar versão 2' }));
    expect(consultar).toHaveBeenCalledWith('proposta-nova');
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      tema === 'escuro' ? 'dark' : 'light',
    );
  },
);

it('preserva o evento sem oferecer navegação para uma versão oculta', async () => {
  vi.mocked(obterProposta).mockResolvedValue({
    ...detalhe,
    historico: [
      {
        dataAlteracao: '2026-11-12T12:00:00Z',
        motivo: 'Evento Histórico',
        observacao: null,
        propostaAnteriorPublicId: detalhe.publicId,
        versaoAnterior: 1,
        propostaNovaPublicId: null,
        versaoNova: 2,
      },
    ],
  });
  render(
    <PropostaDetalhes
      publicId={detalhe.publicId}
      onVoltar={vi.fn()}
      onNovaVersao={vi.fn()}
    />,
  );
  expect(await screen.findByText('Evento Histórico')).toBeTruthy();
  expect(
    screen.queryByRole('button', { name: 'Consultar versão 2' }),
  ).toBeNull();
});

it('bloqueia troca de versão durante edição para preservar alterações locais', async () => {
  const consultar = vi.fn();
  render(
    <PropostaDetalhes
      publicId={detalhe.publicId}
      onVoltar={vi.fn()}
      onNovaVersao={consultar}
    />,
  );
  fireEvent.click(
    await screen.findByRole('button', { name: 'Editar pré proposta' }),
  );
  expect(
    screen
      .getByRole('button', { name: 'Consultar versão 2' })
      .hasAttribute('disabled'),
  ).toBe(true);
  fireEvent.click(screen.getByRole('button', { name: 'Consultar versão 2' }));
  expect(consultar).not.toHaveBeenCalled();
});

it('preserva quatro casas da participação na conferência anterior à confirmação', async () => {
  vi.mocked(obterProposta).mockResolvedValue({
    ...detalhe,
    beneficiarios: [
      {
        publicId: 'beneficiario-1',
        podeAlterar: true,
        nome: 'Maria',
        parentesco: 'Filha',
        percentualParticipacao: 50.0001,
      },
      {
        publicId: 'beneficiario-2',
        podeAlterar: true,
        nome: 'João',
        parentesco: 'Filho',
        percentualParticipacao: 49.9999,
      },
    ],
  });
  render(
    <PropostaDetalhes
      publicId={detalhe.publicId}
      onVoltar={vi.fn()}
      onNovaVersao={vi.fn()}
    />,
  );
  expect(await screen.findByText('50,0001%')).toBeTruthy();
  expect(screen.getByText('49,9999%')).toBeTruthy();
});
