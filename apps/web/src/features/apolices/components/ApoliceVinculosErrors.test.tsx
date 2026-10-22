import type { ReactElement } from 'react';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import * as api from '../api/apolices.api';
import { HttpApiError } from '../../../services/http/httpError';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { useApoliceSubgrupos } from '../hooks/useApoliceSubgrupos';
import { useApoliceSubestipulantes } from '../hooks/useApoliceSubestipulantes';
import { RamosTab } from './tabs/RamosTab';
import { SubgruposTab } from './tabs/SubgruposTab';
import { SubestipulantesTab } from './tabs/SubestipulantesTab';
import { ApoliceRamoFormModal } from './tabs/ApoliceRamoFormModal';
import { SubestipulanteApoliceModal } from './modals/SubestipulanteApoliceModal';
import type {
  ApoliceDetalheResponse,
  ApoliceRamoResult,
  ApoliceSubestipulanteResult,
} from '../types/apolice.types';

vi.mock('../api/apolices.api');
vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../hooks/useApoliceSubgrupos');
vi.mock('../hooks/useApoliceSubestipulantes');

const ramo: ApoliceRamoResult = {
  publicId: 'ramo-1',
  ramoCodigo: 'VG',
  ramoNome: 'Vida em Grupo',
  numeroApolice: '1234',
  iofPercentual: 7.38,
  ativo: true,
};
const subestipulante: ApoliceSubestipulanteResult = {
  subestipulantePublicId: '11111111-1111-4111-8111-111111111111',
  nome: 'Empresa',
  dataInicio: '2026-01-01',
  ativo: true,
};
const apolice: ApoliceDetalheResponse = {
  publicId: 'apolice-1',
  nome: 'Contrato',
  estipulanteId: 1,
  estipulanteNome: 'Empresa',
  seguradoraId: 2,
  seguradoraNome: 'Seguradora',
  status: 'Ativa',
  ativo: true,
  ramos: [ramo],
};

interface Scenario {
  name: string;
  render: (onSuccess: () => void, onClose: () => void) => ReactElement;
  open?: () => void;
  usesRefetch?: boolean;
  button: string;
  reject: (error: unknown) => void;
  resolve: () => void;
  expectRequest: () => void;
  fallback: string;
}

const scenarios: Scenario[] = [
  {
    name: 'inativar Ramo',
    render: (onSuccess) => (
      <RamosTab apolice={apolice} onMutateSucesso={onSuccess} />
    ),
    open: () =>
      fireEvent.click(screen.getByRole('button', { name: 'Inativar' })),
    button: 'Inativar Vínculo',
    reject: (error) =>
      vi.mocked(api.inativarRamoApolice).mockRejectedValueOnce(error),
    resolve: () =>
      vi.mocked(api.inativarRamoApolice).mockResolvedValue(undefined),
    expectRequest: () =>
      expect(api.inativarRamoApolice).toHaveBeenLastCalledWith(
        'apolice-1',
        'ramo-1',
      ),
    fallback: 'Ocorreu um erro inesperado.',
  },
  {
    name: 'inativar Subgrupo',
    usesRefetch: true,
    render: () => <SubgruposTab publicId="apolice-1" />,
    open: () =>
      fireEvent.click(screen.getByRole('button', { name: 'Inativar' })),
    button: 'Inativar',
    reject: (error) =>
      vi.mocked(api.inativarApoliceSubgrupo).mockRejectedValueOnce(error),
    resolve: () =>
      vi.mocked(api.inativarApoliceSubgrupo).mockResolvedValue(undefined),
    expectRequest: () =>
      expect(api.inativarApoliceSubgrupo).toHaveBeenLastCalledWith(
        'apolice-1',
        'subgrupo-1',
      ),
    fallback: 'Erro ao inativar o subgrupo.',
  },
  {
    name: 'inativar Subestipulante',
    usesRefetch: true,
    render: () => <SubestipulantesTab publicId="apolice-1" />,
    open: () =>
      fireEvent.click(screen.getByRole('button', { name: 'Inativar' })),
    button: 'Inativar Vínculo',
    reject: (error) =>
      vi.mocked(api.inativarSubestipulanteApolice).mockRejectedValueOnce(error),
    resolve: () =>
      vi.mocked(api.inativarSubestipulanteApolice).mockResolvedValue(undefined),
    expectRequest: () =>
      expect(api.inativarSubestipulanteApolice).toHaveBeenLastCalledWith(
        'apolice-1',
        subestipulante.subestipulantePublicId,
      ),
    fallback: 'Ocorreu um erro ao inativar o vínculo.',
  },
  {
    name: 'salvar Ramo',
    render: (onSuccess, onClose) => (
      <ApoliceRamoFormModal
        aberto
        apolicePublicId="apolice-1"
        ramoEdicao={ramo}
        onSucesso={onSuccess}
        onClose={onClose}
      />
    ),
    button: 'Salvar',
    reject: (error) =>
      vi.mocked(api.atualizarRamoApolice).mockRejectedValueOnce(error),
    resolve: () =>
      vi.mocked(api.atualizarRamoApolice).mockResolvedValue(undefined),
    expectRequest: () =>
      expect(api.atualizarRamoApolice).toHaveBeenLastCalledWith(
        'apolice-1',
        'ramo-1',
        { numeroApolice: '1234', iofPercentual: 7.38 },
      ),
    fallback: 'Erro inesperado.',
  },
  {
    name: 'salvar Subestipulante',
    render: (onSuccess, onClose) => (
      <SubestipulanteApoliceModal
        aberto
        apolicePublicId="apolice-1"
        subestipulanteEdicao={subestipulante}
        onSucesso={onSuccess}
        onClose={onClose}
      />
    ),
    button: 'Salvar',
    reject: (error) =>
      vi
        .mocked(api.atualizarSubestipulanteApolice)
        .mockRejectedValueOnce(error),
    resolve: () =>
      vi
        .mocked(api.atualizarSubestipulanteApolice)
        .mockResolvedValue(undefined),
    expectRequest: () =>
      expect(api.atualizarSubestipulanteApolice).toHaveBeenLastCalledWith(
        'apolice-1',
        subestipulante.subestipulantePublicId,
        { dataInicio: '2026-01-01', dataFim: undefined },
      ),
    fallback: 'Ocorreu um erro ao salvar o vínculo.',
  },
];

beforeEach(() => vi.resetAllMocks());
afterEach(() => {
  vi.unstubAllGlobals();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Falhas de vínculos no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    describe.each(scenarios)('$name', (scenario) => {
      it.each([
        {
          name: 'mensagem HTTP normalizada',
          error: new HttpApiError({ status: 409, message: 'Vínculo em uso.' }),
        },
        { name: 'rejeição sem erro', error: null },
        { name: 'mensagem vazia', error: new Error('') },
      ])(
        'exibe $name, conserva a edição e permite tentar novamente',
        async ({ error }) => {
          const onSuccess = vi.fn();
          const onClose = vi.fn();
          vi.mocked(useAuthorization).mockReturnValue(
            makeAuthorization({
              permissoes: [
                'apolices.ramos.inativar',
                'apolices.subgrupos.inativar',
                'apolices.subestipulantes.inativar',
              ],
            }),
          );
          const refetch = vi
            .fn<() => Promise<void>>()
            .mockResolvedValue(undefined);
          vi.mocked(useApoliceSubgrupos).mockReturnValue({
            data: [
              { subgrupoPublicId: 'subgrupo-1', nome: 'Grupo', ativo: true },
            ],
            error: null,
            isLoading: false,
            refetch,
          });
          vi.mocked(useApoliceSubestipulantes).mockReturnValue({
            data: [subestipulante],
            error: null,
            isLoading: false,
            refetch,
          });
          localStorage.setItem('webapolice-tema', theme);
          vi.stubGlobal(
            'matchMedia',
            vi.fn().mockReturnValue({
              matches: dark,
              addEventListener: vi.fn(),
              removeEventListener: vi.fn(),
            }),
          );
          scenario.reject(error);
          scenario.resolve();
          render(
            <ThemeProvider>
              {scenario.render(onSuccess, onClose)}
            </ThemeProvider>,
          );
          scenario.open?.();
          const fieldValues = Array.from(
            screen.getByRole('dialog').querySelectorAll('input'),
            (input) => input.value,
          );
          const button = within(
            screen.getByRole('dialog'),
          ).getByRole<HTMLButtonElement>('button', { name: scenario.button });
          fireEvent.click(button);
          const alert = await screen.findByRole('alert');
          expect(alert.textContent).toContain(
            error instanceof Error && error.message
              ? error.message
              : scenario.fallback,
          );
          expect(alert.className).toContain('alert-error');
          scenario.expectRequest();
          expect(onSuccess).not.toHaveBeenCalled();
          expect(onClose).not.toHaveBeenCalled();
          expect(refetch).not.toHaveBeenCalled();
          expect(screen.getByRole('dialog')).toBeTruthy();
          expect(
            Array.from(
              screen.getByRole('dialog').querySelectorAll('input'),
              (input) => input.value,
            ),
          ).toEqual(fieldValues);
          expect(document.documentElement.getAttribute('data-theme')).toBe(
            resolved,
          );
          expect(button.disabled).toBe(false);

          fireEvent.click(button);
          if (scenario.open) {
            await waitFor(() =>
              expect(screen.queryByRole('dialog')).toBeNull(),
            );
          } else {
            await waitFor(() => expect(onClose).toHaveBeenCalledOnce());
          }
          if (scenario.usesRefetch) expect(refetch).toHaveBeenCalledOnce();
          else expect(onSuccess).toHaveBeenCalledOnce();
          scenario.expectRequest();
        },
      );
    });
  },
);
