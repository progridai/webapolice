import { afterEach, expect, it, vi } from 'vitest';
import {
  alterarCancelamentoVigencia,
  retirarCancelamentoVigencia,
  prepararReinclusao,
} from './propostas.api';
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

it.each(['ANTERIOR', 'MODULO'] as const)(
  'preserva o código técnico %s na preparação de reinclusão',
  async (origemCondicoes) => {
    const id = '24b9f5fb-e14c-4fe7-a98b-6d19beae7a33';
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(JSON.stringify({ publicId: 'novo-uuid' }), {
        status: 201,
      }),
    );
    vi.stubGlobal('fetch', fetchMock);
    setTokenProvider(null);
    expect(await prepararReinclusao(id, 3, origemCondicoes)).toBe('novo-uuid');
    expect(fetchMock).toHaveBeenCalledWith(
      `http://localhost:5000/api/propostas/${id}/reinclusao`,
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ revisaoEsperada: 3, origemCondicoes }),
      }),
    );
  },
);

it.each(['alterar', 'retirar'])(
  'envia %s com UUID, revisão e justificativa normalizada pelo cliente HTTP',
  async (acao) => {
    const resposta = {
      cancelada: true,
      agendada: false,
      revisaoPreenchimento: 4,
    };
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValue(
        new Response(JSON.stringify(resposta), { status: 200 }),
      );
    vi.stubGlobal('fetch', fetchMock);
    setTokenProvider(async () => 'token-ficticio');
    const id = '24b9f5fb-e14c-4fe7-a98b-6d19beae7a33';
    const resultado =
      acao === 'alterar'
        ? await alterarCancelamentoVigencia(
            id,
            3,
            '2026-11-10',
            'REVISÃO DA VIGÊNCIA',
          )
        : await retirarCancelamentoVigencia(id, 3, 'REVISÃO DA VIGÊNCIA');
    expect(resultado).toEqual(resposta);
    expect(fetchMock).toHaveBeenCalledWith(
      `http://localhost:5000/api/propostas/${id}/vigencia/cancelamento/${acao}`,
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          Authorization: 'Bearer token-ficticio',
        }),
        body: JSON.stringify(
          acao === 'alterar'
            ? {
                revisaoEsperada: 3,
                dataCancelamentoVigencia: '2026-11-10',
                observacao: 'Revisão da Vigência',
              }
            : { revisaoEsperada: 3, observacao: 'Revisão da Vigência' },
        ),
      }),
    );
  },
);
