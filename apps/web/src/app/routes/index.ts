/**
 * app/routes/index.ts
 *
 * Barrel export do módulo de roteamento.
 */
export { AppRoutes } from './AppRoutes';
export { ProtectedRoute } from './ProtectedRoute';
export { PermissionProtectedRoute } from './PermissionProtectedRoute';
export { ROUTES, createPath } from './routePaths';
export type { RoutePath } from './routePaths';
