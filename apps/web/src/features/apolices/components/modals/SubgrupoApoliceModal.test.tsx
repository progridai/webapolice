import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { SubgrupoApoliceModal } from './SubgrupoApoliceModal';
import {
  criarApoliceSubgrupo,
  alterarApoliceSubgrupo,
} from '../../api/apolices.api';
import { catalogosApi } from '../../../cadastros-seguro/api/catalogos.api';
import type { ApoliceSubgrupoResult } from '../../types/apolice.types';

vi.mock('../../api/apolices.api');
vi.mock('../../../cadastros-seguro/api/catalogos.api', () => ({
  catalogosApi: { conveniosSubgrupo: vi.fn() },
}));

const convenioId = '81d1b3d1-eba7-4705-a662-03bfce8b1b08';
function abrir(subgrupoEdicao?: ApoliceSubgrupoResult) {
  render(
    <SubgrupoApoliceModal
      aberto
      apolicePublicId="apolice"
      onClose={vi.fn()}
      onSucesso={vi.fn()}
      subgrupoEdicao={subgrupoEdicao}
    />,
  );
}
async function salvar() {
  await waitFor(() =>
    expect((screen.getByText('Salvar') as HTMLButtonElement).disabled).toBe(
      false,
    ),
  );
  fireEvent.click(screen.getByText('Salvar'));
}

describe('Convênio do Subgrupo', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(catalogosApi.conveniosSubgrupo).mockResolvedValue([]);
    vi.mocked(criarApoliceSubgrupo).mockResolvedValue('novo');
    vi.mocked(alterarApoliceSubgrupo).mockResolvedValue(undefined);
  });

  it('exige escolher um Convênio para novo Subgrupo', async () => {
    abrir();
    fireEvent.change(screen.getByLabelText('Nome'), {
      target: { value: 'Matriz' },
    });
    await salvar();
    await screen.findByText('Selecione um Convênio de Cobrança.');
    expect(criarApoliceSubgrupo).not.toHaveBeenCalled();
  });

  it('permite editar Subgrupo legado ainda sem Convênio', async () => {
    abrir({ subgrupoPublicId: 'subgrupo', nome: 'Anterior', ativo: true });
    await salvar();
    await waitFor(() =>
      expect(alterarApoliceSubgrupo).toHaveBeenCalledWith(
        'apolice',
        'subgrupo',
        expect.objectContaining({ convenioCobrancaPublicId: null }),
      ),
    );
  });

  it('mantém o Convênio inativo já vinculado na edição', async () => {
    abrir({
      subgrupoPublicId: 'subgrupo',
      nome: 'Anterior',
      ativo: true,
      convenioCobrancaPublicId: convenioId,
      convenioCobrancaNome: 'Convênio anterior',
      convenioCobrancaAtivo: false,
    });
    expect(screen.getByText('Convênio anterior (inativo)')).toBeTruthy();
    await salvar();
    await waitFor(() =>
      expect(alterarApoliceSubgrupo).toHaveBeenCalledWith(
        'apolice',
        'subgrupo',
        expect.objectContaining({ convenioCobrancaPublicId: convenioId }),
      ),
    );
  });
});
