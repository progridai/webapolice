import { HttpApiError } from '../../../services/http/httpError';
import React, { useState, useCallback, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import {
  EstipulanteForm,
  type EstipulanteFormData,
} from '../components/EstipulanteForm';
import {
  obterEstipulante,
  obterConfiguracao,
  alterarEstipulante,
  excluirEstipulante,
} from '../api/estipulantes.api';
import {
  Alert,
  Spinner,
  PageHeader,
  Breadcrumbs,
  BriefcaseIcon,
  HomeIcon,
  EmptyState,
  Button,
} from '../../../components/ui';

export const EditarEstipulantePage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  const { possuiPermissao } = useAuthorization();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const load = useCallback(
    async (signal: AbortSignal) => {
      if (!publicId) return;
      const [estipulanteData, configData] = await Promise.all([
        obterEstipulante(publicId, signal),
        obterConfiguracao(publicId, signal).catch((err: unknown) => {
          // Se a configuração não existir (404), tratamos graciosamente
          if (err instanceof HttpApiError && err.isNotFound()) return null;
          throw err;
        }),
      ]);
      return { estipulanteData, configData };
    },
    [publicId],
  );
  const {
    data,
    isLoading,
    error: erroConsulta,
    refetch,
  } = useConsulta(
    publicId ? load : null,
    'Não foi possível carregar os dados do estipulante.',
  );
  const seguradoraNome = data?.estipulanteData.seguradoraNome;
  const grupoLegadoId = data?.estipulanteData.grupoId ?? null;
  const notFound =
    !publicId ||
    (erroConsulta instanceof HttpApiError && erroConsulta.isNotFound());
  const initialData = useMemo<Partial<EstipulanteFormData> | null>(() => {
    if (!data) return null;
    const { estipulanteData, configData } = data;
    return {
      razaoSocial: estipulanteData.razaoSocial,
      nomeFantasia: estipulanteData.nomeFantasia || '',
      cnpj: estipulanteData.cnpj,
      codigo: estipulanteData.codigo || '',
      seguradoraPublicId: estipulanteData.seguradoraPublicId || '',
      observacao: estipulanteData.observacao || '',
      endereco: estipulanteData.endereco
        ? {
            cep: estipulanteData.endereco.cep || '',
            logradouro: estipulanteData.endereco.logradouro || '',
            numero: estipulanteData.endereco.numero || '',
            complemento: estipulanteData.endereco.complemento || '',
            bairro: estipulanteData.endereco.bairro || '',
            uf: estipulanteData.endereco.uf || '',
            cidadeId: estipulanteData.endereco.cidadeId || undefined,
          }
        : undefined,
      contatos:
        estipulanteData.contatos?.map((c) => ({
          tipoContato: c.tipoContato,
          valor: c.valor,
          principal: c.principal,
        })) || [],
      contatosInstitucionais:
        estipulanteData.contatosInstitucionais?.map((c) => ({
          nome: c.nome,
          departamento: c.departamento,
          email: c.email,
          telefone: c.telefone,
          ramal: c.ramal,
        })) || [],
      configuracao: configData
        ? {
            dataInicioVigencia: configData.dataInicioVigencia
              ? new Date(configData.dataInicioVigencia)
                  .toISOString()
                  .split('T')[0]
              : '',
            dataFimVigencia: configData.dataFimVigencia
              ? new Date(configData.dataFimVigencia).toISOString().split('T')[0]
              : '',
          }
        : undefined,
    };
  }, [data]);

  const handleSubmit = async (data: EstipulanteFormData) => {
    if (!publicId || !data || isSubmitting) return;
    setIsSubmitting(true);
    setError(null);
    try {
      await alterarEstipulante(publicId, {
        razaoSocial: data.razaoSocial,
        nomeFantasia: data.nomeFantasia,
        codigo: data.codigo,
        grupoId: grupoLegadoId,
        seguradoraPublicId: data.seguradoraPublicId || undefined,
        observacao: data.observacao,
        endereco: data.endereco ? { ...data.endereco } : undefined,
        contatos:
          data.contatos && data.contatos.length > 0
            ? data.contatos.map((c) => ({
                tipoContato: c.tipoContato,
                valor: c.valor,
                principal: c.principal,
              }))
            : undefined,
        contatosInstitucionais:
          data.contatosInstitucionais && data.contatosInstitucionais.length > 0
            ? data.contatosInstitucionais.map((c) => ({
                nome: c.nome,
                departamento: c.departamento,
                email: c.email,
                telefone: c.telefone,
                ramal: c.ramal,
              }))
            : undefined,
        configuracao: {
          dataInicioVigencia: data.configuracao.dataInicioVigencia,
          dataFimVigencia: data.configuracao.dataFimVigencia || undefined,
        },
      });

      navigate('/estipulantes');
    } catch (err: unknown) {
      console.error('Erro ao alterar estipulante:', err);
      if (err instanceof HttpApiError && err.isConflict()) {
        setError(
          err.message ||
            'A Razão Social informada diverge de uma Pessoa já compartilhada, impossibilitando a alteração global neste fluxo.',
        );
      } else {
        setError(
          err instanceof Error && err.message
            ? err.message
            : 'Ocorreu um erro ao salvar o estipulante. Verifique os dados e tente novamente.',
        );
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!publicId) return;
    if (!window.confirm('Tem certeza que deseja excluir este estipulante?'))
      return;

    setIsSubmitting(true);
    try {
      await excluirEstipulante(publicId);
      navigate('/estipulantes');
    } catch (err) {
      console.error('Erro ao excluir estipulante', err);
      setError('Ocorreu um erro ao excluir o estipulante.');
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/estipulantes');
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spinner size="large" />
      </div>
    );
  }

  if (notFound) {
    return (
      <main
        className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none"
        role="main"
        tabIndex={-1}
      >
        <EmptyState
          title="Estipulante não encontrado"
          description="O registro que você tentou acessar não existe ou foi removido."
          action={
            <Button onClick={() => navigate('/estipulantes')}>
              Voltar para Listagem
            </Button>
          }
        />
      </main>
    );
  }

  if (erroConsulta && !initialData) {
    return (
      <main
        className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none"
        role="main"
        tabIndex={-1}
      >
        <Alert variant="error" title="Erro">
          Não foi possível carregar os dados do estipulante.
        </Alert>
        <Button variant="secondary" onClick={() => void refetch()}>
          Tentar novamente
        </Button>
        <Button className="mt-4" onClick={() => navigate('/estipulantes')}>
          Voltar
        </Button>
      </main>
    );
  }

  const breadcrumbItems = [
    { label: 'Início', href: '/', icon: <HomeIcon size={14} /> },
    {
      label: 'Estipulantes',
      href: '/estipulantes',
      icon: <BriefcaseIcon size={14} />,
    },
    { label: 'Editar Estipulante' },
  ];

  return (
    <main
      className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none"
      role="main"
      tabIndex={-1}
    >
      <PageHeader
        title="Editar Estipulante"
        description="Atualize os dados e a configuração operacional do estipulante."
        breadcrumbs={<Breadcrumbs items={breadcrumbItems} />}
      />

      <div className="w-full">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro ao Salvar">
              {error}
            </Alert>
          </div>
        )}

        {initialData && (
          <EstipulanteForm
            initialData={initialData}
            initialSeguradoraNome={seguradoraNome}
            podeConsultarSeguradoras={possuiPermissao('seguradoras.visualizar')}
            isEdit={true}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
            onDelete={handleDelete}
            isSubmitting={isSubmitting}
          />
        )}
      </div>
    </main>
  );
};
