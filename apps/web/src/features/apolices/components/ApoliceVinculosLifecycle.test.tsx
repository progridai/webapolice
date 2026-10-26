import { StrictMode } from 'react';
import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ApoliceRamoFormModal } from './tabs/ApoliceRamoFormModal';
import { SubestipulanteApoliceModal } from './modals/SubestipulanteApoliceModal';
import * as api from '../api/apolices.api';
import { ramosApi } from '../../ramos/api/ramos.api';
import { subestipulantesApi } from '../../subestipulantes/api/subestipulantes.api';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';

vi.mock('../api/apolices.api');
vi.mock('../../ramos/api/ramos.api');
vi.mock('../../subestipulantes/api/subestipulantes.api');
const idA = '11111111-1111-4111-8111-111111111111';
const idB = '22222222-2222-4222-8222-222222222222';
type Kind = 'Ramo' | 'Subestipulante';
type Option = { id: string; name: string };
interface Props {
  kind: Kind;
  aberto: boolean;
  record?: 'A' | 'B';
  apolicePublicId: string;
  onClose: () => void;
  onSucesso: () => void;
}
function TestModal({ kind, record, ...props }: Props) {
  if (kind === 'Ramo')
    return (
      <ApoliceRamoFormModal
        {...props}
        ramoEdicao={
          record
            ? {
                publicId: record === 'A' ? idA : idB,
                ramoCodigo: record,
                ramoNome: `Ramo ${record}`,
                numeroApolice: record === 'A' ? 'A-001' : 'B-002',
                iofPercentual: 0,
                ativo: true,
              }
            : undefined
        }
      />
    );
  return (
    <SubestipulanteApoliceModal
      {...props}
      subestipulanteEdicao={
        record
          ? {
              subestipulantePublicId: record === 'A' ? idA : idB,
              nome: `Empresa ${record}`,
              dataInicio: record === 'A' ? '2026-01-01' : '2026-03-01',
              ativo: true,
            }
          : undefined
      }
    />
  );
}
function deferred<T>() {
  let resolve!: (data: T) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<T>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}
function prepare(kind: Kind, props: Partial<Props> = {}) {
  const catalog = vi
    .fn<(signal?: AbortSignal) => Promise<Option[]>>()
    .mockResolvedValue([{ id: idA, name: 'Catálogo Atual' }]);
  const mutation = vi
    .fn<(args: unknown[]) => Promise<void>>()
    .mockResolvedValue(undefined);
  vi.mocked(ramosApi.listar).mockImplementation(async (_query, signal) => ({
    items: (await catalog(signal)).map(({ id, name }) => ({
      publicId: id,
      codigo: 'VG',
      nome: name,
      ativo: true,
      createdAt: '2026-10-22',
      updatedAt: '2026-10-22',
    })),
    totalCount: 1,
  }));
  vi.mocked(subestipulantesApi.listar).mockImplementation(
    async (_query, signal) => ({
      itens: (await catalog(signal)).map(({ id, name }) => ({
        publicId: id,
        nome: name,
        ativo: true,
        createdAt: '2026-10-22',
      })),
      paginaAtual: 1,
      tamanhoPagina: 100,
      totalItens: 1,
      totalPaginas: 1,
    }),
  );
  vi.mocked(api.vincularRamoApolice).mockImplementation((...args) =>
    mutation(args),
  );
  vi.mocked(api.atualizarRamoApolice).mockImplementation((...args) =>
    mutation(args),
  );
  vi.mocked(api.vincularSubestipulanteApolice).mockImplementation((...args) =>
    mutation(args),
  );
  vi.mocked(api.atualizarSubestipulanteApolice).mockImplementation((...args) =>
    mutation(args),
  );
  const initial: Props = {
    kind,
    aberto: true,
    apolicePublicId: 'apolice-A',
    onClose: vi.fn(),
    onSucesso: vi.fn(),
    ...props,
  };
  return { initial, catalog, mutation };
}
const field = (kind: Kind) =>
  screen.getByLabelText<HTMLInputElement>(
    kind === 'Ramo' ? 'Número da Apólice no Ramo' : 'Data de Início',
  );
const initialValue = (kind: Kind) => (kind === 'Ramo' ? 'A-001' : '2026-01-01');
const changedValue = (kind: Kind) =>
  kind === 'Ramo' ? 'ALTERADO-007' : '2026-05-01';
function change(kind: Kind) {
  fireEvent.change(field(kind), { target: { value: changedValue(kind) } });
}
function submit() {
  const form = screen.getByRole('dialog').querySelector('form');
  if (!form) throw new Error('Formulário deve estar aberto');
  fireEvent.submit(form);
}
const save = () =>
  screen.getByRole<HTMLButtonElement>('button', {
    name: /^(Salvar|Adicionar)$/,
  });
const select = (kind: Kind) =>
  screen.getByRole<HTMLSelectElement>('combobox', { name: kind });
async function ready(kind: Kind) {
  await screen.findByRole('option', {
    name: kind === 'Ramo' ? 'VG — Catálogo Atual' : 'Catálogo Atual',
  });
}

beforeEach(() => vi.resetAllMocks());
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

describe.each(['Ramo', 'Subestipulante'] as const)(
  'Ciclo do modal de %s',
  (kind) => {
    it('não monta nem consulta fechado; carrega o catálogo com sinal e filtros ao abrir', async () => {
      const { initial, catalog } = prepare(kind, { aberto: false });
      const pending = deferred<Option[]>();
      catalog.mockReturnValueOnce(pending.promise);
      const { rerender } = render(<TestModal {...initial} />);
      expect(screen.queryByRole('dialog')).toBeNull();
      expect(catalog).not.toHaveBeenCalled();
      rerender(<TestModal {...initial} aberto />);
      expect(select(kind).disabled).toBe(true);
      expect(save().disabled).toBe(true);
      if (kind === 'Ramo')
        expect(ramosApi.listar).toHaveBeenCalledWith(
          { pagina: 1, tamanhoPagina: 100, ativo: true },
          expect.any(AbortSignal),
        );
      else
        expect(subestipulantesApi.listar).toHaveBeenCalledWith(
          { ativo: true, tamanhoPagina: 100 },
          expect.any(AbortSignal),
        );
      await act(async () =>
        pending.resolve([{ id: idA, name: 'Catálogo Atual' }]),
      );
      expect(select(kind).disabled).toBe(false);
      expect(save().disabled).toBe(false);
    });

    it('inicializa edição sem catálogo, preserva IOF zero e mantém alterações em rerender equivalente', () => {
      const { initial, catalog } = prepare(kind, { record: 'A' });
      const { rerender } = render(<TestModal {...initial} />);
      expect(field(kind).value).toBe(initialValue(kind));
      expect(screen.getByLabelText<HTMLInputElement>(kind).readOnly).toBe(true);
      if (kind === 'Ramo')
        expect(screen.getByLabelText<HTMLInputElement>('IOF (%)').value).toBe(
          '0',
        );
      expect(catalog).not.toHaveBeenCalled();
      change(kind);
      rerender(<TestModal {...initial} />); // O objeto de edição é recriado por TestModal.
      expect(field(kind).value).toBe(changedValue(kind));
    });

    it('preserva dados e erro em rerender, limpa ambos ao fechar/reabrir e ao trocar registro', async () => {
      const { initial, mutation } = prepare(kind, { record: 'A' });
      mutation.mockRejectedValueOnce(new Error('Falha Atual'));
      const { rerender } = render(<TestModal {...initial} />);
      change(kind);
      submit();
      await screen.findByText('Falha Atual');
      rerender(<TestModal {...initial} />);
      expect(screen.getByText('Falha Atual')).toBeTruthy();
      expect(field(kind).value).toBe(changedValue(kind));
      rerender(<TestModal {...initial} aberto={false} />);
      rerender(<TestModal {...initial} />);
      expect(field(kind).value).toBe(initialValue(kind));
      expect(screen.queryByRole('alert')).toBeNull();
      change(kind);
      rerender(<TestModal {...initial} record="B" />);
      expect(field(kind).value).toBe(kind === 'Ramo' ? 'B-002' : '2026-03-01');
      rerender(<TestModal {...initial} record={undefined} />);
      expect(field(kind).value).toBe('');
      await ready(kind);
      expect(select(kind).value).toBe('');
    });

    it('reinicializa ao trocar apólice mesmo que o vínculo tenha o mesmo identificador', () => {
      const { initial } = prepare(kind, { record: 'A' });
      const { rerender } = render(<TestModal {...initial} />);
      change(kind);
      rerender(<TestModal {...initial} apolicePublicId="apolice-B" />);
      expect(field(kind).value).toBe(initialValue(kind));
    });

    it('distingue falha de catálogo e vazio, refaz a consulta e não submete ao tentar novamente', async () => {
      const { initial, catalog, mutation } = prepare(kind);
      catalog.mockRejectedValueOnce(null).mockResolvedValueOnce([]);
      render(<TestModal {...initial} />);
      await screen.findByRole('alert');
      expect(save().disabled).toBe(true);
      expect(select(kind).disabled).toBe(true);
      fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
      await screen.findByText(
        kind === 'Ramo'
          ? 'Nenhum ramo ativo disponível.'
          : 'Nenhum subestipulante ativo disponível.',
      );
      expect(screen.queryByRole('alert')).toBeNull();
      expect(save().disabled).toBe(true);
      expect(catalog).toHaveBeenCalledTimes(2);
      expect(catalog.mock.calls[0][0]?.aborted).toBe(true);
      expect(mutation).not.toHaveBeenCalled();
    });

    it.each(['fechar', 'apólice', 'editar', 'desmontar'] as const)(
      'cancela catálogo ao %s e ignora falha tardia',
      async (action) => {
        const { initial, catalog } = prepare(kind);
        const pending = deferred<Option[]>();
        catalog.mockReturnValueOnce(pending.promise);
        const { rerender, unmount } = render(<TestModal {...initial} />);
        const signal = catalog.mock.calls[0][0];
        if (action === 'fechar')
          rerender(<TestModal {...initial} aberto={false} />);
        else if (action === 'apólice')
          rerender(<TestModal {...initial} apolicePublicId="apolice-B" />);
        else if (action === 'editar')
          rerender(<TestModal {...initial} record="A" />);
        else unmount();
        expect(signal?.aborted).toBe(true);
        await act(async () => pending.reject(new Error('Falha Antiga')));
        expect(screen.queryByRole('alert')).toBeNull();
        if (action === 'apólice') await ready(kind);
        if (action === 'editar')
          expect(field(kind).value).toBe(initialValue(kind));
      },
    );

    it('ignora catálogo antigo depois de reabrir e preserva o catálogo atual', async () => {
      const { initial, catalog } = prepare(kind);
      const pending = deferred<Option[]>();
      catalog.mockReturnValueOnce(pending.promise);
      const { rerender } = render(<TestModal {...initial} />);
      rerender(<TestModal {...initial} aberto={false} />);
      rerender(<TestModal {...initial} />);
      await ready(kind);
      await act(async () =>
        pending.resolve([{ id: idB, name: 'Catálogo Antigo' }]),
      );
      expect(
        screen.queryByRole('option', { name: /Catálogo Antigo/ }),
      ).toBeNull();
      expect(save().disabled).toBe(false);
    });

    it('inclui com os identificadores e payloads existentes e notifica uma única vez', async () => {
      const { initial, mutation } = prepare(kind);
      render(<TestModal {...initial} />);
      await ready(kind);
      fireEvent.change(select(kind), { target: { value: idA } });
      change(kind);
      if (kind === 'Ramo')
        fireEvent.change(screen.getByLabelText('IOF (%)'), {
          target: { value: '7.38' },
        });
      submit();
      await waitFor(() => expect(initial.onSucesso).toHaveBeenCalledOnce());
      expect(initial.onClose).toHaveBeenCalledOnce();
      expect(mutation).toHaveBeenCalledExactlyOnceWith(
        kind === 'Ramo'
          ? [
              'apolice-A',
              {
                ramoPublicId: idA,
                numeroApolice: changedValue(kind),
                iofPercentual: 7.38,
              },
            ]
          : [
              'apolice-A',
              {
                subestipulantePublicId: idA,
                dataInicio: changedValue(kind),
                dataFim: undefined,
              },
            ],
      );
    });

    it('bloqueia todos os fechamentos e campos enquanto grava e evita envio duplicado', async () => {
      const { initial, mutation } = prepare(kind, { record: 'A' });
      const pending = deferred<void>();
      mutation.mockReturnValueOnce(pending.promise);
      render(<TestModal {...initial} />);
      submit();
      submit();
      await waitFor(() => expect(mutation).toHaveBeenCalledOnce());
      expect(field(kind).disabled).toBe(true);
      expect(
        screen.getByRole<HTMLButtonElement>('button', { name: 'Cancelar' })
          .disabled,
      ).toBe(true);
      fireEvent.click(screen.getByRole('button', { name: 'Cancelar' }));
      fireEvent.click(screen.getByRole('button', { name: 'Fechar Modal' }));
      fireEvent.keyDown(window, { key: 'Escape' });
      fireEvent.click(screen.getByRole('presentation'));
      submit();
      await act(async () => {});
      expect(mutation).toHaveBeenCalledOnce();
      expect(initial.onClose).not.toHaveBeenCalled();
      await act(async () => pending.resolve());
      expect(initial.onClose).toHaveBeenCalledOnce();
      expect(initial.onSucesso).toHaveBeenCalledOnce();
      expect(mutation).toHaveBeenCalledWith(
        kind === 'Ramo'
          ? ['apolice-A', idA, { numeroApolice: 'A-001', iofPercentual: 0 }]
          : [
              'apolice-A',
              idA,
              { dataInicio: '2026-01-01', dataFim: undefined },
            ],
      );
    });

    it.each([false, true])(
      'descarta conclusão de gravação antiga após trocar contexto (falha: %s)',
      async (failure) => {
        const { initial, mutation } = prepare(kind, { record: 'A' });
        const pending = deferred<void>();
        mutation.mockReturnValueOnce(pending.promise);
        const { rerender } = render(<TestModal {...initial} />);
        submit();
        await waitFor(() => expect(mutation).toHaveBeenCalledOnce());
        rerender(<TestModal {...initial} record="B" />);
        await act(async () => {
          if (failure) pending.reject(new Error('Falha Antiga'));
          else pending.resolve();
        });
        expect(initial.onSucesso).not.toHaveBeenCalled();
        expect(initial.onClose).not.toHaveBeenCalled();
        expect(screen.queryByRole('alert')).toBeNull();
        expect(field(kind).value).toBe(
          kind === 'Ramo' ? 'B-002' : '2026-03-01',
        );
        expect(field(kind).disabled).toBe(false);
        submit();
        await waitFor(() => expect(initial.onSucesso).toHaveBeenCalledOnce());
        expect(mutation).toHaveBeenCalledTimes(2);
      },
    );

    it('não executa gravação cuja validação termina depois da desmontagem', async () => {
      const { initial, mutation } = prepare(kind, { record: 'A' });
      const { unmount } = render(<TestModal {...initial} />);
      submit();
      unmount();
      await act(async () => {});
      expect(mutation).not.toHaveBeenCalled();
    });

    it('valida os campos sem enviar dados inválidos e associa a mensagem ao input', async () => {
      const { initial, mutation } = prepare(kind, { record: 'A' });
      render(<TestModal {...initial} />);
      const invalid = screen.getByLabelText(
        kind === 'Ramo' ? 'IOF (%)' : 'Data de Fim',
      );
      fireEvent.change(invalid, {
        target: { value: kind === 'Ramo' ? '100.01' : '2025-01-01' },
      });
      submit();
      await waitFor(() =>
        expect(invalid.getAttribute('aria-invalid')).toBe('true'),
      );
      expect(
        document
          .getElementById(invalid.getAttribute('aria-describedby')!)
          ?.getAttribute('role'),
      ).toBe('alert');
      expect(mutation).not.toHaveBeenCalled();
    });

    it.each(['Cancelar', 'Fechar Modal', 'Escape', 'fundo'] as const)(
      'permite fechar ocioso por %s',
      (action) => {
        const { initial } = prepare(kind, { record: 'A' });
        render(<TestModal {...initial} />);
        if (action === 'Escape') fireEvent.keyDown(window, { key: 'Escape' });
        else if (action === 'fundo')
          fireEvent.click(screen.getByRole('presentation'));
        else fireEvent.click(screen.getByRole('button', { name: action }));
        expect(initial.onClose).toHaveBeenCalledOnce();
      },
    );

    it('suporta StrictMode sem aceitar a primeira resposta cancelada', async () => {
      const { initial, catalog } = prepare(kind);
      const pending = deferred<Option[]>();
      catalog.mockReturnValueOnce(pending.promise);
      render(
        <StrictMode>
          <TestModal {...initial} />
        </StrictMode>,
      );
      await ready(kind);
      expect(catalog).toHaveBeenCalledTimes(2);
      expect(catalog.mock.calls[0][0]?.aborted).toBe(true);
      await act(async () =>
        pending.resolve([{ id: idB, name: 'Catálogo Antigo' }]),
      );
      expect(
        screen.queryByRole('option', { name: /Catálogo Antigo/ }),
      ).toBeNull();
    });

    it.each([
      { theme: 'claro', dark: false, resolved: 'light' },
      { theme: 'escuro', dark: false, resolved: 'dark' },
      { theme: 'sistema', dark: false, resolved: 'light' },
      { theme: 'sistema', dark: true, resolved: 'dark' },
    ])(
      'carrega, mostra erro e recupera no tema $theme / $resolved',
      async ({ theme, dark, resolved }) => {
        localStorage.setItem('webapolice-tema', theme);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        const { initial, mutation } = prepare(kind);
        mutation.mockRejectedValueOnce(new Error('Falha Atual'));
        render(
          <ThemeProvider>
            <TestModal {...initial} />
          </ThemeProvider>,
        );
        await ready(kind);
        fireEvent.change(select(kind), { target: { value: idA } });
        submit();
        expect(await screen.findByText('Falha Atual')).toBeTruthy();
        expect(initial.onClose).not.toHaveBeenCalled();
        submit();
        await waitFor(() => expect(initial.onClose).toHaveBeenCalledOnce());
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
        if (kind === 'Subestipulante')
          expect(
            within(screen.getByRole('dialog')).getByLabelText('Data de Início')
              .className,
          ).not.toContain('gray');
      },
    );
  },
);
