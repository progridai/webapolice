import { useVinculosVida } from '../../hooks/useVinculosVida';
import type { ComponentProps } from 'react';
import type { ClienteAsyncSelect } from './ClienteAsyncSelect';
import {
  render,
  screen,
  fireEvent,
  waitFor,
  act,
  within,
} from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { VidasTab } from './VidasTab';
import { useApoliceVidas } from '../../hooks/useApoliceVidas';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../../tests/helpers/authorization';
import { ThemeProvider } from '../../../../shared/theme/ThemeContext';
import {
  inativarApoliceVida,
  excluirApoliceVida,
  criarApoliceVida,
  atualizarApoliceVida,
} from '../../api/apolices.api';
import type {
  ApoliceVidaListItem,
  ApoliceModuloResult,
  ApoliceSubgrupoResult,
} from '../../types/apolice.types';
import { useApoliceSubgrupos } from '../../hooks/useApoliceSubgrupos';
import { useApoliceModulos } from '../../hooks/useApoliceModulos';
import {
  listarPropostas,
  criarPropostaDaVida,
  obterProposta,
} from '../../../propostas/api/propostas.api';
vi.mock('../../../propostas/api/propostas.api');

// Mock dependencies
vi.mock('../../hooks/useApoliceVidas');
vi.mock('../../hooks/useVinculosVida');
vi.mock('../../../../auth/AuthorizationProvider');
vi.mock('../../api/apolices.api');
vi.mock('../../hooks/useApoliceSubgrupos');
vi.mock('../../hooks/useApoliceModulos');

// Mock ClienteAsyncSelect to avoid complex async select rendering in these tests
vi.mock('./ClienteAsyncSelect', () => ({
  ClienteAsyncSelect: ({
    value,
    onChange,
    disabled,
  }: Pick<
    ComponentProps<typeof ClienteAsyncSelect>,
    'value' | 'onChange' | 'disabled'
  >) => (
    <input
      data-testid="mock-cliente-select"
      value={value || ''}
      onChange={(e) => onChange(e.target.value)}
      disabled={disabled}
    />
  ),
}));

const mockVidaDireta: ApoliceVidaListItem = {
  apoliceVidaPublicId: 'vida-1',
  clientePublicId: 'cli-1',
  clienteNome: 'João da Silva',
  clienteDocumentoMascarado: '111.222.333-44',
  ativo: true,
  status: 'Ativa',
};

const mockVidaSub: ApoliceVidaListItem = {
  apoliceVidaPublicId: 'vida-2',
  clientePublicId: 'cli-1', // Mesmo cliente, multiplas participacoes
  clienteNome: 'João da Silva',
  clienteDocumentoMascarado: '111.222.333-44',
  apoliceSubgrupoPublicId: 'sub-1',
  subgrupoNome: 'Subgrupo Alpha',
  ativo: true,
  status: 'Ativa',
};

const mockSubgrupos: ApoliceSubgrupoResult[] = [
  { subgrupoPublicId: 'sub-1', nome: 'Subgrupo Alpha', ativo: true },
];

const mockModulos: ApoliceModuloResult[] = [
  {
    publicId: 'mod-1',
    moduloPublicId: 'modulo-global-1',
    nome: 'Modulo Básico',
    ativo: true,
  },
  {
    publicId: 'mod-2',
    moduloPublicId: 'modulo-global-2',
    nome: 'Modulo Premium',
    ativo: true,
  },
];

describe('VidasTab', () => {
  it('consulta propostas da participação exata mesmo encerrada e sem permissão de alterar Vida', async () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        permissoes: ['propostas.visualizar'],
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: ['PROPOSTAS'],
      }),
    );
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: {
        items: [{ ...mockVidaSub, ativo: false }],
        totalCount: 1,
        page: 1,
        pageSize: 10,
      },
      isLoading: false,
      error: null,
      retry: vi.fn().mockResolvedValue(undefined),
    });
    vi.mocked(listarPropostas).mockResolvedValue({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
    });
    render(<VidasTab publicId="apolice-a" />);
    fireEvent.click(
      screen.getByRole('button', { name: 'Consultar propostas' }),
    );
    expect(await screen.findByText('Nenhuma proposta encontrada')).toBeTruthy();
    expect(listarPropostas).toHaveBeenCalledWith(
      expect.objectContaining({
        apolicePublicId: 'apolice-a',
        apoliceVidaPublicId: 'vida-2',
      }),
      expect.any(AbortSignal),
    );
    expect(
      screen.queryByRole('button', { name: 'Criar pré proposta' }),
    ).toBeNull();
  });

  it('cria pré proposta por UUID da Vida sem exigir seleção repetida do Cliente ou matrícula', async () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        permissoes: ['propostas.inserir'],
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: ['PROPOSTAS'],
      }),
    );
    const retry = vi.fn().mockResolvedValue(undefined);
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: {
        items: [
          {
            ...mockVidaDireta,
            clienteVinculoPublicId: 'vinculo-a',
            vinculoMatricula: 'aBc-X009',
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 10,
      },
      isLoading: false,
      error: null,
      retry,
    });
    vi.mocked(criarPropostaDaVida).mockResolvedValue('proposta-a');
    render(<VidasTab publicId="apolice-a" estipulanteNome="Empresa A" />);
    fireEvent.click(screen.getByRole('button', { name: 'Criar pré proposta' }));
    expect(screen.getAllByText('aBc-X009').length).toBeGreaterThan(0);
    fireEvent.click(
      within(
        screen.getByRole('dialog', { name: 'Criar pré proposta' }),
      ).getByRole('button', { name: 'Criar pré proposta' }),
    );
    expect(await screen.findByText('Pré proposta criada')).toBeTruthy();
    expect(criarPropostaDaVida).toHaveBeenCalledWith(
      'apolice-a',
      'vida-1',
      undefined,
    );
    expect(retry).toHaveBeenCalled();
    expect(listarPropostas).not.toHaveBeenCalled();
    expect(obterProposta).not.toHaveBeenCalled();
  });

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useVinculosVida).mockReturnValue({
      data: {
        items: [
          {
            publicId: 'vinculo-1',
            matricula: 'MAT-123',
            estipulante: 'Empresa A',
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      },
      isLoading: false,
      error: null,
      refetch: vi.fn().mockResolvedValue(undefined),
    });

    // Permissões de mutação exigidas pelos cenários deste grupo
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        permissoes: [
          'apolices.vidas.inserir',
          'apolices.vidas.alterar',
          'apolices.vidas.inativar',
          'apolices.vidas.excluir',
          'clientes.alterar',
        ],
      }),
    );

    vi.mocked(useApoliceSubgrupos).mockReturnValue({
      data: mockSubgrupos,
      isLoading: false,
      error: null,
      refetch: vi
        .fn<ReturnType<typeof useApoliceSubgrupos>['refetch']>()
        .mockResolvedValue(undefined),
    });

    vi.mocked(useApoliceModulos).mockReturnValue({
      data: mockModulos,
      isLoading: false,
      error: null,
      refetch: vi
        .fn<ReturnType<typeof useApoliceModulos>['refetch']>()
        .mockResolvedValue(undefined),
    });
  });

  it('deve exibir lista vazia se não houver vidas', () => {
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
      isLoading: false,
      error: null,
      retry: vi
        .fn<ReturnType<typeof useApoliceVidas>['retry']>()
        .mockResolvedValue(undefined),
    });

    render(<VidasTab publicId="apol-123" />);

    expect(screen.queryByText('Nenhuma vida encontrada')).not.toBeNull();
  });

  it('deve renderizar múltiplas participações do mesmo cliente (duplicidade permitida)', () => {
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: {
        items: [mockVidaDireta, mockVidaSub],
        totalCount: 2,
        page: 1,
        pageSize: 10,
      },
      isLoading: false,
      error: null,
      retry: vi
        .fn<ReturnType<typeof useApoliceVidas>['retry']>()
        .mockResolvedValue(undefined),
    });

    render(<VidasTab publicId="apol-123" />);

    // Duas vezes o mesmo nome
    const clientNames = screen.getAllByText('João da Silva');
    expect(clientNames).toHaveLength(2);

    // Verifica os nomes de Subgrupo (onde preenchido)
    expect(screen.queryByText('Subgrupo Alpha')).not.toBeNull();
  });

  it('não deve exibir botões de ação se usuário tiver apenas permissão de visualizar', () => {
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: { items: [mockVidaDireta], totalCount: 1, page: 1, pageSize: 10 },
      isLoading: false,
      error: null,
      retry: vi
        .fn<ReturnType<typeof useApoliceVidas>['retry']>()
        .mockResolvedValue(undefined),
    });

    vi.mocked(useAuthorization).mockReturnValue(makeAuthorization());

    render(<VidasTab publicId="apol-123" />);

    expect(screen.queryByText('Adicionar Vida')).toBeNull();
    expect(screen.queryByText('Editar')).toBeNull();
    expect(screen.queryByText('Encerrar')).toBeNull();
    expect(screen.queryByText('Excluir')).toBeNull();
  });

  describe('Exclusão', () => {
    function configurar(
      vida: ApoliceVidaListItem = mockVidaDireta,
      pagina = 1,
    ) {
      const retry = vi
        .fn<ReturnType<typeof useApoliceVidas>['retry']>()
        .mockResolvedValue(undefined);
      vi.mocked(useApoliceVidas).mockReturnValue({
        data: {
          items: [vida],
          page: pagina,
          pageSize: 10,
          totalCount: pagina === 1 ? 1 : 11,
        },
        isLoading: false,
        error: null,
        retry,
      });
      return retry;
    }

    it.each(['claro', 'escuro', 'sistema'] as const)(
      'confirma exclusão sem encerrar no tema %s',
      async (tema) => {
        const retry = configurar();
        localStorage.setItem('webapolice-tema', tema);
        try {
          vi.mocked(excluirApoliceVida).mockResolvedValueOnce(undefined);
          render(
            <ThemeProvider>
              <VidasTab publicId="apol-123" />
            </ThemeProvider>,
          );
          fireEvent.click(screen.getByText('Excluir'));
          const dialogo = within(screen.getByRole('dialog'));
          expect(
            dialogo.getByText(/participação de João da Silva/),
          ).toBeTruthy();
          expect(excluirApoliceVida).not.toHaveBeenCalled();
          fireEvent.click(dialogo.getByRole('button', { name: 'Excluir' }));
          await waitFor(() => expect(retry).toHaveBeenCalled());
          expect(excluirApoliceVida).toHaveBeenCalledWith('apol-123', 'vida-1');
          expect(inativarApoliceVida).not.toHaveBeenCalled();
          expect(screen.queryByRole('dialog')).toBeNull();
        } finally {
          localStorage.removeItem('webapolice-tema');
          document.documentElement.removeAttribute('data-theme');
        }
      },
    );

    it('cancela sem excluir e exige a permissão específica', () => {
      configurar();
      const { unmount } = render(<VidasTab publicId="apol-123" />);
      fireEvent.click(screen.getByText('Excluir'));
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Cancelar',
        }),
      );
      expect(excluirApoliceVida).not.toHaveBeenCalled();
      unmount();
      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({ permissoes: ['apolices.vidas.inativar'] }),
      );
      render(<VidasTab publicId="apol-123" />);
      expect(screen.getByText('Encerrar')).toBeTruthy();
      expect(screen.queryByText('Excluir')).toBeNull();
    });

    it('bloqueia exclusão com propostas e permite excluir participação encerrada', () => {
      configurar({ ...mockVidaDireta, possuiPropostas: true });
      const { unmount } = render(<VidasTab publicId="apol-123" />);
      const excluir = screen.getByRole<HTMLButtonElement>('button', {
        name: 'Excluir',
      });
      expect(excluir.disabled).toBe(true);
      expect(excluir.title).toContain('possui propostas');
      unmount();
      configurar({ ...mockVidaDireta, ativo: false, status: 'encerrada' });
      render(<VidasTab publicId="apol-123" />);
      expect(screen.queryByText('Encerrar')).toBeNull();
      expect(
        screen.getByRole<HTMLButtonElement>('button', { name: 'Excluir' })
          .disabled,
      ).toBe(false);
    });

    it('mantém o diálogo com erro do backend e permite nova tentativa', async () => {
      const retry = configurar();
      vi.mocked(excluirApoliceVida)
        .mockRejectedValueOnce(new Error('Esta participação possui propostas.'))
        .mockResolvedValueOnce(undefined);
      render(<VidasTab publicId="apol-123" />);
      fireEvent.click(screen.getByText('Excluir'));
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Excluir',
        }),
      );
      expect(
        await screen.findByText('Esta participação possui propostas.'),
      ).toBeTruthy();
      expect(retry).not.toHaveBeenCalled();
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Excluir',
        }),
      );
      await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
      expect(excluirApoliceVida).toHaveBeenCalledTimes(2);
    });

    it('volta uma página ao excluir a última participação da página', async () => {
      configurar(mockVidaDireta, 2);
      vi.mocked(excluirApoliceVida).mockResolvedValueOnce(undefined);
      render(<VidasTab publicId="apol-123" />);
      fireEvent.click(
        screen.getByRole('button', { name: 'Ir para a página 2' }),
      );
      fireEvent.click(screen.getByText('Excluir'));
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Excluir',
        }),
      );
      await waitFor(() =>
        expect(useApoliceVidas).toHaveBeenLastCalledWith('apol-123', {
          page: 1,
          pageSize: 10,
        }),
      );
    });
  });

  describe('Encerramento', () => {
    it('deve abrir confirmDialog e inativar ao confirmar', async () => {
      const retryMock = vi
        .fn<ReturnType<typeof useApoliceVidas>['retry']>()
        .mockResolvedValue(undefined);
      vi.mocked(useApoliceVidas).mockReturnValue({
        data: { items: [mockVidaDireta], totalCount: 1, page: 1, pageSize: 10 },
        isLoading: false,
        error: null,
        retry: retryMock,
      });

      render(<VidasTab publicId="apol-123" />);

      const btnEncerrar = screen.getByText('Encerrar');
      fireEvent.click(btnEncerrar);

      // Modal should appear
      expect(
        screen.queryByText(
          'Deseja encerrar esta participação na Apólice? O Cadastro Global do Cliente será preservado e a participação continuará disponível no histórico.',
        ),
      ).not.toBeNull();

      // Confirm
      vi.mocked(inativarApoliceVida).mockResolvedValueOnce(undefined);
      const btnConfirmar = screen.getByText('Encerrar', {
        selector: 'button.btn-danger',
      }); // Assuming destructive button text or styling

      fireEvent.click(btnConfirmar);

      await waitFor(() => {
        expect(inativarApoliceVida).toHaveBeenCalledWith('apol-123', 'vida-1');
        expect(retryMock).toHaveBeenCalled();
      });
    });
  });

  describe('Formulário de Inclusão', () => {
    it('deve criar Vida com vínculo cadastral e sem Subgrupo/Módulo', async () => {
      const retryMock = vi
        .fn<ReturnType<typeof useApoliceVidas>['retry']>()
        .mockResolvedValue(undefined);
      vi.mocked(useApoliceVidas).mockReturnValue({
        data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
        isLoading: false,
        error: null,
        retry: retryMock,
      });

      render(<VidasTab publicId="apol-123" />);

      fireEvent.click(screen.getByText('Adicionar Vida'));

      // Preenche Cliente
      fireEvent.change(screen.getByTestId('mock-cliente-select'), {
        target: { value: 'cli-novo' },
      });

      fireEvent.change(screen.getByLabelText(/Vínculo cadastral do cliente/), {
        target: { value: 'vinculo-1' },
      });
      // Submit
      vi.mocked(criarApoliceVida).mockResolvedValueOnce({
        publicId: 'nova-vida',
      });
      fireEvent.click(screen.getByText('Salvar'));

      await waitFor(() => {
        expect(criarApoliceVida).toHaveBeenCalledWith(
          'apol-123',
          expect.objectContaining({
            clientePublicId: 'cli-novo',
            clienteVinculoPublicId: 'vinculo-1',
            apoliceSubgrupoPublicId: null,
            apoliceModuloPublicId: null,
          }),
        );
      });
    });

    it('deve permitir seleção independente de Subgrupo e Módulo', async () => {
      const retryMock = vi
        .fn<ReturnType<typeof useApoliceVidas>['retry']>()
        .mockResolvedValue(undefined);
      vi.mocked(useApoliceVidas).mockReturnValue({
        data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
        isLoading: false,
        error: null,
        retry: retryMock,
      });

      render(<VidasTab publicId="apol-123" />);
      fireEvent.click(screen.getByText('Adicionar Vida'));

      const selects = screen.getAllByRole('combobox') as HTMLSelectElement[];
      const subgrupoSelect = selects[1]; // Vínculo cadastral é a primeira seleção
      const moduloSelect = selects[2]; // Módulo é a terceira seleção

      // Muda Subgrupo
      fireEvent.change(subgrupoSelect, { target: { value: 'sub-1' } });
      expect(moduloSelect.value).toBe(''); // Módulo permanece intacto

      // Muda Módulo
      fireEvent.change(moduloSelect, { target: { value: 'mod-1' } });
      expect(subgrupoSelect.value).toBe('sub-1'); // Subgrupo permanece intacto

      // Remove Subgrupo
      fireEvent.change(subgrupoSelect, { target: { value: '' } });
      expect(moduloSelect.value).toBe('mod-1'); // Módulo permanece intacto

      // Remove Módulo
      fireEvent.change(moduloSelect, { target: { value: '' } });
      expect(subgrupoSelect.value).toBe(''); // Ambos vazios
    });
  });

  describe('Formulário de Edição', () => {
    it('deve enviar nuláveis corretamente ao remover vínculos', async () => {
      vi.mocked(useApoliceVidas).mockReturnValue({
        data: { items: [mockVidaSub], totalCount: 1, page: 1, pageSize: 10 },
        isLoading: false,
        error: null,
        retry: vi
          .fn<ReturnType<typeof useApoliceVidas>['retry']>()
          .mockResolvedValue(undefined),
      });

      render(<VidasTab publicId="apol-123" />);

      fireEvent.click(screen.getByText('Editar'));

      // Subgrupo deve vir preenchido, removemos ele
      const subgrupoSelect = screen.getAllByRole(
        'combobox',
      )[1] as HTMLSelectElement;
      expect(subgrupoSelect.value).toBe('sub-1');

      fireEvent.change(subgrupoSelect, { target: { value: '' } });

      vi.mocked(atualizarApoliceVida).mockResolvedValueOnce(undefined);
      fireEvent.click(screen.getByText('Salvar'));

      await waitFor(() => {
        expect(atualizarApoliceVida).toHaveBeenCalledWith(
          'apol-123',
          'vida-2',
          expect.objectContaining({
            apoliceSubgrupoPublicId: null,
            apoliceModuloPublicId: null, // mockVidaSub não tinha módulo
          }),
        );
      });
    });
  });
  it('exige seleção e limpa o vínculo quando o cliente muda', async () => {
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: { items: [], page: 1, pageSize: 10, totalCount: 0 },
      isLoading: false,
      error: null,
      retry: vi.fn().mockResolvedValue(undefined),
    });
    render(<VidasTab publicId="apolice" />);
    fireEvent.click(screen.getByText('Adicionar Vida'));
    fireEvent.change(screen.getByTestId('mock-cliente-select'), {
      target: { value: 'c1' },
    });
    fireEvent.click(screen.getByText('Salvar'));
    expect(
      await screen.findByText('Selecione o vínculo do cliente.'),
    ).toBeTruthy();
    expect(criarApoliceVida).not.toHaveBeenCalled();
    fireEvent.change(screen.getByLabelText(/Vínculo cadastral/), {
      target: { value: 'vinculo-1' },
    });
    fireEvent.change(screen.getByTestId('mock-cliente-select'), {
      target: { value: 'c2' },
    });
    expect(
      (screen.getByLabelText(/Vínculo cadastral/) as HTMLSelectElement).value,
    ).toBe('');
  });
  it('mostra a rejeição do backend sem perder os campos da participação', async () => {
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: { items: [mockVidaSub], page: 1, pageSize: 10, totalCount: 1 },
      isLoading: false,
      error: null,
      retry: vi.fn().mockResolvedValue(undefined),
    });
    vi.mocked(atualizarApoliceVida).mockRejectedValueOnce(
      new Error('Participação já possui propostas.'),
    );
    render(<VidasTab publicId="apolice" />);
    fireEvent.click(screen.getByText('Editar'));
    fireEvent.change(screen.getByLabelText(/Vínculo cadastral/), {
      target: { value: 'vinculo-1' },
    });
    fireEvent.click(screen.getByText('Salvar'));
    expect(
      await screen.findByText('Participação já possui propostas.'),
    ).toBeTruthy();
    expect(
      (screen.getByLabelText(/Vínculo cadastral/) as HTMLSelectElement).value,
    ).toBe('vinculo-1');
    expect(screen.getByRole('dialog')).toBeTruthy();
  });

  it.each(['claro', 'escuro', 'sistema'] as const)(
    'cadastra a matrícula ao salvar a Vida no tema %s',
    async (tema) => {
      localStorage.setItem('webapolice-tema', tema);
      try {
        vi.mocked(useApoliceVidas).mockReturnValue({
          data: { items: [], page: 1, pageSize: 10, totalCount: 0 },
          isLoading: false,
          error: null,
          retry: vi.fn().mockResolvedValue(undefined),
        });
        vi.mocked(useVinculosVida).mockReturnValue({
          data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
          isLoading: false,
          error: null,
          refetch: vi.fn().mockResolvedValue(undefined),
        });
        vi.mocked(criarApoliceVida).mockResolvedValueOnce({
          publicId: 'nova-vida',
        });
        render(
          <ThemeProvider>
            <VidasTab publicId="apolice" estipulanteNome="Empresa da Apólice" />
          </ThemeProvider>,
        );
        await act(async () => {
          fireEvent.click(screen.getByText('Adicionar Vida'));
        });
        await act(async () => {
          fireEvent.change(screen.getByTestId('mock-cliente-select'), {
            target: { value: 'cliente' },
          });
        });
        await act(async () => {
          fireEvent.click(screen.getByText('Cadastrar nova matrícula'));
        });
        expect(
          (screen.getByLabelText('Estipulante do vínculo') as HTMLInputElement)
            .value,
        ).toBe('Empresa da Apólice');
        await act(async () => {
          fireEvent.click(screen.getByText('Salvar'));
        });
        expect(await screen.findByText('Informe a matrícula')).toBeTruthy();
        expect(criarApoliceVida).not.toHaveBeenCalled();
        await act(async () => {
          fireEvent.change(screen.getByLabelText(/Nova matrícula/), {
            target: { value: '  aBc-09  ' },
          });
        });
        await act(async () => {
          fireEvent.click(screen.getByText('Salvar'));
        });
        await waitFor(() =>
          expect(criarApoliceVida).toHaveBeenCalledWith(
            'apolice',
            expect.objectContaining({
              clientePublicId: 'cliente',
              clienteVinculoPublicId: null,
              novaMatricula: 'aBc-09',
            }),
          ),
        );
      } finally {
        localStorage.removeItem('webapolice-tema');
        document.documentElement.removeAttribute('data-theme');
      }
    },
  );

  it('cancela sem criar e limpa a nova matrícula ao mudar de cliente ou modo', async () => {
    render(<VidasTab publicId="apolice" />);
    await act(async () => {
      fireEvent.click(screen.getByText('Adicionar Vida'));
    });
    await act(async () => {
      fireEvent.change(screen.getByTestId('mock-cliente-select'), {
        target: { value: 'c1' },
      });
    });
    await act(async () => {
      fireEvent.click(screen.getByText('Cadastrar nova matrícula'));
    });
    await act(async () => {
      fireEvent.change(screen.getByLabelText(/Nova matrícula/), {
        target: { value: 'ABC' },
      });
    });
    await act(async () => {
      fireEvent.click(screen.getByText('Selecionar vínculo existente'));
    });
    await act(async () => {
      fireEvent.click(screen.getByText('Cadastrar nova matrícula'));
    });
    expect(
      (screen.getByLabelText(/Nova matrícula/) as HTMLInputElement).value,
    ).toBe('');
    await act(async () => {
      fireEvent.change(screen.getByLabelText(/Nova matrícula/), {
        target: { value: 'OUTRA' },
      });
    });
    await act(async () => {
      fireEvent.change(screen.getByTestId('mock-cliente-select'), {
        target: { value: 'c2' },
      });
    });
    expect(screen.queryByLabelText(/Nova matrícula/)).toBeNull();
    await act(async () => {
      fireEvent.click(screen.getByText('Cancelar'));
    });
    expect(criarApoliceVida).not.toHaveBeenCalled();
    expect(atualizarApoliceVida).not.toHaveBeenCalled();
    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it('mantém a matrícula digitada após rejeição e permite completar uma Vida existente', async () => {
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: { items: [mockVidaSub], page: 1, pageSize: 10, totalCount: 1 },
      isLoading: false,
      error: null,
      retry: vi.fn().mockResolvedValue(undefined),
    });
    vi.mocked(atualizarApoliceVida).mockRejectedValueOnce(
      new Error('Matrícula já cadastrada.'),
    );
    render(<VidasTab publicId="apolice" />);
    await act(async () => {
      fireEvent.click(screen.getByText('Editar'));
    });
    await act(async () => {
      fireEvent.click(screen.getByText('Cadastrar nova matrícula'));
    });
    await act(async () => {
      fireEvent.change(screen.getByLabelText(/Nova matrícula/), {
        target: { value: 'aBc-12' },
      });
    });
    await act(async () => {
      fireEvent.click(screen.getByText('Salvar'));
    });
    expect(await screen.findByText('Matrícula já cadastrada.')).toBeTruthy();
    expect(
      (screen.getByLabelText(/Nova matrícula/) as HTMLInputElement).value,
    ).toBe('aBc-12');
    expect(atualizarApoliceVida).toHaveBeenCalledWith(
      'apolice',
      'vida-2',
      expect.objectContaining({
        novaMatricula: 'aBc-12',
        clienteVinculoPublicId: null,
      }),
    );
  });

  it('oculta o cadastro de matrícula sem permissão e preserva a seleção existente', async () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({ permissoes: ['apolices.vidas.inserir'] }),
    );
    render(<VidasTab publicId="apolice" />);
    await act(async () => {
      fireEvent.click(screen.getByText('Adicionar Vida'));
    });
    await act(async () => {
      fireEvent.change(screen.getByTestId('mock-cliente-select'), {
        target: { value: 'c1' },
      });
    });
    expect(screen.queryByText('Cadastrar nova matrícula')).toBeNull();
    expect(screen.getByLabelText(/Vínculo cadastral/)).toBeTruthy();
  });

  it('não permite criar matrícula em uma participação com propostas', async () => {
    vi.mocked(useApoliceVidas).mockReturnValue({
      data: {
        items: [{ ...mockVidaSub, possuiPropostas: true }],
        page: 1,
        pageSize: 10,
        totalCount: 1,
      },
      isLoading: false,
      error: null,
      retry: vi.fn().mockResolvedValue(undefined),
    });
    render(<VidasTab publicId="apolice" />);
    await act(async () => {
      fireEvent.click(screen.getByText('Editar'));
    });
    expect(screen.queryByText('Cadastrar nova matrícula')).toBeNull();
    expect(screen.getByText(/Esta participação possui propostas/)).toBeTruthy();
  });

  it.each(['claro', 'escuro', 'sistema'] as const)(
    'seleciona matrícula existente sem estipulante na edição no tema %s',
    async (tema) => {
      localStorage.setItem('webapolice-tema', tema);
      try {
        vi.mocked(useVinculosVida).mockReturnValue({
          data: {
            items: [
              {
                publicId: 'legado-123',
                matricula: '123',
                estipulante: null,
                requerEstipulante: true,
              },
            ],
            page: 1,
            pageSize: 20,
            totalCount: 1,
          },
          isLoading: false,
          error: null,
          refetch: vi.fn().mockResolvedValue(undefined),
        });
        vi.mocked(useApoliceVidas).mockReturnValue({
          data: { items: [mockVidaSub], page: 1, pageSize: 10, totalCount: 1 },
          isLoading: false,
          error: null,
          retry: vi.fn().mockResolvedValue(undefined),
        });
        vi.mocked(atualizarApoliceVida).mockResolvedValueOnce(undefined);
        render(
          <ThemeProvider>
            <VidasTab publicId="apolice" estipulanteNome="Empresa A" />
          </ThemeProvider>,
        );
        await act(async () => {
          fireEvent.click(screen.getByText('Editar'));
        });
        expect(
          screen.getByRole('option', { name: /123.*Sem estipulante/ }),
        ).toBeTruthy();
        await act(async () => {
          fireEvent.change(screen.getByLabelText(/Vínculo cadastral/), {
            target: { value: 'legado-123' },
          });
        });
        expect(
          screen.getByText(/A matrícula 123 será vinculada a Empresa A/),
        ).toBeTruthy();
        expect(atualizarApoliceVida).not.toHaveBeenCalled();
        await act(async () => {
          fireEvent.click(screen.getByText('Salvar'));
        });
        await waitFor(() =>
          expect(atualizarApoliceVida).toHaveBeenCalledWith(
            'apolice',
            'vida-2',
            expect.objectContaining({
              clienteVinculoPublicId: 'legado-123',
              completarVinculoSemEstipulante: true,
              novaMatricula: null,
            }),
          ),
        );
        expect(criarApoliceVida).not.toHaveBeenCalled();
      } finally {
        localStorage.removeItem('webapolice-tema');
        document.documentElement.removeAttribute('data-theme');
      }
    },
  );
});
