import { fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import * as usuarios from '../../src/features/seguranca/api/usuariosApi';
import * as perfis from '../../src/features/seguranca/api/perfisApi';
import { obterCatalogo } from '../../src/features/seguranca/api/catalogoApi';
import { CadastrarUsuarioPage } from '../../src/features/seguranca/pages/CadastrarUsuarioPage';
import { EditarUsuarioPage } from '../../src/features/seguranca/pages/EditarUsuarioPage';
import { CadastrarPerfilPage } from '../../src/features/seguranca/pages/CadastrarPerfilPage';
import { EditarPerfilPage } from '../../src/features/seguranca/pages/EditarPerfilPage';
import { HttpApiError } from '../../src/services/http/httpError';
import { ThemeProvider } from '../../src/shared/theme/ThemeContext';

vi.mock('../../src/features/seguranca/api/usuariosApi');
vi.mock('../../src/features/seguranca/api/perfisApi');
vi.mock('../../src/features/seguranca/api/catalogoApi');

const perfil: Awaited<ReturnType<typeof perfis.obterPerfilDetalhe>> = {
  publicId: 'perfil-1',
  codigo: 'OPERADOR',
  nome: 'Perfil Teste',
  descricao: 'Descrição Teste',
  ativo: true,
  perfilSistema: false,
  acessoTotal: false,
  permissoesPublicIds: [],
};
const usuario: Awaited<ReturnType<typeof usuarios.obterUsuarioDetalhe>> = {
  publicId: 'usuario-1',
  username: 'Operador.TESTE',
  nome: 'Usuário Teste',
  email: 'operador@EXEMPLO.com',
  ativo: true,
  keycloakSub: 'keycloak-1',
  ultimoLoginEm: null,
  createdAt: '2026-01-01',
  updatedAt: '2026-01-02',
  perfisAtribuidos: [],
  perfisDisponiveis: [perfil],
};
const catalogo: Awaited<ReturnType<typeof obterCatalogo>> = [
  {
    publicId: 'modulo-1',
    codigo: 'CADASTROS',
    nome: 'Cadastros',
    descricao: '',
    icone: '',
    recursos: [
      {
        publicId: 'recurso-1',
        codigo: 'clientes',
        nome: 'Clientes',
        descricao: '',
        rotaFrontend: '/clientes',
        permissoes: [
          {
            publicId: 'permissao-1',
            codigo: 'clientes.visualizar',
            nome: 'Consultar Clientes',
            descricao: '',
          },
        ],
      },
    ],
  },
];
const password = 'FiXtUrA!2026';
type Domain = 'usuarios' | 'perfis';

const scenarios = [
  {
    domain: 'usuarios' as const,
    edit: false,
    fallback:
      'Ocorreu um erro ao cadastrar o usuário. Verifique os dados e tente novamente.',
  },
  {
    domain: 'usuarios' as const,
    edit: true,
    fallback: 'Ocorreu um erro ao salvar o usuário.',
  },
  {
    domain: 'perfis' as const,
    edit: false,
    fallback:
      'Ocorreu um erro ao cadastrar o perfil. Verifique os dados e tente novamente.',
  },
  {
    domain: 'perfis' as const,
    edit: true,
    fallback: 'Ocorreu um erro ao salvar o perfil.',
  },
];

beforeEach(() => {
  vi.resetAllMocks();
  vi.stubGlobal(
    'matchMedia',
    vi.fn().mockReturnValue({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    }),
  );
  vi.spyOn(console, 'error').mockImplementation(() => {});
  vi.mocked(perfis.listarPerfis).mockResolvedValue({
    itens: [perfil],
    paginaAtual: 1,
    tamanhoPagina: 1000,
    totalItens: 1,
    totalPaginas: 1,
  });
  vi.mocked(perfis.obterPerfilDetalhe).mockResolvedValue(perfil);
  vi.mocked(perfis.criarPerfil).mockResolvedValue({ id: 'perfil-1' });
  vi.mocked(perfis.atualizarPerfil).mockResolvedValue(perfil);
  vi.mocked(usuarios.obterUsuarioDetalhe).mockResolvedValue(usuario);
  vi.mocked(usuarios.criarUsuario).mockResolvedValue({ id: 'usuario-1' });
  vi.mocked(usuarios.atualizarUsuario).mockResolvedValue(undefined);
  vi.mocked(obterCatalogo).mockResolvedValue(catalogo);
});
afterEach(() => {
  vi.restoreAllMocks();
  vi.unstubAllGlobals();
  localStorage.removeItem('webapolice-tema');
  document.documentElement.removeAttribute('data-theme');
});

function renderPage(domain: Domain, edit: boolean) {
  const id = domain === 'usuarios' ? 'usuario-1' : 'perfil-1';
  const base = `/seguranca/${domain}`;
  render(
    <ThemeProvider>
      <MemoryRouter
        initialEntries={[edit ? `${base}/${id}/editar` : `${base}/novo`]}
      >
        <Routes>
          <Route
            path="/seguranca/usuarios/novo"
            element={<CadastrarUsuarioPage />}
          />
          <Route
            path="/seguranca/usuarios/:publicId/editar"
            element={<EditarUsuarioPage />}
          />
          <Route
            path="/seguranca/perfis/novo"
            element={<CadastrarPerfilPage />}
          />
          <Route
            path="/seguranca/perfis/:publicId/editar"
            element={<EditarPerfilPage />}
          />
          <Route
            path={`${base}/${id}`}
            element={<div>Detalhe após salvar</div>}
          />
        </Routes>
      </MemoryRouter>
    </ThemeProvider>,
  );
}

async function saveAndRecover(
  domain: Domain,
  edit: boolean,
  error: unknown,
  message: string,
) {
  if (domain === 'usuarios') {
    if (edit) vi.mocked(usuarios.atualizarUsuario).mockRejectedValueOnce(error);
    else vi.mocked(usuarios.criarUsuario).mockRejectedValueOnce(error);
  } else {
    if (edit) vi.mocked(perfis.atualizarPerfil).mockRejectedValueOnce(error);
    else vi.mocked(perfis.criarPerfil).mockRejectedValueOnce(error);
  }
  renderPage(domain, edit);
  const name = await screen.findByLabelText<HTMLInputElement>(
    domain === 'usuarios' ? 'Nome Completo' : 'Nome',
  );
  fireEvent.change(name, { target: { value: 'Cadastro Revisado' } });
  const key = screen.getByLabelText<HTMLInputElement>(
    domain === 'usuarios' ? 'Username' : 'Código',
  );
  expect(key.disabled).toBe(edit);
  if (!edit)
    fireEvent.change(key, {
      target: {
        value: domain === 'usuarios' ? usuario.username : perfil.codigo,
      },
    });
  if (domain === 'usuarios') {
    fireEvent.change(screen.getByLabelText('E-mail'), {
      target: { value: usuario.email },
    });
    if (!edit) {
      fireEvent.change(screen.getByLabelText('Senha Temporária'), {
        target: { value: password },
      });
      fireEvent.change(screen.getByLabelText('Confirmar Senha'), {
        target: { value: password },
      });
    } else expect(screen.queryByLabelText('Senha Temporária')).toBeNull();
  } else
    fireEvent.change(screen.getByLabelText('Descrição'), {
      target: { value: 'Descrição Revisada' },
    });
  const selection = screen.getByRole<HTMLInputElement>('checkbox', {
    name: domain === 'usuarios' ? 'Perfil Teste' : 'Consultar Clientes',
  });
  fireEvent.click(selection);
  const buttonName = edit
    ? 'Salvar Alterações'
    : domain === 'usuarios'
      ? 'Cadastrar Usuário'
      : 'Cadastrar Perfil';
  fireEvent.click(screen.getByRole('button', { name: buttonName }));
  const alert = await screen.findByRole('alert');
  expect(alert.textContent).toContain(message);
  expect(alert.className).toContain('alert-error');
  expect(name.value).toBe('Cadastro Revisado');
  expect(key.value).toBe(
    domain === 'usuarios' ? usuario.username : perfil.codigo,
  );
  expect(selection.checked).toBe(true);
  expect(selection.disabled).toBe(false);
  expect(screen.queryByText('Detalhe após salvar')).toBeNull();
  const button = screen.getByRole<HTMLButtonElement>('button', {
    name: buttonName,
  });
  expect(button.disabled).toBe(false);
  fireEvent.click(button);
  expect(await screen.findByText('Detalhe após salvar')).toBeTruthy();
  if (domain === 'usuarios') {
    const payload = {
      nome: 'Cadastro Revisado',
      email: usuario.email,
      ativo: true,
      perfilPublicIds: ['perfil-1'],
    };
    if (edit) {
      expect(usuarios.atualizarUsuario).toHaveBeenCalledTimes(2);
      expect(usuarios.atualizarUsuario).toHaveBeenLastCalledWith(
        'usuario-1',
        payload,
      );
    } else {
      expect(usuarios.criarUsuario).toHaveBeenCalledTimes(2);
      expect(usuarios.criarUsuario).toHaveBeenLastCalledWith({
        ...payload,
        username: usuario.username,
        senhaTemporaria: password,
      });
      expect(screen.queryByLabelText('Senha Temporária')).toBeNull();
    }
  } else {
    const payload = {
      nome: 'Cadastro Revisado',
      descricao: 'Descrição Revisada',
      ativo: true,
      permissaoPublicIds: ['permissao-1'],
    };
    if (edit) {
      expect(perfis.atualizarPerfil).toHaveBeenCalledTimes(2);
      expect(perfis.atualizarPerfil).toHaveBeenLastCalledWith(
        'perfil-1',
        payload,
      );
    } else {
      expect(perfis.criarPerfil).toHaveBeenCalledTimes(2);
      expect(perfis.criarPerfil).toHaveBeenLastCalledWith({
        ...payload,
        codigo: perfil.codigo,
      });
    }
  }
}

describe.each([
  { theme: 'claro', dark: false, resolved: 'light' },
  { theme: 'escuro', dark: false, resolved: 'dark' },
  { theme: 'sistema', dark: false, resolved: 'light' },
  { theme: 'sistema', dark: true, resolved: 'dark' },
])(
  'Erros de Segurança no tema $theme (sistema escuro: $dark)',
  ({ theme, dark, resolved }) => {
    it.each(scenarios)(
      'preserva mensagem HTTP e seleções: $domain (edição: $edit)',
      async ({ domain, edit }) => {
        localStorage.setItem('webapolice-tema', theme);
        vi.stubGlobal(
          'matchMedia',
          vi.fn().mockReturnValue({
            matches: dark,
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
          }),
        );
        await saveAndRecover(
          domain,
          edit,
          new HttpApiError({ status: 409, message: 'Cadastro em conflito.' }),
          'Cadastro em conflito.',
        );
        expect(document.documentElement.getAttribute('data-theme')).toBe(
          resolved,
        );
      },
    );
  },
);

describe.each(scenarios)(
  'Contingências: $domain (edição: $edit)',
  ({ domain, edit, fallback }) => {
    it.each([
      { name: 'null', error: null },
      { name: 'mensagem vazia', error: new Error('') },
      {
        name: 'formato antigo',
        error: { response: { data: { message: 'Objeto bruto' } } },
      },
    ])('trata $name sem perder os dados e seleções', async ({ error }) => {
      await saveAndRecover(domain, edit, error, fallback);
    });
  },
);

it.each(scenarios)(
  'encerra o carregamento com falha de consulta: $domain (edição: $edit)',
  async ({ domain, edit }) => {
    if (domain === 'usuarios')
      vi.mocked(perfis.listarPerfis).mockRejectedValueOnce(null);
    else vi.mocked(obterCatalogo).mockRejectedValueOnce(null);
    renderPage(domain, edit);
    expect((await screen.findByRole('alert')).textContent).toContain(
      edit ? 'Não foi possível carregar os dados' : 'Erro ao carregar',
    );
    if (edit)
      expect(
        screen.queryByRole('button', { name: 'Salvar Alterações' }),
      ).toBeNull();
  },
);

it('mantém a proteção de edição do perfil ADMINISTRADOR', async () => {
  vi.mocked(perfis.obterPerfilDetalhe).mockResolvedValueOnce({
    ...perfil,
    codigo: 'ADMINISTRADOR',
    perfilSistema: true,
    acessoTotal: true,
  });
  renderPage('perfis', true);
  expect((await screen.findByRole('alert')).textContent).toContain(
    'não pode ser editado',
  );
  expect(
    screen.queryByRole('button', { name: 'Salvar Alterações' }),
  ).toBeNull();
  expect(perfis.atualizarPerfil).not.toHaveBeenCalled();
});

it.each(['nome com espaco', 'usuário', 'user@domain'])(
  'rejeita username técnico inválido: %s',
  async (username) => {
    renderPage('usuarios', false);
    fireEvent.change(await screen.findByLabelText(/Username/), {
      target: { value: username },
    });
    fireEvent.change(screen.getByLabelText(/Nome Completo/), {
      target: { value: 'Usuário Teste' },
    });
    fireEvent.change(screen.getByLabelText(/E-mail/), {
      target: { value: 'usuario@example.com' },
    });
    fireEvent.change(screen.getByLabelText(/Senha Temporária/), {
      target: { value: password },
    });
    fireEvent.change(screen.getByLabelText(/Confirmar Senha/), {
      target: { value: password },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Cadastrar Usuário' }));
    expect(
      await screen.findByText(
        'Use apenas letras, números, ponto, hífen ou sublinhado, sem espaços',
      ),
    ).toBeTruthy();
    expect(usuarios.criarUsuario).not.toHaveBeenCalled();
  },
);

it('remove espaços externos do username sem alterar seu código técnico', async () => {
  renderPage('usuarios', false);
  fireEvent.change(await screen.findByLabelText(/Username/), {
    target: { value: '  Operador.TESTE_1-2  ' },
  });
  fireEvent.change(screen.getByLabelText(/Nome Completo/), {
    target: { value: 'Usuário Teste' },
  });
  fireEvent.change(screen.getByLabelText(/E-mail/), {
    target: { value: 'usuario@example.com' },
  });
  fireEvent.change(screen.getByLabelText(/Senha Temporária/), {
    target: { value: password },
  });
  fireEvent.change(screen.getByLabelText(/Confirmar Senha/), {
    target: { value: password },
  });
  fireEvent.click(screen.getByRole('button', { name: 'Cadastrar Usuário' }));
  await vi.waitFor(() =>
    expect(usuarios.criarUsuario).toHaveBeenCalledTimes(1),
  );
  expect(usuarios.criarUsuario).toHaveBeenCalledWith(
    expect.objectContaining({ username: 'Operador.TESTE_1-2' }),
  );
});
