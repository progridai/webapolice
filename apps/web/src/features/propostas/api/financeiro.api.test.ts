import { afterEach, expect, it, vi } from 'vitest';
import { setTokenProvider } from '../../../services/http/httpClient';
import {
  conferirLote,
  conferirFinanceiro,
  gerarLote,
  fecharLote,
  baixarParcela,
  atualizarFinanceiro,
} from './financeiro.api';
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
it('preserva valores exatos e envia competência por Apólice', async () => {
  const dados = { competencia: 202611, totalAcumulado: '9999999999999999.99' };
  const mock = vi
    .fn<typeof fetch>()
    .mockResolvedValue(new Response(JSON.stringify(dados), { status: 200 }));
  vi.stubGlobal('fetch', mock);
  expect(
    await conferirLote('aBc-UUID', 202611, new AbortController().signal),
  ).toEqual(dados);
  expect(mock).toHaveBeenCalledWith(
    'http://localhost:5000/api/apolices/aBc-UUID/parcelas?competencia=202611',
    expect.objectContaining({ method: 'GET' }),
  );
});
it('envia tokens técnicos intactos nas gravações e não envia valores calculados pelo cliente', async () => {
  const mock = vi
    .fn<typeof fetch>()
    .mockImplementation(
      async () => new Response(JSON.stringify(true), { status: 200 }),
    );
  vi.stubGlobal('fetch', mock);
  await gerarLote('apolice', 202611, 'aBc123');
  await fecharLote('apolice', 202611, 'aBc123');
  await baixarParcela('apolice', 'movimento', 8, '2026-11-30');
  await atualizarFinanceiro('proposta', 8, {
    competenciaInicial: 202611,
    vencimentoInicial: '2026-11-30',
    ramoPublicId: 'ramo-A',
    conferencia: 'aBc123',
  });
  expect(mock.mock.calls[0][1]?.body).toBe(
    JSON.stringify({ competencia: 202611, conferencia: 'aBc123' }),
  );
  expect(mock.mock.calls[1][0]).toBe(
    'http://localhost:5000/api/apolices/apolice/parcelas/fechamento',
  );
  expect(mock.mock.calls[2][0]).toBe(
    'http://localhost:5000/api/apolices/apolice/parcelas/movimento/baixa',
  );
  expect(mock.mock.calls[2][1]?.body).toBe(
    JSON.stringify({ revisaoEsperada: 8, dataPagamento: '2026-11-30' }),
  );
  expect(String(mock.mock.calls[3][1]?.body)).toContain(
    '"conferencia":"aBc123"',
  );
});
it.each(['antes', 'durante'])(
  'cancela a conferência %s da obtenção do token',
  async (momento) => {
    let concluir: (token: string | undefined) => void = () => {};
    setTokenProvider(
      () =>
        new Promise((resolve) => {
          concluir = resolve;
        }),
    );
    const mock = vi.fn<typeof fetch>();
    vi.stubGlobal('fetch', mock);
    const controller = new AbortController();
    if (momento === 'antes') controller.abort();
    const request = conferirFinanceiro(
      'id',
      {
        revisaoEsperada: 1,
        dataInicioVigencia: '2026-11-01',
        competenciaInicial: 202611,
        vencimentoInicial: '2026-11-30',
        ramoPublicId: 'ramo',
      },
      controller.signal,
    );
    const rejeitada = expect(request).rejects.toMatchObject({
      name: 'AbortError',
    });
    controller.abort();
    concluir('token-ficticio');
    await rejeitada;
    expect(mock).not.toHaveBeenCalled();
  },
);
