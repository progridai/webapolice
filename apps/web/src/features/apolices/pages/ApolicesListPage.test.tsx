/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { ApolicesListPage } from './ApolicesListPage';
import * as api from '../api/apolices.api';

vi.mock('../api/apolices.api', () => ({
  listarApolices: vi.fn(),
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
  <MemoryRouter initialEntries={['/apolices']}>
    {children}
  </MemoryRouter>
);

describe('ApolicesListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false, media: query, onchange: null,
      addListener: vi.fn(), removeListener: vi.fn(),
      addEventListener: vi.fn(), removeEventListener: vi.fn(), dispatchEvent: vi.fn(),
    })));
  });

  it('deve renderizar o titulo da pagina', () => {
    vi.mocked(api.listarApolices).mockResolvedValueOnce({ items: [], totalCount: 0, page: 1, pageSize: 20 } as any);
    render(<ApolicesListPage />, { wrapper: Wrapper });
    expect(screen.getByRole('heading', { name: 'Apólices' })).not.toBeNull();
  });

  it('deve exibir tabela com apolices quando dados forem carregados com sucesso', async () => {
    const mockData = {
      items: [
        { 
          publicId: 'a-1', 
          numeroPrincipal: '999123', 
          estipulanteNome: 'Estipulante Teste', 
          seguradoraNome: 'Seguradora X', 
          dataInicioVigencia: '2026-08-11T12:00:00Z', 
          dataFimVigencia: '2027-08-11T12:00:00Z',
          status: 'Ativa', 
          ativo: true, 
          quantidadeRamos: 1, 
          resumoRamos: 'VG'
        }
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1
    };
    vi.mocked(api.listarApolices).mockResolvedValueOnce(mockData as any);

    render(<ApolicesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Estipulante Teste')).not.toBeNull();
    });
    
    expect(screen.getByText('999123')).not.toBeNull();
    expect(screen.getByText('Seguradora X')).not.toBeNull();
    expect(screen.getAllByText('Ativa').length).toBeGreaterThan(0);
    expect(screen.getByRole('table', { name: /lista de apólices/i })).not.toBeNull();
  });

  it('deve exibir estado vazio quando não houver apolices', async () => {
    vi.mocked(api.listarApolices).mockResolvedValueOnce({ items: [], totalCount: 0, page: 1, pageSize: 20 } as any);

    render(<ApolicesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Nenhuma apólice cadastrada')).not.toBeNull();
    });
  });
});
