import { afterEach, expect, it, vi } from 'vitest';
import { setTokenProvider } from '../../../services/http/httpClient';
import { listarPropostas } from './propostas.api';
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
it('envia ambos os UUIDs do vínculo junto com busca e paginação', async () => {
  const dados = { items: [], totalCount: 0, page: 2, pageSize: 20 };
  const fetchMock = vi
    .fn<typeof fetch>()
    .mockResolvedValue(new Response(JSON.stringify(dados), { status: 200 }));
  vi.stubGlobal('fetch', fetchMock);
  expect(
    await listarPropostas(
      {
        pagina: 2,
        tamanhoPagina: 20,
        clientePublicId: 'aBc-CLIENTE',
        clienteVinculoPublicId: 'xYz-VINCULO',
        termoBusca: 'aBc-123',
      },
      new AbortController().signal,
    ),
  ).toEqual(dados);
  expect(fetchMock).toHaveBeenCalledWith(
    'http://localhost:5000/api/propostas?pagina=2&tamanhoPagina=20&clientePublicId=aBc-CLIENTE&clienteVinculoPublicId=xYz-VINCULO&termoBusca=aBc-123',
    expect.objectContaining({ method: 'GET' }),
  );
});
it('cancela enquanto aguarda token sem enviar consulta ampla', async () => {
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
  const request = listarPropostas(
    {
      pagina: 1,
      tamanhoPagina: 20,
      clientePublicId: 'cliente',
      clienteVinculoPublicId: 'vinculo',
    },
    controller.signal,
  );
  const rejeitada = expect(request).rejects.toMatchObject({
    name: 'AbortError',
  });
  controller.abort();
  concluir('token-ficticio');
  await rejeitada;
  expect(fetchMock).not.toHaveBeenCalled();
});
