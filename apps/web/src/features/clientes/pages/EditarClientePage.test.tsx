import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { EditarClientePage } from './EditarClientePage';
import * as clienteWriteApi from '../api/clienteWriteApi';
import * as obterClienteDetalheApi from '../api/obterClienteDetalhe';

vi.mock('../api/clienteWriteApi', () => ({
  alterarCliente: vi.fn(),
}));

vi.mock('../api/obterClienteDetalhe', () => ({
  obterClienteDetalhe: vi.fn(),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useParams: () => ({ id: '123' }),
    useNavigate: () => vi.fn(),
  };
});

describe('EditarClientePage', () => {
  it('deve exibir mensagem de pessoa compartilhada quando houver conflito (409)', async () => {
    (obterClienteDetalheApi.obterClienteDetalhe as ReturnType<typeof vi.fn>).mockResolvedValue({
      id: 123,
      tipoPessoa: 'Física',
      nome: 'João da Silva',
      documento: '03644455544',
      documentoMascarado: '036.***.***-44',
      dataNascimento: '1990-01-01T00:00:00Z',
      contatos: [],
      enderecos: []
    });

    // Simular o erro 409 da API
    const error409 = {
      response: {
        status: 409,
        data: { message: 'Conflito gerado pelo backend' }
      }
    };
    (clienteWriteApi.alterarCliente as ReturnType<typeof vi.fn>).mockRejectedValue(error409);

    render(
      <BrowserRouter>
        <EditarClientePage />
      </BrowserRouter>
    );

    // Aguardar carregamento
    await waitFor(() => {
      expect(screen.getByDisplayValue('João da Silva')).toBeTruthy();
    });

    // Submeter formulário
    fireEvent.click(screen.getByRole('button', { name: /Salvar Cliente/i }));

    // Aguardar a mensagem de erro específica
    await waitFor(() => {
      expect(screen.getByText('Conflito gerado pelo backend')).toBeTruthy();
    });

    // Verificar se os valores foram preservados
    expect(screen.getByDisplayValue('João da Silva')).toBeTruthy();

    // Verificar se o botão foi reabilitado
    const saveBtn = screen.getByRole('button', { name: /Salvar Cliente/i });
    expect(saveBtn.hasAttribute('disabled')).toBe(false);
  });
});
