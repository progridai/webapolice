/**
 * app/routes/index.ts
 *
 * Barrel export do módulo de roteamento.
 */
export { AppRoutes } from './AppRoutes';
export { ProtectedRoute } from './ProtectedRoute';
export { RoleProtectedRoute } from './RoleProtectedRoute';
export { ROUTES, createPath } from './routePaths';
export type { RoutePath } from './routePaths';
