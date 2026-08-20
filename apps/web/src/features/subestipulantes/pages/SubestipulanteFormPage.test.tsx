/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { SubestipulanteFormPage } from './SubestipulanteFormPage';
import * as api from '../api/subestipulantes.api';

vi.mock('../api/subestipulantes.api', () => ({
  subestipulantesApi: {
    obter: vi.fn(),
    criar: vi.fn(),
    alterar: vi.fn(),
  },
}));

vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn().mockReturnValue({
    possuiPermissao: vi.fn().mockReturnValue(true),
    possuiAcessoTotal: vi.fn().mockReturnValue(true),
  }),
  AuthorizationProvider: ({ children }: any) => <>{children}</>,
}));

describe('SubestipulanteFormPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar o formulário no modo de criação', () => {
    render(
      <MemoryRouter initialEntries={['/subestipulantes/novo']}>
        <Routes>
          <Route path="/subestipulantes/novo" element={<SubestipulanteFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByRole('heading', { name: 'Novo Subestipulante' })).toBeTruthy();
    expect(screen.getByLabelText(/Nome \/ Razão Social/i)).toBeTruthy();
    expect(screen.getByLabelText(/CNPJ/i)).toBeTruthy();
    expect(screen.getByLabelText(/Código Interno/i)).toBeTruthy();
    expect(screen.getByLabelText(/Observações Internas/i)).toBeTruthy();
  });

  it('deve exibir validação de erro ao submeter com nome vazio', async () => {
    render(
      <MemoryRouter initialEntries={['/subestipulantes/novo']}>
        <Routes>
          <Route path="/subestipulantes/novo" element={<SubestipulanteFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    const salvarBtn = screen.getByRole('button', { name: 'Salvar Subestipulante' });
    fireEvent.click(salvarBtn);

    await waitFor(() => {
      expect(screen.getByText('O nome ou razão social é obrigatório.')).toBeTruthy();
    });
  });

  it('deve submeter com sucesso ao preencher formulário válido', async () => {
    vi.mocked(api.subestipulantesApi.criar).mockResolvedValueOnce({ publicId: 'sub-1' });

    render(
      <MemoryRouter initialEntries={['/subestipulantes/novo']}>
        <Routes>
          <Route path="/subestipulantes/novo" element={<SubestipulanteFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/Nome \/ Razão Social/i), {
      target: { value: 'Subestipulante Teste' },
    });
    fireEvent.change(screen.getByLabelText(/CNPJ/i), {
      target: { value: '92.682.038/0001-00' },
    });
    fireEvent.change(screen.getByLabelText(/Código Interno/i), {
      target: { value: 'SUB-01' },
    });

    const salvarBtn = screen.getByRole('button', { name: 'Salvar Subestipulante' });
    fireEvent.click(salvarBtn);

    await waitFor(() => {
      expect(api.subestipulantesApi.criar).toHaveBeenCalledWith({
        nome: 'Subestipulante Teste',
        cnpj: '92.682.038/0001-00',
        codigo: 'SUB-01',
        observacao: undefined,
      });
    });
  });

  it('deve carregar dados no modo de edição', async () => {
    vi.mocked(api.subestipulantesApi.obter).mockResolvedValueOnce({
      publicId: 'sub-1',
      nome: 'Subestipulante Existente',
      cnpj: '11.222.333/0001-44',
      codigo: 'SUB-123',
      observacao: 'Anotação',
      ativo: true,
      createdAt: '2024-01-01',
      updatedAt: '2024-01-02',
    });

    render(
      <MemoryRouter initialEntries={['/subestipulantes/sub-1/editar']}>
        <Routes>
          <Route path="/subestipulantes/:publicId/editar" element={<SubestipulanteFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByLabelText(/Carregando subestipulante/i)).toBeTruthy();

    await waitFor(() => {
      expect(api.subestipulantesApi.obter).toHaveBeenCalledWith('sub-1');
      expect(screen.getByRole('heading', { name: 'Editar Subestipulante' })).toBeTruthy();
    });

    expect((screen.getByLabelText(/Nome \/ Razão Social/i) as HTMLInputElement).value).toBe('Subestipulante Existente');
    expect((screen.getByLabelText(/CNPJ/i) as HTMLInputElement).value).toBe('11.222.333/0001-44');
    expect((screen.getByLabelText(/Código Interno/i) as HTMLInputElement).value).toBe('SUB-123');
    expect((screen.getByLabelText(/Observações Internas/i) as HTMLTextAreaElement).value).toBe('Anotação');
  });
});
