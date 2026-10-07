import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { CatalogoFormPage } from './CatalogoFormPage';
import { PLANOS } from '../catalogos.config';
import { catalogosApi } from '../api/catalogos.api';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

vi.mock('../api/catalogos.api');
vi.mock('../../../auth/AuthorizationProvider');

const plano = {
  publicId: 'plano-id',
  nome: 'Plano Teste',
  ativo: true,
  createdAt: '',
  updatedAt: '',
};
const cobertura = {
  publicId: 'cobertura-id',
  nome: 'Morte acidental',
  ativo: true,
  createdAt: '',
  updatedAt: '',
};
const permitir = (codigo: string) =>
  [
    'planos.coberturas.alterar',
    'coberturas.visualizar',
    'coberturas.inserir',
  ].includes(codigo);

function abrir(path: string) {
  render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route
          path="/planos/novo"
          element={<CatalogoFormPage config={PLANOS} />}
        />
        <Route
          path="/planos/:publicId/editar"
          element={<CatalogoFormPage config={PLANOS} />}
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe('Coberturas no cadastro de Plano', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useAuthorization).mockReturnValue({
      possuiPermissao: permitir,
    } as ReturnType<typeof useAuthorization>);
    vi.mocked(catalogosApi.obter).mockResolvedValue(plano);
    vi.mocked(catalogosApi.salvar).mockResolvedValue(plano);
    vi.mocked(catalogosApi.coberturasPlano).mockResolvedValue([]);
    vi.mocked(catalogosApi.listar).mockResolvedValue({
      items: [cobertura],
      totalCount: 1,
    });
    vi.mocked(catalogosApi.vincular).mockResolvedValue(undefined);
    vi.mocked(catalogosApi.salvarPremiosPlano).mockResolvedValue(undefined);
  });

  it('permite vincular uma Cobertura na edição sem reenviar o cadastro do Plano', async () => {
    abrir('/planos/plano-id/editar');
    const select = await screen.findByLabelText('Adicionar cobertura');
    await screen.findByRole('option', { name: 'Morte acidental' });
    fireEvent.change(select, { target: { value: cobertura.publicId } });
    fireEvent.click(screen.getByRole('button', { name: 'Vincular' }));
    fireEvent.change(await screen.findByLabelText('Prêmio titular (R$)'), {
      target: { value: '12,34' },
    });
    fireEvent.change(screen.getByLabelText('Prêmio cônjuge (R$)'), {
      target: { value: '0' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar prêmios' }));
    await waitFor(() =>
      expect(catalogosApi.salvarPremiosPlano).toHaveBeenCalledWith(
        plano.publicId,
        cobertura.publicId,
        { premioTitular: 12.34, premioConjuge: 0 },
        true,
      ),
    );
    expect(catalogosApi.salvar).not.toHaveBeenCalled();
  });

  it('abre a etapa de vínculos depois de salvar um novo Plano', async () => {
    abrir('/planos/novo');
    fireEvent.change(await screen.findByLabelText('Nome'), {
      target: { value: 'Plano Novo' },
    });
    expect(screen.getByText(/Salve o Plano para escolher/)).toBeTruthy();
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar e vincular coberturas' }),
    );
    await screen.findByLabelText('Adicionar cobertura');
    expect(catalogosApi.salvar).toHaveBeenCalledWith(
      'planos',
      undefined,
      expect.objectContaining({ nome: 'Plano Novo' }),
    );
    expect(catalogosApi.coberturasPlano).toHaveBeenCalledWith(
      plano.publicId,
      expect.any(AbortSignal),
    );
  });

  it('informa quando não existe Cobertura ativa para vincular', async () => {
    vi.mocked(catalogosApi.listar).mockResolvedValue({
      items: [],
      totalCount: 0,
    });
    abrir('/planos/plano-id/editar');
    await screen.findByText('Nenhuma Cobertura ativa cadastrada');
    expect(
      screen
        .getByRole('link', { name: 'Cadastrar Cobertura' })
        .getAttribute('href'),
    ).toBe('/coberturas/novo');
  });
});
