import { beforeEach, describe, expect, it, vi } from 'vitest';

const initMock = vi.fn();

vi.mock('keycloak-js', () => ({
  default: vi.fn().mockImplementation(() => ({
    init: initMock,
  })),
}));

describe('keycloak singleton', () => {
  beforeEach(() => {
    vi.resetModules();
    initMock.mockReset();
    initMock.mockResolvedValue(true);
  });

  it('initializes Keycloak only once and reuses the same promise', async () => {
    const { initKeycloakOnce } = await import('./keycloak');

    const options = { onLoad: 'check-sso' as const };
    const firstInit = initKeycloakOnce(options);
    const secondInit = initKeycloakOnce(options);

    await expect(firstInit).resolves.toBe(true);
    await expect(secondInit).resolves.toBe(true);
    expect(secondInit).toBe(firstInit);
    expect(initMock).toHaveBeenCalledTimes(1);
  });
});
