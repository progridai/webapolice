import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { UsuarioForm, type UsuarioFormData } from '../components/UsuarioForm';
import { obterUsuarioDetalhe, atualizarUsuario } from '../api/usuariosApi';
import { listarPerfis } from '../api/perfisApi';
import { Alert, PageHeader, Breadcrumbs, Spinner, Button } from '../../../components/ui';
import type { PerfilDto } from '../types/seguranca.types';

export const EditarUsuarioPage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();

  const [initialData, setInitialData] = useState<Partial<UsuarioFormData> | null>(null);
  const [perfis, setPerfis] = useState<PerfilDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    document.title = 'Editar Usuário | WebApolice';
    const fetch = async () => {
      if (!publicId) return;
      try {
        const [p, usuario] = await Promise.all([
          listarPerfis({ pageSize: 1000 }),
          obterUsuarioDetalhe(publicId)
        ]);
        
        setPerfis(p.itens || []);
        setInitialData({
          username: usuario.username,
          nome: usuario.nome,
          email: usuario.email,
          ativo: usuario.ativo,
          perfilPublicIds: usuario.perfisAtribuidos.map(p => p.publicId),
        });
      } catch (err: unknown) {
        console.error('Erro ao carregar usuário:', err);
        setError('Não foi possível carregar os dados do usuário.');
      } finally {
        setIsLoading(false);
      }
    };
    fetch();
  }, [publicId]);

  const handleSubmit = async (data: UsuarioFormData) => {
    if (!publicId) return;
    setIsSubmitting(true);
    setError(null);
    try {
      await atualizarUsuario(publicId, {
        nome: data.nome,
        email: data.email,
        ativo: data.ativo,
        perfilPublicIds: data.perfilPublicIds,
      });
      navigate(`/seguranca/usuarios/${publicId}`);
    } catch (err: unknown) {
      console.error('Erro ao editar usuário:', err);
      const errorResponse = err as { response?: { status?: number; data?: { message?: string } } };
      setError(errorResponse.response?.data?.message || 'Ocorreu um erro ao salvar o usuário.');
    } finally {
      setIsSubmitting(false);
    }
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
      <main className="p-6 max-w-7xl mx-auto focus:outline-none">
        <Alert variant="error" title="Erro">{error}</Alert>
        <Button className="mt-4" onClick={() => navigate('/seguranca/usuarios')}>
          Voltar
        </Button>
      </main>
    );
  }

  return (
    <main className="p-6 max-w-7xl mx-auto focus:outline-none flex flex-col gap-6">
      <PageHeader
        title="Editar Usuário"
        description="Atualize as informações e os acessos deste usuário."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Usuários', href: '/seguranca/usuarios' },
              { label: 'Editar' },
            ]}
          />
        }
      />

      <div className="max-w-4xl">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro ao Salvar">{error}</Alert>
          </div>
        )}
        
        {initialData && (
          <UsuarioForm
            initialData={initialData}
            perfisDisponiveis={perfis}
            isEdit={true}
            onSubmit={handleSubmit}
            onCancel={() => navigate(`/seguranca/usuarios/${publicId}`)}
            isSubmitting={isSubmitting}
          />
        )}
      </div>
    </main>
  );
};
