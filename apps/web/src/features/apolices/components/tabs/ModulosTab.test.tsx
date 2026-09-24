import { vi } from 'vitest';
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ModulosTab } from './ModulosTab';
import { useApoliceModulos } from '../../hooks/useApoliceModulos';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { inativarApoliceModulo } from '../../api/apolices.api';

// ── Mocks ─────────────────────────────────────────────────────────────────────
vi.mock('../../hooks/useApoliceModulos');
vi.mock('../../../../auth/AuthorizationProvider');
vi.mock('../../api/apolices.api');

vi.mock('../modals/ModuloApoliceModal', () => ({
  ModuloApoliceModal: ({ aberto }: { aberto: boolean }) =>
    aberto ? <div data-testid="mock-modal-aberto">MODAL ABERTO</div> : null,
}));

const mockUseApoliceModulos = useApoliceModulos as any;
const mockUseAuthorization = useAuthorization as any;
const mockInativarApoliceModulo = inativarApoliceModulo as any;

// ── Fixtures ──────────────────────────────────────────────────────────────────
// Distinção explícita: publicId = apoliceModuloPublicId, moduloPublicId = cadastro.modulo
const moduloAtivo = {
  publicId: 'vinculo-uuid-ativo',         // apoliceModuloPublicId (seguro.apolice_modulo)
  moduloPublicId: 'modulo-global-uuid-1', // publicId do cadastro.modulo
  nome: 'Módulo Funeral',
  descricao: 'Cobertura funeral familiar',
  dataInicio: '2025-01-01T00:00:00',
  dataFim: '2025-12-31T00:00:00',
  observacao: 'Obs teste',
  ativo: true,
};

const moduloInativo = {
  publicId: 'vinculo-uuid-inativo',        // apoliceModuloPublicId
  moduloPublicId: 'modulo-global-uuid-2',  // publicId do cadastro.modulo
  nome: 'Módulo Odonto',
  descricao: undefined,
  dataInicio: undefined,
  dataFim: undefined,
  observacao: undefined,
  ativo: false,
};

const renderComponent = () => render(<ModulosTab publicId="apolice-abc-123" />);

// ── Testes ────────────────────────────────────────────────────────────────────
describe('ModulosTab', () => {
  const mockPossuiPermissao = vi.fn();
  const mockRefetch = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAuthorization.mockReturnValue({ possuiPermissao: mockPossuiPermissao });

    mockUseApoliceModulos.mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    // Padrão: sem permissões de mutação
    mockPossuiPermissao.mockReturnValue(false);
  });

  // ── Estado vazio ────────────────────────────────────────────────────────────
  it('renderiza estado vazio sem botão quando sem permissão de inserir', () => {
    renderComponent();
    expect(screen.getByText('Nenhum módulo vinculado')).toBeTruthy();
    expect(screen.queryByText('Vincular Módulo')).toBeNull();
  });

  it('renderiza estado vazio COM botão quando possui permissão de inserir', () => {
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.modulos.inserir'
    );
    renderComponent();
    expect(screen.getByText('Nenhum módulo vinculado')).toBeTruthy();
    // Botão no header e no EmptyState
    expect(screen.getAllByText('Vincular Módulo').length).toBeGreaterThanOrEqual(1);
  });

  // ── Renderização da tabela ──────────────────────────────────────────────────
  it('renderiza tabela com módulo ativo e inativo', () => {
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo, moduloInativo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();

    expect(screen.getByText('Módulo Funeral')).toBeTruthy();
    expect(screen.getByText('Cobertura funeral familiar')).toBeTruthy();
    expect(screen.getByText('Módulo Odonto')).toBeTruthy();
    expect(screen.getByText('Ativo')).toBeTruthy();
    expect(screen.getByText('Inativo')).toBeTruthy();
  });

  // ── Permissões de ações ─────────────────────────────────────────────────────
  it('não exibe ações sem permissões de alterar ou inativar', () => {
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    expect(screen.queryByText('Editar')).toBeNull();
    expect(screen.queryByText('Inativar')).toBeNull();
  });

  it('exibe botão Editar apenas com permissão apolices.modulos.alterar', () => {
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.modulos.alterar'
    );
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    expect(screen.getByText('Editar')).toBeTruthy();
    expect(screen.queryByText('Inativar')).toBeNull();
  });

  it('exibe botão Inativar apenas com permissão apolices.modulos.inativar', () => {
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.modulos.inativar'
    );
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    expect(screen.queryByText('Editar')).toBeNull();
    expect(screen.getByText('Inativar')).toBeTruthy();
  });

  it('não exibe Editar/Inativar para módulo inativo mesmo com permissões', () => {
    mockPossuiPermissao.mockImplementation(() => true);
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloInativo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    expect(screen.queryByText('Editar')).toBeNull();
    expect(screen.queryByText('Inativar')).toBeNull();
  });

  // ── Abertura do modal ───────────────────────────────────────────────────────
  it('abre modal ao clicar em Vincular Módulo (com permissão de inserir)', async () => {
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.modulos.inserir'
    );
    renderComponent();

    fireEvent.click(screen.getAllByText('Vincular Módulo')[0]);

    await waitFor(() => {
      expect(screen.getByTestId('mock-modal-aberto')).toBeTruthy();
    });
  });

  it('abre modal ao clicar em Editar (com permissão de alterar)', async () => {
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.modulos.alterar'
    );
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    fireEvent.click(screen.getByText('Editar'));

    await waitFor(() => {
      expect(screen.getByTestId('mock-modal-aberto')).toBeTruthy();
    });
  });

  // ── Inativação ──────────────────────────────────────────────────────────────
  // Valida que inativarApoliceModulo é chamado com o publicId do VÍNCULO
  // (apoliceModuloPublicId = 'vinculo-uuid-ativo'), NÃO com moduloPublicId do cadastro
  it('chama inativarApoliceModulo com apoliceModuloPublicId (publicId do vínculo)', async () => {
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.modulos.inativar'
    );
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();

    fireEvent.click(screen.getByText('Inativar'));
    expect(screen.getByText('Inativar Módulo')).toBeTruthy();

    mockInativarApoliceModulo.mockResolvedValueOnce(undefined);

    const botoesInativar = screen
      .getAllByText('Inativar')
      .filter((el: any) => el.tagName === 'BUTTON');
    const confirmButton = botoesInativar[botoesInativar.length - 1];
    fireEvent.click(confirmButton!);

    await waitFor(() => {
      // Deve usar publicId do VÍNCULO ('vinculo-uuid-ativo'), nunca moduloPublicId
      expect(mockInativarApoliceModulo).toHaveBeenCalledWith(
        'apolice-abc-123',
        'vinculo-uuid-ativo'  // apoliceModuloPublicId
      );
      // Nunca deve usar o moduloPublicId do cadastro global nesta chamada
      expect(mockInativarApoliceModulo).not.toHaveBeenCalledWith(
        'apolice-abc-123',
        'modulo-global-uuid-1'  // moduloPublicId — ERRADO para inativação
      );
      expect(mockRefetch).toHaveBeenCalled();
    });
  });

  it('exibe erro de inativação e mantém modal fechado', async () => {
    mockPossuiPermissao.mockImplementation(
      (perm: string) => perm === 'apolices.modulos.inativar'
    );
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    fireEvent.click(screen.getByText('Inativar'));

    mockInativarApoliceModulo.mockRejectedValueOnce(
      Object.assign(new Error('Erro de API'), {
        response: { data: { detail: 'Vínculo não encontrado.' } },
      })
    );

    const botoesInativar = screen
      .getAllByText('Inativar')
      .filter((el: any) => el.tagName === 'BUTTON');
    fireEvent.click(botoesInativar[botoesInativar.length - 1]!);

    await waitFor(() => {
      expect(screen.getByText('Vínculo não encontrado.')).toBeTruthy();
      expect(mockRefetch).not.toHaveBeenCalled();
    });
  });

  // ── Erro ao carregar ────────────────────────────────────────────────────────
  it('exibe mensagem de erro quando o hook retorna erro', () => {
    mockUseApoliceModulos.mockReturnValue({
      data: [],
      isLoading: false,
      error: new Error('Falha de rede'),
      refetch: mockRefetch,
    });

    renderComponent();
    expect(screen.getByText('Erro ao carregar Módulos')).toBeTruthy();
    expect(screen.getByText('Falha de rede')).toBeTruthy();
  });

  // ── Isolamento de permissões ────────────────────────────────────────────────
  // Garante que permissões de Subestipulantes não afetam este componente
  it('não usa permissões de subestipulantes', () => {
    mockPossuiPermissao.mockImplementation((perm: string) => {
      // Simulando um usuário que tem permissão de subestipulantes mas NÃO de módulos
      return perm.startsWith('apolices.subestipulantes');
    });
    mockUseApoliceModulos.mockReturnValue({
      data: [moduloAtivo],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderComponent();
    // Nenhuma ação deve aparecer
    expect(screen.queryByText('Vincular Módulo')).toBeNull();
    expect(screen.queryByText('Editar')).toBeNull();
    expect(screen.queryByText('Inativar')).toBeNull();
  });
});
