import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ClienteForm, type ClienteFormData } from '../components/ClienteForm';
import { cadastrarCliente } from '../api/clienteWriteApi';
import { Alert } from '../../../components/ui/Alert';

export const CadastrarClientePage: React.FC = () => {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (data: ClienteFormData) => {
    setIsSubmitting(true);
    setError(null);
    try {
      const response = await cadastrarCliente({
        ...data,
        documento: data.documento.replace(/\D/g, ''), // Limpa o documento
        tipoPessoa: Number(data.tipoPessoa),
        sexo: data.sexo ? Number(data.sexo) : undefined,
        dataNascimento: data.dataNascimento || undefined,
        dataObito: data.dataObito || undefined,
        endereco: data.endereco && Object.values(data.endereco).some(val => val !== "" && val !== undefined && val !== 0) ? {
          ...data.endereco,
          cidadeId: data.endereco.cidadeId || undefined
        } : undefined,
      });
      navigate(`/clientes/${response.id}`);
    } catch (err: unknown) {
      console.error('Erro ao cadastrar cliente:', err);
      const errorResponse = err as { response?: { status?: number, data?: { message?: string } } };
      
      if (errorResponse.response?.status === 409) {
        setError(errorResponse.response?.data?.message || 'A Pessoa deste Cliente está compartilhada com outros papéis e não pode ser cadastrada como cliente novamente.');
      } else {
        setError(errorResponse.response?.data?.message || 'Ocorreu um erro ao cadastrar o cliente. Verifique os dados e tente novamente.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/clientes');
  };

  return (
    <div className="clientes-page" role="main">
      <header className="clientes-page-header">
        <div className="clientes-page-header-text">
          <h1 className="clientes-page-title">Novo Cliente</h1>
          <p className="clientes-page-subtitle">Preencha os dados abaixo para cadastrar um novo cliente.</p>
        </div>
      </header>

      <div className="page-content">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro no Cadastro">{error}</Alert>
          </div>
        )}
        
        <ClienteForm 
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isSubmitting={isSubmitting}
        />
      </div>
    </div>
  );
};

export default CadastrarClientePage;
