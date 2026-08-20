/**
 * SubestipulanteFormPage.tsx
 *
 * Página de Cadastro e Edição de Subestipulante.
 */
import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader, Card, Alert, Spinner, Breadcrumbs } from '../../../components/ui';
import { ROUTES } from '../../../app/routes/routePaths';
import { SubestipulanteForm } from '../components/SubestipulanteForm';
import { subestipulantesApi } from '../api/subestipulantes.api';
import type { SubestipulanteFormData } from '../schemas/subestipulanteFormSchema';

export const SubestipulanteFormPage: React.FC = () => {
  const navigate = useNavigate();
  const { publicId } = useParams<{ publicId: string }>();
  const isEdit = Boolean(publicId);

  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [initialData, setInitialData] = useState<Partial<SubestipulanteFormData>>({});

  useEffect(() => {
    if (!isEdit || !publicId) return;

    const carregarSubestipulante = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const subestipulante = await subestipulantesApi.obter(publicId);
        setInitialData({
          nome: subestipulante.nome,
          codigo: subestipulante.codigo || '',
          cnpj: subestipulante.cnpj || '',
          observacao: subestipulante.observacao || '',
        });
      } catch (err: unknown) {
        console.error('Erro ao carregar dados do subestipulante:', err);
        const apiError = err as { response?: { data?: { message?: string } }, message?: string };
        setError(
          apiError?.response?.data?.message ||
            apiError?.message ||
            'Não foi possível carregar os dados do subestipulante.'
        );
      } finally {
        setIsLoading(false);
      }
    };

    carregarSubestipulante();
  }, [publicId, isEdit]);

  const handleSubmit = async (data: SubestipulanteFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      if (isEdit && publicId) {
        await subestipulantesApi.alterar(publicId, {
          publicId,
          nome: data.nome,
          codigo: data.codigo || undefined,
          cnpj: data.cnpj || undefined,
          observacao: data.observacao || undefined,
        });
      } else {
        await subestipulantesApi.criar({
          nome: data.nome,
          codigo: data.codigo || undefined,
          cnpj: data.cnpj || undefined,
          observacao: data.observacao || undefined,
        });
      }

      navigate(ROUTES.SUBESTIPULANTES);
    } catch (err: unknown) {
      console.error('Erro ao salvar subestipulante:', err);
      const apiError = err as { response?: { data?: { message?: string } }, message?: string };
      const apiMsg = apiError?.response?.data?.message || apiError?.message;
      setError(apiMsg || 'Ocorreu um erro ao salvar o subestipulante. Verifique os dados informados.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(ROUTES.SUBESTIPULANTES);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64" aria-label="Carregando subestipulante">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={isEdit ? 'Editar Subestipulante' : 'Novo Subestipulante'}
        description="Preencha as informações para registrar ou atualizar os dados do subestipulante"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Subestipulantes', href: ROUTES.SUBESTIPULANTES },
              { label: isEdit ? 'Editar' : 'Novo' },
            ]}
          />
        }
      />

      {error && (
        <Alert variant="error" title="Erro ao salvar" onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Card>
        <SubestipulanteForm
          initialData={initialData}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isSubmitting={isSubmitting}
        />
      </Card>
    </div>
  );
};
