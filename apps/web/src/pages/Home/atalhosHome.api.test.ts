import { afterEach, beforeEach, expect, it, vi } from 'vitest';
import { setTokenProvider } from '../../services/http';
import { obterAtalhosHome, salvarAtalhosHome } from './atalhosHome.api';

vi.mock('../../services/http/apiConfig', () => ({
  API_CONFIG: {
    BASE_URL: 'http://127.0.0.1:5007',
    DEFAULT_TIMEOUT_MS: 5000,
    DEFAULT_HEADERS: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  },
}));

const fetchMock = vi.fn<typeof fetch>();
beforeEach(() => {
  fetchMock.mockReset();
  vi.stubGlobal('fetch', fetchMock);
  setTokenProvider(null);
});
afterEach(() => vi.unstubAllGlobals());

it('usa o endpoint pessoal e preserva as rotas técnicas no payload HTTP', async () => {
  const data = { atalhosHome: ['/seguranca/usuarios', '/convenios-cobranca'] };
  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify(data), { status: 200 }),
  );
  expect(
    await salvarAtalhosHome(data.atalhosHome, new AbortController().signal),
  ).toEqual(data);
  const [url, options] = fetchMock.mock.calls[0];
  expect(url).toBe('http://127.0.0.1:5007/api/seguranca/me/atalhos-home');
  expect(options?.method).toBe('PUT');
  expect(JSON.parse(String(options?.body))).toEqual(data);
});

it('distingue a configuração inicial null de uma escolha vazia', async () => {
  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify({ atalhosHome: null }), { status: 200 }),
  );
  expect(await obterAtalhosHome(new AbortController().signal)).toEqual({
    atalhosHome: null,
  });
  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify({ atalhosHome: [] }), { status: 200 }),
  );
  expect(await obterAtalhosHome(new AbortController().signal)).toEqual({
    atalhosHome: [],
  });
});

it('propaga o cancelamento durante a consulta pendente', async () => {
  let started!: () => void;
  const pending = new Promise<void>((resolve) => {
    started = resolve;
  });
  fetchMock.mockImplementation(
    (_url, options) =>
      new Promise<Response>((_resolve, reject) => {
        options?.signal?.addEventListener(
          'abort',
          () => reject(new DOMException('Cancelado', 'AbortError')),
          { once: true },
        );
        started();
      }),
  );
  const controller = new AbortController();
  const result = obterAtalhosHome(controller.signal);
  await pending;
  controller.abort();
  await expect(result).rejects.toMatchObject({ name: 'AbortError' });
});
