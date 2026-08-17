import React, { useEffect, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Skeleton,
  Button,
  Breadcrumbs,
  PageHeader,
  DetailsSection,
  DescriptionList,
  StatusBadge,
  Badge,
  EmptyState,
} from '../../../components/ui';
import { DescriptionItem } from '../../../components/ui/DescriptionList';
import { obterUsuarioDetalhe } from '../api/usuariosApi';
import type { UsuarioDetalheDto } from '../types/seguranca.types';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

export const DetalhesUsuarioPage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  const mainRef = useRef<HTMLElement>(null);
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();

  const [usuario, setUsuario] = useState<UsuarioDetalheDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const podeEditar = possuiAcessoTotal() || possuiPermissao('seguranca.usuarios.alterar');

  useEffect(() => {
    const load = async () => {
      if (!publicId) return;
      setIsLoading(true);
      setError(null);
      try {
        const u = await obterUsuarioDetalhe(publicId);
        setUsuario(u);
      } catch {
        setError('Não foi possível carregar os dados do usuário.');
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, [publicId]);

  useEffect(() => {
    if (!isLoading && mainRef.current) mainRef.current.focus();
  }, [isLoading]);

  useEffect(() => {
    document.title = usuario ? `${usuario.nome} | Usuários | WebApolice` : 'Detalhes do Usuário | WebApolice';
  }, [usuario]);

  if (isLoading) {
    return (
      <main ref={mainRef} tabIndex={-1} className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none" aria-busy="true">
        <Skeleton className="w-32 h-10 mb-4" />
        <Skeleton className="w-full h-32 mb-4" />
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
          <Skeleton className="w-full h-48" />
          <Skeleton className="w-full h-48" />
        </div>
      </main>
    );
  }

  if (error || !usuario) {
    return (
      <main ref={mainRef} tabIndex={-1} className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
        <Button variant="ghost" onClick={() => navigate('/seguranca/usuarios')} className="mb-4">
          ← Voltar para usuários
        </Button>
        <EmptyState
          title="Usuário não encontrado"
          description={error || 'O usuário que você tentou acessar não existe.'}
        />
      </main>
    );
  }

  return (
    <main ref={mainRef} tabIndex={-1} className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
      <PageHeader
        title={usuario.nome}
        titleExtras={<StatusBadge status={usuario.ativo ? 'ativo' : 'inativo'} />}
        description={usuario.username.toLowerCase() !== usuario.nome.toLowerCase() ? usuario.username : undefined}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Usuários', href: '/seguranca/usuarios' },
              { label: usuario.username },
            ]}
          />
        }
        actions={
          <div className="flex flex-wrap gap-2">
            <Button variant="secondary" onClick={() => navigate('/seguranca/usuarios')}>
              Voltar
            </Button>
            {podeEditar && (
              <Button
                variant="primary"
                onClick={() => navigate(`/seguranca/usuarios/${publicId}/editar`)}
              >
                Alterar Usuário
              </Button>
            )}
          </div>
        }
      />
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-3">
        <DetailsSection title="Dados Gerais">
          <DescriptionList columns={1} density="compact">
            <DescriptionItem 
              label="E-mail" 
              value={<a href={`mailto:${usuario.email}`}>{usuario.email}</a>} 
            />
            <DescriptionItem 
              label="Data de Cadastro" 
              value={new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(usuario.createdAt)).replace(',', ' às')} 
            />
            <DescriptionItem 
              label="Último acesso" 
              value={usuario.ultimoLoginEm ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(usuario.ultimoLoginEm)).replace(',', ' às') : <span className="desc-item-empty">Nunca acessou</span>} 
            />
          </DescriptionList>
        </DetailsSection>

        <DetailsSection
          title="Perfis Atribuídos"
          isEmpty={usuario.perfisAtribuidos.length === 0}
          emptyState="Nenhum perfil atribuído a este usuário."
        >
          <div className="flex flex-col gap-2">
            {usuario.perfisAtribuidos.map((perfil) => (
              <div key={perfil.publicId} className="flex justify-between items-center p-3 rounded-lg border border-borda bg-fundo-aplicacao">
                <div className="flex flex-col">
                  <span className="font-medium text-sm text-texto-principal">{perfil.nome}</span>
                  {perfil.codigo.toLowerCase() !== perfil.nome.toLowerCase() && (
                    <span className="text-xs text-texto-secundario font-mono">{perfil.codigo}</span>
                  )}
                </div>
                {perfil.perfilSistema && (
                  <Badge variant="neutral">Sistema</Badge>
                )}
              </div>
            ))}
          </div>
        </DetailsSection>
      </div>
    </main>
  );
};
