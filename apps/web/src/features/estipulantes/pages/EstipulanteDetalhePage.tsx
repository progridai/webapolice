import React, { useEffect, useRef } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { 
  Button, 
  Alert, 
  Skeleton, 
  EmptyState,
  Breadcrumbs,
  PageHeader,
  EntitySummary,
  DetailsSection,
  DescriptionList,
  StatusBadge,
  Badge
} from '../../../components/ui';
import { DescriptionItem } from '../../../components/ui/DescriptionList';
import { useEstipulanteDetalhe } from '../hooks/useEstipulanteDetalhe';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

// Data formatter helper inline for configuration
const formatDate = (dateStr?: string) => {
  if (!dateStr) return 'Não informado';
  return new Date(dateStr).toLocaleDateString('pt-BR');
};

export const EstipulanteDetalhePage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const mainRef = useRef<HTMLElement>(null);
  
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();
  const podeAlterar = possuiAcessoTotal() || possuiPermissao('estipulantes.alterar');
  
  const { data, isLoading, error, retry } = useEstipulanteDetalhe(publicId);

  // Foco acessível ao entrar na página
  useEffect(() => {
    if (!isLoading && mainRef.current) {
      mainRef.current.focus();
    }
  }, [isLoading]);

  // Atualizar document title
  useEffect(() => {
    if (data?.estipulante) {
      document.title = `Detalhes do Estipulante: ${data.estipulante.razaoSocial} | webapolice`;
    } else {
      document.title = 'Detalhes do Estipulante | webapolice';
    }
  }, [data]);

  const handleVoltar = () => {
    if (location.state?.fromListagem) {
      navigate('/estipulantes', { state: location.state });
    } else {
      navigate('/estipulantes');
    }
  };

  if (isLoading) {
    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none" aria-busy="true">
        <div className="mb-6">
          <Skeleton className="w-32 h-10 mb-4" />
          <Skeleton className="w-full h-32" />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Skeleton className="w-full h-48" />
          <Skeleton className="w-full h-48" />
        </div>
      </main>
    );
  }

  if (error) {
    const errorResponse = error as any;
    if (errorResponse?.status === 404) {
      return (
        <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
          <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
            &larr; Voltar para estipulantes
          </Button>
          <EmptyState
            title="Estipulante não encontrado"
            description="O estipulante que você tentou acessar não existe ou foi excluído."
          />
        </main>
      );
    }
    
    if (errorResponse?.status === 403) {
      return (
        <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
          <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
            &larr; Voltar para estipulantes
          </Button>
          <Alert variant="error" title="Acesso negado" role="alert">
            Você não tem permissão para visualizar os detalhes deste estipulante.
          </Alert>
        </main>
      );
    }

    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
        <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
          &larr; Voltar para estipulantes
        </Button>
        <Alert
          variant="error"
          title="Erro ao carregar detalhes"
          role="alert"
          action={
            <Button variant="primary" size="sm" onClick={retry}>
              Tentar novamente
            </Button>
          }
        >
          {error.message || 'Não foi possível carregar os dados do estipulante.'}
        </Alert>
      </main>
    );
  }

  if (!data || !data.estipulante) return null;

  const { estipulante, configuracao } = data;

  const contatosAtivos = estipulante.contatos?.sort((a, b) => (a.principal === b.principal ? 0 : a.principal ? -1 : 1)) || [];
  
  // Format CNPJ Helper
  const formatCnpj = (cnpj: string) => {
    if (!cnpj) return '';
    const unmasked = cnpj.replace(/\D/g, '');
    if (unmasked.length === 14) {
      return unmasked.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5');
    }
    return cnpj;
  };

  return (
    <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none flex flex-col gap-6">
      <PageHeader
        title="Detalhes do Estipulante"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Estipulantes', href: '#' },
              { label: 'Detalhes' }
            ]}
          />
        }
        actions={
          <div className="flex gap-2">
            <Button variant="ghost" onClick={handleVoltar}>Voltar</Button>
            {podeAlterar && (
              <Button variant="primary" onClick={() => navigate(`/estipulantes/${publicId}/editar`)}>Editar Estipulante</Button>
            )}
          </div>
        }
      />

      <EntitySummary
        name={estipulante.razaoSocial}
        documentInfo={formatCnpj(estipulante.cnpj)}
        badges={<StatusBadge status={estipulante.ativo ? 'ativo' : 'inativo'} />}
        secondaryInfo={
          <div className="flex flex-wrap gap-6">
            <div className="flex flex-col">
              <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">Código</span>
              <span className="text-sm text-slate-900 dark:text-slate-50">{estipulante.codigo || 'Não informado'}</span>
            </div>
            {estipulante.nomeFantasia && (
              <div className="flex flex-col">
                <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">Nome Fantasia</span>
                <span className="text-sm text-slate-900 dark:text-slate-50">{estipulante.nomeFantasia}</span>
              </div>
            )}
          </div>
        }
      />

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <div className="flex flex-col gap-6">
          <DetailsSection title="Dados principais">
            <DescriptionList columns={2}>
              <DescriptionItem label="Razão Social" value={estipulante.razaoSocial} />
              <DescriptionItem label="Nome Fantasia" value={estipulante.nomeFantasia || '-'} />
              <DescriptionItem label="CNPJ" value={formatCnpj(estipulante.cnpj)} />
              <DescriptionItem label="Código" value={estipulante.codigo || '-'} />
              <DescriptionItem label="Observação" value={estipulante.observacao || '-'} />
            </DescriptionList>
          </DetailsSection>

          <DetailsSection title="Configuração Operacional" isEmpty={!configuracao} emptyState="Nenhuma configuração cadastrada.">
            {configuracao && (
              <DescriptionList columns={2}>
                <DescriptionItem label="Início da Vigência" value={formatDate(configuracao.dataInicioVigencia)} />
                <DescriptionItem label="Fim da Vigência" value={formatDate(configuracao.dataFimVigencia)} />
              </DescriptionList>
            )}
          </DetailsSection>
        </div>

        <div className="flex flex-col gap-6">
          <DetailsSection title="Endereço principal" isEmpty={!estipulante.endereco} emptyState="Nenhum endereço cadastrado.">
            {estipulante.endereco && (
              <div className="flex flex-col p-4 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50">
                <p className="text-sm text-slate-700 dark:text-slate-300">
                  {estipulante.endereco.logradouro}, {estipulante.endereco.numero} {estipulante.endereco.complemento && `- ${estipulante.endereco.complemento}`}
                </p>
                <p className="text-sm text-slate-700 dark:text-slate-300">
                  {estipulante.endereco.bairro} - {estipulante.endereco.cidadeNome || estipulante.endereco.cidadeId}/{estipulante.endereco.uf}
                </p>
                <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">CEP: {estipulante.endereco.cep}</p>
              </div>
            )}
          </DetailsSection>

          <DetailsSection title="Contatos" isEmpty={contatosAtivos.length === 0} emptyState="Nenhum contato cadastrado.">
            <div className="flex flex-col gap-4">
              {contatosAtivos.map((contato, index) => (
                <div key={index} className="flex items-center justify-between p-3 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50">
                  <div>
                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                      {contato.tipoContato}
                    </p>
                    <p className="text-base text-slate-900 dark:text-slate-50">{contato.valor}</p>
                  </div>
                  {contato.principal && (
                    <Badge variant="neutral" className="bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800">
                      Principal
                    </Badge>
                  )}
                </div>
              ))}
            </div>
          </DetailsSection>
        </div>
      </div>
    </main>
  );
};
