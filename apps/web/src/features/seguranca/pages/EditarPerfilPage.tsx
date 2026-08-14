import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PerfilForm, type PerfilFormData } from '../components/PerfilForm';
import { obterPerfilDetalhe, atualizarPerfil } from '../api/perfisApi';
import { obterCatalogo } from '../api/catalogoApi';
import { Alert, PageHeader, Breadcrumbs, Spinner, Button } from '../../../components/ui';
import type { CatalogoModuloDto } from '../types/seguranca.types';

const PERFIL_ADMIN_CODIGO = 'ADMINISTRADOR';

export const EditarPerfilPage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();

  const [initialData, setInitialData] = useState<Partial<PerfilFormData> | null>(null);
  const [catalogo, setCatalogo] = useState<CatalogoModuloDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isProtegido, setIsProtegido] = useState(false);

  useEffect(() => {
    document.title = 'Editar Perfil | WebApolice';
    const fetch = async () => {
      if (!publicId) return;
      try {
        const [cat, perfil] = await Promise.all([obterCatalogo(), obterPerfilDetalhe(publicId)]);
        
        if (perfil.codigo === PERFIL_ADMIN_CODIGO && perfil.perfilSistema && perfil.acessoTotal) {
          setIsProtegido(true);
          setIsLoading(false);
          return;
        }

        setCatalogo(cat);
        setInitialData({
          codigo: perfil.codigo,
          nome: perfil.nome,
          descricao: perfil.descricao || '',
          ativo: perfil.ativo,
          permissaoPublicIds: perfil.permissoesPublicIds,
        });
      } catch (err: unknown) {
        console.error('Erro ao carregar perfil:', err);
        setError('Não foi possível carregar os dados do perfil.');
      } finally {
        setIsLoading(false);
      }
    };
    fetch();
  }, [publicId]);

  const handleSubmit = async (data: PerfilFormData) => {
    if (!publicId) return;
    setIsSubmitting(true);
    setError(null);
    try {
      await atualizarPerfil(publicId, {
        nome: data.nome,
        descricao: data.descricao || '',
        ativo: data.ativo,
        permissaoPublicIds: data.permissaoPublicIds,
      });
      navigate(`/seguranca/perfis/${publicId}`);
    } catch (err: unknown) {
      console.error('Erro ao editar perfil:', err);
      const errorResponse = err as { response?: { status?: number; data?: { message?: string } } };
      setError(errorResponse.response?.data?.message || 'Ocorreu um erro ao salvar o perfil.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spinner size="lg" />
      </div>
    );
  }

  if (isProtegido) {
    return (
      <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
        <Alert variant="warning" title="Acesso Restrito">
          O perfil <strong>ADMINISTRADOR</strong> é protegido pelo sistema e não pode ser editado.
        </Alert>
        <div className="flex gap-4 mt-4">
          <Button variant="primary" onClick={() => navigate('/seguranca/perfis')}>
            Voltar para Perfis
          </Button>
          <Button variant="secondary" onClick={() => navigate(`/seguranca/perfis/${publicId}`)}>
            Ver Detalhes
          </Button>
        </div>
      </main>
    );
  }

  if (error && !initialData) {
    return (
      <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
        <Alert variant="error" title="Erro">{error}</Alert>
        <Button className="mt-4" onClick={() => navigate('/seguranca/perfis')}>
          Voltar
        </Button>
      </main>
    );
  }

  return (
    <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
      <PageHeader
        title="Editar Perfil"
        description="Atualize os dados e permissões deste perfil."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Perfis', href: '/seguranca/perfis' },
              { label: 'Editar' },
            ]}
          />
        }
      />

      <div className="w-full max-w-[800px]">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro ao Salvar">{error}</Alert>
          </div>
        )}
        
        {initialData && (
          <PerfilForm
            initialData={initialData}
            catalogo={catalogo}
            isEdit={true}
            onSubmit={handleSubmit}
            onCancel={() => navigate(`/seguranca/perfis/${publicId}`)}
            isSubmitting={isSubmitting}
          />
        )}
      </div>
    </main>
  );
};
