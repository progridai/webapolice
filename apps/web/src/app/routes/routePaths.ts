/**
 * routePaths.ts
 *
 * Constantes centralizadas de rotas da aplicação.
 * REGRA: Nunca use strings de rota avulsas — use estas constantes.
 *
 * Decisão de roteamento: HashRouter
 * URLs no formato: /#/app, /#/login, /#/design-system
 * Documentado em: docs/14-fundacao-frontend.md
 */

export const ROUTES = {
  /** Raiz — redireciona conforme autenticação */
  ROOT: '/',

  /** Área autenticada — página inicial */
  APP: '/app',

  /** Autenticação — login via Keycloak */
  LOGIN: '/login',

  /** Módulo de Clientes — Listagem */
  CLIENTES: '/clientes',

  /** Módulo de Clientes — Novo */
  CLIENTE_NOVO: '/clientes/novo',

  /** Módulo de Clientes — Detalhes (temporário) */
  CLIENTE_DETALHES: '/clientes/:id',

  /** Catálogo do Design System (acesso restrito: role admin) */
  DESIGN_SYSTEM: '/design-system',

  /** Usuário não autenticado */
  UNAUTHORIZED: '/unauthorized',

  /** Usuário autenticado sem permissão */
  FORBIDDEN: '/forbidden',

  /** Erro inesperado da aplicação */
  ERROR: '/error',

  /** Qualquer rota não encontrada */
  NOT_FOUND: '*',
} as const;

export type RoutePath = (typeof ROUTES)[keyof typeof ROUTES];

/**
 * Cria uma URL de rota com parâmetros substituídos.
 * Uso: createPath(ROUTES.APP) → '/app'
 */
export function createPath(
  route: string,
  params?: Record<string, string>
): string {
  if (!params) return route;
  return Object.entries(params).reduce(
    (path, [key, value]) => path.replace(`:${key}`, value),
    route
  );
}
