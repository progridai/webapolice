import { describe, expect, it, beforeEach } from 'vitest';
import {
  createPostLoginRedirectUri,
  getOidcRedirectUri,
  restorePostLoginRedirect,
} from './authRedirect';

describe('authRedirect', () => {
  beforeEach(() => {
    window.sessionStorage.clear();
    window.history.replaceState(null, '', '/');
    window.location.hash = '';
  });

  it('uses the origin and pathname without hash as the OIDC redirect URI', () => {
    window.history.replaceState(null, '', '/erp/#/clientes');

    expect(getOidcRedirectUri()).toBe(`${window.location.origin}/erp/`);
  });

  it('stores the post-login hash route and restores it after authentication', () => {
    const redirectUri = createPostLoginRedirectUri('/clientes');

    expect(redirectUri).toBe(`${window.location.origin}/`);

    restorePostLoginRedirect();

    expect(window.location.hash).toBe('#/clientes');
  });

  it('falls back to the authenticated home when the stored route is public', () => {
    createPostLoginRedirectUri('/login');

    restorePostLoginRedirect();

    expect(window.location.hash).toBe('#/app');
  });
});
