import { afterEach, expect, it, vi } from 'vitest';
import { aplicarCondicoes, compararCondicoes } from './revisaoCondicoes.api';
import { setTokenProvider } from '../../../services/http/httpClient';
vi.mock('../../../services/http/apiConfig', () => ({
  API_CONFIG: {
    BASE_URL: 'http://localhost:5000',
    DEFAULT_TIMEOUT_MS: 5000,
    DEFAULT_HEADERS: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  },
}));
afterEach(() => {
  vi.unstubAllGlobals();
  setTokenProvider(null);
});
it('preserva a comparação e a escolha técnica enquanto normaliza a justificativa humana', async () => {
  const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
    new Response(JSON.stringify({ revisaoPreenchimento: 4 }), {
      status: 200,
    }),
  );
  vi.stubGlobal('fetch', fetchMock);
  setTokenProvider(null);
  const dados = {
    revisaoEsperada: 3,
    moduloPublicId: 'modulo-UUID',
    subgrupoPublicId: null,
    comparacao: 'aB123cD',
    tratamentoAjustes: 'MANTER' as const,
    observacao: 'REVISÃO DA PROPOSTA',
  };
  expect(await aplicarCondicoes('proposta-UUID', dados)).toEqual({
    revisaoPreenchimento: 4,
  });
  expect(fetchMock).toHaveBeenCalledWith(
    'http://localhost:5000/api/propostas/proposta-UUID/condicoes/aplicar',
    expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ ...dados, observacao: 'Revisão da Proposta' }),
    }),
  );
});
it('cancela comparação durante a obtenção do token sem enviar a requisição', async () => {
  let resolver: (token: string | undefined) => void = () => {};
  const token = new Promise<string | undefined>((resolve) => {
    resolver = resolve;
  });
  setTokenProvider(() => token);
  const fetchMock = vi.fn<typeof fetch>();
  vi.stubGlobal('fetch', fetchMock);
  const controller = new AbortController();
  const request = compararCondicoes(
    'id',
    { revisaoEsperada: 3, moduloPublicId: 'modulo', subgrupoPublicId: null },
    controller.signal,
  );
  const rejeitada = expect(request).rejects.toMatchObject({
    name: 'AbortError',
  });
  controller.abort();
  resolver('token-ficticio');
  await rejeitada;
  expect(fetchMock).not.toHaveBeenCalled();
});
