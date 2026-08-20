/**
 * CorretoraFormPage.tsx
 *
 * Página de Cadastro e Edição de Corretora.
 */
import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader, Card, Alert, Spinner, Breadcrumbs } from '../../../components/ui';
import { ROUTES } from '../../../app/routes/routePaths';
import { CorretoraForm } from '../components/CorretoraForm';
import { corretorasApi } from '../api/corretoras.api';
import type { CorretoraFormData } from '../schemas/corretoraFormSchema';

export const CorretoraFormPage: React.FC = () => {
  const navigate = useNavigate();
  const { publicId } = useParams<{ publicId: string }>();
  const isEdit = Boolean(publicId);

  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [initialData, setInitialData] = useState<Partial<CorretoraFormData>>({});

  useEffect(() => {
    if (!isEdit || !publicId) return;

    const carregarCorretora = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const corretora = await corretorasApi.obterPorId(publicId);
        setInitialData({
          nome: corretora.nome,
          codigo: corretora.codigo || '',
          codigoProtheus: corretora.codigoProtheus || '',
          cnpj: corretora.cnpj || '',
          observacao: corretora.observacao || '',
        });
      } catch (err: unknown) {
        console.error('Erro ao carregar dados da corretora:', err);
        setError('Não foi possível carregar os dados da corretora.');
      } finally {
        setIsLoading(false);
      }
    };

    carregarCorretora();
  }, [publicId, isEdit]);

  const handleSubmit = async (data: CorretoraFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      if (isEdit && publicId) {
        await corretorasApi.alterar(publicId, {
          publicId,
          nome: data.nome,
          codigo: data.codigo || undefined,
          codigoProtheus: data.codigoProtheus || undefined,
          cnpj: data.cnpj || undefined,
          observacao: data.observacao || undefined,
        });
      } else {
        await corretorasApi.criar({
          nome: data.nome,
          codigo: data.codigo || undefined,
          codigoProtheus: data.codigoProtheus || undefined,
          cnpj: data.cnpj || undefined,
          observacao: data.observacao || undefined,
        });
      }

      navigate(ROUTES.CORRETORAS);
    } catch (err: unknown) {
      console.error('Erro ao salvar corretora:', err);
      setError('Ocorreu um erro ao salvar a corretora. Verifique os dados informados.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(ROUTES.CORRETORAS);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64" aria-label="Carregando corretora">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={isEdit ? 'Editar Corretora' : 'Nova Corretora'}
        description="Preencha as informações para registrar ou atualizar os dados da corretora"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Corretoras', href: ROUTES.CORRETORAS },
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
        <CorretoraForm
          initialData={initialData}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isSubmitting={isSubmitting}
        />
      </Card>
    </div>
  );
};
