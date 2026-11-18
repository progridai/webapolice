import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
import {
  aplicarCondicoes,
  compararCondicoes,
  obterOpcoesCondicoes,
} from '../api/revisaoCondicoes.api';
import { obterProposta } from '../api/propostas.api';
import type {
  ComparacaoCondicoes,
  CondicoesRegistradas,
} from '../types/revisaoCondicoes.types';
import { RevisaoCondicoesProposta } from './RevisaoCondicoesProposta';
import { PropostaDetalhes } from './PropostaDetalhes';
import { detalhe } from './propostas.fixtures';

vi.mock('../api/revisaoCondicoes.api');
vi.mock('../api/propostas.api');
vi.mock('../../../auth/AuthorizationProvider', () => ({
  useAuthorization: () => ({ possuiPermissao: () => true }),
}));
const atual: CondicoesRegistradas = {
  moduloPublicId: 'modulo',
  moduloNomeRegistrado: 'Módulo Anterior',
  subgrupoPublicId: 'subgrupo',
  subgrupoNomeRegistrado: 'Matriz',
  plano: {
    publicId: 'plano',
    nome: 'Plano Anterior',
    ramo: 'VIDA',
    paga: true,
    reajuste: false,
  },
  coberturas: [
    {
      publicId: 'cobertura',
      vinculoPublicId: 'vinculo',
      nome: 'Proteção',
      nomeReduzido: 'Família',
      basica: 'S',
      reajuste: true,
      premioTitular: 15,
      premioConjuge: 0,
      premioAjustado: true,
      premioTitularInicial: 10,
      premioConjugeInicial: 0,
      justificativaAjustePremio: 'Ajuste Aprovado',
    },
  ],
};
const comparacao: ComparacaoCondicoes = {
  comparacao: 'ABC123',
  revisaoPreenchimento: 0,
  atual,
  modulo: {
    ...atual,
    moduloNomeRegistrado: 'Módulo Atual',
    plano: { ...atual.plano!, nome: 'Plano Atual' },
    coberturas: atual.coberturas.map((c) => ({
      ...c,
      premioTitular: 40,
      premioAjustado: false,
      premioTitularInicial: null,
      premioConjugeInicial: null,
      justificativaAjustePremio: null,
    })),
  },
  mantendoAjustes: { ...atual, moduloNomeRegistrado: 'Módulo Atual' },
  alteracaoModulo: true,
  alteracaoMantendoAjustes: true,
};
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
  vi.mocked(obterOpcoesCondicoes)
    .mockReset()
    .mockResolvedValue({
      modulos: [{ publicId: 'modulo', nome: 'Módulo Atual' }],
      subgrupos: [{ publicId: 'subgrupo', nome: 'Matriz' }],
      moduloPublicId: 'modulo',
      subgrupoPublicId: 'subgrupo',
    });
  vi.mocked(compararCondicoes).mockReset().mockResolvedValue(comparacao);
  vi.mocked(aplicarCondicoes)
    .mockReset()
    .mockResolvedValue({ revisaoPreenchimento: 1 });
  vi.mocked(obterProposta).mockResolvedValue(detalhe);
});
function montar() {
  const props = {
    proposta: detalhe,
    podeAlterar: true,
    disabled: false,
    onBusy: vi.fn(),
    onConcluida: vi.fn(),
    onRecarregar: vi.fn(),
  };
  return { ...render(<RevisaoCondicoesProposta {...props} />), props };
}
async function abrirComparacao() {
  fireEvent.click(screen.getByRole('button', { name: 'Revisar condições' }));
  fireEvent.click(
    await screen.findByRole('button', { name: 'Comparar condições' }),
  );
  return screen.findByRole('form', { name: 'Aplicação das condições' });
}
it.each(['claro', 'escuro', 'sistema'])(
  'compara antes de aplicar com escolha explícita no tema %s',
  async (tema) => {
    localStorage.setItem('webapolice-tema', tema);
    for (const tratamentoAjustes of ['MANTER', 'MODULO']) {
      const concluida = vi.fn();
      const { unmount } = render(
        <ThemeProvider>
          <RevisaoCondicoesProposta
            proposta={detalhe}
            podeAlterar
            disabled={false}
            onBusy={vi.fn()}
            onConcluida={concluida}
            onRecarregar={vi.fn()}
          />
        </ThemeProvider>,
      );
      await abrirComparacao();
      expect(aplicarCondicoes).not.toHaveBeenCalled();
      expect(screen.getByText('Plano Anterior')).toBeTruthy();
      expect(screen.getByText('Plano Atual')).toBeTruthy();
      expect(
        screen.getByLabelText('Tratamento dos ajustes', { exact: false }),
      ).toHaveProperty('value', '');
      fireEvent.click(
        screen.getByRole('button', { name: 'Aplicar condições comparadas' }),
      );
      expect(
        await screen.findByText('Escolha o tratamento dos ajustes.'),
      ).toBeTruthy();
      fireEvent.change(
        screen.getByLabelText('Tratamento dos ajustes', { exact: false }),
        { target: { value: tratamentoAjustes } },
      );
      expect(screen.getByText('Resultado da revisão')).toBeTruthy();
      fireEvent.change(
        screen.getByLabelText('Justificativa da revisão', { exact: false }),
        { target: { value: 'Revisão Aprovada' } },
      );
      fireEvent.click(
        screen.getByRole('button', { name: 'Aplicar condições comparadas' }),
      );
      await waitFor(() => expect(concluida).toHaveBeenCalledOnce());
      expect(aplicarCondicoes).toHaveBeenCalledWith(detalhe.publicId, {
        revisaoEsperada: 0,
        moduloPublicId: 'modulo',
        subgrupoPublicId: 'subgrupo',
        comparacao: 'ABC123',
        tratamentoAjustes,
        observacao: 'Revisão Aprovada',
      });
      unmount();
      vi.mocked(aplicarCondicoes).mockClear();
    }
  },
);
it('mantém a comparação e entradas após erro, permite comparar novamente e recarregar', async () => {
  vi.mocked(aplicarCondicoes).mockRejectedValue(
    new Error('A configuração mudou. Compare novamente.'),
  );
  const { props } = montar();
  await abrirComparacao();
  fireEvent.change(
    screen.getByLabelText('Tratamento dos ajustes', { exact: false }),
    { target: { value: 'MODULO' } },
  );
  const justificativa = screen.getByLabelText('Justificativa da revisão', {
    exact: false,
  });
  fireEvent.change(justificativa, { target: { value: 'Conferência' } });
  fireEvent.click(
    screen.getByRole('button', { name: 'Aplicar condições comparadas' }),
  );
  expect(
    await screen.findByText('A configuração mudou. Compare novamente.'),
  ).toBeTruthy();
  expect(justificativa).toHaveProperty('value', 'Conferência');
  expect(props.onConcluida).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Comparar novamente' }));
  expect(
    screen.getByRole('form', { name: 'Comparação das condições' }),
  ).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Comparar condições' }));
  await screen.findByRole('form', { name: 'Aplicação das condições' });
  expect(
    screen.getByLabelText('Tratamento dos ajustes', { exact: false }),
  ).toHaveProperty('value', '');
});
it('impede envios duplicados e saída enquanto aplica', async () => {
  let resolver: (value: { revisaoPreenchimento: number }) => void = () => {
    throw new Error('Não iniciado');
  };
  vi.mocked(aplicarCondicoes).mockReturnValue(
    new Promise((resolve) => {
      resolver = resolve;
    }),
  );
  const { props } = montar();
  const form = await abrirComparacao();
  fireEvent.change(
    screen.getByLabelText('Tratamento dos ajustes', { exact: false }),
    { target: { value: 'MANTER' } },
  );
  fireEvent.change(
    screen.getByLabelText('Justificativa da revisão', { exact: false }),
    { target: { value: 'Revisão' } },
  );
  fireEvent.submit(form);
  fireEvent.submit(form);
  await waitFor(() => expect(aplicarCondicoes).toHaveBeenCalledOnce());
  expect(
    screen.getByRole('button', { name: 'Voltar sem aplicar' }),
  ).toHaveProperty('disabled', true);
  expect(
    screen.getByRole('button', { name: 'Comparar novamente' }),
  ).toHaveProperty('disabled', true);
  await act(async () => resolver({ revisaoPreenchimento: 1 }));
  expect(props.onConcluida).toHaveBeenCalledOnce();
});
it('carrega com erro, tenta novamente e mostra ausência de Módulos', async () => {
  vi.mocked(obterOpcoesCondicoes)
    .mockRejectedValueOnce(new Error('Indisponível'))
    .mockResolvedValue({
      modulos: [],
      subgrupos: [],
      moduloPublicId: null,
      subgrupoPublicId: null,
    });
  montar();
  fireEvent.click(screen.getByRole('button', { name: 'Revisar condições' }));
  expect(await screen.findByText('Indisponível')).toBeTruthy();
  fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));
  expect(await screen.findByText('Nenhum Módulo disponível')).toBeTruthy();
  expect(compararCondicoes).not.toHaveBeenCalled();
});
it('aborta consulta ao voltar e não grava dados', async () => {
  let signal: AbortSignal | undefined;
  vi.mocked(obterOpcoesCondicoes).mockImplementation((_id, value) => {
    signal = value;
    return new Promise(() => {});
  });
  const { props } = montar();
  fireEvent.click(screen.getByRole('button', { name: 'Revisar condições' }));
  await waitFor(() => expect(signal).toBeDefined());
  fireEvent.click(screen.getByRole('button', { name: 'Voltar sem aplicar' }));
  expect(signal?.aborted).toBe(true);
  expect(aplicarCondicoes).not.toHaveBeenCalled();
  expect(props.onBusy).toHaveBeenLastCalledWith(false);
});
it('aborta comparação pendente na desmontagem e ignora resposta tardia', async () => {
  let signal: AbortSignal | undefined;
  let resolver: (value: ComparacaoCondicoes) => void = () => {};
  vi.mocked(compararCondicoes).mockImplementation((_id, _dados, value) => {
    signal = value;
    return new Promise((resolve) => {
      resolver = resolve;
    });
  });
  const { unmount, props } = montar();
  fireEvent.click(screen.getByRole('button', { name: 'Revisar condições' }));
  fireEvent.click(
    await screen.findByRole('button', { name: 'Comparar condições' }),
  );
  await waitFor(() => expect(signal).toBeDefined());
  unmount();
  expect(signal?.aborted).toBe(true);
  await act(async () => resolver(comparacao));
  expect(props.onConcluida).not.toHaveBeenCalled();
});
it('integra o bloqueio de navegação, confirmação e preenchimento', async () => {
  vi.mocked(obterProposta).mockResolvedValue({
    ...detalhe,
    podeConfirmar: true,
  });
  const busy = vi.fn();
  render(
    <PropostaDetalhes
      publicId={detalhe.publicId}
      onVoltar={vi.fn()}
      onNovaVersao={vi.fn()}
      onBusy={busy}
    />,
  );
  fireEvent.click(
    await screen.findByRole('button', { name: 'Revisar condições' }),
  );
  await screen.findByRole('button', { name: 'Comparar condições' });
  expect(
    screen.getByRole('button', { name: 'Voltar às propostas' }),
  ).toHaveProperty('disabled', true);
  expect(
    screen.getByRole('button', { name: 'Editar pré proposta' }),
  ).toHaveProperty('disabled', true);
  expect(
    screen.getByRole('button', { name: 'Confirmar proposta' }),
  ).toHaveProperty('disabled', true);
  fireEvent.click(
    await screen.findByRole('button', { name: 'Voltar sem aplicar' }),
  );
  expect(
    screen.getByRole('button', { name: 'Voltar às propostas' }),
  ).toHaveProperty('disabled', false);
});
it('respeita permissão, elegibilidade, bloqueio e ausência de diferenças', async () => {
  const { props, rerender } = montar();
  rerender(<RevisaoCondicoesProposta {...props} podeAlterar={false} />);
  expect(screen.queryByRole('button')).toBeNull();
  rerender(
    <RevisaoCondicoesProposta
      {...props}
      proposta={{ ...detalhe, podeRevisarCondicoes: false }}
    />,
  );
  expect(screen.queryByRole('button')).toBeNull();
  rerender(<RevisaoCondicoesProposta {...props} disabled />);
  expect(screen.getByRole('button')).toHaveProperty('disabled', true);
  rerender(<RevisaoCondicoesProposta {...props} />);
  vi.mocked(compararCondicoes).mockResolvedValue({
    ...comparacao,
    alteracaoModulo: false,
  });
  const form = await abrirComparacao();
  fireEvent.change(
    screen.getByLabelText('Tratamento dos ajustes', { exact: false }),
    { target: { value: 'MODULO' } },
  );
  expect(
    within(form).getByRole('button', { name: 'Aplicar condições comparadas' }),
  ).toHaveProperty('disabled', true);
});
