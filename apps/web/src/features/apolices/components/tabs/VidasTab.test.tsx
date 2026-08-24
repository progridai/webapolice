import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { VidasTab } from './VidasTab';
import { useApoliceVidas } from '../../hooks/useApoliceVidas';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { inativarApoliceVida, criarApoliceVida, atualizarApoliceVida } from '../../api/apolices.api';
import type { ApoliceVidaListItem } from '../../types/apolice.types';
import { useApoliceSubestipulantes } from '../../hooks/useApoliceSubestipulantes';

// Mock dependencies
vi.mock('../../hooks/useApoliceVidas');
vi.mock('../../../../auth/AuthorizationProvider');
vi.mock('../../api/apolices.api');
vi.mock('../../hooks/useApoliceSubestipulantes');

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
  contexto: 'direto',
  ativo: true,
  status: 'Ativa'
};

const mockVidaSub: ApoliceVidaListItem = {
  apoliceVidaPublicId: 'vida-2',
  clientePublicId: 'cli-1', // Mesmo cliente, multiplas participacoes
  clienteNome: 'João da Silva',
  clienteDocumentoMascarado: '111.222.333-44',
  contexto: 'subestipulante',
  subestipulantePublicId: 'sub-1',
  subestipulanteNome: 'Empresa Alpha',
  ativo: true,
  status: 'Ativa'
};

const mockSubestipulantes = [
  {
    subestipulantePublicId: 'sub-1',
    nome: 'Empresa Alpha',
    ativo: true,
    modulos: [
      { moduloPublicId: 'mod-1', moduloNome: 'Modulo Básico', vinculoAtivo: true },
      { moduloPublicId: 'mod-2', moduloNome: 'Modulo Premium', vinculoAtivo: true }
    ]
  }
];

describe('VidasTab', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Default auth mock: all permissions
    (useAuthorization as any).mockReturnValue({
      possuiPermissao: () => true
    });

    (useApoliceSubestipulantes as any).mockReturnValue({
      data: mockSubestipulantes,
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
    
    expect(screen.getByText('Nenhuma vida encontrada')).toBeInTheDocument();
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
    const rows = screen.getAllByText('João da Silva');
    expect(rows).toHaveLength(2);
    
    // Verifica os badges de contexto
    expect(screen.getByText('Direto na Apólice')).toBeInTheDocument();
    expect(screen.getByText('Empresa Alpha')).toBeInTheDocument();
  });

  it('não deve exibir botões de ação se usuário tiver apenas permissão de visualizar', () => {
    (useAuthorization as any).mockReturnValue({
      possuiPermissao: (perm: string) => perm === 'apolices.visualizar'
    });

    (useApoliceVidas as any).mockReturnValue({
      data: { items: [mockVidaDireta], totalCount: 1, page: 1, pageSize: 10 },
      isLoading: false,
      error: null,
      retry: vi.fn()
    });

    render(<VidasTab publicId="apol-123" />);
    
    expect(screen.queryByText('Adicionar Vida')).not.toBeInTheDocument();
    expect(screen.queryByText('Editar')).not.toBeInTheDocument();
    expect(screen.queryByText('Encerrar')).not.toBeInTheDocument();
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
      expect(screen.getByText('Deseja encerrar esta participação na Apólice? O Cadastro Global do Cliente será preservado e a participação continuará disponível no histórico.')).toBeInTheDocument();
      
      // Confirm
      (inativarApoliceVida as any).mockResolvedValueOnce();
      const btnConfirmar = screen.getByText('Encerrar', { selector: 'button.btn-error' }); // Assuming destructive button text or styling
      
      fireEvent.click(btnConfirmar);
      
      await waitFor(() => {
        expect(inativarApoliceVida).toHaveBeenCalledWith('apol-123', 'vida-1');
        expect(retryMock).toHaveBeenCalled();
      });
    });
  });

  describe('Formulário de Inclusão', () => {
    it('deve criar Vida com contexto Direto (sem Subestipulante/Módulo)', async () => {
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
      
      // Contexto por default é direto
      expect(screen.getAllByRole('combobox')[0]).toHaveValue('direto');
      
      // Campos de subestipulante não devem estar na tela
      expect(screen.queryByText('Subestipulante da Apólice')).not.toBeInTheDocument();
      
      // Submit
      (criarApoliceVida as any).mockResolvedValueOnce({ publicId: 'nova-vida' });
      fireEvent.click(screen.getByText('Salvar'));
      
      await waitFor(() => {
        expect(criarApoliceVida).toHaveBeenCalledWith('apol-123', expect.objectContaining({
          clientePublicId: 'cli-novo',
          subestipulantePublicId: null,
          moduloPublicId: null
        }));
      });
    });

    it('deve limpar módulo em cascata ao mudar contexto ou subestipulante', async () => {
      const retryMock = vi.fn();
      (useApoliceVidas as any).mockReturnValue({
        data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
        isLoading: false,
        retry: retryMock
      });

      render(<VidasTab publicId="apol-123" />);
      fireEvent.click(screen.getByText('Adicionar Vida'));
      
      // Muda contexto para módulo
      fireEvent.change(screen.getAllByRole('combobox')[0], { target: { value: 'modulo' } });
      
      // Agora seleciona subestipulante
      fireEvent.change(screen.getAllByRole('combobox')[1], { target: { value: 'sub-1' } });
      
      // Agora o módulo deve estar disponível
      expect(screen.getByText('Modulo Básico')).toBeInTheDocument();
      
      // Muda de volta para Direto -> deve esconder subestipulante
      fireEvent.change(screen.getByRole('combobox', { name: /contexto/i }), { target: { value: 'direto' } });
      
      expect(screen.queryByText('Modulo Básico')).not.toBeInTheDocument();
      expect(screen.queryByText('Empresa Alpha')).not.toBeInTheDocument();
    });
  });

  describe('Formulário de Edição', () => {
    it('deve manter dados estruturais read-only no modo edição', async () => {
      (useApoliceVidas as any).mockReturnValue({
        data: { items: [mockVidaSub], totalCount: 1, page: 1, pageSize: 10 },
        isLoading: false,
        error: null,
        retry: vi.fn()
      });

      render(<VidasTab publicId="apol-123" />);
      
      fireEvent.click(screen.getByText('Editar'));
      
      // Campos read-only devem renderizar como textos
      expect(screen.getByText('Informações Estruturais')).toBeInTheDocument();
      
      // Não deve ter inputs select para contexto ou cliente
      expect(screen.queryByTestId('mock-cliente-select')).not.toBeInTheDocument();
      expect(screen.queryByRole('combobox', { name: /contexto/i })).not.toBeInTheDocument();
      
      // Submit edit apenas de datas
      fireEvent.change(screen.getByLabelText(/Observação/i), { target: { value: 'Obs atualizada' } });
      
      (atualizarApoliceVida as any).mockResolvedValueOnce();
      fireEvent.click(screen.getByText('Salvar'));
      
      await waitFor(() => {
        expect(atualizarApoliceVida).toHaveBeenCalledWith('apol-123', 'vida-2', expect.objectContaining({
          observacao: 'Obs atualizada'
        }));
      });
    });
  });
});
