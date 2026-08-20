import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader, Card, Alert, Spinner } from '../../../components/ui';
import { ROUTES } from '../../../app/routes/routePaths';
import { RamoForm } from '../components/RamoForm';
import { ramosApi } from '../api/ramos.api';
import { type RamoFormData } from '../schemas/ramoFormSchema';

export const RamoFormPage: React.FC = () => {
  const navigate = useNavigate();
  const { publicId } = useParams();
  const isEdit = !!publicId;

  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [initialData, setInitialData] = useState<Partial<RamoFormData>>({});

  useEffect(() => {
    if (!isEdit || !publicId) return;

    const loadData = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const ramo = await ramosApi.obter(publicId);
        setInitialData({
          codigo: ramo.codigo,
          nome: ramo.nome,
          descricao: ramo.descricao,
        });
      } catch (err: any) {
        console.error(err);
        setError('Não foi possível carregar os dados do Ramo.');
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, [publicId, isEdit]);

  const handleSubmit = async (data: RamoFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      if (isEdit && publicId) {
        await ramosApi.alterar(publicId, {
          nome: data.nome,
          descricao: data.descricao,
        });
      } else {
        await ramosApi.criar({
          codigo: data.codigo,
          nome: data.nome,
          descricao: data.descricao,
        });
      }

      navigate(ROUTES.RAMOS);
    } catch (err: any) {
      console.error(err);
      setError(err?.response?.data?.message || 'Ocorreu um erro ao salvar o Ramo.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(ROUTES.RAMOS);
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
        title={isEdit ? 'Editar Ramo' : 'Novo Ramo'} 
        subtitle="Preencha os dados abaixo para o cadastro de Ramo"
      />

      {error && (
        <Alert type="error" title="Erro ao salvar" onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Card>
        <RamoForm
          initialData={initialData}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isSubmitting={isSubmitting}
        />
      </Card>
    </div>
  );
};
