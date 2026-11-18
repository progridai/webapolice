import { afterEach, expect, it, vi } from 'vitest';
import { setTokenProvider } from '../../../services/http/httpClient';
import { obterRamosSimulacao, simularValorMensal } from './simulacaoMensal.api';
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
it('preserva UUID, revisão e centavos textuais no contrato de simulação', async () => {
  const dados = { totalMensal: '9999999999999999.99' };
  const fetchMock = vi
    .fn<typeof fetch>()
    .mockResolvedValue(new Response(JSON.stringify(dados), { status: 200 }));
  vi.stubGlobal('fetch', fetchMock);
  setTokenProvider(null);
  expect(
    await simularValorMensal(
      'aBc-UUID',
      3,
      'rAmO-UUID',
      new AbortController().signal,
    ),
  ).toEqual(dados);
  expect(fetchMock).toHaveBeenCalledWith(
    'http://localhost:5000/api/propostas/aBc-UUID/simulacao-mensal',
    expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ revisaoEsperada: 3, ramoPublicId: 'rAmO-UUID' }),
    }),
  );
});
it.each(['opcoes', 'calculo'])(
  'cancela %s durante obtenção do token sem enviar fetch',
  async (tipo) => {
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
    const request =
      tipo === 'opcoes'
        ? obterRamosSimulacao('id', controller.signal)
        : simularValorMensal('id', 1, 'ramo', controller.signal);
    const rejeitada = expect(request).rejects.toMatchObject({
      name: 'AbortError',
    });
    controller.abort();
    concluir('token-ficticio');
    await rejeitada;
    expect(fetchMock).not.toHaveBeenCalled();
  },
);
