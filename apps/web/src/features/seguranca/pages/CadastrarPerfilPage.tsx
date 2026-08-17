import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PerfilForm, type PerfilFormData } from '../components/PerfilForm';
import { criarPerfil } from '../api/perfisApi';
import { obterCatalogo } from '../api/catalogoApi';
import { Alert, PageHeader, Breadcrumbs, Spinner } from '../../../components/ui';
import type { CatalogoModuloDto } from '../types/seguranca.types';

export const CadastrarPerfilPage: React.FC = () => {
  const navigate = useNavigate();
  const [catalogo, setCatalogo] = useState<CatalogoModuloDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    document.title = 'Novo Perfil | WebApolice';
    const fetch = async () => {
      try {
        const cat = await obterCatalogo();
        setCatalogo(cat);
      } catch {
        setError('Erro ao carregar o catálogo de permissões.');
      } finally {
        setIsLoading(false);
      }
    };
    fetch();
  }, []);

  const handleSubmit = async (data: PerfilFormData) => {
    setIsSubmitting(true);
    setError(null);
    try {
      const response = await criarPerfil({
        codigo: data.codigo,
        nome: data.nome,
        descricao: data.descricao || '',
        ativo: data.ativo,
        permissaoPublicIds: data.permissaoPublicIds,
      });
      navigate(`/seguranca/perfis/${response.id}`);
    } catch (err: unknown) {
      console.error('Erro ao cadastrar perfil:', err);
      const errorResponse = err as { response?: { status?: number; data?: { message?: string } } };
      setError(errorResponse.response?.data?.message || 'Ocorreu um erro ao cadastrar o perfil. Verifique os dados e tente novamente.');
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

  return (
    <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none" role="main" tabIndex={-1}>
      <PageHeader
        title="Novo Perfil"
        description="Preencha os dados e selecione as permissões do novo perfil."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Perfis', href: '/seguranca/perfis' },
              { label: 'Novo' },
            ]}
          />
        }
      />

      <div className="w-full">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro no Cadastro">{error}</Alert>
          </div>
        )}
        <PerfilForm
          catalogo={catalogo}
          onSubmit={handleSubmit}
          onCancel={() => navigate('/seguranca/perfis')}
          isSubmitting={isSubmitting}
        />
      </div>
    </main>
  );
};
