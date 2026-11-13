import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ClienteVinculosSection } from './ClienteVinculosSection';
import {
  listarClienteVinculos,
  alterarSituacaoClienteVinculo,
  listarEstipulantesVinculo,
  salvarClienteVinculo,
} from '../api/clienteVinculosApi';
import { ThemeProvider } from '../../../shared/theme/ThemeContext';
vi.mock('../api/clienteVinculosApi');
const vinculo = {
  publicId: 'v1',
  matricula: 'AbC-001',
  ativo: true,
  estipulantePublicId: 'e1',
  estipulante: 'Empresa A',
  grupo: null,
  subgrupo: null,
  lotacao: null,
  apolices: [],
};
beforeEach(() => {
  vi.mocked(listarClienteVinculos).mockResolvedValue({
    items: [
      vinculo,
      { ...vinculo, publicId: 'v2', matricula: 'LEG-2', ativo: false },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 2,
  });
  vi.mocked(listarEstipulantesVinculo).mockResolvedValue({
    items: [{ publicId: 'e1', nome: 'Empresa A' }],
    page: 1,
    pageSize: 20,
    totalCount: 1,
  });
  vi.mocked(salvarClienteVinculo).mockResolvedValue(undefined);
});
afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});
describe('Manutenção de vínculos', () => {
  it.each(['claro', 'escuro', 'sistema'] as const)(
    'identifica apólices por matrícula do mesmo estipulante no tema %s',
    async (tema) => {
      localStorage.setItem('webapolice-tema', tema);
      vi.mocked(listarClienteVinculos).mockResolvedValue({
        items: [
          {
            ...vinculo,
            apolices: [
              {
                publicId: 'a1',
                nome: 'Seguro de Vida',
                numeros: ['aBc-123', '456'],
                ativa: true,
                participacaoAtiva: true,
              },
              {
                publicId: 'a2',
                nome: null,
                numeros: [],
                ativa: false,
                participacaoAtiva: false,
              },
            ],
          },
          {
            ...vinculo,
            publicId: 'v2',
            matricula: 'OUTRA',
            apolices: [
              {
                publicId: 'a3',
                nome: 'Acidentes Pessoais',
                numeros: ['789'],
                ativa: true,
                participacaoAtiva: true,
              },
            ],
          },
          { ...vinculo, publicId: 'v3', matricula: 'SEM-APOLICE' },
        ],
        page: 1,
        pageSize: 10,
        totalCount: 3,
      });
      render(
        <ThemeProvider>
          <ClienteVinculosSection clienteId="cliente" podeAlterar={false} />
        </ThemeProvider>,
      );
      const primeira = within(
        await screen.findByRole('list', {
          name: 'Apólices da matrícula AbC-001',
        }),
      );
      expect(
        primeira.getByText('Seguro de Vida — Nº aBc-123, 456'),
      ).toBeTruthy();
      expect(
        primeira.getByText(
          'Apólice a2 · Apólice inativa · Participação inativa',
        ),
      ).toBeTruthy();
      expect(primeira.queryByText(/Acidentes Pessoais/)).toBeNull();
      const segunda = within(
        screen.getByRole('list', { name: 'Apólices da matrícula OUTRA' }),
      );
      expect(segunda.getByText('Acidentes Pessoais — Nº 789')).toBeTruthy();
      expect(screen.getByText(/Nenhuma apólice associada/)).toBeTruthy();
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        tema === 'escuro' ? 'dark' : 'light',
      );
    },
  );
  it.each(['claro', 'escuro', 'sistema'] as const)(
    'mostra ativos e histórico e inclui vínculo no tema %s',
    async (tema) => {
      localStorage.setItem('webapolice-tema', tema);
      render(
        <ThemeProvider>
          <ClienteVinculosSection clienteId="cliente" podeAlterar />
        </ThemeProvider>,
      );
      expect(await screen.findByText('Inativo')).toBeTruthy();
      fireEvent.click(screen.getByText('Adicionar vínculo'));
      expect(screen.queryByLabelText('Buscar estipulante')).toBeNull();
      await screen.findByRole('option', { name: 'Empresa A' });
      fireEvent.change(screen.getByLabelText(/Estipulante/), {
        target: { value: 'e1' },
      });
      fireEvent.change(screen.getByLabelText(/Matrícula/), {
        target: { value: 'AbC-007' },
      });
      fireEvent.click(screen.getByText('Salvar vínculo'));
      await waitFor(() =>
        expect(salvarClienteVinculo).toHaveBeenCalledWith(
          'cliente',
          undefined,
          { matricula: 'AbC-007', estipulantePublicId: 'e1', ativo: true },
        ),
      );
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        tema === 'escuro' ? 'dark' : 'light',
      );
    },
  );
  it('permite selecionar estipulante de outra página sem filtro de busca', async () => {
    vi.mocked(listarEstipulantesVinculo)
      .mockResolvedValueOnce({
        items: [{ publicId: 'e1', nome: 'Empresa A' }],
        page: 1,
        pageSize: 1,
        totalCount: 2,
      })
      .mockResolvedValueOnce({
        items: [{ publicId: 'e2', nome: 'Empresa B' }],
        page: 2,
        pageSize: 1,
        totalCount: 2,
      });
    render(<ClienteVinculosSection clienteId="cliente" podeAlterar />);
    fireEvent.click(screen.getByText('Adicionar vínculo'));
    await screen.findByRole('option', { name: 'Empresa A' });
    fireEvent.click(
      screen.getByRole('button', { name: 'Ir para a próxima página' }),
    );
    await screen.findByRole('option', { name: 'Empresa B' });
    fireEvent.change(screen.getByLabelText('Estipulante'), {
      target: { value: 'e2' },
    });
    fireEvent.change(screen.getByLabelText('Matrícula'), {
      target: { value: 'PAG-2' },
    });
    fireEvent.click(screen.getByText('Salvar vínculo'));
    await waitFor(() =>
      expect(salvarClienteVinculo).toHaveBeenCalledWith('cliente', undefined, {
        matricula: 'PAG-2',
        estipulantePublicId: 'e2',
        ativo: true,
      }),
    );
    expect(listarEstipulantesVinculo).toHaveBeenCalledWith(
      'cliente',
      '',
      2,
      expect.any(AbortSignal),
    );
  });
  it('permite leitura sem ações de alteração', async () => {
    render(<ClienteVinculosSection clienteId="cliente" podeAlterar={false} />);
    await screen.findByText('Matrícula: AbC-001');
    expect(screen.queryByText('Adicionar vínculo')).toBeNull();
    expect(screen.queryByText('Editar vínculo AbC-001')).toBeNull();
  });
  it('preserva a edição na rejeição do backend e permite nova tentativa', async () => {
    vi.mocked(salvarClienteVinculo).mockRejectedValueOnce(
      new Error('Vínculo já possui participação.'),
    );
    render(<ClienteVinculosSection clienteId="cliente" podeAlterar />);
    fireEvent.click(await screen.findByText('Editar vínculo AbC-001'));
    const matricula = screen.getByLabelText(/Matrícula/);
    await screen.findByRole('option', { name: 'Empresa A' });
    fireEvent.change(matricula, { target: { value: 'CORR-002' } });
    fireEvent.click(screen.getByText('Salvar vínculo'));
    expect(
      await screen.findByText('Vínculo já possui participação.'),
    ).toBeTruthy();
    expect((matricula as HTMLInputElement).value).toBe('CORR-002');
    fireEvent.click(screen.getByText('Salvar vínculo'));
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
    expect(salvarClienteVinculo).toHaveBeenCalledTimes(2);
  });
  it('mostra erro de consulta e recupera com nova tentativa', async () => {
    vi.mocked(listarClienteVinculos).mockRejectedValueOnce(
      new Error('Consulta indisponível.'),
    );
    render(<ClienteVinculosSection clienteId="cliente" podeAlterar />);
    await screen.findByText('Consulta indisponível.');
    fireEvent.click(screen.getByText('Tentar novamente'));
    expect(await screen.findByText('Matrícula: AbC-001')).toBeTruthy();
  });
  it('inativa o vínculo sem modificar matrícula ou estipulante', async () => {
    vi.mocked(alterarSituacaoClienteVinculo).mockResolvedValue(undefined);
    render(<ClienteVinculosSection clienteId="cliente" podeAlterar />);
    fireEvent.click(await screen.findByText('Inativar vínculo AbC-001'));
    await waitFor(() =>
      expect(alterarSituacaoClienteVinculo).toHaveBeenCalledWith(
        'cliente',
        'v1',
        false,
      ),
    );
    expect(salvarClienteVinculo).not.toHaveBeenCalled();
  });
});
