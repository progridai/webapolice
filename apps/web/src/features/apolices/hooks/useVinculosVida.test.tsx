import { act, renderHook, waitFor } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { useVinculosVida, type VinculoVidaOpcao } from './useVinculosVida';
import { httpClient } from '../../../services/http/httpClient';
import type { PagedResult } from '../../../shared/types/pagination';
import type { HttpResponse } from '../../../services/http/http.types';
vi.mock('../../../services/http/httpClient', () => ({
  httpClient: { get: vi.fn() },
}));
const vazio: PagedResult<VinculoVidaOpcao> = {
  items: [],
  page: 1,
  pageSize: 20,
  totalCount: 0,
};
it('descarta resposta antiga e cancela consultas ao trocar cliente, apólice e desmontar', async () => {
  let resolver!: (
    response: HttpResponse<PagedResult<VinculoVidaOpcao>>,
  ) => void;
  const pendente = new Promise<HttpResponse<PagedResult<VinculoVidaOpcao>>>(
    (resolve) => {
      resolver = resolve;
    },
  );
  vi.mocked(httpClient.get)
    .mockReturnValueOnce(pendente)
    .mockResolvedValue({ status: 200, data: vazio });
  const { result, rerender, unmount } = renderHook(
    ({ a, c }) => useVinculosVida(a, c, 1),
    { initialProps: { a: 'a1', c: 'c1' } },
  );
  const signal = vi.mocked(httpClient.get).mock.calls[0][1]?.signal;
  rerender({ a: 'a1', c: 'c2' });
  expect(signal?.aborted).toBe(true);
  await waitFor(() => expect(result.current.data).toEqual(vazio));
  await act(async () =>
    resolver({
      status: 200,
      data: {
        ...vazio,
        items: [{ publicId: 'antigo', matricula: 'antiga', estipulante: null }],
      },
    }),
  );
  expect(result.current.data?.items).toEqual([]);
  rerender({ a: 'a2', c: 'c2' });
  const ultimoSignal = vi.mocked(httpClient.get).mock.calls.at(-1)?.[1]?.signal;
  unmount();
  expect(ultimoSignal?.aborted).toBe(true);
});
it('não consulta antes da escolha do cliente', () => {
  renderHook(() => useVinculosVida('apolice', '', 1));
  expect(httpClient.get).not.toHaveBeenCalled();
});
