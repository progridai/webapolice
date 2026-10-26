import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { CadastrarClientePage } from './CadastrarClientePage';
import * as clienteWriteApi from '../api/clienteWriteApi';
import { makeAuthorization } from '../../../../tests/helpers/authorization';

vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: () => makeAuthorization(),
}));

vi.mock('../api/clienteWriteApi', () => ({
  cadastrarCliente: vi.fn(),
}));

describe('CadastrarClientePage', () => {
  it('deve renderizar o formulário corretamente', () => {
    render(
      <BrowserRouter>
        <CadastrarClientePage />
      </BrowserRouter>,
    );

    expect(screen.getByRole('heading', { name: 'Novo Cliente' })).toBeTruthy();
    expect(screen.getByLabelText(/Nome Completo/i)).toBeTruthy();
    expect(screen.getByLabelText(/Documento/i)).toBeTruthy();
  });

  it('deve chamar a API ao submeter formulário válido', async () => {
    (
      clienteWriteApi.cadastrarCliente as ReturnType<typeof vi.fn>
    ).mockResolvedValue({ id: '123' });

    render(
      <BrowserRouter>
        <CadastrarClientePage />
      </BrowserRouter>,
    );

    fireEvent.change(screen.getByLabelText(/Nome Completo/i), {
      target: { value: 'João da Silva' },
    });
    fireEvent.input(screen.getByLabelText(/Documento/i), {
      target: { value: '52998224725' },
    });
    fireEvent.change(screen.getByLabelText(/Data de Nascimento/i), {
      target: { value: '1990-01-01' },
    });

    fireEvent.click(screen.getByRole('button', { name: /Salvar Cliente/i }));

    await waitFor(() => {
      expect(clienteWriteApi.cadastrarCliente).toHaveBeenCalled();
    });
  });
});
