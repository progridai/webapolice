import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  criarApolice,
  alterarApolice,
  listarApolices,
  listarApoliceVidas,
} from '../../src/features/apolices/api/apolices.api';
import { listarEstipulantes } from '../../src/features/estipulantes/api/estipulantes.api';
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
