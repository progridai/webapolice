/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { SubestipulantesListPage } from './SubestipulantesListPage';
import * as api from '../api/subestipulantes.api';
import type { SubestipulanteListItem, PagedResult } from '../types/subestipulante.types';

vi.mock('../api/subestipulantes.api', () => ({
  subestipulantesApi: {
    listar: vi.fn(),
    inativar: vi.fn(),
    reativar: vi.fn(),
  },
}));

vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn().mockReturnValue({
    possuiPermissao: vi.fn().mockReturnValue(true),
    possuiAcessoTotal: vi.fn().mockReturnValue(true),
  }),
  AuthorizationProvider: ({ children }: any) => <>{children}</>,
}));

const mockData: PagedResult<SubestipulanteListItem> = {
  itens: [
    {
      publicId: 'sub-1',
      nome: 'Subestipulante Alpha',
      cnpj: '11.111.111/0001-11',
      codigo: 'A01',
      ativo: true,
      createdAt: '2024-01-01T00:00:00Z',
    },
    {
      publicId: 'sub-2',
      nome: 'Subestipulante Beta',
      cnpj: '22.222.222/0001-22',
      codigo: 'B02',
      ativo: false,
      createdAt: '2024-01-02T00:00:00Z',
    },
  ],
  paginaAtual: 1,
  tamanhoPagina: 10,
  totalItens: 2,
  totalPaginas: 1,
};

describe('SubestipulantesListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve listar subestipulantes corretamente', async () => {
    vi.mocked(api.subestipulantesApi.listar).mockResolvedValueOnce(mockData);

    render(
      <MemoryRouter initialEntries={['/subestipulantes']}>
        <Routes>
          <Route path="/subestipulantes" element={<SubestipulantesListPage />} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Cadastro de Subestipulantes')).toBeTruthy();

    await waitFor(() => {
      expect(api.subestipulantesApi.listar).toHaveBeenCalledWith({
        pagina: 1,
        tamanhoPagina: 10,
        busca: undefined,
        ativo: undefined,
      });
    });

    expect(screen.getByText('Subestipulante Alpha')).toBeTruthy();
    expect(screen.getByText('11.111.111/0001-11')).toBeTruthy();
    expect(screen.getByText('Subestipulante Beta')).toBeTruthy();
  });

  it('deve realizar busca ao preencher filtro e limpar corretamente', async () => {
    vi.mocked(api.subestipulantesApi.listar).mockResolvedValue(mockData);

    render(
      <MemoryRouter initialEntries={['/subestipulantes']}>
        <Routes>
          <Route path="/subestipulantes" element={<SubestipulantesListPage />} />
        </Routes>
      </MemoryRouter>
    );

    const buscaInput = screen.getByLabelText(/Buscar subestipulante/i);
    fireEvent.change(buscaInput, { target: { value: 'Alpha' } });

    await waitFor(() => {
      expect(api.subestipulantesApi.listar).toHaveBeenCalledWith(
        expect.objectContaining({ busca: 'Alpha', pagina: 1 })
      );
    });

    const limparBtn = screen.getByRole('button', { name: 'Limpar' });
    fireEvent.click(limparBtn);

    await waitFor(() => {
      expect(api.subestipulantesApi.listar).toHaveBeenCalledWith(
        expect.objectContaining({ busca: undefined, ativo: undefined, pagina: 1 })
      );
    });
  });

  it('deve chamar modal de inativar ao clicar em inativar', async () => {
    vi.mocked(api.subestipulantesApi.listar).mockResolvedValueOnce(mockData);
    vi.mocked(api.subestipulantesApi.inativar).mockResolvedValueOnce();

    render(
      <MemoryRouter initialEntries={['/subestipulantes']}>
        <Routes>
          <Route path="/subestipulantes" element={<SubestipulantesListPage />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Subestipulante Alpha')).toBeTruthy();
    });

    const menuBtns = screen.getAllByRole('button', { name: /Mais ações/i });
    fireEvent.click(menuBtns[0]); // abre dropdown do Alpha

    const inativarBtn = screen.getByText('Inativar');
    fireEvent.click(inativarBtn);

    expect(screen.getByText(/Tem certeza que deseja inativar o subestipulante/i)).toBeTruthy();

    const confirmarBtn = screen.getByRole('button', { name: 'Sim, inativar' });
    fireEvent.click(confirmarBtn);

    await waitFor(() => {
      expect(api.subestipulantesApi.inativar).toHaveBeenCalledWith('sub-1');
      expect(api.subestipulantesApi.listar).toHaveBeenCalledTimes(2); // Recarregou lista
    });
  });
});
