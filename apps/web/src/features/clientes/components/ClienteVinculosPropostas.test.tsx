import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import { listarPropostas } from '../../propostas/api/propostas.api';
import { proposta } from '../../propostas/components/propostas.fixtures';
import { listarClienteVinculos } from '../api/clienteVinculosApi';
import { ClienteVinculosSection } from './ClienteVinculosSection';
vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../api/clienteVinculosApi');
vi.mock('../../propostas/api/propostas.api');
vi.mock('../../propostas/components/PropostaDetalhes', () => ({
  PropostaDetalhes: ({
    publicId,
    onVoltar,
  }: {
    publicId: string;
    onVoltar: () => void;
  }) => (
    <div>
      Detalhe {publicId}
      <button onClick={onVoltar}>Voltar para propostas</button>
    </div>
  ),
}));
const vinculo = {
  publicId: 'vinculo-A',
  matricula: 'aBc-001',
  ativo: true,
  estipulantePublicId: 'estipulante',
  estipulante: 'Empresa A',
  grupo: null,
  subgrupo: null,
  lotacao: null,
  apolices: [],
};
const pagina = {
  items: [
    proposta,
    { ...proposta, publicId: 'versao-2', numero: 'aBc-456', versao: 2 },
  ],
  page: 1,
  pageSize: 20,
  totalCount: 21,
};
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(useAuthorization).mockReturnValue(
    makeAuthorization({
      modulosHabilitados: ['APOLICES'],
      recursosHabilitados: ['PROPOSTAS'],
      permissoes: ['propostas.visualizar'],
    }),
  );
  vi.mocked(listarClienteVinculos).mockResolvedValue({
    items: [
      vinculo,
      { ...vinculo, publicId: 'vinculo-B', matricula: 'LEG-2', ativo: false },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 2,
  });
  vi.mocked(listarPropostas).mockReset().mockResolvedValue(pagina);
});
function montar(permitido = true) {
  return render(
    <ThemeProvider>
      <ClienteVinculosSection
        clienteId="cliente-A"
        podeAlterar={false}
        podeConsultarPropostas={permitido}
      />
    </ThemeProvider>,
  );
}
async function abrir(matricula = 'aBc-001') {
  fireEvent.click(
    await screen.findByRole('button', {
      name: `Abrir propostas do vínculo ${matricula}`,
    }),
  );
}
it.each(['claro', 'escuro', 'sistema'])(
  'abre versões do vínculo sem exigir alteração de Cliente no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    montar();
    await abrir();
    const modal = screen.getByRole('dialog', {
      name: 'Propostas do vínculo aBc-001',
    });
    const consultar = await within(modal).findByRole('button', {
      name: 'Consultar proposta aBc-456 de João da Silva',
    });
    expect(listarPropostas).toHaveBeenLastCalledWith(
      expect.objectContaining({
        clientePublicId: 'cliente-A',
        clienteVinculoPublicId: 'vinculo-A',
      }),
      expect.any(AbortSignal),
    );
    fireEvent.click(consultar);
    expect(within(modal).getByText('Detalhe versao-2')).toBeTruthy();
    fireEvent.click(
      within(modal).getByRole('button', { name: 'Voltar para propostas' }),
    );
    expect(await within(modal).findByText('aBc-123')).toBeTruthy();
  },
);
it('preserva os dois UUIDs ao paginar e buscar, e permite consultar vínculo inativo', async () => {
  montar();
  await abrir('LEG-2');
  const modal = screen.getByRole('dialog');
  await within(modal).findByText('aBc-123');
  fireEvent.click(
    within(modal).getByRole('button', { name: 'Ir para a próxima página' }),
  );
  await waitFor(() =>
    expect(listarPropostas).toHaveBeenLastCalledWith(
      expect.objectContaining({
        pagina: 2,
        clientePublicId: 'cliente-A',
        clienteVinculoPublicId: 'vinculo-B',
      }),
      expect.any(AbortSignal),
    ),
  );
  fireEvent.change(within(modal).getByLabelText('Número da proposta'), {
    target: { value: 'aBc-456' },
  });
  fireEvent.submit(
    within(modal).getByLabelText('Número da proposta').closest('form')!,
  );
  await waitFor(() =>
    expect(listarPropostas).toHaveBeenLastCalledWith(
      expect.objectContaining({
        pagina: 1,
        termoBusca: 'aBc-456',
        clientePublicId: 'cliente-A',
        clienteVinculoPublicId: 'vinculo-B',
      }),
      expect.any(AbortSignal),
    ),
  );
});
it('oculta a ação sem acesso à consulta', async () => {
  montar(false);
  await screen.findByText('Matrícula: aBc-001');
  expect(screen.queryByRole('button', { name: /Abrir propostas/ })).toBeNull();
  expect(listarPropostas).not.toHaveBeenCalled();
});
it('cancela a consulta ao fechar e ignora resposta antiga ao abrir outro vínculo', async () => {
  let resolver: (dados: typeof pagina) => void = () => {};
  vi.mocked(listarPropostas).mockImplementationOnce(
    () =>
      new Promise((resolve) => {
        resolver = resolve;
      }),
  );
  montar();
  await abrir();
  await waitFor(() => expect(listarPropostas).toHaveBeenCalledTimes(1));
  const signal = vi.mocked(listarPropostas).mock.calls[0][1];
  fireEvent.keyDown(window, { key: 'Escape' });
  expect(signal.aborted).toBe(true);
  await abrir('LEG-2');
  await screen.findByText('aBc-123');
  await act(async () =>
    resolver({
      ...pagina,
      items: [{ ...proposta, numero: 'RESPOSTA-ANTIGA' }],
    }),
  );
  expect(screen.queryByText('RESPOSTA-ANTIGA')).toBeNull();
  expect(listarPropostas).toHaveBeenLastCalledWith(
    expect.objectContaining({ clienteVinculoPublicId: 'vinculo-B' }),
    expect.any(AbortSignal),
  );
});
it('permite repetir a consulta após falha e exibe ausência de propostas', async () => {
  vi.mocked(listarPropostas)
    .mockRejectedValueOnce(new Error('Falha de conexão'))
    .mockResolvedValue({ ...pagina, items: [], totalCount: 0 });
  montar();
  await abrir();
  const modal = screen.getByRole('dialog');
  expect(await within(modal).findByText('Falha de conexão')).toBeTruthy();
  fireEvent.click(
    within(modal).getByRole('button', { name: 'Tentar novamente' }),
  );
  expect(await within(modal).findByText(/Nenhuma proposta/)).toBeTruthy();
});
