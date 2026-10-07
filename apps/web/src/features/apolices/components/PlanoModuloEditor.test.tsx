import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { PlanoModuloEditor } from './PlanoModuloEditor';
import { planoModuloApi } from '../api/planoModulo.api';
import type { PlanoModulo } from '../api/planoModulo.api';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

vi.mock('../api/planoModulo.api');
vi.mock('../../../auth/AuthorizationProvider');
const plano: PlanoModulo = {
  publicId: 'plano-contextual',
  nome: 'Plano familiar',
  ramo: 'Vida',
  paga: null,
  reajuste: true,
  ativo: true,
  coberturas: [],
};
const renderizar = () =>
  render(
    <MemoryRouter>
      <PlanoModuloEditor
        apolicePublicId="apolice"
        moduloPublicId="vinculo-modulo"
      />
    </MemoryRouter>,
  );
const criarCobertura = async () => {
  fireEvent.click(
    await screen.findByRole('button', { name: 'Vincular Cobertura' }),
  );
  await screen.findByRole('option', { name: 'Morte' });
  fireEvent.change(screen.getByLabelText('Cobertura'), {
    target: { value: 'base-morte' },
  });
};

describe('Coberturas compartilhadas com prêmios no vínculo do Módulo', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(planoModuloApi.opcoesCoberturas).mockResolvedValue({
      items: [{ publicId: 'base-morte', nome: 'Morte' }],
      totalCount: 1,
    });
    vi.mocked(useAuthorization).mockReturnValue({
      possuiPermissao: (p: string) => p === 'apolices.modulos.alterar',
    } as unknown as ReturnType<typeof useAuthorization>);
    vi.mocked(planoModuloApi.obter).mockResolvedValue({
      apoliceModuloPublicId: 'vinculo-modulo',
      podeAlterar: true,
      plano: { ...plano, coberturas: [] },
    });
  });

  it('cria o único Plano antes de disponibilizar o cadastro de Coberturas', async () => {
    vi.mocked(planoModuloApi.obter).mockResolvedValue({
      apoliceModuloPublicId: 'vinculo-modulo',
      podeAlterar: true,
      plano: null,
    });
    vi.mocked(planoModuloApi.salvarPlano).mockResolvedValue(plano);
    renderizar();
    fireEvent.change(await screen.findByLabelText('Nome do Plano'), {
      target: { value: 'Plano familiar' },
    });
    expect(
      screen.queryByRole('button', { name: 'Vincular Cobertura' }),
    ).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Criar Plano' }));
    await waitFor(() =>
      expect(planoModuloApi.salvarPlano).toHaveBeenCalledWith(
        'apolice',
        'vinculo-modulo',
        {
          nome: 'Plano familiar',
          ramo: null,
          paga: null,
          reajuste: null,
          ativo: true,
        },
      ),
    );
    expect(
      await screen.findByRole('button', { name: 'Vincular Cobertura' }),
    ).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Criar Plano' })).toBeNull();
  });

  it('vincula a Cobertura existente com centavos e zero, usando os UUIDs do cadastro e do vínculo do Módulo', async () => {
    vi.mocked(planoModuloApi.salvarCobertura).mockResolvedValue({
      publicId: 'cobertura-contextual',
      coberturaPublicId: 'base-morte',
      coberturaAtiva: true,
      nome: 'Morte',
      nomeReduzido: null,
      basica: null,
      reajuste: null,
      premioTitular: 12.34,
      premioConjuge: 0,
      ativo: true,
    });
    renderizar();
    await criarCobertura();
    expect(screen.queryByLabelText('Nome da Cobertura')).toBeNull();
    fireEvent.change(screen.getByLabelText('Prêmio titular (R$)'), {
      target: { value: '12,34' },
    });
    fireEvent.change(screen.getByLabelText('Prêmio cônjuge (R$)'), {
      target: { value: '0' },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar vínculo da Cobertura' }),
    );
    await waitFor(() =>
      expect(planoModuloApi.salvarCobertura).toHaveBeenCalledWith(
        'apolice',
        'vinculo-modulo',
        undefined,
        {
          coberturaPublicId: 'base-morte',
          premioTitular: 12.34,
          premioConjuge: 0,
          ativo: true,
        },
      ),
    );
    expect(await screen.findByText(/12,34/)).toBeTruthy();
    expect(
      screen.queryByRole('form', {
        name: 'Vínculo da Cobertura no Plano do Módulo',
      }),
    ).toBeNull();
  });

  it('rejeita prêmio negativo ou com casas adicionais sem chamar a API', async () => {
    renderizar();
    await criarCobertura();
    fireEvent.change(screen.getByLabelText('Prêmio titular (R$)'), {
      target: { value: '-1' },
    });
    fireEvent.change(screen.getByLabelText('Prêmio cônjuge (R$)'), {
      target: { value: '0' },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar vínculo da Cobertura' }),
    );
    expect(
      await screen.findByText(/Informe os dois prêmios em R\$/),
    ).toBeTruthy();
    fireEvent.change(screen.getByLabelText('Prêmio titular (R$)'), {
      target: { value: '1,234' },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar vínculo da Cobertura' }),
    );
    expect(planoModuloApi.salvarCobertura).not.toHaveBeenCalled();
  });

  it('mantém valores para corrigir e tentar novamente após falha da API', async () => {
    vi.mocked(planoModuloApi.salvarCobertura).mockRejectedValue(
      new Error('Falha na auditoria'),
    );
    renderizar();
    await criarCobertura();
    fireEvent.change(screen.getByLabelText('Prêmio titular (R$)'), {
      target: { value: '1,25' },
    });
    fireEvent.change(screen.getByLabelText('Prêmio cônjuge (R$)'), {
      target: { value: '0' },
    });
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar vínculo da Cobertura' }),
    );
    expect(await screen.findByText('Falha na auditoria')).toBeTruthy();
    expect(
      (screen.getByLabelText('Prêmio titular (R$)') as HTMLInputElement).value,
    ).toBe('1,25');
  });

  it('consulta sem permitir alteração quando há somente a permissão geral da Apólice', async () => {
    vi.mocked(useAuthorization).mockReturnValue({
      possuiPermissao: (p: string) => p === 'apolices.alterar',
    } as unknown as ReturnType<typeof useAuthorization>);
    renderizar();
    expect(await screen.findByText('Plano familiar')).toBeTruthy();
    expect(
      screen.queryByRole('button', { name: 'Vincular Cobertura' }),
    ).toBeNull();
    expect(screen.queryByRole('button', { name: 'Editar Plano' })).toBeNull();
  });

  it('preserva a consulta histórica quando o vínculo está inativo', async () => {
    vi.mocked(planoModuloApi.obter).mockResolvedValue({
      apoliceModuloPublicId: 'vinculo-modulo',
      podeAlterar: false,
      plano,
    });
    renderizar();
    expect(await screen.findByText('Plano familiar')).toBeTruthy();
    expect(screen.getByText(/Apólice ou o Módulo está inativo/)).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Editar Plano' })).toBeNull();
  });

  it('permite editar e inativar a Cobertura existente sem substituí-la por outro cadastro', async () => {
    const cobertura = {
      publicId: 'cobertura-contextual',
      coberturaPublicId: 'base-morte',
      coberturaAtiva: true,
      nome: 'Morte',
      nomeReduzido: 'M',
      basica: null,
      reajuste: false,
      premioTitular: 12.34,
      premioConjuge: 0,
      ativo: true,
    };
    vi.mocked(planoModuloApi.obter).mockResolvedValue({
      apoliceModuloPublicId: 'vinculo-modulo',
      podeAlterar: true,
      plano: { ...plano, coberturas: [cobertura] },
    });
    vi.mocked(planoModuloApi.salvarCobertura).mockResolvedValue({
      ...cobertura,
      ativo: false,
    });
    renderizar();
    fireEvent.click(
      await screen.findByRole('button', { name: 'Editar vínculo' }),
    );
    fireEvent.click(screen.getByLabelText('Vínculo ativo'));
    fireEvent.click(
      screen.getByRole('button', { name: 'Salvar vínculo da Cobertura' }),
    );
    await waitFor(() =>
      expect(planoModuloApi.salvarCobertura).toHaveBeenCalledWith(
        'apolice',
        'vinculo-modulo',
        'cobertura-contextual',
        {
          coberturaPublicId: 'base-morte',
          premioTitular: 12.34,
          premioConjuge: 0,
          ativo: false,
        },
      ),
    );
  });

  it('oferece acesso ao cadastro quando não há Cobertura ativa para vincular', async () => {
    vi.mocked(useAuthorization).mockReturnValue({
      possuiPermissao: () => true,
    } as unknown as ReturnType<typeof useAuthorization>);
    vi.mocked(planoModuloApi.opcoesCoberturas).mockResolvedValue({
      items: [],
      totalCount: 0,
    });
    renderizar();
    fireEvent.click(
      await screen.findByRole('button', { name: 'Vincular Cobertura' }),
    );
    expect(
      await screen.findByText(/Nenhuma Cobertura ativa disponível/),
    ).toBeTruthy();
    expect(
      screen
        .getByRole('link', { name: 'Cadastro de Coberturas' })
        .getAttribute('href'),
    ).toBe('/coberturas');
    expect(
      (
        screen.getByRole('button', {
          name: 'Salvar vínculo da Cobertura',
        }) as HTMLButtonElement
      ).disabled,
    ).toBe(true);
  });

  it('carrega todas as páginas e exclui Coberturas já vinculadas, inclusive vínculos inativos', async () => {
    vi.mocked(planoModuloApi.obter).mockResolvedValue({
      apoliceModuloPublicId: 'vinculo-modulo',
      podeAlterar: true,
      plano: {
        ...plano,
        coberturas: [
          {
            publicId: 'vinculo-inativo',
            coberturaPublicId: 'base-morte',
            coberturaAtiva: true,
            nome: 'Morte',
            nomeReduzido: null,
            basica: null,
            reajuste: null,
            premioTitular: 1,
            premioConjuge: 0,
            ativo: false,
          },
        ],
      },
    });
    vi.mocked(planoModuloApi.opcoesCoberturas).mockImplementation(
      async (_a, _m, pagina) => ({
        items:
          pagina === 1
            ? [{ publicId: 'base-morte', nome: 'Morte' }]
            : [{ publicId: 'base-funeral', nome: 'Funeral' }],
        totalCount: 2,
      }),
    );
    renderizar();
    fireEvent.click(
      await screen.findByRole('button', { name: 'Vincular Cobertura' }),
    );
    expect(await screen.findByRole('option', { name: 'Funeral' })).toBeTruthy();
    expect(screen.queryByRole('option', { name: 'Morte' })).toBeNull();
    expect(planoModuloApi.opcoesCoberturas).toHaveBeenCalledTimes(2);
  });

  it('mantém o cadastro inativo visível e bloqueia a reativação do vínculo até reativar a Cobertura', async () => {
    vi.mocked(planoModuloApi.obter).mockResolvedValue({
      apoliceModuloPublicId: 'vinculo-modulo',
      podeAlterar: true,
      plano: {
        ...plano,
        coberturas: [
          {
            publicId: 'vinculo-inativo',
            coberturaPublicId: 'base-morte',
            coberturaAtiva: false,
            nome: 'Morte',
            nomeReduzido: null,
            basica: null,
            reajuste: null,
            premioTitular: 1,
            premioConjuge: 0,
            ativo: false,
          },
        ],
      },
    });
    renderizar();
    fireEvent.click(
      await screen.findByRole('button', { name: 'Editar vínculo' }),
    );
    expect(
      (screen.getByLabelText('Vínculo ativo') as HTMLInputElement).disabled,
    ).toBe(true);
    expect(screen.getByText(/reative primeiro a Cobertura/)).toBeTruthy();
    expect(screen.queryByLabelText('Cobertura')).toBeNull();
    expect(screen.queryByLabelText('Nome da Cobertura')).toBeNull();
  });
});
