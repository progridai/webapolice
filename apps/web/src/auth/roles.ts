/**
 * roles.ts
 *
 * Constantes centralizadas de roles da aplicação.
 * Helpers tipados para verificação de autorização.
 *
 * IMPORTANTE: A autorização do frontend serve para navegação e experiência do usuário.
 * O backend é responsável pela autorização real de todas as operações.
 * Ocultar elementos da UI NÃO substitui a proteção no backend.
 */

/** Roles oficiais da aplicação — use estas constantes, nunca strings avulsas */
export const APP_ROLES = {
  ADMIN: 'admin',
  GESTOR: 'gestor',
  OPERADOR: 'operador',
} as const;

export type AppRole = (typeof APP_ROLES)[keyof typeof APP_ROLES];

/**
 * Verifica se o usuário possui uma role específica.
 * @param userRoles - Roles do usuário autenticado
 * @param role - Role a verificar
 */
export function hasRole(userRoles: string[], role: string): boolean {
  return userRoles.includes(role);
}

/**
 * Verifica se o usuário possui ao menos uma das roles informadas.
 * @param userRoles - Roles do usuário autenticado
 * @param roles - Lista de roles aceitáveis
 */
export function hasAnyRole(userRoles: string[], roles: string[]): boolean {
  return roles.some((role) => userRoles.includes(role));
}

/**
 * Verifica se o usuário possui todas as roles informadas.
 * @param userRoles - Roles do usuário autenticado
 * @param roles - Lista de roles exigidas
 */
export function hasAllRoles(userRoles: string[], roles: string[]): boolean {
  return roles.every((role) => userRoles.includes(role));
}
