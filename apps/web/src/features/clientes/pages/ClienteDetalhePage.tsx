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
import { formatarDataOuVazio, formatarValorContato, formatarCep } from '../../../shared/utils/formatters';
import { useAuthorization } from '../../../auth/AuthorizationProvider';

export const ClienteDetalhePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const mainRef = useRef<HTMLElement>(null);
  const { possuiPermissao, possuiAcessoTotal, possuiRecurso } = useAuthorization();
  const podeAlterar = possuiAcessoTotal() || possuiPermissao('clientes.alterar');
  
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
            {podeAlterar && (
              <Button variant="primary" onClick={() => navigate(`/clientes/${id}/editar`)}>Editar Cliente</Button>
            )}
          </div>
        }
      />

      <EntitySummary
        name={cliente.nome}
        badges={<StatusBadge status={cliente.status.codigo} />}
        secondaryInfo={
          cliente.falecido ? (
            <div className="flex flex-wrap gap-6">
              <div className="flex flex-col">
                <span className="text-xs font-semibold text-erro uppercase tracking-wider">Falecido</span>
                <span className="text-sm text-texto-principal">
                  Sim {cliente.dataObito ? `(${formatarDataOuVazio(cliente.dataObito)})` : ''}
                </span>
              </div>
            </div>
          ) : undefined
        }
      />

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <div className="flex flex-col gap-6">
          <DetailsSection title="Dados pessoais">
            <DescriptionList columns={2}>
              <DescriptionItem
                label="Tipo de Pessoa"
                value={
                  cliente.tipoPessoa === 2
                    ? 'Pessoa Jurídica'
                    : cliente.documento?.replace(/\D/g, '').length === 14
                    ? 'Pessoa Jurídica'
                    : 'Pessoa Física'
                }
              />
              <DescriptionItem label="Documento principal" value={cliente.documentoMascarado} />
              <DescriptionItem label="Nome completo" value={cliente.nome} />
              <DescriptionItem
                label="Sexo"
                value={
                  cliente.sexo === 1 ? 'Masculino'
                  : cliente.sexo === 2 ? 'Feminino'
                  : 'Não informado'
                }
              />
              <DescriptionItem
                label="Data de Nascimento"
                value={formatarDataOuVazio(cliente.dataNascimento)}
              />
              {possuiRecurso('RE') && (
                <DescriptionItem label="RE" value={cliente.re || 'Não informado'} />
              )}
            </DescriptionList>
          </DetailsSection>

          <DetailsSection title="Contatos" isEmpty={contatosAtivos.length === 0} emptyState="Nenhum contato cadastrado.">
            <div className="flex flex-col gap-4">
              {contatosAtivos.map((contato, index) => (
                <div key={index} className="flex items-center justify-between p-3 rounded-md bg-fundo-aplicacao border border-borda">
                  <div>
                    <p className="text-xs font-medium text-texto-secundario uppercase tracking-wider mb-1">
                      {contato.tipo}
                    </p>
                    <p className="text-base text-texto-principal">{formatarValorContato(contato.tipo, contato.valor)}</p>
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
                <div key={index} className="flex flex-col p-4 rounded-md bg-fundo-aplicacao border border-borda">
                  <div className="flex items-center gap-2 mb-2">
                    <p className="text-sm font-semibold text-texto-principal">{endereco.tipo}</p>
                    {endereco.principal && (
                      <Badge variant="neutral" className="bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800">
                        Principal
                      </Badge>
                    )}
                  </div>
                  <p className="text-sm text-texto-secundario">
                    {endereco.logradouro}, {endereco.numero} {endereco.complemento && `- ${endereco.complemento}`}
                  </p>
                  <p className="text-sm text-texto-secundario">
                    {endereco.bairro} - {endereco.cidade}/{endereco.uf}
                  </p>
                  <p className="text-sm text-texto-secundario mt-1">CEP: {formatarCep(endereco.cep)}</p>
                </div>
              ))}
            </div>
          </DetailsSection>

          <DetailsSection title="Informações Adicionais">
            <DescriptionList columns={2}>
              <DescriptionItem
                label="Cliente Falecido/Extinto"
                value={cliente.falecido ? 'Sim' : 'Não'}
              />
              {cliente.falecido && (
                <DescriptionItem
                  label="Data de Óbito"
                  value={formatarDataOuVazio(cliente.dataObito)}
                />
              )}
              <DescriptionItem
                label="Observações"
                value={cliente.observacao || '—'}
                className="col-span-2"
              />
            </DescriptionList>
          </DetailsSection>
        </div>

        <div className="flex flex-col gap-6">
          <DetailsSection title="Vínculos" isEmpty={cliente.vinculos.length === 0} emptyState="Nenhum vínculo cadastrado.">
            <div className="flex flex-col gap-4">
              {cliente.vinculos.map((vinculo, index) => (
                <div key={index} className="p-4 rounded-md bg-fundo-aplicacao border border-borda">
                  <div className="flex items-center justify-between mb-3 border-b border-borda pb-3">
                    <div>
                      <p className="text-xs font-medium text-texto-secundario uppercase tracking-wider mb-1">Matrícula</p>
                      <p className="text-base font-semibold text-texto-principal">{vinculo.matricula}</p>
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
                <div key={index} className="p-4 rounded-md bg-fundo-aplicacao border border-borda">
                  <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-2">
                    <div>
                      <p className="text-base font-medium text-texto-principal">{dependente.nome}</p>
                      <p className="text-sm text-texto-secundario">{dependente.tipoRelacao}</p>
                    </div>
                    <div className="text-left sm:text-right">
                      <p className="text-sm font-medium text-texto-principal">
                        {dependente.documentoMascarado || 'Documento não informado'}
                      </p>
                      {dependente.dataNascimento && (
                        <p className="text-xs text-texto-secundario mt-0.5">
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
