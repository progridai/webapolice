
import { cleanup } from '@testing-library/react';
import { afterEach, vi, beforeEach } from 'vitest';

// 1. Limpeza global da árvore do React Testing Library
afterEach(() => {
  cleanup();
  
  // Limpa todos os mocks após cada teste para garantir isolamento
  vi.clearAllMocks();
  
  // Restaura timers caso algum teste tenha feito mock de setTimeout/setInterval
  vi.useRealTimers();
});

// 2. Mock Global para matchMedia (usado em múltiplos testes, p.ex., ThemeProvider)
beforeEach(() => {
  vi.stubGlobal(
    'matchMedia',
    vi.fn().mockImplementation((query) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    }))
  );
});
