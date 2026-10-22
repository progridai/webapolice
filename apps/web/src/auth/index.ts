/**
 * auth/index.ts
 *
 * Barrel export do módulo de autenticação.
 * Importe sempre daqui, nunca de arquivos individuais.
 */
export { AuthProvider } from './AuthProvider';
export { AuthContext } from './AuthContext';
export { useAuth } from './useAuth';
export { APP_ROLES, hasRole, hasAnyRole, hasAllRoles } from './roles';
export type { AuthContextValue, AuthUser, AuthStatus } from './auth.types';
export type { AppRole } from './roles';
