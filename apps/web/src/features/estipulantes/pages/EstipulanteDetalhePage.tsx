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

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-3">
        <div className="flex flex-col gap-3">
          <DetailsSection title="Dados principais">
            <DescriptionList columns={2} density="compact">
              <DescriptionItem label="Razão Social" value={estipulante.razaoSocial} />
              {estipulante.nomeFantasia && (
                <DescriptionItem label="Nome Fantasia" value={estipulante.nomeFantasia} />
              )}
              <DescriptionItem label="CNPJ" value={formatCnpj(estipulante.cnpj)} />
              <DescriptionItem label="Código" value={estipulante.codigo || '-'} />
              <DescriptionItem label="Grupo" value={estipulante.grupoPublicId || 'Não vinculado'} />
              <DescriptionItem label="Seguradora" value={estipulante.seguradoraPublicId || 'Não vinculada'} />
              <DescriptionItem label="Observação" value={estipulante.observacao || '-'} className="sm:col-span-2" />
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

        <div className="flex flex-col gap-3">
          <DetailsSection title="Endereço principal" isEmpty={!estipulante.endereco} emptyState="Nenhum endereço cadastrado.">
            {estipulante.endereco && (
              <DescriptionList columns={4} density="compact">
                <DescriptionItem label="Logradouro" value={`${estipulante.endereco.logradouro}, ${estipulante.endereco.numero}`} className="sm:col-span-2 lg:col-span-3" />
                {estipulante.endereco.complemento && (
                  <DescriptionItem label="Complemento" value={estipulante.endereco.complemento} className="sm:col-span-1" />
                )}
                <DescriptionItem label="Bairro" value={estipulante.endereco.bairro} className="sm:col-span-1 lg:col-span-1" />
                <DescriptionItem label="Cidade / UF" value={`${estipulante.endereco.cidadeNome || estipulante.endereco.cidadeId} / ${estipulante.endereco.uf}`} className="sm:col-span-1 lg:col-span-2" />
                <DescriptionItem label="CEP" value={formatarCep(estipulante.endereco.cep)} className="sm:col-span-1" />
              </DescriptionList>
            )}
          </DetailsSection>

          <DetailsSection title="Contatos" isEmpty={contatosAtivos.length === 0} emptyState="Nenhum contato cadastrado.">
            <DescriptionList columns={2} density="compact">
              {contatosAtivos.map((contato, index) => (
                <DescriptionItem 
                  key={index} 
                  label={
                    <span className="flex items-center gap-2">
                      {contato.tipoContato}
                      {contato.principal && <span className="bg-marca-principal/10 text-marca-principal px-1 rounded text-[9px] font-bold">PRINCIPAL</span>}
                    </span>
                  } 
                  value={formatarValorContato(contato.tipoContato, contato.valor)} 
                  className={contato.tipoContato.toUpperCase() === 'EMAIL' ? 'sm:col-span-2' : 'sm:col-span-1'}
                />
              ))}
            </DescriptionList>
          </DetailsSection>

          <DetailsSection title="Contatos Institucionais" isEmpty={!estipulante.contatosInstitucionais || estipulante.contatosInstitucionais.length === 0} emptyState="Nenhum contato institucional cadastrado.">
            {estipulante.contatosInstitucionais && estipulante.contatosInstitucionais.length > 0 && (
              <DescriptionList columns={2} density="compact">
                {estipulante.contatosInstitucionais.map((contato, index) => (
                  <React.Fragment key={index}>
                    <DescriptionItem label="Nome / Departamento" value={`${contato.nome} - ${contato.departamento}`} className="sm:col-span-2" />
                    <DescriptionItem label="E-mail" value={contato.email || '-'} className="sm:col-span-2" />
                    <DescriptionItem label="Telefone" value={contato.telefone || '-'} className="sm:col-span-1" />
                    <DescriptionItem label="Ramal" value={contato.ramal || '-'} className="sm:col-span-1" />
                    {index < estipulante.contatosInstitucionais!.length - 1 && (
                      <div className="sm:col-span-2 border-b border-borda my-1" />
                    )}
                  </React.Fragment>
                ))}
              </DescriptionList>
            )}
          </DetailsSection>
        </div>
      </div>
    </main>
  );
};
