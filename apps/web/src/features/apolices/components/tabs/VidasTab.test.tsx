import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { VidasTab } from './VidasTab';
import { useApoliceVidas } from '../../hooks/useApoliceVidas';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { inativarApoliceVida, criarApoliceVida, atualizarApoliceVida } from '../../api/apolices.api';
import type { ApoliceVidaListItem } from '../../types/apolice.types';
import { useApoliceSubgrupos } from '../../hooks/useApoliceSubgrupos';
import { useApoliceModulos } from '../../hooks/useApoliceModulos';

// Mock dependencies
vi.mock('../../hooks/useApoliceVidas');
vi.mock('../../../../auth/AuthorizationProvider');
vi.mock('../../api/apolices.api');
vi.mock('../../hooks/useApoliceSubgrupos');
vi.mock('../../hooks/useApoliceModulos');

// Mock ClienteAsyncSelect to avoid complex async select rendering in these tests
vi.mock('./ClienteAsyncSelect', () => ({
  ClienteAsyncSelect: ({ value, onChange, disabled }: any) => (
    <input 
      data-testid="mock-cliente-select" 
      value={value || ''} 
      onChange={(e) => onChange(e.target.value)} 
      disabled={disabled}
    />
  )
}));

const mockVidaDireta: ApoliceVidaListItem = {
  apoliceVidaPublicId: 'vida-1',
  clientePublicId: 'cli-1',
  clienteNome: 'João da Silva',
  clienteDocumentoMascarado: '111.222.333-44',
  ativo: true,
  status: 'Ativa'
};

const mockVidaSub: ApoliceVidaListItem = {
  apoliceVidaPublicId: 'vida-2',
  clientePublicId: 'cli-1', // Mesmo cliente, multiplas participacoes
  clienteNome: 'João da Silva',
  clienteDocumentoMascarado: '111.222.333-44',
  apoliceSubgrupoPublicId: 'sub-1',
  subgrupoNome: 'Subgrupo Alpha',
  ativo: true,
  status: 'Ativa'
};

const mockSubgrupos = [
  { subgrupoPublicId: 'sub-1', nome: 'Subgrupo Alpha', ativo: true }
];

const mockModulos = [
  { publicId: 'mod-1', nome: 'Modulo Básico', ativo: true },
  { publicId: 'mod-2', nome: 'Modulo Premium', ativo: true }
];

describe('VidasTab', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Default auth mock: all permissions
    (useAuthorization as any).mockReturnValue({
      possuiPermissao: () => true
    });

    (useApoliceSubgrupos as any).mockReturnValue({
      data: mockSubgrupos,
      isLoading: false
    });

    (useApoliceModulos as any).mockReturnValue({
      data: mockModulos,
      isLoading: false
    });
  });

  it('deve exibir lista vazia se não houver vidas', () => {
    (useApoliceVidas as any).mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
      isLoading: false,
      error: null,
      retry: vi.fn()
    });

    render(<VidasTab publicId="apol-123" />);
    
    expect(screen.queryByText('Nenhuma vida encontrada')).not.toBeNull();
  });

  it('deve renderizar múltiplas participações do mesmo cliente (duplicidade permitida)', () => {
    (useApoliceVidas as any).mockReturnValue({
      data: { items: [mockVidaDireta, mockVidaSub], totalCount: 2, page: 1, pageSize: 10 },
      isLoading: false,
      error: null,
      retry: vi.fn()
    });

    render(<VidasTab publicId="apol-123" />);
    
    // Duas vezes o mesmo nome
    const clientNames = screen.getAllByText('João da Silva');
    expect(clientNames).toHaveLength(2);
    
    // Verifica os nomes de Subgrupo (onde preenchido)
    expect(screen.queryByText('Subgrupo Alpha')).not.toBeNull();
  });

  it('não deve exibir botões de ação se usuário tiver apenas permissão de visualizar', () => {
    (useApoliceVidas as any).mockReturnValue({
      data: { items: [mockVidaDireta], totalCount: 1, page: 1, pageSize: 10 },
      isLoading: false,
      error: null,
      retry: vi.fn()
    });

    (useAuthorization as any).mockReturnValue({
      possuiPermissao: () => false // Só pode ver
    });

    render(<VidasTab publicId="apol-123" />);
    
    expect(screen.queryByText('Adicionar Vida')).toBeNull();
    expect(screen.queryByText('Editar')).toBeNull();
    expect(screen.queryByText('Encerrar')).toBeNull();
  });

  describe('Encerramento', () => {
    it('deve abrir confirmDialog e inativar ao confirmar', async () => {
      const retryMock = vi.fn();
      (useApoliceVidas as any).mockReturnValue({
        data: { items: [mockVidaDireta], totalCount: 1, page: 1, pageSize: 10 },
        isLoading: false,
        error: null,
        retry: retryMock
      });

      render(<VidasTab publicId="apol-123" />);
      
      const btnEncerrar = screen.getByText('Encerrar');
      fireEvent.click(btnEncerrar);
      
      // Modal should appear
      expect(screen.queryByText('Deseja encerrar esta participação na Apólice? O Cadastro Global do Cliente será preservado e a participação continuará disponível no histórico.')).not.toBeNull();
      
      // Confirm
      (inativarApoliceVida as any).mockResolvedValueOnce();
      const btnConfirmar = screen.getByText('Encerrar', { selector: 'button.btn-danger' }); // Assuming destructive button text or styling
      
      fireEvent.click(btnConfirmar);
      
      await waitFor(() => {
        expect(inativarApoliceVida).toHaveBeenCalledWith('apol-123', 'vida-1');
        expect(retryMock).toHaveBeenCalled();
      });
    });
  });

  describe('Formulário de Inclusão', () => {
    it('deve criar Vida sem vínculos (sem Subgrupo/Módulo)', async () => {
      const retryMock = vi.fn();
      (useApoliceVidas as any).mockReturnValue({
        data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
        isLoading: false,
        retry: retryMock
      });

      render(<VidasTab publicId="apol-123" />);
      
      fireEvent.click(screen.getByText('Adicionar Vida'));
      
      // Preenche Cliente
      fireEvent.change(screen.getByTestId('mock-cliente-select'), { target: { value: 'cli-novo' } });
      
      // Submit
      (criarApoliceVida as any).mockResolvedValueOnce({ publicId: 'nova-vida' });
      fireEvent.click(screen.getByText('Salvar'));
      
      await waitFor(() => {
        expect(criarApoliceVida).toHaveBeenCalledWith('apol-123', expect.objectContaining({
          clientePublicId: 'cli-novo',
          apoliceSubgrupoPublicId: null,
          apoliceModuloPublicId: null
        }));
      });
    });

    it('deve permitir seleção independente de Subgrupo e Módulo', async () => {
      const retryMock = vi.fn();
      (useApoliceVidas as any).mockReturnValue({
        data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
        isLoading: false,
        retry: retryMock
      });

      render(<VidasTab publicId="apol-123" />);
      fireEvent.click(screen.getByText('Adicionar Vida'));
      
      const selects = screen.getAllByRole('combobox') as HTMLSelectElement[];
      const subgrupoSelect = selects[0]; // First select is Subgrupo
      const moduloSelect = selects[1];   // Second select is Módulo

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
      (useApoliceVidas as any).mockReturnValue({
        data: { items: [mockVidaSub], totalCount: 1, page: 1, pageSize: 10 },
        isLoading: false,
        error: null,
        retry: vi.fn()
      });

      render(<VidasTab publicId="apol-123" />);
      
      fireEvent.click(screen.getByText('Editar'));
      
      // Subgrupo deve vir preenchido, removemos ele
      const subgrupoSelect = screen.getAllByRole('combobox')[0] as HTMLSelectElement;
      expect(subgrupoSelect.value).toBe('sub-1');
      
      fireEvent.change(subgrupoSelect, { target: { value: '' } });
      
      (atualizarApoliceVida as any).mockResolvedValueOnce();
      fireEvent.click(screen.getByText('Salvar'));
      
      await waitFor(() => {
        expect(atualizarApoliceVida).toHaveBeenCalledWith('apol-123', 'vida-2', expect.objectContaining({
          apoliceSubgrupoPublicId: null,
          apoliceModuloPublicId: null // mockVidaSub não tinha módulo
        }));
      });
    });
  });
});
