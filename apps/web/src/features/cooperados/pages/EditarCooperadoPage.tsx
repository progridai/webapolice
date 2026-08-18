import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader, Breadcrumbs, Alert, Skeleton } from '../../../components/ui';
import { ROUTES } from '../../../app/routes/routePaths';
import { CooperadoForm } from '../components/CooperadoForm';
import { obterCooperadoDetalhe, alterarCooperado } from '../api/cooperadosApi';
import type { CooperadoFormData } from '../types/cooperados.types';

export const EditarCooperadoPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [initialData, setInitialData] = useState<CooperadoFormData | null>(null);
  const [isLoadingData, setIsLoadingData] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    async function loadData() {
      if (!id) return;
      setIsLoadingData(true);
      setError(null);
      try {
        const dto = await obterCooperadoDetalhe(id);
        const formData: CooperadoFormData = {
          nome: dto.nome,
          cpf: dto.cpf,
          dataNascimento: dto.dataNascimento || '',
          telefone: dto.telefone || '',
          email: dto.email || '',
          cep: dto.cep || '',
          logradouro: dto.logradouro || '',
          numero: dto.numero || '',
          complemento: dto.complemento || '',
          bairro: dto.bairro || '',
          cidadeId: dto.cidadeId || 0,
          uf: dto.uf || '',
          tipo: dto.tipo,
          codigo: dto.codigo || '',
          rg: dto.rg || '',
          orgaoEmissor: dto.orgaoEmissor || '',
          dataEmissaoRg: dto.dataEmissaoRg || '',
          susep: dto.susep || '',
          inss: dto.inss || '',
          issqn: dto.issqn || '',
          numeroDependentes: dto.numeroDependentes,
          dataInscricao: dto.dataInscricao || '',
          credenciado: dto.credenciado || false,
          coordenadorId: dto.coordenadorId || 0,
          bancoId: dto.bancoId || 0,
          agencia: dto.agencia || '',
          contaCorrente: dto.contaCorrente || '',
          observacao: dto.observacao || ''
        };
        setInitialData(formData);
      } catch (err: any) {
        console.error(err);
        setError('Não foi possível carregar os dados do cooperado.');
      } finally {
        setIsLoadingData(false);
      }
    }
    loadData();
  }, [id]);

  const handleSubmit = async (data: CooperadoFormData) => {
    if (!id) return;
    setIsSubmitting(true);
    setError(null);
    try {
      await alterarCooperado(id, data);
      navigate(ROUTES.COOPERADOS);
    } catch (err: any) {
      console.error(err);
      setError(err?.response?.data?.message || 'Ocorreu um erro ao alterar o cooperado/coordenador.');
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
        title="Editar Cooperado"
        description="Altere os dados do cooperado ou coordenador no sistema."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Cooperados', href: ROUTES.COOPERADOS },
              { label: 'Editar Cooperado' },
            ]}
          />
        }
      />

      {error && (
        <Alert variant="error" title="Atenção">
          {error}
        </Alert>
      )}

      <div className="cooperado-form-container mt-6">
        {isLoadingData ? (
          <div className="space-y-4">
            <Skeleton className="h-20" />
            <Skeleton className="h-40" />
          </div>
        ) : initialData ? (
          <CooperadoForm
            initialData={initialData}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
            isLoading={isSubmitting}
          />
        ) : null}
      </div>
    </main>
  );
};
