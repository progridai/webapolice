import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ClienteForm, type ClienteFormData } from '../components/ClienteForm';
import { alterarCliente } from '../api/clienteWriteApi';
import { obterClienteDetalhe } from '../api/obterClienteDetalhe';
import { Alert } from '../../../components/ui/Alert';
import { Spinner } from '../../../components/ui/Spinner';
import { PageHeader, Breadcrumbs, UsersIcon, HomeIcon } from '../../../components/ui';

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
          tipoPessoa: data.documento?.replace(/\D/g, '').length === 11 ? 1 : 2,
          nome: data.nome,
          documento: data.documento || data.documentoMascarado || '',
          dataNascimento: data.dataNascimento ? new Date(data.dataNascimento).toISOString().split('T')[0] : '',
          sexo: data.sexo,
          observacao: data.observacao || '',
          re: data.re || '',
          falecido: data.falecido || false,
          dataObito: data.dataObito ? new Date(data.dataObito).toISOString().split('T')[0] : '',
          contatos: data.contatos?.map((c: { tipo: string; valor: string; principal: boolean }) => ({
            tipoContato: c.tipo,
            valor: c.valor,
            principal: c.principal,
          })) || [],
          enderecos: data.enderecos?.map((e: Record<string, string>) => ({
            tipoEndereco: e.tipo || 'RESIDENCIAL',
            cep: e.cep || '',
            logradouro: e.logradouro || '',
            numero: e.numero || '',
            complemento: e.complemento || '',
            bairro: e.bairro || '',
            cidadeId: e.cidadeId || undefined,
            uf: e.uf || '',
            principal: e.principal || false,
          })) || [],
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
        documento: data.documento.replace(/\D/g, ''),
        dataNascimento: data.dataNascimento || undefined,
        sexo: data.sexo ? Number(data.sexo) : undefined,
        observacao: data.observacao || undefined,
        falecido: data.falecido,
        dataObito: data.dataObito || undefined,
        re: data.re || undefined,
        contatos: data.contatos.map(c => ({
          tipoContato: c.tipoContato,
          valor: c.valor,
          principal: c.principal,
        })),
        enderecos: data.enderecos.map(e => ({
          tipoEndereco: e.tipoEndereco,
          cep: e.cep || undefined,
          logradouro: e.logradouro || undefined,
          numero: e.numero || undefined,
          complemento: e.complemento || undefined,
          bairro: e.bairro || undefined,
          cidadeId: e.cidadeId ? Number(e.cidadeId) : undefined,
          uf: e.uf || undefined,
          principal: e.principal,
        })),
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

  const breadcrumbItems = [
    { label: 'Início', href: '/', icon: <HomeIcon size={14} /> },
    { label: 'Clientes', href: '/clientes', icon: <UsersIcon size={14} /> },
    { label: 'Editar Cliente' }
  ];

  return (
    <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none" role="main" tabIndex={-1}>
      <PageHeader 
        title="Editar Cliente"
        icon={<UsersIcon size={24} />}
        breadcrumbs={<Breadcrumbs items={breadcrumbItems} />}
      />

      <div className="w-full">
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
    </main>
  );
};

export default EditarClientePage;
