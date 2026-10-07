import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { PremiosCoberturaModal } from './PremiosCoberturaModal';

describe('Prêmios monetários', () => {
  it('distingue zero de voltar ao padrão da Apólice', async () => {
    const salvar = vi.fn().mockResolvedValue(undefined);
    render(
      <PremiosCoberturaModal
        nome="Cobertura"
        usarPadrao
        valores={{ premioTitular: 20, premioConjuge: null }}
        padroes={{ premioTitular: 10, premioConjuge: 5 }}
        onClose={vi.fn()}
        onSalvar={salvar}
      />,
    );
    fireEvent.change(screen.getByLabelText('Prêmio titular (R$)'), {
      target: { value: '' },
    });
    fireEvent.change(screen.getByLabelText('Prêmio cônjuge (R$)'), {
      target: { value: '0,00' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar prêmios' }));
    await waitFor(() =>
      expect(salvar).toHaveBeenCalledWith({
        premioTitular: null,
        premioConjuge: 0,
      }),
    );
  });

  it('rejeita valores negativos e mais de duas casas sem salvar', async () => {
    const salvar = vi.fn();
    render(
      <PremiosCoberturaModal
        nome="Cobertura"
        valores={{ premioTitular: null, premioConjuge: null }}
        onClose={vi.fn()}
        onSalvar={salvar}
      />,
    );
    fireEvent.change(screen.getByLabelText('Prêmio titular (R$)'), {
      target: { value: '-1' },
    });
    fireEvent.change(screen.getByLabelText('Prêmio cônjuge (R$)'), {
      target: { value: '1,234' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar prêmios' }));
    expect(
      await screen.findAllByText(
        'Informe um valor não negativo, com até duas casas decimais.',
      ),
    ).toHaveLength(2);
    expect(salvar).not.toHaveBeenCalled();
  });

  it('mantém os valores digitados quando o servidor rejeita a alteração', async () => {
    const fechar = vi.fn();
    render(
      <PremiosCoberturaModal
        nome="Cobertura"
        valores={{ premioTitular: 12.34, premioConjuge: 5.67 }}
        onClose={fechar}
        onSalvar={vi.fn().mockRejectedValue(new Error('Sem permissão'))}
      />,
    );
    fireEvent.click(screen.getByRole('button', { name: 'Salvar prêmios' }));
    await screen.findByText('Sem permissão');
    expect(
      (screen.getByLabelText('Prêmio titular (R$)') as HTMLInputElement).value,
    ).toBe('12,34');
    expect(fechar).not.toHaveBeenCalled();
  });
});
