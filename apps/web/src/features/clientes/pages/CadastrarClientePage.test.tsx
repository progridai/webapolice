import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { CadastrarClientePage } from './CadastrarClientePage';
import * as clienteWriteApi from '../api/clienteWriteApi';

vi.mock('../api/clienteWriteApi', () => ({
  cadastrarCliente: vi.fn(),
}));

describe('CadastrarClientePage', () => {
  it('deve renderizar o formulário corretamente', () => {
    render(
      <BrowserRouter>
        <CadastrarClientePage />
      </BrowserRouter>
    );

    expect(screen.getByText('Novo Cliente')).toBeTruthy();
    expect(screen.getByLabelText(/Nome Completo/i)).toBeTruthy();
    expect(screen.getByLabelText(/Documento/i)).toBeTruthy();
  });

  it('deve chamar a API ao submeter formulário válido', async () => {
    (clienteWriteApi.cadastrarCliente as ReturnType<typeof vi.fn>).mockResolvedValue({ id: '123' });

    render(
      <BrowserRouter>
        <CadastrarClientePage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByLabelText(/Nome Completo/i), { target: { value: 'João da Silva' } });
    fireEvent.change(screen.getByLabelText(/Documento/i), { target: { value: '03619574044' } });

    fireEvent.click(screen.getByRole('button', { name: /Salvar Cliente/i }));

    await waitFor(() => {
      expect(clienteWriteApi.cadastrarCliente).toHaveBeenCalled();
    });
  });
});
