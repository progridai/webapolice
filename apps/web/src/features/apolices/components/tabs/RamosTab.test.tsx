import { render, screen, fireEvent, within } from '@testing-library/react';
import { RamosTab } from './RamosTab';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { inativarRamoApolice } from '../../api/apolices.api';

vi.mock('../../api/apolices.api', () => ({ inativarRamoApolice: vi.fn() }));

vi.mock('../../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn(),
}));

vi.mock('./ApoliceRamoFormModal', () => ({
  ApoliceRamoFormModal: ({ aberto }: { aberto: boolean }) =>
    aberto ? <div data-testid="ramo-modal">Modal</div> : null,
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
    },
  ],
};

describe('RamosTab', () => {
  const mockPossuiPermissao = vi.fn();

  beforeEach(() => {
    (useAuthorization as ReturnType<typeof vi.fn>).mockReturnValue({
      possuiPermissao: mockPossuiPermissao,
    });
    mockPossuiPermissao.mockReset();
  });

  it('deve renderizar a lista de ramos corretamente', () => {
    mockPossuiPermissao.mockReturnValue(false); // Apenas visualização
    render(<RamosTab apolice={mockApolice} />);

    expect(screen.getByText(/Vida em Grupo/i)).toBeTruthy();
    expect(screen.getByText('(VG)')).toBeTruthy();
    expect(screen.getByText('1234')).toBeTruthy();
    expect(screen.getByText('7.38%')).toBeTruthy();

    expect(screen.getByText(/Acidentes Pessoais/i)).toBeTruthy();
    expect(screen.getByText('Inativo')).toBeTruthy();
  });

  it('não deve exibir botões de ação se usuário não tem permissão', () => {
    mockPossuiPermissao.mockReturnValue(false);
    render(<RamosTab apolice={mockApolice} />);

    expect(
      screen.queryByRole('button', { name: /Adicionar Ramo/i }),
    ).toBeNull();
    expect(screen.queryByRole('button', { name: /Editar/i })).toBeNull();
    expect(screen.queryByRole('button', { name: /Inativar/i })).toBeNull();
  });

  it('deve exibir botão Adicionar se tiver permissão e abrir modal', () => {
    mockPossuiPermissao.mockImplementation(
      (perm) => perm === 'apolices.ramos.inserir',
    );
    render(<RamosTab apolice={mockApolice} />);

    const btn = screen.getByRole('button', { name: /Adicionar Ramo/i });
    expect(btn).toBeTruthy();

    fireEvent.click(btn);
    expect(screen.getByTestId('ramo-modal')).toBeTruthy();
  });

  it('deve exibir Editar e Inativar apenas em ramos ativos e se tiver permissão', () => {
    mockPossuiPermissao.mockImplementation((perm) =>
      ['apolices.ramos.alterar', 'apolices.ramos.inativar'].includes(perm),
    );
    render(<RamosTab apolice={mockApolice} />);

    // O ramo Vida em Grupo (ativo) deve ter os botões
    const editBtns = screen.getAllByRole('button', { name: /Editar/i });
    const inativarBtns = screen.getAllByRole('button', { name: /Inativar/i });

    expect(editBtns).toHaveLength(1); // Só tem 1 ramo ativo
    expect(inativarBtns).toHaveLength(1);
  });

  it.each([true, false])(
    'exibe feedback semântico após confirmar inativação (sucesso: %s)',
    async (success) => {
      mockPossuiPermissao.mockImplementation(
        (perm) => perm === 'apolices.ramos.inativar',
      );
      const onMutateSucesso = vi.fn();
      vi.mocked(inativarRamoApolice).mockReset();
      if (success) vi.mocked(inativarRamoApolice).mockResolvedValue(undefined);
      else
        vi.mocked(inativarRamoApolice).mockRejectedValue(
          new Error('Falha temporária'),
        );
      render(
        <RamosTab apolice={mockApolice} onMutateSucesso={onMutateSucesso} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Inativar' }));
      expect(inativarRamoApolice).not.toHaveBeenCalled();
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Inativar Vínculo',
        }),
      );

      const message = await screen.findByText(
        success ? 'Vínculo inativado com sucesso.' : 'Falha temporária',
      );
      const feedback = message.closest('[role]');
      expect(feedback?.getAttribute('role')).toBe(success ? 'status' : 'alert');
      expect(feedback?.className).toContain(
        success ? 'alert-success' : 'alert-error',
      );
      expect(inativarRamoApolice).toHaveBeenCalledExactlyOnceWith(
        'apolice-1',
        'ramo-1',
      );
      expect(onMutateSucesso).toHaveBeenCalledTimes(success ? 1 : 0);
    },
  );
});
