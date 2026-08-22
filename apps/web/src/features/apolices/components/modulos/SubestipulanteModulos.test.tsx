import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { SubestipulanteModulos } from './SubestipulanteModulos';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { inativarModuloSubestipulanteApolice } from '../../api/apolices.api';
import type { ApoliceSubestipulanteResult } from '../../types/apolice.types';

vi.mock('../../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));

vi.mock('../../api/apolices.api', () => ({
  inativarModuloSubestipulanteApolice: vi.fn(),
}));

vi.mock('../modals/ModuloSubestipulanteApoliceModal', () => ({
  ModuloSubestipulanteApoliceModal: ({ aberto, onClose }: any) => 
    aberto ? <div data-testid="modulo-modal"><button onClick={onClose}>Close Modal</button></div> : null,
}));

const mockSubestipulante: ApoliceSubestipulanteResult = {
  subestipulantePublicId: 'sub-1',
  nome: 'Empresa',
  ativo: true,
  modulos: [
    {
      moduloPublicId: 'mod-1',
      moduloNome: 'Módulo A',
      moduloAtivoGlobal: true,
      vinculoAtivo: true,
    },
    {
      moduloPublicId: 'mod-2',
      moduloNome: 'Módulo B',
      moduloDescricao: 'Desc B',
      moduloAtivoGlobal: false,
      vinculoAtivo: false,
      dataInicio: '2026-01-01',
      dataFim: '2026-12-31'
    }
  ]
};

describe('SubestipulanteModulos', () => {
  const mockPossuiPermissao = vi.fn();
  const mockRefresh = vi.fn();

  beforeEach(() => {
    (useAuthorization as any).mockReturnValue({
      possuiPermissao: mockPossuiPermissao,
    });
    mockPossuiPermissao.mockReset();
    mockRefresh.mockReset();
    vi.clearAllMocks();
  });

  it('deve renderizar a tabela com os módulos', () => {
    mockPossuiPermissao.mockReturnValue(true);
    render(
      <SubestipulanteModulos 
        apolicePublicId="apolice-1" 
        subestipulante={mockSubestipulante} 
        onRefresh={mockRefresh} 
      />
    );

    expect(screen.getByText('Módulos Vinculados')).toBeTruthy();
    expect(screen.getByText('Módulo A')).toBeTruthy();
    expect(screen.getByText('Módulo B')).toBeTruthy();
    expect(screen.getByText('Desc B')).toBeTruthy();
    expect(screen.getByText('Cadastro Global Inativo')).toBeTruthy();
  });

  it('não deve exibir botão de Adicionar se não tiver permissão', () => {
    mockPossuiPermissao.mockReturnValue(false);
    render(
      <SubestipulanteModulos 
        apolicePublicId="apolice-1" 
        subestipulante={mockSubestipulante} 
        onRefresh={mockRefresh} 
      />
    );

    expect(screen.queryByRole('button', { name: /Adicionar Módulo/i })).toBeNull();
  });

  it('deve abrir modal ao clicar em Adicionar Módulo', () => {
    mockPossuiPermissao.mockImplementation((perm) => perm === 'apolices.subestipulantes.modulos.inserir');
    render(
      <SubestipulanteModulos 
        apolicePublicId="apolice-1" 
        subestipulante={mockSubestipulante} 
        onRefresh={mockRefresh} 
      />
    );

    const btn = screen.getByRole('button', { name: /Adicionar Módulo/i });
    fireEvent.click(btn);

    expect(screen.getByTestId('modulo-modal')).toBeTruthy();
  });

  it('deve abrir modal ao clicar em Editar', () => {
    mockPossuiPermissao.mockImplementation((perm) => perm === 'apolices.subestipulantes.modulos.alterar');
    render(
      <SubestipulanteModulos 
        apolicePublicId="apolice-1" 
        subestipulante={mockSubestipulante} 
        onRefresh={mockRefresh} 
      />
    );

    const btns = screen.getAllByRole('button', { name: /Editar/i });
    expect(btns).toHaveLength(1); // Somente o vínculo ativo tem botão editar
    
    fireEvent.click(btns[0]);
    expect(screen.getByTestId('modulo-modal')).toBeTruthy();
  });

  it('deve exibir confirm dialog ao inativar e chamar api após confirmação', async () => {
    mockPossuiPermissao.mockImplementation((perm) => perm === 'apolices.subestipulantes.modulos.inativar');
    (inativarModuloSubestipulanteApolice as any).mockResolvedValue({});
    
    render(
      <SubestipulanteModulos 
        apolicePublicId="apolice-1" 
        subestipulante={mockSubestipulante} 
        onRefresh={mockRefresh} 
      />
    );

    const btns = screen.getAllByRole('button', { name: /Inativar/i });
    expect(btns).toHaveLength(1); // Somente o vínculo ativo
    
    fireEvent.click(btns[0]);
    
    // Verifica se confirm dialog abriu
    expect(screen.getByText('Deseja inativar o vínculo deste Módulo com o Subestipulante nesta Apólice? O Cadastro Global do Módulo será preservado.')).toBeTruthy();
    
    const confirmBtn = screen.getByRole('button', { name: 'Inativar Vínculo' });
    fireEvent.click(confirmBtn);

    await waitFor(() => {
      expect(inativarModuloSubestipulanteApolice).toHaveBeenCalledWith('apolice-1', 'sub-1', 'mod-1');
      expect(mockRefresh).toHaveBeenCalled();
    });
  });
});
