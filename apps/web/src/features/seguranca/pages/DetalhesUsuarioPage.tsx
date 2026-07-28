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
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none" aria-busy="true">
        <Skeleton className="w-32 h-10 mb-4" />
        <Skeleton className="w-full h-32 mb-4" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Skeleton className="w-full h-48" />
          <Skeleton className="w-full h-48" />
        </div>
      </main>
    );
  }

  if (error || !usuario) {
    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
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
    <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none flex flex-col gap-6">
      <PageHeader
        title={`Usuário: ${usuario.nome}`}
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
          <div className="flex gap-2">
            <Button variant="ghost" onClick={() => navigate('/seguranca/usuarios')}>
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

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <DetailsSection title="Dados Gerais">
          <DescriptionList columns={2}>
            <DescriptionItem label="Username" value={usuario.username} />
            <DescriptionItem label="Nome" value={usuario.nome} />
            <DescriptionItem label="E-mail" value={usuario.email} />
            <DescriptionItem
              label="Status"
              value={<StatusBadge status={usuario.ativo ? 'ativo' : 'inativo'} />}
            />
            <DescriptionItem 
              label="Último Login" 
              value={usuario.ultimoLoginEm ? new Date(usuario.ultimoLoginEm).toLocaleString('pt-BR') : 'Nunca acessou'} 
            />
            <DescriptionItem 
              label="Data de Cadastro" 
              value={new Date(usuario.createdAt).toLocaleString('pt-BR')} 
            />
          </DescriptionList>
        </DetailsSection>

        <DetailsSection
          title="Perfis Atribuídos"
          isEmpty={usuario.perfisAtribuidos.length === 0}
          emptyState="Nenhum perfil atribuído a este usuário."
        >
          <div className="flex flex-col gap-3">
            {usuario.perfisAtribuidos.map((perfil) => (
              <div key={perfil.publicId} className="flex justify-between items-center p-3 rounded-md border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50">
                <div className="flex flex-col">
                  <span className="font-medium text-sm text-slate-900 dark:text-slate-100">{perfil.nome}</span>
                  <span className="text-xs text-slate-500 font-mono">{perfil.codigo}</span>
                </div>
                {perfil.perfilSistema && (
                  <Badge variant="neutral" className="text-xs">Sistema</Badge>
                )}
              </div>
            ))}
          </div>
        </DetailsSection>
      </div>
    </main>
  );
};
