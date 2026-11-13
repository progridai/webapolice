import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ExcluirVinculoButton } from './ExcluirVinculoButton';
import {
  excluirVinculoApolice,
  permissoesExclusaoVinculos,
  type TipoVinculoApolice,
} from '../api/exclusaoVinculos.api';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { Modal } from '../../../components/ui';
import { RamosTab } from './tabs/RamosTab';
import type { ApoliceDetalheResponse } from '../types/apolice.types';
vi.mock('../api/exclusaoVinculos.api', async (importOriginal) => {
  const original =
    await importOriginal<typeof import('../api/exclusaoVinculos.api')>();
  return { ...original, excluirVinculoApolice: vi.fn() };
});
vi.mock('../../../auth/AuthorizationProvider');
beforeEach(() => {
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      permissoes: Object.values(permissoesExclusaoVinculos),
    }),
  );
  vi.mocked(excluirVinculoApolice).mockResolvedValue(undefined);
});
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});
const tipos: TipoVinculoApolice[] = [
  'ramos',
  'subgrupos',
  'subestipulantes',
  'modulos',
  'planos-modulo',
  'coberturas-modulo',
  'produtos-anteriores',
  'planos-anteriores',
  'coberturas-anteriores',
];
describe.each(['claro', 'escuro', 'sistema'] as const)(
  'Exclusão no tema %s',
  (tema) => {
    it.each(tipos)('confirma o UUID contextual para %s', async (tipo) => {
      localStorage.setItem('webapolice-tema', tema);
      const sucesso = vi.fn();
      render(
        <ThemeProvider>
          <ExcluirVinculoButton
            apolicePublicId="apolice"
            tipo={tipo}
            vinculoPublicId="contextual"
            identificacao="Vínculo Teste"
            onSucesso={sucesso}
          />
        </ThemeProvider>,
      );
      fireEvent.click(
        screen.getByRole('button', { name: 'Excluir Vínculo Teste' }),
      );
      expect(excluirVinculoApolice).not.toHaveBeenCalled();
      fireEvent.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Excluir',
        }),
      );
      await waitFor(() => expect(sucesso).toHaveBeenCalledOnce());
      expect(excluirVinculoApolice).toHaveBeenCalledWith(
        'apolice',
        tipo,
        'contextual',
      );
      expect(screen.queryByRole('dialog')).toBeNull();
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        tema === 'escuro' ? 'dark' : 'light',
      );
    });
  },
);
it('oculta sem permissão e sem UUID contextual', () => {
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({ permissoes: ['apolices.modulos.alterar'] }),
  );
  const { rerender } = render(
    <ExcluirVinculoButton
      apolicePublicId="a"
      tipo="modulos"
      vinculoPublicId="m"
      identificacao="Módulo"
      onSucesso={vi.fn()}
    />,
  );
  expect(screen.queryByRole('button')).toBeNull();
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({ permissoes: ['apolices.modulos.excluir'] }),
  );
  rerender(
    <ExcluirVinculoButton
      apolicePublicId="a"
      tipo="modulos"
      identificacao="Módulo"
      onSucesso={vi.fn()}
    />,
  );
  expect(screen.queryByRole('button')).toBeNull();
});
it('cancela, informa dependência e permite nova tentativa', async () => {
  const sucesso = vi.fn();
  vi.mocked(excluirVinculoApolice)
    .mockRejectedValueOnce(new Error('Existem coberturas neste plano.'))
    .mockResolvedValueOnce(undefined);
  render(
    <ExcluirVinculoButton
      apolicePublicId="a"
      tipo="planos-modulo"
      vinculoPublicId="p"
      identificacao="Plano"
      onSucesso={sucesso}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Excluir Plano' }));
  fireEvent.click(
    within(screen.getByRole('dialog')).getByRole('button', {
      name: 'Cancelar',
    }),
  );
  expect(excluirVinculoApolice).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Excluir Plano' }));
  fireEvent.click(
    within(screen.getByRole('dialog')).getByRole('button', { name: 'Excluir' }),
  );
  expect(
    await screen.findByText('Existem coberturas neste plano.'),
  ).toBeTruthy();
  expect(sucesso).not.toHaveBeenCalled();
  fireEvent.click(
    within(screen.getByRole('dialog')).getByRole('button', { name: 'Excluir' }),
  );
  await waitFor(() => expect(sucesso).toHaveBeenCalledOnce());
});
it('impede envio duplicado e fechamento enquanto a exclusão está pendente', async () => {
  let resolver!: () => void;
  vi.mocked(excluirVinculoApolice).mockReturnValueOnce(
    new Promise<void>((resolve) => {
      resolver = resolve;
    }),
  );
  render(
    <ExcluirVinculoButton
      apolicePublicId="a"
      tipo="modulos"
      vinculoPublicId="m"
      identificacao="Módulo"
      onSucesso={vi.fn()}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Excluir Módulo' }));
  const dialogo = within(screen.getByRole('dialog'));
  fireEvent.click(dialogo.getByRole('button', { name: 'Excluir' }));
  fireEvent.click(dialogo.getByRole('button', { name: 'Cancelar' }));
  expect(screen.getByRole('dialog')).toBeTruthy();
  expect(excluirVinculoApolice).toHaveBeenCalledTimes(1);
  await act(async () => resolver());
  expect(screen.queryByRole('dialog')).toBeNull();
});

it('Escape fecha a confirmação e mantém o editor pai aberto', () => {
  const fecharPai = vi.fn();
  render(
    <Modal aberto title="Editor do Módulo" onClose={fecharPai}>
      <ExcluirVinculoButton
        apolicePublicId="a"
        tipo="planos-modulo"
        vinculoPublicId="p"
        identificacao="Plano"
        onSucesso={vi.fn()}
      />
    </Modal>,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Excluir Plano' }));
  expect(screen.getByRole('dialog', { name: 'Excluir Vínculo' })).toBeTruthy();
  fireEvent.keyDown(document.activeElement ?? document.body, { key: 'Escape' });
  expect(screen.queryByRole('dialog', { name: 'Excluir Vínculo' })).toBeNull();
  expect(screen.getByRole('dialog', { name: 'Editor do Módulo' })).toBeTruthy();
  expect(fecharPai).not.toHaveBeenCalled();
});

it('exclui o vínculo escolhido quando dois ramos compartilham o cadastro global', async () => {
  const apolice: ApoliceDetalheResponse = {
    publicId: 'apolice',
    nome: 'Apólice',
    estipulanteId: 1,
    estipulanteNome: 'Empresa',
    seguradoraId: 1,
    seguradoraNome: 'Seguradora',
    dataInicioVigencia: '2026-01-01',
    status: 'ativa',
    ativo: true,
    ramos: [
      {
        publicId: 'ramo-global',
        vinculoPublicId: 'contexto-1',
        ramoNome: 'Vida',
        ramoCodigo: '09',
        ativo: true,
        numeroApolice: 'AT-1',
      },
      {
        publicId: 'ramo-global',
        vinculoPublicId: 'contexto-2',
        ramoNome: 'Vida',
        ramoCodigo: '09',
        ativo: false,
        numeroApolice: 'ANT-2',
      },
    ],
  };
  const sucesso = vi.fn();
  const { rerender } = render(
    <RamosTab apolice={apolice} onMutateSucesso={sucesso} />,
  );
  fireEvent.click(
    screen.getAllByRole('button', { name: 'Excluir Ramo Vida' })[0],
  );
  fireEvent.click(
    within(screen.getByRole('dialog')).getByRole('button', { name: 'Excluir' }),
  );
  await waitFor(() => expect(sucesso).toHaveBeenCalledOnce());
  expect(excluirVinculoApolice).toHaveBeenLastCalledWith(
    'apolice',
    'ramos',
    'contexto-1',
  );
  rerender(
    <RamosTab
      apolice={{ ...apolice, ramos: apolice.ramos.slice(1) }}
      onMutateSucesso={sucesso}
    />,
  );
  fireEvent.click(screen.getByRole('button', { name: 'Excluir Ramo Vida' }));
  fireEvent.click(
    within(screen.getByRole('dialog')).getByRole('button', { name: 'Excluir' }),
  );
  await waitFor(() => expect(sucesso).toHaveBeenCalledTimes(2));
  expect(excluirVinculoApolice).toHaveBeenLastCalledWith(
    'apolice',
    'ramos',
    'contexto-2',
  );
});
