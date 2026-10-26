import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ClienteForm } from './ClienteForm';

vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: () => ({ possuiRecurso: () => false }),
}));

const contatos = [
  { tipoContato: 'EMAIL', valor: 'um@example.com', principal: true },
  { tipoContato: 'EMAIL', valor: 'dois@example.com', principal: false },
  { tipoContato: 'TELEFONE', valor: '5132419416', principal: false },
  { tipoContato: 'CELULAR', valor: '51999861341', principal: false },
];

function renderForm(isEdit = false) {
  const onSubmit = vi.fn();
  render(
    <ClienteForm
      isEdit={isEdit}
      onSubmit={onSubmit}
      onCancel={vi.fn()}
      initialData={{
        nome: 'João da Silva',
        tipoPessoa: 1,
        documento: '52998224725',
        dataNascimento: '1990-01-01',
        contatos,
        enderecos: [],
      }}
    />,
  );
  const principals = () =>
    screen
      .getAllByRole<HTMLInputElement>('checkbox', { name: 'Principal' })
      .filter((el) => el.id.startsWith('contato-principal-'));
  return { onSubmit, principals };
}

describe('Principal dos contatos do cliente', () => {
  it.each([false, true])(
    'mantém um principal por grupo e envia ambos (edição: %s)',
    async (isEdit) => {
      const { onSubmit, principals } = renderForm(isEdit);
      fireEvent.click(principals()[2]);
      expect(principals().map((c) => c.checked)).toEqual([
        true,
        false,
        true,
        false,
      ]);
      fireEvent.click(principals()[3]);
      expect(principals().map((c) => c.checked)).toEqual([
        true,
        false,
        false,
        true,
      ]);
      fireEvent.click(principals()[1]);
      expect(principals().map((c) => c.checked)).toEqual([
        false,
        true,
        false,
        true,
      ]);
      fireEvent.click(screen.getByRole('button', { name: 'Salvar Cliente' }));
      await waitFor(() => expect(onSubmit).toHaveBeenCalled());
      expect(
        onSubmit.mock.calls[0][0].contatos.map(
          (c: { principal: boolean }) => c.principal,
        ),
      ).toEqual([false, true, false, true]);
    },
  );

  it('reconcilia os principais ao mudar o tipo de um contato principal', () => {
    const { principals } = renderForm();
    fireEvent.click(principals()[2]);
    const types = screen
      .getAllByRole('combobox')
      .filter((el) => el.getAttribute('name')?.startsWith('contatos.'));
    fireEvent.change(types[0], { target: { value: 'CELULAR' } });
    expect(principals().map((c) => c.checked)).toEqual([
      true,
      false,
      false,
      false,
    ]);
    fireEvent.click(principals()[1]);
    expect(principals().map((c) => c.checked)).toEqual([
      true,
      true,
      false,
      false,
    ]);
    fireEvent.change(types[0], { target: { value: 'EMAIL' } });
    expect(principals().map((c) => c.checked)).toEqual([
      true,
      false,
      false,
      false,
    ]);
    fireEvent.click(principals()[0]);
    expect(principals().map((c) => c.checked)).toEqual([
      false,
      false,
      false,
      false,
    ]);
  });
});
