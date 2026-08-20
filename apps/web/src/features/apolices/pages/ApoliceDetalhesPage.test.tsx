/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ApoliceDetalhesPage } from './ApoliceDetalhesPage';
import * as api from '../api/apolices.api';

vi.mock('../api/apolices.api', () => ({
  obterApolice: vi.fn(),
}));

vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn().mockReturnValue({
    possuiPermissao: vi.fn().mockReturnValue(true),
    possuiAcessoTotal: vi.fn().mockReturnValue(true),
  }),
  AuthorizationProvider: ({ children }: any) => <>{children}</>
}));

const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <MemoryRouter initialEntries={['/apolices/a-123']}>
    <Routes>
      <Route path="/apolices/:publicId" element={children} />
    </Routes>
  </MemoryRouter>
);

describe('ApoliceDetalhesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve exibir as informações da apolice no resumo', async () => {
    const mockData = {
      publicId: 'a-123',
      nome: '999123',
      numeroPrincipal: '999123',
      estipulanteNome: 'Estipulante Teste',
      seguradoraNome: 'Seguradora X',
      status: 'Ativa',
      ativo: true,
      ramos: [],
      configuracao: null
    };
    vi.mocked(api.obterApolice).mockResolvedValueOnce(mockData as any);

    render(<ApoliceDetalhesPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Apólice 999123')).not.toBeNull();
    });
    
    expect(screen.getByText('Estipulante Teste • Seguradora X')).not.toBeNull();
    expect(screen.getByText('Resumo')).not.toBeNull();
    expect(screen.getByText('Ramos')).not.toBeNull();
  });

  it('deve exibir mensagem de erro quando falhar', async () => {
    vi.mocked(api.obterApolice).mockRejectedValueOnce(new Error('Failed to fetch'));

    render(<ApoliceDetalhesPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText(/Erro ao carregar detalhes/i)).not.toBeNull();
    });
  });
});
