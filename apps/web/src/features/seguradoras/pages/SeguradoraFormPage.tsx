/**
 * SeguradoraFormPage.tsx
 *
 * Página de Cadastro e Edição de Seguradora.
 */
import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader, Card, Alert, Spinner, Breadcrumbs } from '../../../components/ui';
import { ROUTES } from '../../../app/routes/routePaths';
import { SeguradoraForm } from '../components/SeguradoraForm';
import { seguradorasApi } from '../api/seguradoras.api';
import type { SeguradoraFormData } from '../schemas/seguradoraFormSchema';

export const SeguradoraFormPage: React.FC = () => {
  const navigate = useNavigate();
  const { publicId } = useParams<{ publicId: string }>();
  const isEdit = Boolean(publicId);

  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [initialData, setInitialData] = useState<Partial<SeguradoraFormData>>({});

  useEffect(() => {
    if (!isEdit || !publicId) return;

    const carregarSeguradora = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const seguradora = await seguradorasApi.obter(publicId);
        setInitialData({
          nome: seguradora.nome,
          codigo: seguradora.codigo || '',
          susep: seguradora.susep || '',
          cnpj: seguradora.cnpj || '',
          observacao: seguradora.observacao || '',
        });
      } catch (err: any) {
        console.error('Erro ao carregar dados da seguradora:', err);
        setError(
          err?.response?.data?.message ||
            err?.message ||
            'Não foi possível carregar os dados da seguradora.'
        );
      } finally {
        setIsLoading(false);
      }
    };

    carregarSeguradora();
  }, [publicId, isEdit]);

  const handleSubmit = async (data: SeguradoraFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      if (isEdit && publicId) {
        await seguradorasApi.alterar(publicId, {
          publicId,
          nome: data.nome,
          codigo: data.codigo || undefined,
          susep: data.susep || undefined,
          cnpj: data.cnpj || undefined,
          observacao: data.observacao || undefined,
        });
      } else {
        await seguradorasApi.criar({
          nome: data.nome,
          codigo: data.codigo || undefined,
          susep: data.susep || undefined,
          cnpj: data.cnpj || undefined,
          observacao: data.observacao || undefined,
        });
      }

      navigate(ROUTES.SEGURADORAS);
    } catch (err: any) {
      console.error('Erro ao salvar seguradora:', err);
      const apiMsg = err?.response?.data?.message || err?.message;
      setError(apiMsg || 'Ocorreu um erro ao salvar a seguradora. Verifique os dados informados.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(ROUTES.SEGURADORAS);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64" aria-label="Carregando seguradora">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={isEdit ? 'Editar Seguradora' : 'Nova Seguradora'}
        description="Preencha as informações para registrar ou atualizar os dados da seguradora"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Seguradoras', href: ROUTES.SEGURADORAS },
              { label: isEdit ? 'Editar' : 'Nova' },
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
        <SeguradoraForm
          initialData={initialData}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isSubmitting={isSubmitting}
        />
      </Card>
    </div>
  );
};
