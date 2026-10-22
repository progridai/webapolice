import type { ApoliceSubestipulanteResult } from '../../types/apolice.types';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { SubestipulantesTab } from './SubestipulantesTab';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../../tests/helpers/authorization';
import { useApoliceSubestipulantes } from '../../hooks/useApoliceSubestipulantes';

vi.mock('../../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));

vi.mock('../../hooks/useApoliceSubestipulantes', () => ({
  useApoliceSubestipulantes: vi.fn(),
}));

vi.mock('../modals/SubestipulanteApoliceModal', () => ({
  SubestipulanteApoliceModal: ({ aberto }: { aberto: boolean }) =>
    aberto ? <div data-testid="sub-modal">Modal</div> : null,
}));

const mockData: ApoliceSubestipulanteResult[] = [
  {
    subestipulantePublicId: 'sub-1',
    nome: 'Empresa A',
    documento: '11.111.111/0001-11',
    dataInicio: '2026-01-01',
    dataFim: undefined,
    ativo: true,
  },
  {
    subestipulantePublicId: 'sub-2',
    nome: 'Empresa B',
    codigo: 'COD-123',
    dataInicio: '2026-02-01',
    dataFim: '2026-12-31',
    ativo: false,
  },
];

describe('SubestipulantesTab', () => {
  const mockPossuiPermissao =
    vi.fn<ReturnType<typeof useAuthorization>['possuiPermissao']>();
  const mockRefetch = vi
    .fn<ReturnType<typeof useApoliceSubestipulantes>['refetch']>()
    .mockResolvedValue(undefined);

  beforeEach(() => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        possuiPermissao: mockPossuiPermissao,
      }),
    );
    mockPossuiPermissao.mockReset();
    mockRefetch.mockReset().mockResolvedValue(undefined);
  });

  it('deve renderizar estado de loading', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: true,
      data: [],
      error: null,
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockReturnValue(false);

    render(<SubestipulantesTab publicId="apolice-1" />);
    // Verifica se a tabela não mostra "Nenhum Subestipulante" quando está carregando
    expect(screen.queryByText('Nenhum Subestipulante')).toBeNull();
  });

  it('deve renderizar estado de erro', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: false,
      data: [],
      error: new Error('Falha'),
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockReturnValue(false);

    render(<SubestipulantesTab publicId="apolice-1" />);
    expect(
      screen.queryByText('Erro ao carregar subestipulantes'),
    ).not.toBeNull();
    expect(screen.queryByText('Falha')).not.toBeNull();
  });

  it('deve renderizar estado vazio', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: false,
      data: [],
      error: null,
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockReturnValue(false);

    render(<SubestipulantesTab publicId="apolice-1" />);
    expect(screen.queryByText('Nenhum Subestipulante')).not.toBeNull();
    expect(
      screen.queryByText('Nenhum Subestipulante vinculado a esta Apólice.'),
    ).not.toBeNull();
  });

  it('deve renderizar a lista de subestipulantes corretamente (Ativo e Inativo)', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: false,
      data: mockData,
      error: null,
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockReturnValue(false); // Apenas visualização

    render(<SubestipulantesTab publicId="apolice-1" />);

    expect(screen.queryByText('Empresa A')).not.toBeNull();
    expect(screen.queryByText('11.111.111/0001-11')).not.toBeNull();
    expect(screen.queryByText(/01\/01\/2026/)).not.toBeNull();

    expect(screen.queryByText('Empresa B')).not.toBeNull();
    expect(screen.queryByText('COD-123')).not.toBeNull();
    expect(screen.queryByText('Inativo')).not.toBeNull();
  });

  it('não deve exibir botões de ação se usuário não tem permissão', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: false,
      data: mockData,
      error: null,
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockReturnValue(false);

    render(<SubestipulantesTab publicId="apolice-1" />);

    expect(
      screen.queryByRole('button', { name: /Adicionar Subestipulante/i }),
    ).toBeNull();
    expect(screen.queryByRole('button', { name: /Editar/i })).toBeNull();
    expect(screen.queryByRole('button', { name: /Inativar/i })).toBeNull();
  });

  it('deve exibir botão Adicionar se tiver permissão e abrir modal', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: false,
      data: mockData,
      error: null,
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.subestipulantes.inserir',
    );

    render(<SubestipulantesTab publicId="apolice-1" />);

    const btn = screen.getByRole('button', {
      name: /Adicionar Subestipulante/i,
    });
    expect(btn).not.toBeNull();

    fireEvent.click(btn);
    expect(screen.queryByTestId('sub-modal')).not.toBeNull();
  });

  it('deve exibir Editar e Inativar apenas no vínculo ativo e se tiver permissão', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: false,
      data: mockData,
      error: null,
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockImplementation((perm: string) =>
      [
        'apolices.subestipulantes.alterar',
        'apolices.subestipulantes.inativar',
      ].includes(perm),
    );

    render(<SubestipulantesTab publicId="apolice-1" />);

    // O vínculo Empresa A (ativo) deve ter os botões
    const editBtns = screen.getAllByRole('button', { name: /Editar/i });
    const inativarBtns = screen.getAllByRole('button', { name: /Inativar/i });

    expect(editBtns).toHaveLength(1); // Só 1 ativo
    expect(inativarBtns).toHaveLength(1);

    fireEvent.click(editBtns[0]);
    expect(screen.queryByTestId('sub-modal')).not.toBeNull();
  });

  it('não deve apresentar ações ou indicadores de Módulos Vinculados', () => {
    vi.mocked(useApoliceSubestipulantes).mockReturnValue({
      isLoading: false,
      data: mockData,
      error: null,
      refetch: mockRefetch,
    });
    mockPossuiPermissao.mockReturnValue(true);

    render(<SubestipulantesTab publicId="apolice-1" />);

    expect(screen.queryByText('Módulos')).toBeNull();
    expect(screen.queryByText('Módulos Vinculados')).toBeNull();
    expect(
      screen.queryByRole('button', { name: /Adicionar Módulo/i }),
    ).toBeNull();
  });
});
