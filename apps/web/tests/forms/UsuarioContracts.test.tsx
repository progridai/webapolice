import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { UsuarioForm } from '../../src/features/seguranca/components/UsuarioForm';
import { CadastrarUsuarioPage } from '../../src/features/seguranca/pages/CadastrarUsuarioPage';
import { EditarUsuarioPage } from '../../src/features/seguranca/pages/EditarUsuarioPage';
import {
  criarUsuario,
  atualizarUsuario,
  obterUsuarioDetalhe,
} from '../../src/features/seguranca/api/usuariosApi';
import { listarPerfis } from '../../src/features/seguranca/api/perfisApi';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/features/seguranca/api/usuariosApi', () => ({
  criarUsuario: vi.fn(),
  atualizarUsuario: vi.fn(),
  obterUsuarioDetalhe: vi.fn(),
}));
vi.mock('../../src/features/seguranca/api/perfisApi', () => ({
  listarPerfis: vi.fn(),
}));

const user = {
  username: 'Operador.TESTE',
  nome: 'João da Silva',
  email: 'operador@EXEMPLO.com',
  ativo: true,
  perfilPublicIds: [],
};
const password = 'FiXtUrA!123';
const perfil = {
  publicId: 'perfil-1',
  codigo: 'OPERADOR',
  nome: 'Perfil Ativo',
  descricao: '',
  ativo: true,
  perfilSistema: false,
  acessoTotal: false,
};
const perfis = [
  perfil,
  { ...perfil, publicId: 'perfil-2', nome: 'Perfil Inativo', ativo: false },
];

afterEach(() => {
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

function submit() {
  fireEvent.submit(document.querySelector('form')!);
}
function fillPasswords(senha = password, confirmacao = senha) {
  fireEvent.change(screen.getByLabelText('Senha Temporária'), {
    target: { value: senha },
  });
  fireEvent.change(screen.getByLabelText('Confirmar Senha'), {
    target: { value: confirmacao },
  });
}

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Usuários no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    beforeEach(() => {
      localStorage.setItem('webapolice-tema', theme);
      vi.stubGlobal(
        'matchMedia',
        vi.fn().mockReturnValue({
          matches: dark,
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
        }),
      );
    });
    it('cria com senha confirmada e somente perfis ativos selecionáveis', async () => {
      const onSubmit = vi.fn();
      render(
        <ThemeProvider>
          <UsuarioForm
            initialData={user}
            perfisDisponiveis={perfis}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      fillPasswords();
      fireEvent.click(screen.getByRole('checkbox', { name: 'Perfil Ativo' }));
      expect(
        screen.getByRole<HTMLInputElement>('checkbox', {
          name: 'Perfil Inativo',
        }).disabled,
      ).toBe(true);
      submit();
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toEqual({
        ...user,
        perfilPublicIds: ['perfil-1'],
        senhaTemporaria: password,
        confirmacaoSenhaTemporaria: password,
      });
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
    it('edita sem exigir ou devolver campos de senha', async () => {
      const onSubmit = vi.fn();
      render(
        <ThemeProvider>
          <UsuarioForm
            isEdit
            initialData={user}
            perfisDisponiveis={perfis}
            onSubmit={onSubmit}
            onCancel={vi.fn()}
          />
        </ThemeProvider>,
      );
      expect(screen.queryByLabelText('Senha Temporária')).toBeNull();
      expect(screen.queryByLabelText('Confirmar Senha')).toBeNull();
      expect(screen.getByLabelText<HTMLInputElement>('Username').disabled).toBe(
        true,
      );
      fireEvent.change(screen.getByLabelText('Nome Completo'), {
        target: { value: 'Maria de Souza' },
      });
      fireEvent.click(screen.getByRole('checkbox', { name: /Usuário ativo/ }));
      submit();
      await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
      expect(onSubmit.mock.calls[0][0]).toEqual({
        ...user,
        nome: 'Maria de Souza',
        ativo: false,
      });
      expect(document.documentElement.getAttribute('data-theme')).toBe(
        resolved,
      );
    });
  },
);

describe('Regras de criação de Usuários', () => {
  it.each([
    ['curta', '123', '123', 'Senha Temporária'],
    ['divergente', password, 'outra-senha', 'Confirmar Senha'],
  ])(
    'bloqueia senha %s e associa a mensagem ao campo',
    async (_, senha, confirmacao, label) => {
      const onSubmit = vi.fn();
      render(
        <UsuarioForm
          initialData={user}
          perfisDisponiveis={[]}
          onSubmit={onSubmit}
          onCancel={vi.fn()}
        />,
      );
      fillPasswords(senha, confirmacao);
      submit();
      const field = screen.getByLabelText(label);
      await waitFor(() =>
        expect(field.getAttribute('aria-invalid')).toBe('true'),
      );
      expect(
        document
          .getElementById(field.getAttribute('aria-describedby')!)
          ?.getAttribute('role'),
      ).toBe('alert');
      expect(onSubmit).not.toHaveBeenCalled();
    },
  );

  it('mantém a validação de e-mail também na edição', async () => {
    const onSubmit = vi.fn();
    render(
      <UsuarioForm
        isEdit
        initialData={user}
        perfisDisponiveis={[]}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    fireEvent.change(screen.getByLabelText('E-mail'), {
      target: { value: 'invalido' },
    });
    submit();
    await waitFor(() =>
      expect(screen.getByLabelText('E-mail').getAttribute('aria-invalid')).toBe(
        'true',
      ),
    );
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('desabilita o envio e as senhas durante processamento', () => {
    const onSubmit = vi.fn();
    render(
      <UsuarioForm
        initialData={user}
        isSubmitting
        perfisDisponiveis={[]}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    const save = screen.getByRole<HTMLButtonElement>('button', {
      name: /Cadastrar Usuário/,
    });
    expect(save.disabled).toBe(true);
    expect(save.getAttribute('aria-busy')).toBe('true');
    expect(
      screen.getByLabelText<HTMLInputElement>('Senha Temporária').disabled,
    ).toBe(true);
    fireEvent.click(save);
    expect(onSubmit).not.toHaveBeenCalled();
  });
});

describe('Payload das páginas reais de Usuários', () => {
  beforeEach(() => {
    vi.mocked(listarPerfis).mockResolvedValue({
      itens: perfis,
      paginaAtual: 1,
      tamanhoPagina: 1000,
      totalItens: 2,
      totalPaginas: 1,
    });
    vi.mocked(criarUsuario).mockResolvedValue({ id: 'usuario-1' });
    vi.mocked(atualizarUsuario).mockResolvedValue(undefined);
    vi.mocked(obterUsuarioDetalhe).mockResolvedValue({
      publicId: 'usuario-1',
      ...user,
      keycloakSub: 'keycloak-1',
      ultimoLoginEm: null,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
      perfisAtribuidos: [],
      perfisDisponiveis: perfis,
    });
  });
  function renderPage(edit: boolean) {
    render(
      <MemoryRouter
        initialEntries={[
          edit ? '/usuarios/usuario-1/editar' : '/usuarios/novo',
        ]}
      >
        <Routes>
          <Route path="/usuarios/novo" element={<CadastrarUsuarioPage />} />
          <Route
            path="/usuarios/:publicId/editar"
            element={<EditarUsuarioPage />}
          />
          <Route
            path="/seguranca/usuarios/usuario-1"
            element={<div>Detalhe após salvar</div>}
          />
        </Routes>
      </MemoryRouter>,
    );
  }
  it('envia senha temporária e omite sua confirmação na criação', async () => {
    renderPage(false);
    await screen.findByLabelText('Username');
    fireEvent.change(screen.getByLabelText('Username'), {
      target: { value: user.username },
    });
    fireEvent.change(screen.getByLabelText('Nome Completo'), {
      target: { value: user.nome },
    });
    fireEvent.change(screen.getByLabelText('E-mail'), {
      target: { value: user.email },
    });
    fillPasswords();
    submit();
    expect(await screen.findByText('Detalhe após salvar')).toBeTruthy();
    expect(criarUsuario).toHaveBeenCalledExactlyOnceWith({
      ...user,
      senhaTemporaria: password,
    });
    expect(atualizarUsuario).not.toHaveBeenCalled();
    expect(screen.queryByLabelText('Senha Temporária')).toBeNull();
  });
  it('envia apenas os campos editáveis e aceita salvar sem senha', async () => {
    renderPage(true);
    await screen.findByLabelText('Username');
    submit();
    expect(await screen.findByText('Detalhe após salvar')).toBeTruthy();
    expect(atualizarUsuario).toHaveBeenCalledExactlyOnceWith('usuario-1', {
      nome: user.nome,
      email: user.email,
      ativo: true,
      perfilPublicIds: [],
    });
    expect(criarUsuario).not.toHaveBeenCalled();
  });
});
