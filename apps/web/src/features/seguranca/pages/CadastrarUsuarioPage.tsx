import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { UsuarioForm, type UsuarioFormData } from '../components/UsuarioForm';
import { criarUsuario } from '../api/usuariosApi';
import { listarPerfis } from '../api/perfisApi';
import { Alert, PageHeader, Breadcrumbs, Spinner } from '../../../components/ui';
import type { PerfilDto } from '../types/seguranca.types';

export const CadastrarUsuarioPage: React.FC = () => {
  const navigate = useNavigate();
  const [perfis, setPerfis] = useState<PerfilDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    document.title = 'Novo Usuário | WebApolice';
    const fetch = async () => {
      try {
        const p = await listarPerfis({ pageSize: 1000 }); // Busca todos
        setPerfis(p.itens || []);
      } catch {
        setError('Erro ao carregar os perfis disponíveis.');
      } finally {
        setIsLoading(false);
      }
    };
    fetch();
  }, []);

  const handleSubmit = async (data: UsuarioFormData) => {
    setIsSubmitting(true);
    setError(null);
    try {
      const response = await criarUsuario({
        username: data.username,
        nome: data.nome,
        email: data.email,
        senhaTemporaria: data.senhaTemporaria || '', // existirá pq é criação
        ativo: data.ativo,
        perfilPublicIds: data.perfilPublicIds,
      });
      // "O formulário de criação de usuário deve limpar imediatamente os campos de senha após sucesso"
      // Redirecionar para detalhes (ou listagem)
      navigate(`/seguranca/usuarios/${response.id}`);
    } catch (err: unknown) {
      console.error('Erro ao cadastrar usuário:', err);
      const errorResponse = err as { response?: { status?: number; data?: { message?: string } } };
      setError(errorResponse.response?.data?.message || 'Ocorreu um erro ao cadastrar o usuário. Verifique os dados e tente novamente.');
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

  return (
    <main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
      <PageHeader
        title="Novo Usuário"
        description="Crie um novo usuário para acesso à plataforma."
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: '/' },
              { label: 'Segurança' },
              { label: 'Usuários', href: '/seguranca/usuarios' },
              { label: 'Novo' },
            ]}
          />
        }
      />

      <div className="w-full max-w-[800px]">
        {error && (
          <div className="mb-4">
            <Alert variant="error" title="Erro no Cadastro">{error}</Alert>
          </div>
        )}
        <UsuarioForm
          perfisDisponiveis={perfis}
          onSubmit={handleSubmit}
          onCancel={() => navigate('/seguranca/usuarios')}
          isSubmitting={isSubmitting}
        />
      </div>
    </main>
  );
};
