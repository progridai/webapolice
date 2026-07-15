/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { ClientesListPage } from './ClientesListPage';
import * as api from '../api/clientesApi';

vi.mock('../api/clientesApi', () => ({
  listarClientes: vi.fn(),
}));

const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <MemoryRouter initialEntries={['/clientes']}>
    {children}
  </MemoryRouter>
);

describe('ClientesListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false, media: query, onchange: null,
      addListener: vi.fn(), removeListener: vi.fn(),
      addEventListener: vi.fn(), removeEventListener: vi.fn(), dispatchEvent: vi.fn(),
    })));
  });

  it('deve exibir carregamento (skeletons) no início', () => {
    vi.mocked(api.listarClientes).mockImplementation(
      (_, signal) =>
        new Promise((_, reject) => {
          if (signal?.aborted) {
            reject(new DOMException('Aborted', 'AbortError'));
          } else {
            signal?.addEventListener('abort', () => reject(new DOMException('Aborted', 'AbortError')));
          }
        })
    );
    
    render(<ClientesListPage />, { wrapper: Wrapper });
    
    expect(screen.getByRole('heading', { name: 'Clientes' })).not.toBeNull();
    // Como os Skeletons não tem role fácil, verificamos o aria-busy no container
    expect(document.querySelector('[aria-busy="true"]')).not.toBeNull();
  });

  it('deve exibir tabela com clientes quando dados forem carregados com sucesso', async () => {
    const mockData = {
      itens: [
        { id: 1, nome: 'João da Silva', cpfMascarado: '111.123.456-11', status: 'ativo', dataCadastroUtc: '2026-07-04T12:00:00Z' }
      ],
      paginaAtual: 1,
      tamanhoPagina: 20,
      totalItens: 1,
      totalPaginas: 1,
    };
    vi.mocked(api.listarClientes).mockResolvedValueOnce(mockData as any);

    render(<ClientesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('João da Silva')).not.toBeNull();
    });
    
    expect(screen.getByText('111.123.456-11')).not.toBeNull();
    expect(screen.getByText('Ativo')).not.toBeNull();
    // Deve ter a tabela renderizada
    expect(screen.getByRole('table', { name: /lista de clientes/i })).not.toBeNull();
  });

  it('deve exibir estado vazio quando não houver clientes e sem filtros ativos', async () => {
    vi.mocked(api.listarClientes).mockResolvedValueOnce({ itens: [], totalItens: 0, totalPaginas: 0 } as any);

    render(<ClientesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Nenhum cliente cadastrado')).not.toBeNull();
    });
  });

  it('deve exibir mensagem segura quando houver erro de rede', async () => {
    vi.mocked(api.listarClientes).mockRejectedValueOnce(new Error('Failed to fetch'));

    render(<ClientesListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText(/Não foi possível carregar os clientes/i)).not.toBeNull();
      expect(screen.getByText(/Não foi possível conectar ao servidor/i)).not.toBeNull();
    });

    expect(screen.queryByText('Failed to fetch')).toBeNull();
    expect(screen.getByRole('button', { name: /tentar novamente/i })).not.toBeNull();
  });

  it('deve alterar o filtro de busca ao digitar (debounce)', async () => {
    vi.mocked(api.listarClientes).mockResolvedValue({ itens: [], totalItens: 0 } as any);
    render(<ClientesListPage />, { wrapper: Wrapper });

    const input = screen.getByPlaceholderText(/Nome ou CPF/i);
    
    fireEvent.change(input, { target: { value: 'maria' } });
    
    // A API não é chamada instantaneamente
    expect(api.listarClientes).toHaveBeenCalledTimes(1); // 1ª chamada montagem
    
    // Avança 600ms (vitest faria isso se usássemos fake timers, mas vamos fazer normal aguardando)
    await waitFor(() => {
      expect(api.listarClientes).toHaveBeenCalledTimes(2);
    });
  });
});
