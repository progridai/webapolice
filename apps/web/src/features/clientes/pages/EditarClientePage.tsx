import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ClienteForm, type ClienteFormData } from '../components/ClienteForm';
import { alterarCliente } from '../api/clienteWriteApi';
import { obterClienteDetalhe } from '../api/obterClienteDetalhe';
import { Alert } from '../../../components/ui/Alert';
import { Spinner } from '../../../components/ui/Spinner';

export const EditarClientePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [initialData, setInitialData] = useState<Partial<ClienteFormData> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCliente = async () => {
      if (!id) return;
      try {
        const data = await obterClienteDetalhe(id);
        
        // Mapeia o ClienteDetalheResponse para ClienteFormData
        setInitialData({
          tipoPessoa: data.tipoPessoa === 'Física' ? 1 : 2, // Depende do retorno real, ajustar se necessário
          nome: data.nome,
          documento: data.cpfCnpjMascarado || data.documentoMascarado || '',
          dataNascimento: data.dataNascimento ? new Date(data.dataNascimento).toISOString().split('T')[0] : '',
          sexo: undefined, // Ajustar mapeamento real do backend
          observacao: '',
          falecido: false,
          dataObito: '',
          // Contatos e Endereço devem ser mapeados conforme o array retornado
          email: data.contatos?.find((c: { tipo: string; valor: string }) => c.tipo === 'EMAIL')?.valor || '',
          telefone: data.contatos?.find((c: { tipo: string; valor: string }) => c.tipo === 'TELEFONE')?.valor || '',
          celular: data.contatos?.find((c: { tipo: string; valor: string }) => c.tipo === 'CELULAR')?.valor || '',
          endereco: data.enderecos?.[0] ? {
            cep: data.enderecos[0].cep,
            logradouro: data.enderecos[0].logradouro,
            numero: data.enderecos[0].numero,
            complemento: data.enderecos[0].complemento,
            bairro: data.enderecos[0].bairro,
            cidadeId: data.enderecos[0].cidadeId,
            uf: data.enderecos[0].uf,
          } : undefined,
        });
      } catch (err) {
        console.error('Erro ao carregar cliente:', err);
        setError('Não foi possível carregar os dados do cliente.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchCliente();
  }, [id]);

  const handleSubmit = async (data: ClienteFormData) => {
    if (!id) return;
    setIsSubmitting(true);
    setError(null);
    try {
      await alterarCliente(id, {
        nome: data.nome,
        dataNascimento: data.dataNascimento || undefined,
        sexo: data.sexo ? Number(data.sexo) : undefined,
        observacao: data.observacao || undefined,
        falecido: data.falecido,
        dataObito: data.dataObito || undefined,
        email: data.email || undefined,
        telefone: data.telefone || undefined,
        celular: data.celular || undefined,
        endereco: data.endereco && Object.values(data.endereco).some(val => val !== "" && val !== undefined && val !== 0) ? {
          ...data.endereco,
          cidadeId: data.endereco.cidadeId || undefined
        } : undefined,
      });
      navigate(`/clientes/${id}`);
    } catch (err: unknown) {
      console.error('Erro ao alterar cliente:', err);
      const errorResponse = err as { response?: { status?: number, data?: { message?: string } } };
      
      if (errorResponse.response?.status === 409) {
        setError(errorResponse.response?.data?.message || 'A Pessoa deste Cliente está compartilhada com outros papéis e não pode ser alterada.');
      } else {
        setError(errorResponse.response?.data?.message || 'Ocorreu um erro ao salvar o cliente. Verifique os dados e tente novamente.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(`/clientes/${id}`);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error && !initialData) {
    return (
      <div className="page-container">
        <Alert variant="error" title="Erro">{error}</Alert>
        <button className="btn mt-4" onClick={() => navigate('/clientes')}>Voltar</button>
      </div>
    );
  }

  return (
    <div className="clientes-page" role="main">
      <header className="clientes-page-header">
        <div className="clientes-page-header-text">
          <h1 className="clientes-page-title">Editar Cliente</h1>
          <p className="clientes-page-subtitle">Atualize os dados do cliente.</p>
        </div>
      </header>

      <div className="page-content">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro ao Salvar">{error}</Alert>
          </div>
        )}
        
        {initialData && (
          <ClienteForm 
            initialData={initialData}
            isEdit={true}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
            isSubmitting={isSubmitting}
          />
        )}
      </div>
    </div>
  );
};

export default EditarClientePage;
