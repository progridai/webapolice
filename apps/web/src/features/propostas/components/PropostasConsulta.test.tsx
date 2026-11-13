import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { PropostasConsulta } from './PropostasConsulta';
import { CriarPropostaDialog } from './CriarPropostaDialog';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import {
  criarPropostaDaVida,
  listarPropostas,
  obterProposta,
} from '../api/propostas.api';
import { proposta, detalhe } from './propostas.fixtures';
import type { Proposta } from '../types/proposta.types';
import type { PagedResult } from '../../../shared/types/pagination';

vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../api/propostas.api');

const pagina: PagedResult<Proposta> = {
  items: [proposta],
  totalCount: 1,
  page: 1,
  pageSize: 20,
};

beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      permissoes: ['propostas.visualizar', 'propostas.inserir'],
      modulosHabilitados: ['APOLICES'],
      recursosHabilitados: ['PROPOSTAS'],
    }),
  );
  vi.mocked(listarPropostas).mockReset().mockResolvedValue(pagina);
  vi.mocked(obterProposta).mockReset().mockResolvedValue(detalhe);
  vi.mocked(criarPropostaDaVida)
    .mockReset()
    .mockResolvedValue(proposta.publicId);
});

describe('Consulta e criação de Propostas', () => {
  it.each(['claro', 'escuro', 'sistema'])(
    'consulta no tema %s, preservando condições registradas e códigos',
    async (tema) => {
      localStorage.setItem('webapolice-tema', tema);
      render(
        <ThemeProvider>
          <PropostasConsulta
            apolicePublicId="apolice-a"
            apoliceVidaPublicId="vida-a"
          />
        </ThemeProvider>,
      );
      expect(await screen.findByText('aBc-X009')).toBeTruthy();
      expect(listarPropostas).toHaveBeenCalledWith(
        expect.objectContaining({
          apolicePublicId: 'apolice-a',
          apoliceVidaPublicId: 'vida-a',
        }),
        expect.any(AbortSignal),
      );
      fireEvent.click(
        screen.getByRole('button', {
          name: 'Consultar proposta aBc-123 de João da Silva',
        }),
      );
      expect(await screen.findByText('Plano da Adesão')).toBeTruthy();
      expect(screen.getByText('Nome na Adesão')).toBeTruthy();
      expect(screen.getByText('Nome Atual')).toBeTruthy();
      expect(screen.getByText('R$ 0,00', { exact: false })).toBeTruthy();
      expect(screen.getByText('Maria da Silva')).toBeTruthy();
      expect(
        screen.queryByRole('button', { name: 'Preparar nova versão' }),
      ).toBeNull();
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        tema === 'escuro' ? 'dark' : 'light',
      );
      fireEvent.click(
        screen.getByRole('button', { name: 'Voltar às propostas' }),
      );
      expect(screen.getByText('aBc-123')).toBeTruthy();
    },
  );

  it('nega consulta sem permissão e não inicia requisição', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        permissoes: ['propostas.inserir'],
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: ['PROPOSTAS'],
      }),
    );
    render(<PropostasConsulta />);
    expect(screen.getByText('Acesso restrito')).toBeTruthy();
    expect(listarPropostas).not.toHaveBeenCalled();
  });

  it('pagina e pesquisa preservando o contexto da Vida e reiniciando a página', async () => {
    vi.mocked(listarPropostas).mockResolvedValue({ ...pagina, totalCount: 41 });
    render(
      <PropostasConsulta
        apolicePublicId="apolice-a"
        apoliceVidaPublicId="vida-a"
      />,
    );
    await screen.findByText('aBc-123');
    fireEvent.click(screen.getByRole('button', { name: /próxima/i }));
    await waitFor(() =>
      expect(listarPropostas).toHaveBeenLastCalledWith(
        expect.objectContaining({ pagina: 2, apoliceVidaPublicId: 'vida-a' }),
        expect.any(AbortSignal),
      ),
    );
    fireEvent.change(screen.getByLabelText('Número da proposta'), {
      target: { value: ' aBc- ' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Pesquisar' }));
    await waitFor(() =>
      expect(listarPropostas).toHaveBeenLastCalledWith(
        expect.objectContaining({
          pagina: 1,
          termoBusca: 'aBc-',
          apolicePublicId: 'apolice-a',
          apoliceVidaPublicId: 'vida-a',
        }),
        expect.any(AbortSignal),
      ),
    );
    fireEvent.click(screen.getByRole('button', { name: 'Limpar' }));
    await waitFor(() =>
      expect(listarPropostas).toHaveBeenLastCalledWith(
        expect.objectContaining({ termoBusca: '' }),
        expect.any(AbortSignal),
      ),
    );
  });

  it('mostra erro e permite nova tentativa e lista vazia', async () => {
    vi.mocked(listarPropostas)
      .mockRejectedValueOnce(new Error('Falha de conexão'))
      .mockResolvedValue({ ...pagina, items: [], totalCount: 0 });
    render(<PropostasConsulta />);
    expect(await screen.findByText('Falha de conexão')).toBeTruthy();
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
    expect(await screen.findByText('Nenhuma proposta encontrada')).toBeTruthy();
  });

  it('cancela a consulta pendente ao trocar de Vida e descarta sua resposta atrasada', async () => {
    let concluir: (value: PagedResult<Proposta>) => void = () => {
      throw new Error('Consulta não iniciada');
    };
    let sinal: AbortSignal | undefined;
    vi.mocked(listarPropostas)
      .mockImplementationOnce((_query, signal) => {
        sinal = signal;
        return new Promise((resolve) => {
          concluir = resolve;
        });
      })
      .mockResolvedValue({ ...pagina, items: [], totalCount: 0 });
    const view = render(<PropostasConsulta apoliceVidaPublicId="vida-a" />);
    expect(sinal?.aborted).toBe(false);
    view.rerender(<PropostasConsulta apoliceVidaPublicId="vida-b" />);
    expect(sinal?.aborted).toBe(true);
    await screen.findByText('Nenhuma proposta encontrada');
    await act(async () => {
      concluir(pagina);
    });
    expect(screen.queryByText('João da Silva')).toBeNull();
  });

  it('erro de detalhe tem nova tentativa e não exibe condições antigas', async () => {
    vi.mocked(obterProposta)
      .mockRejectedValueOnce(new Error('Proposta indisponível'))
      .mockResolvedValue(detalhe);
    render(<PropostasConsulta initialPublicId={proposta.publicId} />);
    expect(await screen.findByText('Proposta indisponível')).toBeTruthy();
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
    expect(await screen.findByText('Plano da Adesão')).toBeTruthy();
  });

  it('prepara versão com origem explícita e preserva a indicação de vigência anterior', async () => {
    vi.mocked(obterProposta)
      .mockResolvedValueOnce({
        ...detalhe,
        vigente: true,
        status: 'Confirmada',
      })
      .mockResolvedValue({
        ...detalhe,
        publicId: 'nova-versao',
        versao: 2,
        propostaOrigemPublicId: proposta.publicId,
      });
    vi.mocked(criarPropostaDaVida).mockResolvedValue('nova-versao');
    render(<PropostasConsulta initialPublicId={proposta.publicId} />);
    fireEvent.click(
      await screen.findByRole('button', { name: 'Preparar nova versão' }),
    );
    expect(
      screen.getByText(/A proposta anterior continuará vigente/),
    ).toBeTruthy();
    fireEvent.click(
      screen.getByRole('button', { name: 'Criar versão em rascunho' }),
    );
    await waitFor(() =>
      expect(criarPropostaDaVida).toHaveBeenCalledWith(
        'apolice-a',
        'vida-a',
        proposta.publicId,
      ),
    );
    expect(
      await screen.findByRole('button', {
        name: 'Consultar proposta anterior',
      }),
    ).toBeTruthy();
    expect(obterProposta).toHaveBeenLastCalledWith(
      'nova-versao',
      expect.any(AbortSignal),
    );
  });

  it('criação confirma o contexto, bloqueia duplicidade e fechamento durante envio', async () => {
    let resolver: (id: string) => void = () => {
      throw new Error('Não iniciado');
    };
    vi.mocked(criarPropostaDaVida).mockImplementation(
      () =>
        new Promise((resolve) => {
          resolver = resolve;
        }),
    );
    const onCriada = vi.fn();
    const onClose = vi.fn();
    render(
      <CriarPropostaDialog
        apolicePublicId="apolice-a"
        vida={{
          apoliceVidaPublicId: 'vida-a',
          clientePublicId: 'cliente-a',
          clienteNome: 'João da Silva',
          clienteDocumentoMascarado: '',
          ativo: true,
          status: 'ativa',
          vinculoMatricula: 'aBc-X009',
        }}
        onCriada={onCriada}
        onClose={onClose}
      />,
    );
    expect(screen.getByText('aBc-X009')).toBeTruthy();
    expect(screen.getByText('Vida sem Módulo')).toBeTruthy();
    const botao = screen.getByRole('button', { name: 'Criar rascunho' });
    fireEvent.click(botao);
    fireEvent.click(botao);
    fireEvent.keyDown(window, { key: 'Escape' });
    expect(onClose).not.toHaveBeenCalled();
    expect(criarPropostaDaVida).toHaveBeenCalledTimes(1);
    expect(criarPropostaDaVida).toHaveBeenCalledWith(
      'apolice-a',
      'vida-a',
      undefined,
    );
    await act(async () => resolver(proposta.publicId));
    expect(onCriada).toHaveBeenCalledWith(proposta.publicId);
  });

  it('falha da criação conserva a confirmação para nova tentativa', async () => {
    vi.mocked(criarPropostaDaVida).mockRejectedValueOnce(
      new Error('Status inicial não configurado'),
    );
    const onCriada = vi.fn();
    render(
      <CriarPropostaDialog
        apolicePublicId="apolice-a"
        vida={{
          apoliceVidaPublicId: 'vida-a',
          clientePublicId: 'cliente-a',
          clienteNome: 'João da Silva',
          clienteDocumentoMascarado: '',
          ativo: true,
          status: 'ativa',
        }}
        onCriada={onCriada}
        onClose={vi.fn()}
      />,
    );
    fireEvent.click(screen.getByRole('button', { name: 'Criar rascunho' }));
    expect(
      await screen.findByText('Status inicial não configurado'),
    ).toBeTruthy();
    expect(onCriada).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole('button', { name: 'Criar rascunho' }));
    await waitFor(() =>
      expect(onCriada).toHaveBeenCalledWith(proposta.publicId),
    );
  });
});
