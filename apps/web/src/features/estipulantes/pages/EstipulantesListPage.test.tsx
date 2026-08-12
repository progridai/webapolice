/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { EstipulantesListPage } from './EstipulantesListPage';
import * as api from '../api/estipulantes.api';

vi.mock('../api/estipulantes.api', () => ({
  listarEstipulantes: vi.fn(),
  inativarEstipulante: vi.fn(),
  reativarEstipulante: vi.fn(),
}));

vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: vi.fn().mockReturnValue({
    possuiPermissao: vi.fn().mockReturnValue(true),
    possuiAcessoTotal: vi.fn().mockReturnValue(true),
  }),
  AuthorizationProvider: ({ children }: any) => <>{children}</>
}));

const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <MemoryRouter initialEntries={['/estipulantes']}>
    {children}
  </MemoryRouter>
);

describe('EstipulantesListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false, media: query, onchange: null,
      addListener: vi.fn(), removeListener: vi.fn(),
      addEventListener: vi.fn(), removeEventListener: vi.fn(), dispatchEvent: vi.fn(),
    })));
  });

  it('deve renderizar o titulo da pagina', () => {
    vi.mocked(api.listarEstipulantes).mockResolvedValueOnce({ itens: [], totalItens: 0, totalPaginas: 0, paginaAtual: 1, tamanhoPagina: 20 } as any);
    render(<EstipulantesListPage />, { wrapper: Wrapper });
    expect(screen.getByRole('heading', { name: 'Estipulantes' })).not.toBeNull();
  });

  it('deve exibir tabela com estipulantes quando dados forem carregados com sucesso', async () => {
    const mockData = {
      itens: [
        { publicId: 'e-1', razaoSocial: 'Estipulante Teste', nomeFantasia: 'Teste Fantasia', cnpj: '12345678000190', codigo: 'EST-123', ativo: true, dataCadastro: '2026-08-11T12:00:00Z' }
      ],
      paginaAtual: 1,
      tamanhoPagina: 20,
      totalItens: 1,
      totalPaginas: 1,
    };
    vi.mocked(api.listarEstipulantes).mockResolvedValueOnce(mockData as any);

    render(<EstipulantesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Estipulante Teste')).not.toBeNull();
    });
    
    expect(screen.getByText('12.345.678/0001-90')).not.toBeNull();
    expect(screen.getByText('EST-123')).not.toBeNull();
    expect(screen.getAllByText('Ativo').length).toBeGreaterThan(0);
    expect(screen.getByRole('table', { name: /lista de estipulantes/i })).not.toBeNull();
  });

  it('deve exibir estado vazio quando não houver estipulantes e sem filtros ativos', async () => {
    vi.mocked(api.listarEstipulantes).mockResolvedValueOnce({ itens: [], totalItens: 0, totalPaginas: 0, paginaAtual: 1, tamanhoPagina: 20 } as any);

    render(<EstipulantesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Nenhum estipulante cadastrado')).not.toBeNull();
    });
  });

  it('deve exibir mensagem de erro da api', async () => {
    vi.mocked(api.listarEstipulantes).mockRejectedValueOnce(new Error('Failed to fetch'));

    render(<EstipulantesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText(/Não foi possível carregar os estipulantes/i)).not.toBeNull();
    });
    expect(screen.getByRole('button', { name: /tentar novamente/i })).not.toBeNull();
  });

  it('deve alterar o filtro de busca ao digitar', async () => {
    vi.mocked(api.listarEstipulantes).mockResolvedValue({ itens: [], totalItens: 0, paginaAtual: 1, tamanhoPagina: 20, totalPaginas: 0 } as any);
    render(<EstipulantesListPage />, { wrapper: Wrapper });

    const input = screen.getByPlaceholderText(/Razão Social, CNPJ ou Código/i);
    
    fireEvent.change(input, { target: { value: 'teste' } });
    
    expect(api.listarEstipulantes).toHaveBeenCalledTimes(1);
    
    await waitFor(() => {
      expect(api.listarEstipulantes).toHaveBeenCalledTimes(2);
    });
  });
});
