import React, { useEffect, useRef } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { 
  Button, 
  Alert, 
  Skeleton, 
  EmptyState,
  Breadcrumbs,
  PageHeader,
  DetailsSection,
  DescriptionList,
  StatusBadge,
  Badge
} from '../../../components/ui';
import { DescriptionItem } from '../../../components/ui/DescriptionList';
import { useEstipulanteDetalhe } from '../hooks/useEstipulanteDetalhe';
import { formatarValorContato, formatarCep } from '../../../shared/utils/formatters';
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
    <main ref={mainRef} tabIndex={-1} className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
      <PageHeader
        title={estipulante.razaoSocial}
        titleExtras={<StatusBadge status={estipulante.ativo ? 'ativo' : 'inativo'} />}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Estipulantes', href: '/estipulantes' },
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

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <div className="flex flex-col gap-6">
          <DetailsSection title="Dados principais">
            <DescriptionList columns={2} density="compact">
              <DescriptionItem label="Razão Social" value={estipulante.razaoSocial} />
              {estipulante.nomeFantasia && (
                <DescriptionItem label="Nome Fantasia" value={estipulante.nomeFantasia} />
              )}
              <DescriptionItem label="CNPJ" value={formatCnpj(estipulante.cnpj)} />
              <DescriptionItem label="Código" value={estipulante.codigo || '-'} />
              <DescriptionItem label="Observação" value={estipulante.observacao || '-'} />
            </DescriptionList>
          </DetailsSection>

          <DetailsSection title="Configuração Operacional" isEmpty={!configuracao} emptyState="Nenhuma configuração cadastrada.">
            {configuracao && (
              <DescriptionList columns={2} density="compact">
                <DescriptionItem label="Início da Vigência" value={formatDate(configuracao.dataInicioVigencia)} />
                <DescriptionItem label="Fim da Vigência" value={formatDate(configuracao.dataFimVigencia)} />
              </DescriptionList>
            )}
          </DetailsSection>
        </div>

        <div className="flex flex-col gap-6">
          <DetailsSection title="Endereço principal" isEmpty={!estipulante.endereco} emptyState="Nenhum endereço cadastrado.">
            {estipulante.endereco && (
              <div className="p-4 rounded-md bg-fundo-aplicacao border border-borda">
                <p className="text-sm text-texto-principal">
                  {estipulante.endereco.logradouro}, {estipulante.endereco.numero} {estipulante.endereco.complemento && `- ${estipulante.endereco.complemento}`}
                </p>
                <p className="text-sm text-texto-principal">
                  {estipulante.endereco.bairro} - {estipulante.endereco.cidadeNome || estipulante.endereco.cidadeId}/{estipulante.endereco.uf}
                </p>
                <p className="text-sm text-texto-secundario mt-1">CEP: {formatarCep(estipulante.endereco.cep)}</p>
              </div>
            )}
          </DetailsSection>

          <DetailsSection title="Contatos" isEmpty={contatosAtivos.length === 0} emptyState="Nenhum contato cadastrado.">
            <div className="flex flex-col gap-6">
              {contatosAtivos.map((contato, index) => (
                <div key={index} className="p-4 rounded-md bg-fundo-aplicacao border border-borda flex items-center justify-between">
                  <div>
                    <p className="text-xs font-medium text-texto-secundario uppercase tracking-[0.05em] mb-1">
                      {contato.tipoContato}
                    </p>
                    <p className="text-base text-texto-principal">{formatarValorContato(contato.tipoContato, contato.valor)}</p>
                  </div>
                  {contato.principal && (
                    <Badge variant="primary">
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
