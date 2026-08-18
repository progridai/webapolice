import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageHeader, Breadcrumbs, Alert } from '../../../components/ui';
import { ROUTES } from '../../../app/routes/routePaths';
import { CooperadoForm } from '../components/CooperadoForm';
import { cadastrarCooperado } from '../api/cooperadosApi';
import type { CooperadoFormData } from '../types/cooperados.types';

export const NovoCooperadoPage: React.FC = () => {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (data: CooperadoFormData) => {
    setIsSubmitting(true);
    setError(null);
    try {
      await cadastrarCooperado(data);
      navigate(ROUTES.COOPERADOS);
    } catch (err: any) {
      console.error(err);
      setError(err?.response?.data?.message || 'Ocorreu um erro ao cadastrar o cooperado/coordenador.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(ROUTES.COOPERADOS);
  };

  return (
    <main className="cooperado-page" tabIndex={-1}>
      <PageHeader
        title="Novo Cooperado"
        description="Cadastre um novo cooperado ou coordenador no sistema."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Cooperados', href: ROUTES.COOPERADOS },
              { label: 'Novo Cooperado' },
            ]}
          />
        }
      />

      {error && (
        <Alert variant="error" title="Erro ao cadastrar">
          {error}
        </Alert>
      )}

      <div className="cooperado-form-container mt-6">
        <CooperadoForm
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isLoading={isSubmitting}
        />
      </div>
    </main>
  );
};
