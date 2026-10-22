import { vi } from 'vitest';
import type { useAuthorization } from '../../src/auth/AuthorizationProvider';

type Authorization = ReturnType<typeof useAuthorization>;

// O padrão é negar acesso. Cada teste declara apenas as concessões do seu cenário.
export function makeAuthorization(
  overrides: Partial<Authorization> = {},
): Authorization {
  const state = {
    isLoading: false,
    error: false,
    usuarioEncontrado: true,
    usuarioAtivo: true,
    acessoTotal: false,
    operadorSistema: false,
    modulosHabilitados: [] as string[],
    recursosHabilitados: [] as string[],
    permissoes: [] as string[],
    ...overrides,
  };

  return {
    ...state,
    possuiModulo: (codigo) => state.modulosHabilitados.includes(codigo),
    possuiRecurso: (codigo) => state.recursosHabilitados.includes(codigo),
    possuiPermissao: (codigo) =>
      state.acessoTotal || state.permissoes.includes(codigo),
    possuiAcessoTotal: () => state.acessoTotal,
    ehOperadorSistema: () => state.operadorSistema,
    refresh: vi.fn().mockResolvedValue(undefined),
    ...overrides,
  };
}
