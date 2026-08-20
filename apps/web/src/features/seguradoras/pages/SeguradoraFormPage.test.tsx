/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { SeguradoraFormPage } from './SeguradoraFormPage';
import * as api from '../api/seguradoras.api';

vi.mock('../api/seguradoras.api', () => ({
  seguradorasApi: {
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

describe('SeguradoraFormPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar o formulário no modo de criação', () => {
    render(
      <MemoryRouter initialEntries={['/seguradoras/nova']}>
        <Routes>
          <Route path="/seguradoras/nova" element={<SeguradoraFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByRole('heading', { name: 'Nova Seguradora' })).toBeTruthy();
    expect(screen.getByLabelText(/Nome \/ Razão Social/i)).toBeTruthy();
    expect(screen.getByLabelText(/CNPJ/i)).toBeTruthy();
    expect(screen.getByLabelText(/Código Interno/i)).toBeTruthy();
    expect(screen.getByLabelText(/Código SUSEP/i)).toBeTruthy();
    expect(screen.getByLabelText(/Observação/i)).toBeTruthy();
  });

  it('deve exibir validação de erro ao submeter com nome vazio', async () => {
    render(
      <MemoryRouter initialEntries={['/seguradoras/nova']}>
        <Routes>
          <Route path="/seguradoras/nova" element={<SeguradoraFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    const salvarBtn = screen.getByRole('button', { name: 'Salvar' });
    fireEvent.click(salvarBtn);

    await waitFor(() => {
      expect(screen.getByText('O nome da seguradora é obrigatório.')).toBeTruthy();
    });
  });

  it('deve submeter com sucesso ao preencher formulário válido', async () => {
    vi.mocked(api.seguradorasApi.criar).mockResolvedValueOnce({ publicId: 'seg-1' });

    render(
      <MemoryRouter initialEntries={['/seguradoras/nova']}>
        <Routes>
          <Route path="/seguradoras/nova" element={<SeguradoraFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/Nome \/ Razão Social/i), {
      target: { value: 'Bradesco Auto/RE Companhia de Seguros' },
    });
    fireEvent.change(screen.getByLabelText(/CNPJ/i), {
      target: { value: '92.682.038/0001-00' },
    });
    fireEvent.change(screen.getByLabelText(/Código Interno/i), {
      target: { value: 'BRD-01' },
    });
    fireEvent.change(screen.getByLabelText(/Código SUSEP/i), {
      target: { value: '05312' },
    });

    const salvarBtn = screen.getByRole('button', { name: 'Salvar' });
    fireEvent.click(salvarBtn);

    await waitFor(() => {
      expect(api.seguradorasApi.criar).toHaveBeenCalledWith({
        nome: 'Bradesco Auto/RE Companhia de Seguros',
        cnpj: '92.682.038/0001-00',
        codigo: 'BRD-01',
        susep: '05312',
        observacao: undefined,
      });
    });
  });

  it('deve carregar e preencher dados existentes no modo de edição', async () => {
    const mockSeguradora = {
      publicId: 's-edit-1',
      nome: 'Mapfre Seguros',
      codigo: 'MPF-99',
      susep: '06238',
      cnpj: '61074175000138',
      ativo: true,
      observacao: 'Parceiro estratégico',
      createdAt: '2026-08-19T10:00:00Z',
      updatedAt: '2026-08-19T10:00:00Z',
    };

    vi.mocked(api.seguradorasApi.obter).mockResolvedValueOnce(mockSeguradora);

    render(
      <MemoryRouter initialEntries={['/seguradoras/s-edit-1/editar']}>
        <Routes>
          <Route path="/seguradoras/:publicId/editar" element={<SeguradoraFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Editar Seguradora' })).toBeTruthy();
    });

    expect((screen.getByLabelText(/Nome \/ Razão Social/i) as HTMLInputElement).value).toBe('Mapfre Seguros');
    expect((screen.getByLabelText(/Código Interno/i) as HTMLInputElement).value).toBe('MPF-99');
    expect((screen.getByLabelText(/Código SUSEP/i) as HTMLInputElement).value).toBe('06238');
    expect((screen.getByLabelText(/CNPJ/i) as HTMLInputElement).value).toBe('61074175000138');
    expect((screen.getByLabelText(/Observação/i) as HTMLTextAreaElement).value).toBe('Parceiro estratégico');
  });

  it('deve exibir mensagem de erro quando a API rejeitar a criação', async () => {
    vi.mocked(api.seguradorasApi.criar).mockRejectedValueOnce({
      response: {
        data: { message: 'Já existe uma seguradora cadastrada com este CNPJ.' },
      },
    });

    render(
      <MemoryRouter initialEntries={['/seguradoras/nova']}>
        <Routes>
          <Route path="/seguradoras/nova" element={<SeguradoraFormPage />} />
        </Routes>
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/Nome \/ Razão Social/i), {
      target: { value: 'Seguradora Duplicada' },
    });

    const salvarBtn = screen.getByRole('button', { name: 'Salvar' });
    fireEvent.click(salvarBtn);

    await waitFor(() => {
      expect(screen.getByText('Já existe uma seguradora cadastrada com este CNPJ.')).toBeTruthy();
    });
  });
});
