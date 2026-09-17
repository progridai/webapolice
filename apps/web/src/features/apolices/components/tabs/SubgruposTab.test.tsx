import { vi } from 'vitest';
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { SubgruposTab } from './SubgruposTab';
import { useApoliceSubgrupos } from '../../hooks/useApoliceSubgrupos';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { inativarApoliceSubgrupo } from '../../api/apolices.api';

// Mock dos hooks e API
vi.mock('../../hooks/useApoliceSubgrupos');
vi.mock('../../../../auth/AuthorizationProvider');
vi.mock('../../api/apolices.api');

vi.mock('../modals/SubgrupoApoliceModal', () => ({
  SubgrupoApoliceModal: ({ aberto }: { aberto: boolean }) => (
    aberto ? <div data-testid="mock-modal-aberto">MODAL ABERTO</div> : null
  )
}));

const mockUseApoliceSubgrupos = useApoliceSubgrupos as any;
const mockUseAuthorization = useAuthorization as any;
const mockInativarApoliceSubgrupo = inativarApoliceSubgrupo as any;

const renderComponent = () => {
  return render(
    <SubgruposTab publicId="apolice-123" />
  );
};

describe('SubgruposTab', () => {
  const mockPossuiPermissao = vi.fn();
  const mockRefetch = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAuthorization.mockReturnValue({ possuiPermissao: mockPossuiPermissao });
    
    // Default mock implementation para o hook
    mockUseApoliceSubgrupos.mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    // Default permissões (nenhuma mutação)
    mockPossuiPermissao.mockReturnValue(false);
  });

  // Teste de loading removido pois o DataTable gerencia internamente

  it('renderiza o estado vazio sem botão se não tiver permissão', () => {
    renderComponent();
    expect(screen.getByText('Nenhum Subgrupo cadastrado')).toBeTruthy();
    expect(screen.queryByText('Adicionar Subgrupo')).toBeNull();
  });

  it('renderiza o estado vazio com botão se tiver permissão de inserir', () => {
    mockPossuiPermissao.mockImplementation((perm: string) => perm === 'apolices.subgrupos.inserir');
    renderComponent();
    expect(screen.getByText('Nenhum Subgrupo cadastrado')).toBeTruthy();
    expect(screen.getByText('Adicionar Subgrupo')).toBeTruthy();
  });

  it('renderiza a tabela com dados', () => {
    mockUseApoliceSubgrupos.mockReturnValue({
      data: [
        {
          subgrupoPublicId: 'sub-1',
          nome: 'Matriz',
          observacao: 'Sede central',
          ativo: true,
        },
        {
          subgrupoPublicId: 'sub-2',
          nome: 'Filial 1',
          observacao: null,
          ativo: false,
        }
      ],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    expect(screen.getByText('Matriz')).toBeTruthy();
    expect(screen.getByText('Sede central')).toBeTruthy();
    expect(screen.getByText('Filial 1')).toBeTruthy();
    expect(screen.getByText('—')).toBeTruthy(); // observacao nula agora usa dash m-dash
    
    // Status badges
    expect(screen.getByText('Ativo')).toBeTruthy();
    expect(screen.getByText('Inativo')).toBeTruthy();
  });

  it('abre modal ao clicar em Adicionar Subgrupo', async () => {
    mockPossuiPermissao.mockImplementation((perm: string) => perm === 'apolices.subgrupos.inserir');
    renderComponent();
    
    const addButton = screen.getByText('Adicionar Subgrupo');
    fireEvent.click(addButton);
    
    await waitFor(() => {
      expect(screen.getByTestId('mock-modal-aberto')).toBeTruthy();
    });
  });

  it('mostra ações apenas se tiver permissões de alterar ou inativar', () => {
    mockUseApoliceSubgrupos.mockReturnValue({
      data: [
        {
          subgrupoPublicId: 'sub-1',
          nome: 'Matriz',
          ativo: true,
        }
      ],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    const { rerender } = renderComponent();
    // Sem permissões
    expect(screen.queryByText('Editar')).toBeNull();
    expect(screen.queryByText('Inativar')).toBeNull();

    // Com permissões
    mockPossuiPermissao.mockImplementation((perm: string) => 
      perm === 'apolices.subgrupos.alterar' || perm === 'apolices.subgrupos.inativar'
    );
    rerender(
      <SubgruposTab publicId="apolice-123" />
    );
    expect(screen.getByText('Editar')).toBeTruthy();
    expect(screen.getByText('Inativar')).toBeTruthy();
  });

  it('simula a inativação de um subgrupo ativo', async () => {
    mockPossuiPermissao.mockImplementation((perm: string) => perm === 'apolices.subgrupos.inativar');
    mockUseApoliceSubgrupos.mockReturnValue({
      data: [
        {
          subgrupoPublicId: 'sub-1',
          nome: 'Matriz',
          ativo: true,
        }
      ],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    
    // Clicar Inativar
    fireEvent.click(screen.getByText('Inativar'));

    // Modal de confirmação
    expect(screen.getByText('Inativar Subgrupo')).toBeTruthy();
    expect(screen.getByText(/Deseja inativar este Subgrupo/)).toBeTruthy();

    mockInativarApoliceSubgrupo.mockResolvedValueOnce(undefined);

    // Confirmar inativação (botão Inativar do modal)
    const inativarButtons = screen.getAllByText('Inativar').filter((el: any) => el.tagName === 'BUTTON');
    const confirmButton = inativarButtons[inativarButtons.length - 1]; // O último é o do modal
    fireEvent.click(confirmButton!);

    await waitFor(() => {
      expect(mockInativarApoliceSubgrupo).toHaveBeenCalledWith('apolice-123', 'sub-1');
      expect(mockRefetch).toHaveBeenCalled();
      // O dialog deve ter sumido (ou ao menos o título mudado/escondido)
      expect(screen.queryByText(/Deseja inativar este Subgrupo/)).toBeNull();
    });
  });
});
