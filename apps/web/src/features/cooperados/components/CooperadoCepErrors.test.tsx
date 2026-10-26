import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { CooperadoForm } from './CooperadoForm';
import { consultarCep } from '../../../shared/api/enderecosApi';
import { buscarCidadesPorUf } from '../../clientes/api/localidadesApi';
import { listarCoordenadoresAtivos } from '../api/cooperadosApi';
import { HttpApiError } from '../../../services/http/httpError';

vi.mock('../../../shared/api/enderecosApi');
vi.mock('../../clientes/api/localidadesApi');
vi.mock('../api/cooperadosApi');

beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(buscarCidadesPorUf).mockResolvedValue([]);
  vi.mocked(listarCoordenadoresAtivos).mockResolvedValue([]);
});

function renderForm() {
  render(
    <CooperadoForm
      initialData={{
        nome: 'Cooperado Teste',
        cpf: '52998224725',
        tipo: 2,
        logradouro: 'Rua Atual',
      }}
      onSubmit={vi.fn()}
      onCancel={vi.fn()}
    />,
  );
}
function enterCep(value = '12345678') {
  fireEvent.input(screen.getByLabelText(/CEP/), { target: { value } });
}

describe('Erros da consulta de CEP de Cooperados', () => {
  it('encerra o indicador ao apagar o CEP durante uma consulta', async () => {
    let rejectLookup!: (error: unknown) => void;
    vi.mocked(consultarCep).mockReturnValueOnce(
      new Promise((_resolve, reject) => {
        rejectLookup = reject;
      }),
    );
    renderForm();
    enterCep();
    expect(screen.getByText(/CEP \(Buscando/)).toBeTruthy();
    enterCep('');
    expect(vi.mocked(consultarCep).mock.calls[0][1]?.aborted).toBe(true);
    expect(screen.queryByText(/Buscando/)).toBeNull();
    await act(async () => rejectLookup(null));
    expect(
      screen.queryByText('Serviço indisponível. Preencha manualmente.'),
    ).toBeNull();
  });

  it.each([
    {
      name: '404 normalizado',
      error: new HttpApiError({ status: 404, message: 'Não encontrado.' }),
      message: 'CEP não encontrado',
    },
    {
      name: '503 normalizado',
      error: new HttpApiError({ status: 503, message: 'Indisponível.' }),
      message: 'Serviço indisponível. Preencha manualmente.',
    },
    {
      name: 'null',
      error: null,
      message: 'Serviço indisponível. Preencha manualmente.',
    },
  ])(
    'trata $name e permite preenchimento manual',
    async ({ error, message }) => {
      vi.mocked(consultarCep).mockRejectedValueOnce(error);
      renderForm();
      enterCep();
      expect(await screen.findByText(message)).toBeTruthy();
      expect(screen.queryByText(/Buscando/)).toBeNull();
      const street = screen.getByLabelText<HTMLInputElement>('Logradouro');
      fireEvent.change(street, { target: { value: 'Rua Manual' } });
      expect(street.value).toBe('Rua Manual');
    },
  );

  it.each([
    new DOMException('Cancelado', 'AbortError'),
    Object.assign(new Error('Cancelado'), { name: 'CanceledError' }),
    new HttpApiError({ code: 'ERR_CANCELED', message: 'Cancelado' }),
  ])(
    'preserva endereço e não exibe falha para cancelamento: %s',
    async (error) => {
      vi.mocked(consultarCep).mockRejectedValueOnce(error);
      renderForm();
      enterCep();
      await waitFor(() => expect(consultarCep).toHaveBeenCalledOnce());
      await waitFor(() => expect(screen.queryByText(/Buscando/)).toBeNull());
      expect(screen.getByLabelText<HTMLInputElement>('Logradouro').value).toBe(
        'Rua Atual',
      );
      expect(
        screen.queryByText('Serviço indisponível. Preencha manualmente.'),
      ).toBeNull();
    },
  );

  it('descarta resposta e falha tardias de consultas abortadas, preservando o carregamento atual', async () => {
    let resolveOld!: (value: Awaited<ReturnType<typeof consultarCep>>) => void;
    let rejectSecond!: (error: unknown) => void;
    let resolveCurrent!: (
      value: Awaited<ReturnType<typeof consultarCep>>,
    ) => void;
    vi.mocked(consultarCep)
      .mockReturnValueOnce(
        new Promise((resolve) => {
          resolveOld = resolve;
        }),
      )
      .mockReturnValueOnce(
        new Promise((_resolve, reject) => {
          rejectSecond = reject;
        }),
      )
      .mockReturnValueOnce(
        new Promise((resolve) => {
          resolveCurrent = resolve;
        }),
      );
    renderForm();
    enterCep();
    enterCep('87654321');
    enterCep('12345670');
    await waitFor(() => expect(consultarCep).toHaveBeenCalledTimes(3));
    expect(vi.mocked(consultarCep).mock.calls[0][1]?.aborted).toBe(true);
    const address = {
      cep: '12345670',
      logradouro: 'Rua Nova',
      bairro: 'Centro',
      cidade: 'Cidade Teste',
      uf: 'SP',
      cidadeId: null,
      estadoId: null,
    };
    await act(async () => {
      resolveOld({ ...address, logradouro: 'Rua Antiga' });
      rejectSecond(null);
    });
    expect(screen.getByLabelText<HTMLInputElement>('Logradouro').value).toBe(
      'Rua Atual',
    );
    expect(screen.getByText(/CEP \(Buscando/)).toBeTruthy();
    expect(
      screen.queryByText('Serviço indisponível. Preencha manualmente.'),
    ).toBeNull();
    await act(async () => resolveCurrent(address));
    expect(screen.getByLabelText<HTMLInputElement>('Logradouro').value).toBe(
      'Rua Nova',
    );
    expect(screen.queryByText(/Buscando/)).toBeNull();
  });
});
