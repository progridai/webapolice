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
import { useClienteDetalhe } from '../hooks/useClienteDetalhe';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';

export const ClienteDetalhePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const mainRef = useRef<HTMLElement>(null);
  
  const { data: cliente, isLoading, error, retry } = useClienteDetalhe(id);

  // Foco acessível ao entrar na página
  useEffect(() => {
    if (!isLoading && mainRef.current) {
      mainRef.current.focus();
    }
  }, [isLoading]);

  // Atualizar document title
  useEffect(() => {
    document.title = cliente ? `Detalhes do cliente: ${cliente.nome} | webapolice` : 'Detalhes do cliente | webapolice';
  }, [cliente]);

  const handleVoltar = () => {
    // Retorna para a listagem preservando filtros se existirem no state
    if (location.state?.fromListagem) {
      navigate('/clientes', { state: location.state });
    } else {
      navigate('/clientes');
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
          <Skeleton className="w-full h-48" />
          <Skeleton className="w-full h-48" />
        </div>
      </main>
    );
  }

  if (error) {
    if (error.name === 'HttpApiError') {
      const httpError = error as unknown as { status: number };
      if (httpError.status === 404) {
        return (
          <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
            <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
              &larr; Voltar para clientes
            </Button>
            <EmptyState
              title="Cliente não encontrado"
              description="O cliente que você tentou acessar não existe ou foi excluído."
            />
          </main>
        );
      }
      if (httpError.status === 403) {
        return (
          <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
            <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
              &larr; Voltar para clientes
            </Button>
            <Alert variant="error" title="Acesso negado" role="alert">
              Você não tem permissão para visualizar os detalhes deste cliente.
            </Alert>
          </main>
        );
      }
    }

    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none">
        <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
          &larr; Voltar para clientes
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
          {error.message || 'Não foi possível carregar os dados do cliente.'}
        </Alert>
      </main>
    );
  }

  if (!cliente) return null;

  const contatosAtivos = cliente.contatos.filter((c) => c.ativo).sort((a, b) => (a.principal === b.principal ? 0 : a.principal ? -1 : 1));
  const enderecosAtivos = cliente.enderecos.filter((e) => e.ativo).sort((a, b) => (a.principal === b.principal ? 0 : a.principal ? -1 : 1));

  return (
    <main ref={mainRef} tabIndex={-1} className="p-6 max-w-7xl mx-auto focus:outline-none flex flex-col gap-6">
      <PageHeader
        title="Detalhes do Cliente"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Clientes', href: '#' },
              { label: 'Detalhes' }
            ]}
          />
        }
        actions={
          <div className="flex gap-2">
            <Button variant="ghost" onClick={handleVoltar}>Voltar</Button>
            <Button variant="primary" onClick={() => navigate(`/clientes/${id}/editar`)}>Editar Cliente</Button>
          </div>
        }
      />

      <EntitySummary
        name={cliente.nome}
        documentInfo={cliente.documentoMascarado || 'Documento não informado'}
        badges={<StatusBadge status={cliente.status.codigo} />}
        secondaryInfo={
          <div className="flex flex-wrap gap-6">
            <div className="flex flex-col">
              <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">Nascimento</span>
              <span className="text-sm text-slate-900 dark:text-slate-50">{formatarDataOuVazio(cliente.dataNascimento)}</span>
            </div>
            {cliente.falecido && (
              <div className="flex flex-col">
                <span className="text-xs font-semibold text-red-500 uppercase tracking-wider">Falecido</span>
                <span className="text-sm text-slate-900 dark:text-slate-50">
                  Sim {cliente.dataObito ? `(${formatarDataOuVazio(cliente.dataObito)})` : ''}
                </span>
              </div>
            )}
          </div>
        }
      />

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <div className="flex flex-col gap-6">
          <DetailsSection title="Dados pessoais">
            <DescriptionList columns={2}>
              <DescriptionItem label="Nome completo" value={cliente.nome} />
              <DescriptionItem label="Documento principal" value={cliente.documentoMascarado} />
              {/* Mais dados poderiam ser exibidos aqui */}
            </DescriptionList>
          </DetailsSection>

          <DetailsSection title="Contatos" isEmpty={contatosAtivos.length === 0} emptyState="Nenhum contato cadastrado.">
            <div className="flex flex-col gap-4">
              {contatosAtivos.map((contato, index) => (
                <div key={index} className="flex items-center justify-between p-3 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50">
                  <div>
                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                      {contato.tipo}
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

          <DetailsSection title="Endereços" isEmpty={enderecosAtivos.length === 0} emptyState="Nenhum endereço cadastrado.">
            <div className="flex flex-col gap-4">
              {enderecosAtivos.map((endereco, index) => (
                <div key={index} className="flex flex-col p-4 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50">
                  <div className="flex items-center gap-2 mb-2">
                    <p className="text-sm font-semibold text-slate-900 dark:text-slate-50">{endereco.tipo}</p>
                    {endereco.principal && (
                      <Badge variant="neutral" className="bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800">
                        Principal
                      </Badge>
                    )}
                  </div>
                  <p className="text-sm text-slate-700 dark:text-slate-300">
                    {endereco.logradouro}, {endereco.numero} {endereco.complemento && `- ${endereco.complemento}`}
                  </p>
                  <p className="text-sm text-slate-700 dark:text-slate-300">
                    {endereco.bairro} - {endereco.cidade}/{endereco.uf}
                  </p>
                  <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">CEP: {endereco.cep}</p>
                </div>
              ))}
            </div>
          </DetailsSection>
        </div>

        <div className="flex flex-col gap-6">
          <DetailsSection title="Vínculos" isEmpty={cliente.vinculos.length === 0} emptyState="Nenhum vínculo cadastrado.">
            <div className="flex flex-col gap-4">
              {cliente.vinculos.map((vinculo, index) => (
                <div key={index} className="p-4 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50">
                  <div className="flex items-center justify-between mb-3 border-b border-slate-200 dark:border-slate-700 pb-3">
                    <div>
                      <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">Matrícula</p>
                      <p className="text-base font-semibold text-slate-900 dark:text-slate-50">{vinculo.matricula}</p>
                    </div>
                    <Badge variant={vinculo.ativo ? 'success' : 'neutral'}>{vinculo.ativo ? 'Ativo' : 'Inativo'}</Badge>
                  </div>
                  <DescriptionList columns={2} className="!gap-4">
                    {vinculo.estipulante && <DescriptionItem label="Estipulante" value={vinculo.estipulante} />}
                    {vinculo.subestipulante && <DescriptionItem label="Subestipulante" value={vinculo.subestipulante} />}
                    {vinculo.grupo && <DescriptionItem label="Grupo" value={vinculo.grupo} />}
                    {vinculo.subgrupo && <DescriptionItem label="Subgrupo" value={vinculo.subgrupo} />}
                    {vinculo.lotacao && <DescriptionItem label="Lotação" value={vinculo.lotacao} />}
                  </DescriptionList>
                </div>
              ))}
            </div>
          </DetailsSection>

          <DetailsSection title="Dependentes" isEmpty={cliente.dependentes.length === 0} emptyState="Nenhum dependente cadastrado.">
            <div className="flex flex-col gap-4">
              {cliente.dependentes.map((dependente, index) => (
                <div key={index} className="p-4 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50">
                  <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-2">
                    <div>
                      <p className="text-base font-medium text-slate-900 dark:text-slate-50">{dependente.nome}</p>
                      <p className="text-sm text-slate-500 dark:text-slate-400">{dependente.tipoRelacao}</p>
                    </div>
                    <div className="text-left sm:text-right">
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-300">
                        {dependente.documentoMascarado || 'Documento não informado'}
                      </p>
                      {dependente.dataNascimento && (
                        <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                          Nasc: {formatarDataOuVazio(dependente.dataNascimento)}
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </DetailsSection>
        </div>
      </div>
    </main>
  );
};
