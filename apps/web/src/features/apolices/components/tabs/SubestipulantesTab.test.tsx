import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { SubestipulantesTab } from './SubestipulantesTab';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { useApoliceSubestipulantes } from '../../hooks/useApoliceSubestipulantes';

vi.mock('../../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));

vi.mock('../../hooks/useApoliceSubestipulantes', () => ({
  useApoliceSubestipulantes: vi.fn(),
}));

vi.mock('../modals/SubestipulanteApoliceModal', () => ({
  SubestipulanteApoliceModal: ({ aberto }: any) => aberto ? <div data-testid="sub-modal">Modal</div> : null,
}));

const mockData = [
  {
    subestipulantePublicId: 'sub-1',
    nome: 'Empresa A',
    documento: '11.111.111/0001-11',
    dataInicio: '2026-01-01',
    dataFim: null,
    ativo: true,
    modulos: [{ moduloIdInternal: 1 }]
  },
  {
    subestipulantePublicId: 'sub-2',
    nome: 'Empresa B',
    codigo: 'COD-123',
    dataInicio: '2026-02-01',
    dataFim: '2026-12-31',
    ativo: false,
    modulos: []
  }
];

describe('SubestipulantesTab', () => {
  const mockPossuiPermissao = vi.fn();
  const mockRefetch = vi.fn();

  beforeEach(() => {
    (useAuthorization as any).mockReturnValue({
      possuiPermissao: mockPossuiPermissao,
    });
    mockPossuiPermissao.mockReset();
    mockRefetch.mockReset();
  });

  it('deve renderizar estado de loading', () => {
    (useApoliceSubestipulantes as any).mockReturnValue({ isLoading: true, data: [], error: null, refetch: mockRefetch });
    mockPossuiPermissao.mockReturnValue(false);
    
    render(<SubestipulantesTab publicId="apolice-1" />);
    // Verifica se a tabela não mostra "Nenhum Subestipulante" quando está carregando
    expect(screen.queryByText('Nenhum Subestipulante')).not.toBeInTheDocument();
  });

  it('deve renderizar estado de erro', () => {
    (useApoliceSubestipulantes as any).mockReturnValue({ isLoading: false, data: [], error: new Error('Falha'), refetch: mockRefetch });
    mockPossuiPermissao.mockReturnValue(false);
    
    render(<SubestipulantesTab publicId="apolice-1" />);
    expect(screen.getByText('Erro ao carregar subestipulantes')).toBeInTheDocument();
    expect(screen.getByText('Falha')).toBeInTheDocument();
  });

  it('deve renderizar estado vazio', () => {
    (useApoliceSubestipulantes as any).mockReturnValue({ isLoading: false, data: [], error: null, refetch: mockRefetch });
    mockPossuiPermissao.mockReturnValue(false);
    
    render(<SubestipulantesTab publicId="apolice-1" />);
    expect(screen.getByText('Nenhum Subestipulante')).toBeInTheDocument();
    expect(screen.getByText('Nenhum Subestipulante vinculado a esta Apólice.')).toBeInTheDocument();
  });

  it('deve renderizar a lista de subestipulantes corretamente (Ativo e Inativo)', () => {
    (useApoliceSubestipulantes as any).mockReturnValue({ isLoading: false, data: mockData, error: null, refetch: mockRefetch });
    mockPossuiPermissao.mockReturnValue(false); // Apenas visualização
    
    render(<SubestipulantesTab publicId="apolice-1" />);
    
    expect(screen.getByText('Empresa A')).toBeInTheDocument();
    expect(screen.getByText('11.111.111/0001-11')).toBeInTheDocument();
    expect(screen.getByText(/01\/01\/2026/)).toBeInTheDocument();
    expect(screen.getByText('1 módulo(s)')).toBeInTheDocument();
    
    expect(screen.getByText('Empresa B')).toBeInTheDocument();
    expect(screen.getByText('COD-123')).toBeInTheDocument();
    expect(screen.getByText('Sem módulos')).toBeInTheDocument();
    expect(screen.getByText('Inativo')).toBeInTheDocument();
  });

  it('não deve exibir botões de ação se usuário não tem permissão', () => {
    (useApoliceSubestipulantes as any).mockReturnValue({ isLoading: false, data: mockData, error: null, refetch: mockRefetch });
    mockPossuiPermissao.mockReturnValue(false);
    
    render(<SubestipulantesTab publicId="apolice-1" />);
    
    expect(screen.queryByRole('button', { name: /Adicionar Subestipulante/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Editar/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Inativar/i })).not.toBeInTheDocument();
  });

  it('deve exibir botão Adicionar se tiver permissão e abrir modal', () => {
    (useApoliceSubestipulantes as any).mockReturnValue({ isLoading: false, data: mockData, error: null, refetch: mockRefetch });
    mockPossuiPermissao.mockImplementation((perm: string) => perm === 'apolices.subestipulantes.inserir');
    
    render(<SubestipulantesTab publicId="apolice-1" />);
    
    const btn = screen.getByRole('button', { name: /Adicionar Subestipulante/i });
    expect(btn).toBeInTheDocument();

    fireEvent.click(btn);
    expect(screen.getByTestId('sub-modal')).toBeInTheDocument();
  });

  it('deve exibir Editar e Inativar apenas no vínculo ativo e se tiver permissão', () => {
    (useApoliceSubestipulantes as any).mockReturnValue({ isLoading: false, data: mockData, error: null, refetch: mockRefetch });
    mockPossuiPermissao.mockImplementation((perm: string) => ['apolices.subestipulantes.alterar', 'apolices.subestipulantes.inativar'].includes(perm));
    
    render(<SubestipulantesTab publicId="apolice-1" />);
    
    // O vínculo Empresa A (ativo) deve ter os botões
    const editBtns = screen.getAllByRole('button', { name: /Editar/i });
    const inativarBtns = screen.getAllByRole('button', { name: /Inativar/i });
    
    expect(editBtns).toHaveLength(1); // Só 1 ativo
    expect(inativarBtns).toHaveLength(1);
    
    fireEvent.click(editBtns[0]);
    expect(screen.getByTestId('sub-modal')).toBeInTheDocument();
  });
});
