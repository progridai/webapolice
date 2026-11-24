import { afterEach, expect, it, vi } from 'vitest';
import { setTokenProvider } from '../../../services/http/httpClient';
import { listarParcelas } from './parcelas.api';
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
it('preserva UUID, paginação, sinais e centavos textuais do histórico', async () => {
  const dados = {
    items: [{ premioTotal: '-9999999999999999.99' }],
    page: 2,
    pageSize: 20,
    totalCount: 21,
  };
  const fetchMock = vi
    .fn<typeof fetch>()
    .mockResolvedValue(new Response(JSON.stringify(dados), { status: 200 }));
  vi.stubGlobal('fetch', fetchMock);
  setTokenProvider(null);
  expect(
    await listarParcelas('aBc-UUID', 2, new AbortController().signal),
  ).toEqual(dados);
  expect(fetchMock).toHaveBeenCalledWith(
    'http://localhost:5000/api/propostas/aBc-UUID/parcelas?pagina=2&tamanhoPagina=20',
    expect.objectContaining({ method: 'GET' }),
  );
});
it.each(['antes', 'durante'])(
  'cancela %s da obtenção do token sem enviar fetch',
  async (momento) => {
    let concluir: (token: string | undefined) => void = () => {};
    setTokenProvider(
      () =>
        new Promise((resolve) => {
          concluir = resolve;
        }),
    );
    const fetchMock = vi.fn<typeof fetch>();
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();
    if (momento === 'antes') controller.abort();
    const request = listarParcelas('id', 1, controller.signal);
    const rejeitada = expect(request).rejects.toMatchObject({
      name: 'AbortError',
    });
    controller.abort();
    concluir('token-ficticio');
    await rejeitada;
    expect(fetchMock).not.toHaveBeenCalled();
  },
);
