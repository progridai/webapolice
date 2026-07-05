import { ROUTES } from '../app/routes/routePaths';

const POST_LOGIN_REDIRECT_KEY = 'webapolice.postLoginRedirect';

function normalizeHashRoute(route?: string): string {
  if (!route || route === ROUTES.LOGIN || route === ROUTES.UNAUTHORIZED) {
    return ROUTES.APP;
  }

  return route.startsWith('/') ? route : `/${route}`;
}

export function getOidcRedirectUri(): string {
  return `${window.location.origin}${window.location.pathname}`;
}

export function createPostLoginRedirectUri(route?: string): string {
  const normalizedRoute = normalizeHashRoute(route);
  window.sessionStorage.setItem(POST_LOGIN_REDIRECT_KEY, normalizedRoute);

  return getOidcRedirectUri();
}

export function restorePostLoginRedirect(): void {
  const storedRoute = window.sessionStorage.getItem(POST_LOGIN_REDIRECT_KEY);
  if (!storedRoute) return;

  window.sessionStorage.removeItem(POST_LOGIN_REDIRECT_KEY);

  const normalizedRoute = normalizeHashRoute(storedRoute);
  if (window.location.hash !== `#${normalizedRoute}`) {
    window.location.hash = normalizedRoute;
  }
}
