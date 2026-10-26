import { act, renderHook, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useEstipulanteDetalhe } from './useEstipulanteDetalhe';
import { obterConfiguracao, obterEstipulante } from '../api/estipulantes.api';
import { HttpApiError } from '../../../services/http/httpError';

vi.mock('../api/estipulantes.api');
const dto: Awaited<ReturnType<typeof obterEstipulante>> = {
  publicId: 'estip-1',
  razaoSocial: 'Empresa Teste',
  cnpj: '61198164000160',
  ativo: true,
};

beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(obterEstipulante).mockResolvedValue(dto);
  vi.mocked(obterConfiguracao).mockResolvedValue({
    dataInicioVigencia: '2026-01-01',
  });
});

describe('Erros da consulta de Estipulante', () => {
  it.each([
    {
      name: 'HTTP',
      error: new HttpApiError({
        status: 403,
        message: 'Sem acesso.',
        traceId: 'trace-teste',
      }),
    },
    { name: 'Error', error: new Error('Falha temporária') },
    { name: 'null', error: null },
    { name: 'objeto fora do contrato', error: { message: 'Erro bruto' } },
  ])(
    'preserva ou normaliza $name e recupera na nova tentativa',
    async ({ error }) => {
      vi.mocked(obterEstipulante).mockRejectedValueOnce(error);
      const { result } = renderHook(() => useEstipulanteDetalhe('estip-1'));
      await waitFor(() => expect(result.current.error).toBeInstanceOf(Error));
      if (error instanceof Error) expect(result.current.error).toBe(error);
      else
        expect(result.current.error?.message).toBe(
          'Não foi possível carregar os dados do estipulante.',
        );
      expect(result.current.isLoading).toBe(false);
      act(() => result.current.retry());
      await waitFor(() =>
        expect(result.current.data?.estipulante).toEqual(dto),
      );
      expect(result.current.error).toBeNull();
      expect(result.current.isLoading).toBe(false);
    },
  );

  it.each([
    null,
    new HttpApiError({ status: 403, message: 'Configuração restrita.' }),
  ])(
    'não trata falha de configuração como configuração ausente: %s',
    async (error) => {
      vi.mocked(obterConfiguracao).mockRejectedValueOnce(error);
      const { result } = renderHook(() => useEstipulanteDetalhe('estip-1'));
      await waitFor(() => expect(result.current.error).toBeInstanceOf(Error));
      expect(result.current.data).toBeNull();
      expect(result.current.isLoading).toBe(false);
    },
  );

  it.each([
    new DOMException('Cancelado', 'AbortError'),
    Object.assign(new Error('Cancelado'), { name: 'AbortError' }),
  ])(
    'não apresenta cancelamento como erro nem mantém carregamento: %s',
    async (error) => {
      vi.mocked(obterEstipulante).mockRejectedValueOnce(error);
      const { result } = renderHook(() => useEstipulanteDetalhe('estip-1'));
      await waitFor(() => expect(result.current.isLoading).toBe(false));
      expect(result.current.error).toBeNull();
    },
  );
});
