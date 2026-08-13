import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EstipulanteForm, type EstipulanteFormData } from '../components/EstipulanteForm';
import { cadastrarEstipulante } from '../api/estipulantes.api';
import { Alert, PageHeader, Breadcrumbs, BriefcaseIcon, HomeIcon } from '../../../components/ui';
import type { CriarEstipulanteRequest } from '../types/estipulante.types';

export const CadastrarEstipulantePage: React.FC = () => {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (data: EstipulanteFormData) => {
    setIsSubmitting(true);
    setError(null);
    try {
      const payload: CriarEstipulanteRequest = {
        razaoSocial: data.razaoSocial,
        nomeFantasia: data.nomeFantasia,
        cnpj: data.cnpj,
        codigo: data.codigo,
        grupoPublicId: undefined, // WIP (Backend/Mock)
        seguradoraPublicId: undefined, // WIP (Backend/Mock)
        observacao: data.observacao,
        endereco: data.endereco ? {
          ...data.endereco,
        } : undefined,
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
          dataFimVigencia: data.configuracao.dataFimVigencia,
        },
      };

      await cadastrarEstipulante(payload);
      
      // Quando for criada a página de detalhes, mudar este roteamento
      // navigate(`/estipulantes/${response.publicId}`);
      navigate('/estipulantes');

    } catch (err: unknown) {
      console.error('Erro ao cadastrar estipulante:', err);
      const errorResponse = err as { response?: { status?: number, data?: { message?: string } } };
      
      if (errorResponse.response?.status === 409) {
        setError(errorResponse.response?.data?.message || 'Já existe um cadastro com este CNPJ.');
      } else {
        setError(errorResponse.response?.data?.message || 'Ocorreu um erro ao cadastrar o estipulante. Verifique os dados e tente novamente.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/estipulantes');
  };

  const breadcrumbItems = [
    { label: 'Início', href: '/', icon: <HomeIcon size={14} /> },
    { label: 'Estipulantes', href: '/estipulantes', icon: <BriefcaseIcon size={14} /> },
    { label: 'Novo Estipulante' }
  ];

  return (
    <div className="estipulantes-page" role="main">
      <PageHeader 
        title="Novo Estipulante"
        description="Cadastre os dados básicos da empresa estipulante."
        icon={<BriefcaseIcon size={24} />}
        breadcrumbs={<Breadcrumbs items={breadcrumbItems} />}
      />

      <div className="page-content" style={{ maxWidth: '800px' }}>
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro no Cadastro">{error}</Alert>
          </div>
        )}
        
        <EstipulanteForm 
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isSubmitting={isSubmitting}
        />
      </div>
    </div>
  );
};
