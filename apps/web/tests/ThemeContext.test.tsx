import { render, screen, fireEvent, act } from '@testing-library/react';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ThemeProvider, useTema } from '../src/shared/theme/ThemeContext';

// Componente auxiliar para testar o hook
function ThemeTestComponent() {
  const { temaPreferido, temaResolvido, alterarTema } = useTema();
  return (
    <div>
      <span data-testid="preferido">{temaPreferido}</span>
      <span data-testid="resolvido">{temaResolvido}</span>
      <button onClick={() => alterarTema('claro')}>Set Claro</button>
      <button onClick={() => alterarTema('escuro')}>Set Escuro</button>
      <button onClick={() => alterarTema('sistema')}>Set Sistema</button>
    </div>
  );
}

describe('ThemeContext & useTema', () => {
  let matchMediaMock: ReturnType<typeof vi.fn>;
  let listeners: (() => void)[] = [];
  let currentMatches = false;

  beforeEach(() => {
    localStorage.clear();
    listeners = [];
    currentMatches = false;

    // Mock matchMedia
    matchMediaMock = vi.fn().mockImplementation((query) => ({
      get matches() {
        return query.includes('dark') ? currentMatches : !currentMatches;
      },
      media: query,
      onchange: null,
      addListener: vi.fn((fn) => listeners.push(fn)),
      removeListener: vi.fn(),
      addEventListener: vi.fn((_, fn) => listeners.push(fn)),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    }));

    vi.stubGlobal('matchMedia', matchMediaMock);
  });

  it('deve inicializar com o tema do sistema padrão se o localStorage estiver vazio', () => {
    // Mock matchMedia retornando light (matches: false para dark)
    matchMediaMock.mockImplementation(() => ({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    }));

    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('preferido').textContent).toBe('sistema');
    expect(screen.getByTestId('resolvido').textContent).toBe('light');
  });

  it('deve inicializar com o tema dark do sistema se matchMedia for dark', () => {
    // Mock matchMedia retornando dark (matches: true para dark)
    matchMediaMock.mockImplementation((query) => ({
      matches: query.includes('dark'),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    }));

    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('preferido').textContent).toBe('sistema');
    expect(screen.getByTestId('resolvido').textContent).toBe('dark');
  });

  it('deve recuperar a preferência persistida do localStorage na inicialização', () => {
    localStorage.setItem('webapolice-tema', 'escuro');

    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('preferido').textContent).toBe('escuro');
    expect(screen.getByTestId('resolvido').textContent).toBe('dark');
  });

  it('deve persistir no localStorage ao alterar o tema', () => {
    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    fireEvent.click(screen.getByText('Set Claro'));

    expect(screen.getByTestId('preferido').textContent).toBe('claro');
    expect(screen.getByTestId('resolvido').textContent).toBe('light');
    expect(localStorage.getItem('webapolice-tema')).toBe('claro');

    fireEvent.click(screen.getByText('Set Escuro'));

    expect(screen.getByTestId('preferido').textContent).toBe('escuro');
    expect(screen.getByTestId('resolvido').textContent).toBe('dark');
    expect(localStorage.getItem('webapolice-tema')).toBe('escuro');
  });

  it('deve reagir a mudanças no esquema de cores do sistema se a preferência for sistema', () => {
    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('resolvido').textContent).toBe('light');

    // Simula alteração do sistema operacional para dark
    currentMatches = true;
    
    act(() => {
      listeners.forEach(listener => listener());
    });

    expect(screen.getByTestId('resolvido').textContent).toBe('dark');
  });
});
