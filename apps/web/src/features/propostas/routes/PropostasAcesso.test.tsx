import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { makeAuthorization } from '../../../../tests/helpers/authorization';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AppNavigation } from '../../../layouts/AuthenticatedLayout/AppNavigation';
import { PropostasConsulta } from '../components/PropostasConsulta';
import { listarPropostas } from '../api/propostas.api';

vi.mock('../../../auth/AuthorizationProvider');
vi.mock('../api/propostas.api');

beforeEach(() => {
  vi.clearAllMocks();
});

describe('Disponibilidade de Propostas no frontend', () => {
  it.each([
    {
      modulo: true,
      recurso: true,
      acessoTotal: false,
      permissao: true,
      permitido: true,
    },
    {
      modulo: true,
      recurso: true,
      acessoTotal: true,
      permissao: false,
      permitido: true,
    },
    {
      modulo: false,
      recurso: true,
      acessoTotal: true,
      permissao: true,
      permitido: false,
    },
    {
      modulo: true,
      recurso: false,
      acessoTotal: true,
      permissao: true,
      permitido: false,
    },
    {
      modulo: true,
      recurso: true,
      acessoTotal: false,
      permissao: false,
      permitido: false,
    },
  ])(
    'protege rota e menu com $modulo/$recurso/$acessoTotal/$permissao',
    async ({ modulo, recurso, acessoTotal, permissao, permitido }) => {
      vi.mocked(useAuthorization).mockReturnValue(
        makeAuthorization({
          acessoTotal,
          modulosHabilitados: modulo ? ['APOLICES'] : [],
          recursosHabilitados: recurso ? ['PROPOSTAS'] : [],
          permissoes: permissao ? ['propostas.visualizar'] : [],
        }),
      );
      render(
        <MemoryRouter initialEntries={['/propostas']}>
          <AppNavigation />
          <Routes>
            <Route
              path="/propostas"
              element={
                <PermissionProtectedRoute
                  moduloCodigo="APOLICES"
                  recursoCodigo="PROPOSTAS"
                  permissaoCodigo="propostas.visualizar"
                >
                  <p>Consulta autorizada</p>
                </PermissionProtectedRoute>
              }
            />
            <Route path="/forbidden" element={<p>Acesso bloqueado</p>} />
          </Routes>
        </MemoryRouter>,
      );
      if (permitido) {
        expect(await screen.findByText('Consulta autorizada')).toBeTruthy();
        expect(screen.getByRole('link', { name: 'Propostas' })).toBeTruthy();
      } else {
        expect(await screen.findByText('Acesso bloqueado')).toBeTruthy();
        expect(screen.queryByRole('link', { name: 'Propostas' })).toBeNull();
      }
    },
  );

  it('não consulta API quando o recurso está desabilitado, inclusive com acesso total', () => {
    vi.mocked(useAuthorization).mockReturnValue(
      makeAuthorization({
        acessoTotal: true,
        modulosHabilitados: ['APOLICES'],
        recursosHabilitados: [],
      }),
    );
    render(<PropostasConsulta />);
    expect(screen.getByText('Acesso restrito')).toBeTruthy();
    expect(listarPropostas).not.toHaveBeenCalled();
  });
});
