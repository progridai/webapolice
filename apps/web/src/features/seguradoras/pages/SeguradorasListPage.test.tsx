/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { SeguradorasListPage } from './SeguradorasListPage';
import * as api from '../api/seguradoras.api';

vi.mock('../api/seguradoras.api', () => ({
  seguradorasApi: {
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

const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <MemoryRouter initialEntries={['/seguradoras']}>
    {children}
  </MemoryRouter>
);

describe('SeguradorasListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal(
      'matchMedia',
      vi.fn().mockImplementation((query) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      }))
    );
  });

  it('deve renderizar o título da página de Seguradoras', async () => {
    vi.mocked(api.seguradorasApi.listar).mockResolvedValueOnce({
      itens: [],
      totalItens: 0,
      totalPaginas: 0,
      paginaAtual: 1,
      tamanhoPagina: 10,
    });

    render(<SeguradorasListPage />, { wrapper: Wrapper });
    expect(screen.getByRole('heading', { name: 'Cadastro de Seguradoras' })).toBeTruthy();
  });

  it('deve exibir tabela com seguradoras quando a API responder com sucesso', async () => {
    const mockData = {
      itens: [
        {
          publicId: 's-1',
          nome: 'Porto Seguro Cia',
          codigo: 'SEG-001',
          susep: '05886',
          cnpj: '61198164000160',
          ativo: true,
          createdAt: '2026-08-19T12:00:00Z',
        },
      ],
      paginaAtual: 1,
      tamanhoPagina: 10,
      totalItens: 1,
      totalPaginas: 1,
    };
    vi.mocked(api.seguradorasApi.listar).mockResolvedValueOnce(mockData);

    render(<SeguradorasListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Porto Seguro Cia')).toBeTruthy();
    });

    expect(screen.getByText('61.198.164/0001-60')).toBeTruthy();
    expect(screen.getByText('SEG-001')).toBeTruthy();
    expect(screen.getByText('05886')).toBeTruthy();
    expect(screen.getByText('Ativo')).toBeTruthy();
  });

  it('deve exibir estado vazio quando não houver registros cadastrados', async () => {
    vi.mocked(api.seguradorasApi.listar).mockResolvedValueOnce({
      itens: [],
      totalItens: 0,
      totalPaginas: 0,
      paginaAtual: 1,
      tamanhoPagina: 10,
    });

    render(<SeguradorasListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Nenhuma seguradora encontrada')).toBeTruthy();
    });
  });

  it('deve exibir mensagem de erro e permitir retentativa ao falhar', async () => {
    vi.mocked(api.seguradorasApi.listar).mockRejectedValueOnce(new Error('Falha de conexão'));

    render(<SeguradorasListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Não foi possível carregar a lista de seguradoras.')).toBeTruthy();
    });

    const retryBtn = screen.getByRole('button', { name: /tentar novamente/i });
    expect(retryBtn).toBeTruthy();
  });

  it('deve disparar filtro de busca ao digitar no campo de pesquisa', async () => {
    vi.mocked(api.seguradorasApi.listar).mockResolvedValue({
      itens: [],
      totalItens: 0,
      paginaAtual: 1,
      tamanhoPagina: 10,
      totalPaginas: 0,
    });

    render(<SeguradorasListPage />, { wrapper: Wrapper });

    const searchInput = screen.getByPlaceholderText(/Buscar por nome, CNPJ, código ou SUSEP/i);
    fireEvent.change(searchInput, { target: { value: 'Porto' } });

    expect(api.seguradorasApi.listar).toHaveBeenCalledTimes(1);

    await waitFor(() => {
      expect(api.seguradorasApi.listar).toHaveBeenCalledTimes(2);
    });
  });

  it('deve abrir modal de confirmação ao clicar em inativar e executar a ação', async () => {
    const mockData = {
      itens: [
        {
          publicId: 's-123',
          nome: 'Allianz Seguros',
          codigo: 'ALL-10',
          susep: '05185',
          cnpj: '00000000000191',
          ativo: true,
          createdAt: '2026-08-19T12:00:00Z',
        },
      ],
      paginaAtual: 1,
      tamanhoPagina: 10,
      totalItens: 1,
      totalPaginas: 1,
    };
    vi.mocked(api.seguradorasApi.listar).mockResolvedValue(mockData);
    vi.mocked(api.seguradorasApi.inativar).mockResolvedValueOnce();

    render(<SeguradorasListPage />, { wrapper: Wrapper });

    await waitFor(() => {
      expect(screen.getByText('Allianz Seguros')).toBeTruthy();
    });

    const menuTrigger = screen.getByLabelText('Mais ações');
    fireEvent.click(menuTrigger);

    const inativarBtn = await screen.findByRole('menuitem', { name: /Inativar/i });
    fireEvent.click(inativarBtn);

    expect(screen.getByText(/Tem certeza que deseja inativar a seguradora/i)).toBeTruthy();

    const confirmBtn = screen.getByRole('button', { name: 'Sim, inativar' });
    fireEvent.click(confirmBtn);

    await waitFor(() => {
      expect(api.seguradorasApi.inativar).toHaveBeenCalledWith('s-123');
    });
  });
});
