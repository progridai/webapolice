import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { EditarClientePage } from './EditarClientePage';
import * as clienteWriteApi from '../api/clienteWriteApi';
import * as obterClienteDetalheApi from '../api/obterClienteDetalhe';
import { HttpApiError } from '../../../services/http/httpError';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import type { ClienteDetalheResponse } from '../types/clienteDetalhe.types';

const navigate = vi.hoisted(() => vi.fn());
const clienteId = 'afbe6a15-1841-45c6-8324-70aff26ce507';
const cliente: ClienteDetalheResponse = {
  id: clienteId,
  nome: 'João da Silva',
  documento: '52998224725',
  documentoMascarado: '529.***.***-25',
  status: { codigo: 'ATIVO', nome: 'Ativo' },
  dataNascimento: '1990-01-01',
  falecido: false,
  contatos: [],
  enderecos: [],
  vinculos: [],
  dependentes: [],
};

vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: () => makeAuthorization(),
}));

vi.mock('../api/clienteWriteApi', () => ({
  alterarCliente: vi.fn(),
}));

vi.mock('../api/obterClienteDetalhe', () => ({
  obterClienteDetalhe: vi.fn(),
}));

vi.mock('../api/localidadesApi', () => ({
  buscarCidadesPorUf: vi.fn().mockImplementation(async () => [
    { id: 3509502, nome: 'Campinas' },
    { id: 3550308, nome: 'São Paulo' },
  ]),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useParams: () => ({ id: 'afbe6a15-1841-45c6-8324-70aff26ce507' }),
    useNavigate: () => navigate,
  };
});

describe('EditarClientePage', () => {
  beforeEach(() => {
    vi.mocked(obterClienteDetalheApi.obterClienteDetalhe).mockResolvedValue(
      cliente,
    );
    vi.mocked(clienteWriteApi.alterarCliente).mockResolvedValue(undefined);
  });

  it('deve exibir mensagem de pessoa compartilhada quando houver conflito (409)', async () => {
    // Simular o erro 409 da API
    const error409 = new HttpApiError({
      status: 409,
      message: 'Conflito gerado pelo backend',
    });
    vi.mocked(clienteWriteApi.alterarCliente).mockRejectedValue(error409);

    render(
      <BrowserRouter>
        <EditarClientePage />
      </BrowserRouter>,
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

  it.each([3509502, null])(
    'preserva números e booleanos dos endereços na edição (cidade: %s)',
    async (cidadeId) => {
      const endereco = {
        tipo: 'RESIDENCIAL',
        cep: '',
        logradouro: 'Rua de Teste',
        numero: '100',
        complemento: '',
        bairro: 'Centro',
        cidade: 'Campinas',
        cidadeId,
        uf: 'SP',
        principal: true,
        ativo: true,
      };
      vi.mocked(obterClienteDetalheApi.obterClienteDetalhe).mockResolvedValue({
        ...cliente,
        enderecos: [
          endereco,
          {
            ...endereco,
            tipo: 'COMERCIAL',
            logradouro: 'Rua Comercial',
            cidadeId: 3550308,
            principal: false,
          },
        ],
      });
      render(
        <BrowserRouter>
          <EditarClientePage />
        </BrowserRouter>,
      );
      await screen.findByDisplayValue('Rua de Teste');
      const cidades = screen.getAllByLabelText<HTMLSelectElement>('Cidade');
      await waitFor(() => expect(cidades[1].value).toBe('3550308'));
      expect(cidades[0].value).toBe(cidadeId ? String(cidadeId) : '');
      const principals = screen
        .getAllByRole<HTMLInputElement>('checkbox', { name: 'Principal' })
        .filter((field) => field.id.startsWith('endereco-principal-'));
      expect(principals.map((field) => field.checked)).toEqual([true, false]);
      fireEvent.click(screen.getByRole('button', { name: 'Salvar Cliente' }));
      await waitFor(() =>
        expect(clienteWriteApi.alterarCliente).toHaveBeenCalledOnce(),
      );
      const [id, payload] = vi.mocked(clienteWriteApi.alterarCliente).mock
        .calls[0];
      expect(id).toBe(clienteId);
      expect(payload.enderecos).toMatchObject([
        {
          tipoEndereco: 'RESIDENCIAL',
          cidadeId: cidadeId ?? undefined,
          principal: true,
        },
        { tipoEndereco: 'COMERCIAL', cidadeId: 3550308, principal: false },
      ]);
      expect(navigate).toHaveBeenCalledWith(`/clientes/${clienteId}`);
    },
  );
});
