import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader, Card, Alert, Spinner, Breadcrumbs } from '../../../components/ui';
import { ROUTES } from '../../../app/routes/routePaths';
import { ModuloForm } from '../components/ModuloForm';
import { modulosApi } from '../api/modulos.api';
import type { ModuloFormData } from '../schemas/modulo.schema';

export const ModuloFormPage: React.FC = () => {
  const navigate = useNavigate();
  const { publicId } = useParams<{ publicId: string }>();
  const isEdit = Boolean(publicId);

  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [initialData, setInitialData] = useState<Partial<ModuloFormData>>({});

  useEffect(() => {
    if (!isEdit || !publicId) return;

    const loadData = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const modulo = await modulosApi.obter(publicId);
        setInitialData({
          nome: modulo.nome,
          descricao: modulo.descricao,
          ativo: modulo.ativo,
        });
      } catch (err: any) {
        console.error(err);
        setError('Não foi possível carregar os dados do módulo.');
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, [publicId, isEdit]);

  const handleSubmit = async (data: ModuloFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      if (isEdit && publicId) {
        await modulosApi.alterar(publicId, {
          nome: data.nome,
          descricao: data.descricao,
          ativo: data.ativo,
        });
      } else {
        await modulosApi.criar({
          nome: data.nome,
          descricao: data.descricao,
        });
      }

      navigate(ROUTES.MODULOS);
    } catch (err: any) {
      console.error(err);
      setError(err?.response?.data?.detail || 'Ocorreu um erro ao salvar o módulo.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(ROUTES.MODULOS);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={isEdit ? 'Editar Módulo' : 'Novo Módulo'}
        description="Preencha as informações para registrar ou atualizar um módulo no catálogo global"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Módulos', href: ROUTES.MODULOS },
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
        <ModuloForm
          initialData={initialData}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isSubmitting={isSubmitting}
        />
      </Card>
    </div>
  );
};
