import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { UniversoPermitidoTab } from './UniversoPermitidoTab';
import { useApoliceUniversoPermitido } from '../../hooks/useApoliceUniversoPermitido';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { alterarPremiosApolice } from '../../api/apolices.api';

vi.mock('../../hooks/useApoliceUniversoPermitido');
vi.mock('../../../../auth/AuthorizationProvider');
vi.mock('../../api/apolices.api');
const refetch = vi.fn().mockResolvedValue(undefined);

describe('Prêmios no Universo Permitido', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useAuthorization).mockReturnValue({
      possuiPermissao: () => true,
    } as ReturnType<typeof useAuthorization>);
    vi.mocked(useApoliceUniversoPermitido).mockReturnValue({
      isLoading: false,
      error: null,
      refetch,
      data: {
        produtos: [
          {
            produtoIdInternal: 1,
            ativo: true,
            planos: [
              {
                planoIdInternal: 2,
                nome: 'Plano',
                ativo: true,
                coberturas: [
                  {
                    coberturaIdInternal: 3,
                    publicId: 'vinculo-uuid',
                    nome: 'Cobertura',
                    ativo: true,
                    premioTitularPadrao: 10,
                    premioConjugePadrao: 5,
                    premioTitularOverride: 0,
                    premioConjugeOverride: null,
                    premioTitularEfetivo: 0,
                    premioConjugeEfetivo: 5,
                  },
                ],
              },
            ],
          },
        ],
      },
    });
    vi.mocked(alterarPremiosApolice).mockResolvedValue(undefined);
  });

  it('preserva a consulta histórica e direciona o cadastro para Módulos', () => {
    render(<UniversoPermitidoTab publicId="apolice-uuid" />);
    expect(screen.getByText(/Prêmio titular:/).textContent).toContain('0,00');
    expect(screen.getByText('Registros anteriores')).toBeTruthy();
    expect(
      screen.queryByRole('button', { name: 'Ajustar prêmios' }),
    ).toBeNull();
    expect(alterarPremiosApolice).not.toHaveBeenCalled();
  });

  it('exibe os prêmios e oculta o ajuste quando não há permissão de alterar', () => {
    vi.mocked(useAuthorization).mockReturnValue({
      possuiPermissao: () => false,
    } as ReturnType<typeof useAuthorization>);
    render(<UniversoPermitidoTab publicId="apolice-uuid" />);
    expect(screen.getByText(/Prêmio cônjuge:/).textContent).toContain('5,00');
    expect(
      screen.queryByRole('button', { name: 'Ajustar prêmios' }),
    ).toBeNull();
  });
});
