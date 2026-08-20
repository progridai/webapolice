import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader, Alert } from '../../../components/ui';
import { ApoliceForm } from '../components/forms/ApoliceForm';
import { obterApolice, criarApolice, alterarApolice } from '../api/apolices.api';
import type { ApoliceFormValues } from '../schemas/apoliceForm.schema';
import { PageLoading } from '../../../components/application/PageLoading';

export const ApoliceFormPage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const isEdicao = Boolean(publicId);
  const navigate = useNavigate();

  const [initialData, setInitialData] = useState<Partial<ApoliceFormValues> | undefined>(undefined);
  const [isLoadingData, setIsLoadingData] = useState(isEdicao);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadData() {
      if (!isEdicao || !publicId) return;
      try {
        const apolice = await obterApolice(publicId);
        setInitialData({
          nome: apolice.nome,
          estipulanteId: apolice.estipulanteId,
          seguradoraId: apolice.seguradoraId,
          corretoraId: apolice.corretoraId || '',
          dataInicioVigencia: apolice.dataInicioVigencia,
          dataFimVigencia: apolice.dataFimVigencia || '',
          dataAniversario: apolice.dataAniversario || '',
          observacao: apolice.observacao || '',
        });
      } catch (err: any) {
        setError('Não foi possível carregar os dados da apólice.');
        // navigate('/apolices');
      } finally {
        setIsLoadingData(false);
      }
    }
    loadData();
  }, [isEdicao, publicId, navigate]);

  const handleSubmit = async (data: ApoliceFormValues) => {
    setIsSaving(true);
    setError(null);
    try {
      if (isEdicao && publicId) {
        await alterarApolice(publicId, data);
        alert('Apólice alterada com sucesso.');
        navigate(`/apolices/${publicId}`);
      } else {
        const result = await criarApolice(data);
        alert('Apólice criada com sucesso.');
        navigate(`/apolices/${result.publicId}`);
      }
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Ocorreu um erro ao salvar a apólice. Verifique os dados.');
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoadingData) {
    return <PageLoading />;
  }

  return (
    <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none" tabIndex={-1}>
      <PageHeader
        title={isEdicao ? 'Editar Apólice' : 'Nova Apólice'}
        description={isEdicao ? 'Altere os dados básicos da apólice' : 'Crie uma nova apólice vinculada a um estipulante'}
        showBackButton
        onBackClick={() => navigate(-1)}
      />
      <div className="max-w-4xl">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro">{error}</Alert>
          </div>
        )}
        <ApoliceForm
          initialData={initialData}
          onSubmit={handleSubmit}
          isLoading={isSaving}
        />
      </div>
    </main>
  );
};
