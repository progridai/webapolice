import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ApoliceFormPage } from './ApoliceFormPage';

vi.mock('../components/forms/ApoliceForm', () => ({
  ApoliceForm: () => <div>Formulário da Apólice</div>,
}));

describe('Navegação no cadastro de Apólice', () => {
  it('exibe a ação Voltar no cabeçalho e retorna ao histórico anterior', async () => {
    render(
      <MemoryRouter
        initialEntries={['/apolices', '/apolices/nova']}
        initialIndex={1}
      >
        <Routes>
          <Route path="/apolices" element={<div>Listagem anterior</div>} />
          <Route path="/apolices/nova" element={<ApoliceFormPage />} />
        </Routes>
      </MemoryRouter>,
    );
    expect(screen.getByRole('heading', { name: 'Nova Apólice' })).toBeTruthy();
    fireEvent.click(screen.getByRole('button', { name: 'Voltar' }));
    expect(await screen.findByText('Listagem anterior')).toBeTruthy();
  });
});
