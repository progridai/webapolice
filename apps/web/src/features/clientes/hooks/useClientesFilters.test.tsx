 
import { renderHook, act } from '@testing-library/react';
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import React from 'react';
import { MemoryRouter, useLocation } from 'react-router-dom';
import { useClientesFilters } from './useClientesFilters';

const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <MemoryRouter initialEntries={['/clientes?page=2&nome=maria&status=1']}>
    {children}
  </MemoryRouter>
);

describe('useClientesFilters', () => {
  it('deve extrair valores iniciais da URL', () => {
    const { result } = renderHook(() => useClientesFilters(), { wrapper: Wrapper });
    
    expect(result.current.filters).toEqual({
      page: 2,
      pageSize: 20,
      nome: 'maria',
      cpf: '',
      status: '1',
      sortBy: '',
      direction: 'asc',
    });
  });

  it('deve resetar a página para 1 quando outros filtros forem alterados', () => {
    let finalLocationSearch = '';
    
    const TestComponent = () => {
      const location = useLocation();
      const filters = useClientesFilters();
      
      React.useEffect(() => {
        finalLocationSearch = location.search;
      }, [location.search]);

      return <div onClick={() => filters.setFilters({ status: '2' })} data-testid="btn" />;
    };

    render(
      <MemoryRouter initialEntries={['/clientes?page=3&status=1']}>
        <TestComponent />
      </MemoryRouter>
    );

    fireEvent.click(screen.getByTestId('btn'));

    expect(finalLocationSearch).toContain('page=1');
    expect(finalLocationSearch).toContain('status=2');
  });

  it('setFilters com novo valor deve alterar o location search', () => {
    const { result } = renderHook(() => useClientesFilters(), { wrapper: Wrapper });

    act(() => {
      result.current.setFilters({ nome: 'joão' });
    });

    expect(result.current.filters.nome).toBe('joão');
    expect(result.current.filters.page).toBe(1);
    expect(result.current.filters.status).toBe('1');
  });

  it('setFilters apenas com page NÃO deve limpar os outros filtros nem resetar a página para 1 bizarramente', () => {
    const { result } = renderHook(() => useClientesFilters(), { wrapper: Wrapper });

    act(() => {
      result.current.setFilters({ page: 3 });
    });

    expect(result.current.filters.page).toBe(3);
    expect(result.current.filters.nome).toBe('maria');
  });

  it('clearFilters deve limpar a querystring e resetar para defaults', () => {
    const { result } = renderHook(() => useClientesFilters(), { wrapper: Wrapper });

    act(() => {
      result.current.clearFilters();
    });

    expect(result.current.filters).toEqual({
      page: 1,
      pageSize: 20,
      nome: '',
      cpf: '',
      status: '',
      sortBy: '',
      direction: 'asc',
    });
  });
});
