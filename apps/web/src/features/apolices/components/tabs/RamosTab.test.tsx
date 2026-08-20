import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { RamosTab } from './RamosTab';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import '@testing-library/jest-dom';

jest.mock('../../../../auth/AuthorizationProvider', () => ({
  useAuthorization: jest.fn(),
}));

jest.mock('./ApoliceRamoFormModal', () => ({
  ApoliceRamoFormModal: ({ aberto, title }: any) => aberto ? <div data-testid="ramo-modal">Modal</div> : null,
}));

const mockApolice = {
  publicId: 'apolice-1',
  nome: 'Apolice 1',
  estipulanteId: 1,
  estipulanteNome: 'Estip',
  seguradoraId: 1,
  seguradoraNome: 'Seg',
  status: 'Ativa',
  ativo: true,
  ramos: [
    {
      publicId: 'ramo-1',
      ramoCodigo: 'VG',
      ramoNome: 'Vida em Grupo',
      numeroApolice: '1234',
      iofPercentual: 7.38,
      ativo: true,
    },
    {
      publicId: 'ramo-2',
      ramoCodigo: 'AP',
      ramoNome: 'Acidentes Pessoais',
      numeroApolice: '5678',
      iofPercentual: 0,
      ativo: false,
    }
  ]
};

describe('RamosTab', () => {
  const mockPossuiPermissao = jest.fn();

  beforeEach(() => {
    (useAuthorization as jest.Mock).mockReturnValue({
      possuiPermissao: mockPossuiPermissao,
    });
    mockPossuiPermissao.mockReset();
  });

  it('deve renderizar a lista de ramos corretamente', () => {
    mockPossuiPermissao.mockReturnValue(false); // Apenas visualização
    render(<RamosTab apolice={mockApolice} />);
    
    expect(screen.getByText(/Vida em Grupo/i)).toBeInTheDocument();
    expect(screen.getByText('(VG)')).toBeInTheDocument();
    expect(screen.getByText('1234')).toBeInTheDocument();
    expect(screen.getByText('7.38%')).toBeInTheDocument();

    expect(screen.getByText(/Acidentes Pessoais/i)).toBeInTheDocument();
    expect(screen.getByText('Inativo')).toBeInTheDocument();
  });

  it('não deve exibir botões de ação se usuário não tem permissão', () => {
    mockPossuiPermissao.mockReturnValue(false);
    render(<RamosTab apolice={mockApolice} />);
    
    expect(screen.queryByRole('button', { name: /Adicionar Ramo/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Editar/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Inativar/i })).not.toBeInTheDocument();
  });

  it('deve exibir botão Adicionar se tiver permissão e abrir modal', () => {
    mockPossuiPermissao.mockImplementation((perm) => perm === 'apolices.ramos.inserir');
    render(<RamosTab apolice={mockApolice} />);
    
    const btn = screen.getByRole('button', { name: /Adicionar Ramo/i });
    expect(btn).toBeInTheDocument();

    fireEvent.click(btn);
    expect(screen.getByTestId('ramo-modal')).toBeInTheDocument();
  });

  it('deve exibir Editar e Inativar apenas em ramos ativos e se tiver permissão', () => {
    mockPossuiPermissao.mockImplementation((perm) => ['apolices.ramos.alterar', 'apolices.ramos.inativar'].includes(perm));
    render(<RamosTab apolice={mockApolice} />);
    
    // O ramo Vida em Grupo (ativo) deve ter os botões
    const editBtns = screen.getAllByRole('button', { name: /Editar/i });
    const inativarBtns = screen.getAllByRole('button', { name: /Inativar/i });
    
    expect(editBtns).toHaveLength(1); // Só tem 1 ramo ativo
    expect(inativarBtns).toHaveLength(1);
  });
});
