import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { CadastrarEstipulantePage } from './CadastrarEstipulantePage';
import * as api from '../api/estipulantes.api';

vi.mock('../api/estipulantes.api', () => ({
  cadastrarEstipulante: vi.fn(),
}));

vi.mock('../../clientes/api/localidadesApi', () => ({
  buscarCidadesPorUf: vi.fn(),
}));

describe('CadastrarEstipulantePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar o formulário corretamente', () => {
    render(
      <BrowserRouter>
        <CadastrarEstipulantePage />
      </BrowserRouter>
    );

    expect(screen.getByRole('heading', { name: 'Novo Estipulante' })).toBeTruthy();
    expect(screen.getByLabelText(/Razão Social/i)).toBeTruthy();
    expect(screen.getByLabelText(/CNPJ/i)).toBeTruthy();
    expect(screen.getByLabelText(/Início de Vigência/i)).toBeTruthy();
  });

  it('deve exibir erro se tentar submeter sem preencher campos obrigatórios', async () => {
    render(
      <BrowserRouter>
        <CadastrarEstipulantePage />
      </BrowserRouter>
    );

    fireEvent.click(screen.getByRole('button', { name: /Salvar Estipulante/i }));

    await waitFor(() => {
      expect(screen.getByText(/A Razão Social deve ter no mínimo 3 caracteres/i)).toBeTruthy();
      expect(screen.getByText(/CNPJ inválido/i)).toBeTruthy();
      expect(screen.getByText(/A data de início da vigência é obrigatória/i)).toBeTruthy();
    });

    expect(api.cadastrarEstipulante).not.toHaveBeenCalled();
  });

  it('deve chamar a API ao submeter formulário válido', async () => {
    (api.cadastrarEstipulante as ReturnType<typeof vi.fn>).mockResolvedValue({ publicId: '123' });

    render(
      <BrowserRouter>
        <CadastrarEstipulantePage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByLabelText(/Razão Social/i), { target: { value: 'Empresa Teste' } });
    fireEvent.change(screen.getByLabelText(/CNPJ/i), { target: { value: '12.345.678/0001-90' } });
    fireEvent.change(screen.getByLabelText(/Início de Vigência/i), { target: { value: '2026-01-01' } });

    fireEvent.click(screen.getByRole('button', { name: /Salvar Estipulante/i }));

    await waitFor(() => {
      expect(api.cadastrarEstipulante).toHaveBeenCalledWith({
        razaoSocial: 'Empresa Teste',
        nomeFantasia: '',
        cnpj: '12345678000190',
        codigo: '',
        observacao: '',
        configuracao: {
          dataInicioVigencia: '2026-01-01',
          dataFimVigencia: '',
        }
      });
    });
  });

  it('deve exibir feedback ao receber erro 409 da API (Conflito)', async () => {
    (api.cadastrarEstipulante as ReturnType<typeof vi.fn>).mockRejectedValue({
      response: {
        status: 409,
        data: { message: 'Já existe um Estipulante com este CNPJ' }
      }
    });

    render(
      <BrowserRouter>
        <CadastrarEstipulantePage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByLabelText(/Razão Social/i), { target: { value: 'Empresa Teste' } });
    fireEvent.change(screen.getByLabelText(/CNPJ/i), { target: { value: '12.345.678/0001-90' } });
    fireEvent.change(screen.getByLabelText(/Início de Vigência/i), { target: { value: '2026-01-01' } });

    fireEvent.click(screen.getByRole('button', { name: /Salvar Estipulante/i }));

    await waitFor(() => {
      expect(screen.getByText('Já existe um Estipulante com este CNPJ')).toBeTruthy();
    });
  });

  it('deve exibir mensagem de erro genérica', async () => {
    (api.cadastrarEstipulante as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('Erro Fatal'));

    render(
      <BrowserRouter>
        <CadastrarEstipulantePage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByLabelText(/Razão Social/i), { target: { value: 'Empresa Teste' } });
    fireEvent.change(screen.getByLabelText(/CNPJ/i), { target: { value: '12.345.678/0001-90' } });
    fireEvent.change(screen.getByLabelText(/Início de Vigência/i), { target: { value: '2026-01-01' } });

    fireEvent.click(screen.getByRole('button', { name: /Salvar Estipulante/i }));

    await waitFor(() => {
      expect(screen.getByText('Ocorreu um erro ao cadastrar o estipulante. Verifique os dados e tente novamente.')).toBeTruthy();
    });
  });
});
