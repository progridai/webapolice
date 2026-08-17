import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { EstipulanteForm, type EstipulanteFormData } from '../components/EstipulanteForm';
import { obterEstipulante, obterConfiguracao, alterarEstipulante, excluirEstipulante } from '../api/estipulantes.api';
import { Alert, Spinner, PageHeader, Breadcrumbs, BriefcaseIcon, HomeIcon, EmptyState, Button } from '../../../components/ui';

export const EditarEstipulantePage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  
  const [initialData, setInitialData] = useState<Partial<EstipulanteFormData> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    const fetchEstipulante = async () => {
      if (!publicId) return;
      try {
        const [estipulanteData, configData] = await Promise.all([
          obterEstipulante(publicId).catch((err) => {
            if (err.status === 404) throw new Error('NOT_FOUND');
            throw err;
          }),
          obterConfiguracao(publicId).catch((err) => {
            // Se a configuração não existir (404), tratamos graciosamente
            if (err.status === 404) return null;
            throw err;
          })
        ]);
        
        setInitialData({
          razaoSocial: estipulanteData.razaoSocial,
          nomeFantasia: estipulanteData.nomeFantasia || '',
          cnpj: estipulanteData.cnpj,
          codigo: estipulanteData.codigo || '',
          grupoPublicId: estipulanteData.grupoPublicId || '',
          seguradoraPublicId: estipulanteData.seguradoraPublicId || '',
          observacao: estipulanteData.observacao || '',
          endereco: estipulanteData.endereco ? {
            cep: estipulanteData.endereco.cep || '',
            logradouro: estipulanteData.endereco.logradouro || '',
            numero: estipulanteData.endereco.numero || '',
            complemento: estipulanteData.endereco.complemento || '',
            bairro: estipulanteData.endereco.bairro || '',
            uf: estipulanteData.endereco.uf || '',
            cidadeId: estipulanteData.endereco.cidadeId || undefined,
          } : undefined,
          contatos: estipulanteData.contatos?.map((c: any) => ({
            tipoContato: c.tipoContato,
            valor: c.valor,
            principal: c.principal
          })) || [],
          contatosInstitucionais: estipulanteData.contatosInstitucionais?.map((c: any) => ({
            nome: c.nome,
            departamento: c.departamento,
            email: c.email,
            telefone: c.telefone,
            ramal: c.ramal
          })) || [],
          configuracao: configData ? {
            dataInicioVigencia: configData.dataInicioVigencia ? new Date(configData.dataInicioVigencia).toISOString().split('T')[0] : '',
            dataFimVigencia: configData.dataFimVigencia ? new Date(configData.dataFimVigencia).toISOString().split('T')[0] : '',
          } : undefined,
        });
      } catch (err: unknown) {
        if (err instanceof Error && err.message === 'NOT_FOUND') {
          setNotFound(true);
        } else {
          console.error('Erro ao carregar estipulante:', err);
          setError('Não foi possível carregar os dados do estipulante.');
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchEstipulante();
  }, [publicId]);

  const handleSubmit = async (data: EstipulanteFormData) => {
    if (!publicId) return;
    setIsSubmitting(true);
    setError(null);
    try {
      await alterarEstipulante(publicId, {
        razaoSocial: data.razaoSocial,
        nomeFantasia: data.nomeFantasia,
        codigo: data.codigo,
        grupoPublicId: data.grupoPublicId || undefined,
        seguradoraPublicId: data.seguradoraPublicId || undefined,
        observacao: data.observacao,
        endereco: data.endereco ? { ...data.endereco } : undefined,
        contatos: data.contatos && data.contatos.length > 0 ? data.contatos.map(c => ({
          tipoContato: c.tipoContato,
          valor: c.valor,
          principal: c.principal,
        })) : undefined,
        contatosInstitucionais: data.contatosInstitucionais && data.contatosInstitucionais.length > 0 ? data.contatosInstitucionais.map(c => ({
          nome: c.nome,
          departamento: c.departamento,
          email: c.email,
          telefone: c.telefone,
          ramal: c.ramal
        })) : undefined,
        configuracao: {
          dataInicioVigencia: data.configuracao.dataInicioVigencia,
          dataFimVigencia: data.configuracao.dataFimVigencia || undefined,
        }
      });

      navigate('/estipulantes');
    } catch (err: unknown) {
      console.error('Erro ao alterar estipulante:', err);
      
      const apiError = err as { status?: number, message?: string };
      
      if (apiError.status === 409) {
        setError(apiError.message || 'A Razão Social informada diverge de uma Pessoa já compartilhada, impossibilitando a alteração global neste fluxo.');
      } else {
        setError(apiError.message || 'Ocorreu um erro ao salvar o estipulante. Verifique os dados e tente novamente.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!publicId) return;
    if (!window.confirm('Tem certeza que deseja excluir este estipulante?')) return;
    
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
        <Spinner size="lg" />
      </div>
    );
  }

  if (notFound) {
    return (
      <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none" role="main" tabIndex={-1}>
        <EmptyState
          title="Estipulante não encontrado"
          description="O registro que você tentou acessar não existe ou foi removido."
          action={<Button onClick={() => navigate('/estipulantes')}>Voltar para Listagem</Button>}
        />
      </main>
    );
  }

  if (error && !initialData) {
    return (
      <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none" role="main" tabIndex={-1}>
        <Alert variant="error" title="Erro">{error}</Alert>
        <Button className="mt-4" onClick={() => navigate('/estipulantes')}>Voltar</Button>
      </main>
    );
  }

  const breadcrumbItems = [
    { label: 'Início', href: '/', icon: <HomeIcon size={14} /> },
    { label: 'Estipulantes', href: '/estipulantes', icon: <BriefcaseIcon size={14} /> },
    { label: 'Editar Estipulante' }
  ];

  return (
    <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none" role="main" tabIndex={-1}>
      <PageHeader 
        title="Editar Estipulante"
        description="Atualize os dados e a configuração operacional do estipulante."
        breadcrumbs={<Breadcrumbs items={breadcrumbItems} />}
      />

      <div className="w-full">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro ao Salvar">{error}</Alert>
          </div>
        )}
        
        {initialData && (
          <EstipulanteForm 
            initialData={initialData}
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
