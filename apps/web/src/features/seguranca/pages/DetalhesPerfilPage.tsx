import React, { useEffect, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Alert,
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
import { obterPerfilDetalhe } from '../api/perfisApi';
import { obterCatalogo } from '../api/catalogoApi';
import type { PerfilDetalheDto, CatalogoModuloDto } from '../types/seguranca.types';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

const PERFIL_ADMIN_CODIGO = 'ADMINISTRADOR';

export const DetalhesPerfilPage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  const mainRef = useRef<HTMLElement>(null);
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();

  const [perfil, setPerfil] = useState<PerfilDetalheDto | null>(null);
  const [catalogo, setCatalogo] = useState<CatalogoModuloDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const podeEditar = possuiAcessoTotal() || possuiPermissao('seguranca.perfis.alterar');

  useEffect(() => {
    const load = async () => {
      if (!publicId) return;
      setIsLoading(true);
      setError(null);
      try {
        const [p, c] = await Promise.all([obterPerfilDetalhe(publicId), obterCatalogo()]);
        setPerfil(p);
        setCatalogo(c);
      } catch {
        setError('Não foi possível carregar os dados do perfil.');
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
    document.title = perfil ? `${perfil.nome} | Perfis | WebApolice` : 'Detalhes do Perfil | WebApolice';
  }, [perfil]);

  const isAdmin = perfil
    ? perfil.codigo === PERFIL_ADMIN_CODIGO && perfil.perfilSistema && perfil.acessoTotal
    : false;

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

  if (error || !perfil) {
    return (
      <main ref={mainRef} tabIndex={-1} className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
        <Button variant="ghost" onClick={() => navigate('/seguranca/perfis')} className="mb-4">
          ← Voltar para perfis
        </Button>
        <EmptyState
          title="Perfil não encontrado"
          description={error || 'O perfil que você tentou acessar não existe.'}
        />
      </main>
    );
  }

  // Mapear permissões selecionadas do catálogo
  const permSelecionadas = new Set(perfil.permissoesPublicIds);

  return (
    <main ref={mainRef} tabIndex={-1} className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
      <PageHeader
        title={perfil.nome}
        titleExtras={<StatusBadge status={perfil.ativo ? 'ativo' : 'inativo'} />}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Perfis', href: '/seguranca/perfis' },
              { label: perfil.nome },
            ]}
          />
        }
        actions={
          <div className="flex flex-wrap gap-2">
            <Button variant="ghost" onClick={() => navigate('/seguranca/perfis')}>
              Voltar
            </Button>
            {podeEditar && !perfil.perfilSistema && (
              <Button
                variant="primary"
                onClick={() => navigate(`/seguranca/perfis/${publicId}/editar`)}
              >
                Alterar Perfil
              </Button>
            )}
          </div>
        }
      />

      {isAdmin && (
        <Alert variant="warning" title="Perfil protegido do sistema">
          O perfil <strong>ADMINISTRADOR</strong> possui acesso total ao sistema de forma nativa.
          Ele não pode ser editado ou inativado. Suas permissões não dependem de vínculos explícitos.
        </Alert>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <DetailsSection title="Dados do Perfil">
          <DescriptionList columns={1} density="compact">
            {perfil.codigo.toLowerCase() !== perfil.nome.toLowerCase() && (
              <DescriptionItem label="Código" value={perfil.codigo} />
            )}
            <DescriptionItem label="Nome" value={perfil.nome} />
            {perfil.descricao && <DescriptionItem label="Descrição" value={perfil.descricao} />}
            <DescriptionItem
              label="Tipo"
              value={
                <Badge variant={perfil.perfilSistema ? 'neutral' : 'neutral'}>
                  {perfil.perfilSistema ? 'Perfil de Sistema' : 'Personalizado'}
                </Badge>
              }
            />
            <DescriptionItem
              label="Acesso Total"
              value={
                <Badge variant={perfil.acessoTotal ? 'success' : 'neutral'}>
                  {perfil.acessoTotal ? 'Sim' : 'Não'}
                </Badge>
              }
            />
          </DescriptionList>
        </DetailsSection>

        <DetailsSection
          title="Permissões Atribuídas"
          isEmpty={!isAdmin && perfil.permissoesPublicIds.length === 0}
          emptyState="Nenhuma permissão atribuída a este perfil."
        >
          {isAdmin ? (
            <p className="text-sm text-slate-600 dark:text-slate-400">
              Este perfil possui acesso total ao sistema. As permissões individuais não se aplicam.
            </p>
          ) : (
            <div className="flex flex-col gap-3">
              {catalogo.map((modulo) => {
                const recursosComPermissao = modulo.recursos.filter((r) =>
                  r.permissoes.some((p) => permSelecionadas.has(p.publicId))
                );
                if (recursosComPermissao.length === 0) return null;

                return (
                  <div key={modulo.publicId} className="mb-3">
                    <p className="text-xs font-semibold text-texto-secundario uppercase tracking-[0.05em] mb-1">
                      {modulo.nome}
                    </p>
                    {recursosComPermissao.map((recurso) => (
                      <div key={recurso.publicId} className="pl-3 mb-2">
                        {recurso.nome.toLowerCase() !== modulo.nome.toLowerCase() && (
                          <p className="text-sm font-medium text-texto-principal mb-1">
                            {recurso.nome}
                          </p>
                        )}
                        <div className="flex flex-wrap gap-1 pl-3">
                          {recurso.permissoes
                            .filter((p) => permSelecionadas.has(p.publicId))
                            .map((perm) => (
                              <Badge key={perm.publicId} variant="neutral" className="text-xs">
                                {perm.nome}
                              </Badge>
                            ))}
                        </div>
                      </div>
                    ))}
                  </div>
                );
              })}
            </div>
          )}
        </DetailsSection>
      </div>
    </main>
  );
};
