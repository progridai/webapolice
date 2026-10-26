import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  criarApolice,
  alterarApolice,
  listarApolices,
  obterApolice,
  listarApoliceVidas,
} from '../../src/features/apolices/api/apolices.api';
import { listarCooperados } from '../../src/features/cooperados/api/cooperadosApi';
import { listarEstipulantes } from '../../src/features/estipulantes/api/estipulantes.api';
import { modulosApi } from '../../src/features/modulos/api/modulos.api';
import { ramosApi } from '../../src/features/ramos/api/ramos.api';
import { corretorasApi } from '../../src/features/corretoras/api/corretoras.api';
import {
  atualizarUsuario,
  criarUsuario,
} from '../../src/features/seguranca/api/usuariosApi';
import { setTokenProvider } from '../../src/services/http/httpClient';

vi.mock('../../src/services/http/apiConfig', () => ({
  API_CONFIG: {
    BASE_URL: 'http://127.0.0.1:5000',
    DEFAULT_TIMEOUT_MS: 5000,
    DEFAULT_HEADERS: { 'Content-Type': 'application/json' },
  },
}));

const mockFetch = vi.fn<typeof fetch>();
beforeEach(() => {
  setTokenProvider(null);
  vi.stubGlobal('fetch', mockFetch);
  mockFetch.mockResolvedValue(
    new Response(JSON.stringify({ publicId: 'apolice-1' }), { status: 200 }),
  );
});

function request() {
  const [url, options] = mockFetch.mock.calls[0];
  return {
    url: new URL(String(url)),
    options,
    body: options?.body
      ? (JSON.parse(String(options.body)) as Record<string, unknown>)
      : undefined,
  };
}

describe('Contratos HTTP de Usuários e Apólices', () => {
  it.each(['cooperados', 'modulos', 'ramos', 'corretoras'] as const)(
    'propaga cancelamento de %s enquanto a rede está pendente',
    async (feature) => {
      const controller = new AbortController();
      mockFetch.mockImplementationOnce(
        (_url, options) =>
          new Promise<Response>((_resolve, reject) => {
            options?.signal?.addEventListener(
              'abort',
              () => reject(options.signal?.reason),
              { once: true },
            );
          }),
      );
      const load = {
        cooperados: () =>
          listarCooperados({ page: 1, limit: 20 }, controller.signal),
        modulos: () =>
          modulosApi.listar(
            { pagina: 1, tamanhoPagina: 20 },
            controller.signal,
          ),
        ramos: () =>
          ramosApi.listar({ pagina: 1, tamanhoPagina: 20 }, controller.signal),
        corretoras: () => corretorasApi.listar({}, controller.signal),
      };
      const pending = load[feature]();
      const sent = request();
      const assertion = expect(pending).rejects.toMatchObject({
        name: 'AbortError',
      });
      controller.abort();
      expect(sent.options?.signal?.aborted).toBe(true);
      await assertion;
    },
  );

  it.each(['criar', 'alterar'] as const)(
    'envia apenas o contrato básico ao %s Apólice',
    async (operation) => {
      const data = {
        nome: 'apólice de teste',
        estipulanteId: '101',
        seguradoraId: '202',
        corretoraId: '303',
        dataInicioVigencia: '2026-01-01',
        dataFimVigencia: '',
        dataAniversario: '',
        observacao: 'texto de teste',
        _estipulanteNome: 'Rótulo Local',
        ramos: [{ tipoRamo: 'antigo' }],
      };
      if (operation === 'criar') await criarApolice(data);
      else await alterarApolice('apolice-1', data);
      const sent = request();
      expect(sent.url.pathname).toBe(
        operation === 'criar' ? '/api/apolices' : '/api/apolices/apolice-1',
      );
      expect(sent.options?.method).toBe(operation === 'criar' ? 'POST' : 'PUT');
      expect(sent.body).toEqual({
        nome: 'Apólice de Teste',
        estipulanteId: 101,
        seguradoraId: 202,
        corretoraId: 303,
        dataInicioVigencia: '2026-01-01',
        dataFimVigencia: null,
        dataAniversario: null,
        observacao: 'Texto de Teste',
      });
      expect(data.nome).toBe('apólice de teste');
    },
  );

  it('preserva datas informadas e corretora ausente no payload', async () => {
    await criarApolice({
      nome: 'Contrato',
      estipulanteId: '101',
      seguradoraId: '202',
      dataInicioVigencia: '2026-01-01',
      dataFimVigencia: '2026-12-31',
      dataAniversario: '2026-06-01',
    });
    expect(request().body).toMatchObject({
      corretoraId: null,
      dataFimVigencia: '2026-12-31',
      dataAniversario: '2026-06-01',
    });
  });

  it.each([false, true])(
    'consulta Detalhe e encaminha cancelamento opcional: %s',
    async (withSignal) => {
      let finish!: (response: Response) => void;
      mockFetch.mockReturnValueOnce(
        new Promise<Response>((resolve) => {
          finish = resolve;
        }),
      );
      const controller = new AbortController();
      const pending = obterApolice(
        'apolice-1',
        withSignal ? controller.signal : undefined,
      );
      const sent = request();
      expect(sent.url.pathname).toBe('/api/apolices/apolice-1');
      expect(sent.url.search).toBe('');
      expect(sent.options?.method).toBe('GET');
      expect(sent.body).toBeUndefined();
      expect(sent.options?.signal?.aborted).toBe(false);
      controller.abort();
      expect(sent.options?.signal?.aborted).toBe(withSignal);
      finish(
        new Response(JSON.stringify({ publicId: 'apolice-1' }), {
          status: 200,
        }),
      );
      if (withSignal) {
        await expect(pending).rejects.toMatchObject({ name: 'AbortError' });
      } else {
        expect(await pending).toEqual({ publicId: 'apolice-1' });
      }
    },
  );

  it('serializa os filtros de Vidas com os nomes aceitos pelo backend', async () => {
    const signal = new AbortController().signal;
    await listarApoliceVidas(
      'apolice-1',
      {
        page: 2,
        pageSize: 30,
        busca: 'JOÃO',
        status: 'Ativa',
        apoliceSubgrupoPublicId: 'subgrupo-1',
        apoliceModuloPublicId: 'modulo-1',
        vigenciaDataReferencia: '2026-10-21',
      },
      signal,
    );
    expect(request().url.pathname).toBe('/api/apolices/apolice-1/vidas');
    expect(Object.fromEntries(request().url.searchParams)).toEqual({
      pagina: '2',
      tamanhoPagina: '30',
      busca: 'JOÃO',
      status: 'Ativa',
      apoliceSubgrupoPublicId: 'subgrupo-1',
      apoliceModuloPublicId: 'modulo-1',
      vigenciaDataReferencia: '2026-10-21',
    });
    expect(request().options?.signal).toBeInstanceOf(AbortSignal);
  });

  it('omite filtros não informados da consulta de Vidas', async () => {
    await listarApoliceVidas('apolice-1', {});
    expect(request().url.search).toBe('');
  });

  it('envia data de referência e false nos filtros da listagem de Apólices', async () => {
    await listarApolices({
      ativo: false,
      vigenciaDataReferencia: '2026-10-21',
    });
    expect(Object.fromEntries(request().url.searchParams)).toEqual({
      ativo: 'false',
      vigenciaDataReferencia: '2026-10-21',
    });
  });

  it('envia os parâmetros de nome/CNPJ e paginação aceitos em Estipulantes', async () => {
    await listarEstipulantes({
      page: 1,
      pageSize: 20,
      nome: 'EMPRESA TESTE',
      cnpj: '11222333000181',
    });
    expect(Object.fromEntries(request().url.searchParams)).toEqual({
      pagina: '1',
      tamanhoPagina: '20',
      nome: 'EMPRESA TESTE',
      cnpj: '11222333000181',
    });
  });

  it('preserva false e os parâmetros de paginação de Corretoras', async () => {
    await corretorasApi.listar({
      pagina: 2,
      tamanhoPagina: 20,
      busca: 'EMPRESA',
      ativo: false,
    });
    expect(Object.fromEntries(request().url.searchParams)).toEqual({
      pagina: '2',
      tamanhoPagina: '20',
      busca: 'EMPRESA',
      ativo: 'false',
    });
  });

  it.each([false, true])(
    'preserva filtros e cancelamento da listagem de Cooperados: %s',
    async (withSignal) => {
      const controller = new AbortController();
      await listarCooperados(
        {
          page: 2,
          limit: 50,
          nome: '  JOÃO da SILVA  ',
          cpf: '123.456.789-09',
          status: '2',
          sortBy: 'nome',
          direction: 'desc',
        },
        withSignal ? controller.signal : undefined,
      );
      const sent = request();
      expect(sent.url.pathname).toBe('/api/cooperados');
      expect(sent.options?.method).toBe('GET');
      expect(Object.fromEntries(sent.url.searchParams)).toEqual({
        pagina: '2',
        tamanho_pagina: '50',
        nome: '  JOÃO da SILVA  ',
        cpf: '123.456.789-09',
        status: '2',
        ordenar_por: 'nome',
        direcao: 'desc',
      });
      expect(sent.options?.signal?.aborted).toBe(false);
      controller.abort();
      expect(sent.options?.signal?.aborted).toBe(false);
    },
  );

  it('omite filtros opcionais ausentes na listagem de Cooperados', async () => {
    await listarCooperados({ page: 1, limit: 20 });
    expect(Object.fromEntries(request().url.searchParams)).toEqual({
      pagina: '1',
      tamanho_pagina: '20',
    });
  });

  it.each([false, true])(
    'preserva filtros e cancelamento opcional na listagem de Módulos: %s',
    async (withSignal) => {
      const controller = new AbortController();
      await modulosApi.listar(
        { pagina: 2, tamanhoPagina: 10, busca: '  JOÃO  ', ativo: false },
        withSignal ? controller.signal : undefined,
      );
      const sent = request();
      expect(sent.url.pathname).toBe('/api/modulos');
      expect(Object.fromEntries(sent.url.searchParams)).toEqual({
        pagina: '2',
        tamanhoPagina: '10',
        busca: '  JOÃO  ',
        ativo: 'false',
      });
      expect(sent.options?.signal?.aborted).toBe(false);
      controller.abort();
      expect(sent.options?.signal?.aborted).toBe(false);
    },
  );

  it.each([false, true])(
    'preserva filtros e cancelamento opcional na listagem de Ramos: %s',
    async (withSignal) => {
      const controller = new AbortController();
      const query = { pagina: 1, tamanhoPagina: 100, ativo: true };
      await ramosApi.listar(query, withSignal ? controller.signal : undefined);
      const sent = request();
      expect(sent.url.pathname).toBe('/api/ramos');
      expect(Object.fromEntries(sent.url.searchParams)).toEqual({
        pagina: '1',
        tamanhoPagina: '100',
        ativo: 'true',
      });
      expect(sent.options?.signal?.aborted).toBe(false);
      controller.abort();
      expect(sent.options?.signal?.aborted).toBe(false);
    },
  );

  it('encaminha cancelamento opcional na listagem de Corretoras', async () => {
    const controller = new AbortController();
    await corretorasApi.listar(
      { busca: 'EMPRESA', ativo: false },
      controller.signal,
    );
    const sent = request();
    expect(sent.url.pathname).toBe('/api/corretoras');
    expect(Object.fromEntries(sent.url.searchParams)).toEqual({
      busca: 'EMPRESA',
      ativo: 'false',
    });
    expect(sent.options?.signal?.aborted).toBe(false);
    controller.abort();
    expect(sent.options?.signal?.aborted).toBe(false);
  });

  it('aceita consulta de Corretoras sem parâmetros', async () => {
    await corretorasApi.listar();
    expect(request().url.search).toBe('');
  });

  it('preserva credenciais na criação e aplica a normalização compartilhada aos textos humanos', async () => {
    await criarUsuario({
      username: 'Operador.TESTE',
      nome: 'joão da silva',
      email: 'Operador@EXEMPLO.com',
      senhaTemporaria: 'FiXtUrA!123',
      ativo: true,
      perfilPublicIds: ['perfil-1'],
    });
    expect(request().body).toEqual({
      username: 'Operador.TESTE',
      nome: 'João da Silva',
      email: 'Operador@EXEMPLO.com',
      senhaTemporaria: 'FiXtUrA!123',
      ativo: true,
      perfilPublicIds: ['perfil-1'],
    });
  });

  it('trata 204 na edição de Usuário e envia somente os campos editáveis', async () => {
    mockFetch.mockResolvedValue(new Response(null, { status: 204 }));
    await expect(
      atualizarUsuario('usuario-1', {
        nome: 'joão da silva',
        email: 'operador@exemplo.com',
        ativo: false,
        perfilPublicIds: [],
      }),
    ).resolves.toBeUndefined();
    expect(request().url.pathname).toBe('/api/seguranca/usuarios/usuario-1');
    expect(request().body).toEqual({
      nome: 'João da Silva',
      email: 'operador@exemplo.com',
      ativo: false,
      perfilPublicIds: [],
    });
  });
});
