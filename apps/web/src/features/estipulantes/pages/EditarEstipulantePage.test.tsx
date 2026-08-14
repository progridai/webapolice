import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { EditarEstipulantePage } from './EditarEstipulantePage';
import * as api from '../api/estipulantes.api';

vi.mock('../api/estipulantes.api', () => ({
  obterEstipulante: vi.fn(),
  obterConfiguracao: vi.fn(),
  alterarEstipulante: vi.fn(),
}));

vi.mock('../../clientes/api/localidadesApi', () => ({
  buscarCidadesPorUf: vi.fn().mockResolvedValue([]),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useParams: () => ({ publicId: 'estip-123' }),
    useNavigate: () => vi.fn(),
  };
});

describe('EditarEstipulantePage', () => {
  const mockEstipulante = {
    publicId: 'estip-123',
    razaoSocial: 'Estipulante Teste',
    nomeFantasia: 'Teste',
    cnpj: '12345678000190',
    codigo: '123',
    grupoPublicId: '',
    seguradoraPublicId: '',
    observacao: '',
    ativo: true,
  };

  const mockConfiguracao = {
    dataInicioVigencia: '2026-01-01',
    dataFimVigencia: '',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve exibir spinner durante carregamento e renderizar formulário em seguida', async () => {
    (api.obterEstipulante as ReturnType<typeof vi.fn>).mockResolvedValue(mockEstipulante);
    (api.obterConfiguracao as ReturnType<typeof vi.fn>).mockResolvedValue(mockConfiguracao);

    render(
      <BrowserRouter>
        <EditarEstipulantePage />
      </BrowserRouter>
    );

    // Na inicialização, deve mostrar carregando

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Editar Estipulante' })).toBeTruthy();
    });

    // Validar carregamento do CNPJ imutável no formato formatado
    expect(screen.getByDisplayValue('12.345.678/0001-90')).toBeTruthy();
    // Validar preenchimento da Razão Social
    expect(screen.getByDisplayValue('Estipulante Teste')).toBeTruthy();
  });

  it('deve exibir EmptyState (404) quando estipulante não for encontrado', async () => {
    (api.obterEstipulante as ReturnType<typeof vi.fn>).mockRejectedValue({
      status: 404
    });
    (api.obterConfiguracao as ReturnType<typeof vi.fn>).mockResolvedValue(mockConfiguracao);

    render(
      <BrowserRouter>
        <EditarEstipulantePage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Estipulante não encontrado')).toBeTruthy();
    });
  });

  it('deve permitir submissão com sucesso chamando as duas APIs sequencialmente', async () => {
    (api.obterEstipulante as ReturnType<typeof vi.fn>).mockResolvedValue(mockEstipulante);
    (api.obterConfiguracao as ReturnType<typeof vi.fn>).mockResolvedValue(mockConfiguracao);
    (api.alterarEstipulante as ReturnType<typeof vi.fn>).mockResolvedValue(undefined);

    render(
      <BrowserRouter>
        <EditarEstipulantePage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByDisplayValue('Estipulante Teste')).toBeTruthy();
    });

    // Disparar o botão de salvar
    fireEvent.click(screen.getByRole('button', { name: /Salvar Estipulante/i }));

    await waitFor(() => {
      expect(api.alterarEstipulante).toHaveBeenCalledWith('estip-123', expect.any(Object));
    });
  });

  it('deve tratar conflito HTTP 409 (Pessoa Compartilhada)', async () => {
    (api.obterEstipulante as ReturnType<typeof vi.fn>).mockResolvedValue(mockEstipulante);
    (api.obterConfiguracao as ReturnType<typeof vi.fn>).mockResolvedValue(mockConfiguracao);
    
    // Forçar erro na primeira promise (alterarEstipulante)
    (api.alterarEstipulante as ReturnType<typeof vi.fn>).mockRejectedValue({
      status: 409,
      message: 'Conflito com Pessoa existente.'
    });

    render(
      <BrowserRouter>
        <EditarEstipulantePage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByDisplayValue('Estipulante Teste')).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: /Salvar Estipulante/i }));

    await waitFor(() => {
      expect(screen.getByText('Conflito com Pessoa existente.')).toBeTruthy();
    });
  });
});
