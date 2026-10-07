import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ModuloApoliceModal } from './ModuloApoliceModal';
import { criarApoliceModulo } from '../../api/apolices.api';
import { modulosGlobaisApi } from '../../api/modulosGlobais.api';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';

vi.mock('../../api/apolices.api');
vi.mock('../../api/modulosGlobais.api');
vi.mock('../../../../auth/AuthorizationProvider');
vi.mock('../PlanoModuloEditor', () => ({
  PlanoModuloEditor: ({ moduloPublicId }: { moduloPublicId: string }) => (
    <div>Plano do vínculo {moduloPublicId}</div>
  ),
}));
const globalId = '8d198830-f45d-4236-8b37-80cfac9d4b89';

describe('Continuação do cadastro após vincular Módulo', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useAuthorization).mockReturnValue({
      possuiPermissao: () => true,
    } as unknown as ReturnType<typeof useAuthorization>);
    vi.mocked(modulosGlobaisApi.listar).mockResolvedValue({
      items: [{ publicId: globalId, nome: 'Funeral', ativo: true }],
      totalCount: 1,
    } as Awaited<ReturnType<typeof modulosGlobaisApi.listar>>);
    vi.mocked(criarApoliceModulo).mockResolvedValue({
      publicId: 'uuid-do-vinculo',
    });
  });
  it('mantém o modal aberto e usa o UUID retornado para cadastrar o Plano exclusivo', async () => {
    const onClose = vi.fn();
    const onSucesso = vi.fn();
    render(
      <ModuloApoliceModal
        aberto
        onClose={onClose}
        onSucesso={onSucesso}
        apolicePublicId="apolice"
        modulosVinculados={[]}
      />,
    );
    await screen.findByRole('option', { name: 'Funeral' });
    fireEvent.change(screen.getByLabelText('Módulo'), {
      target: { value: globalId },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar vínculo e continuar' }),
    );
    await waitFor(() =>
      expect(criarApoliceModulo).toHaveBeenCalledWith('apolice', {
        moduloPublicId: globalId,
        dataInicio: null,
        dataFim: null,
        observacao: null,
      }),
    );
    expect(
      await screen.findByText('Plano do vínculo uuid-do-vinculo'),
    ).toBeTruthy();
    expect(onClose).not.toHaveBeenCalled();
    expect(onSucesso).toHaveBeenCalled();
  });
});
