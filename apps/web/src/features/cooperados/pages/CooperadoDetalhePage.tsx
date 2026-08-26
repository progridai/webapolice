import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader, Breadcrumbs, Alert, Skeleton, Button, DetailsSection, DescriptionList, DescriptionItem, StatusBadge } from '../../../components/ui';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import { obterCooperadoDetalhe } from '../api/cooperadosApi';
import type { CooperadoDetalheDto } from '../types/cooperados.types';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { formatarDataOuVazio, formatarTelefone, formatarCep } from '../../../shared/utils/formatters';

export const CooperadoDetalhePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [data, setData] = useState<CooperadoDetalheDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { possuiPermissao, possuiAcessoTotal } = useAuthorization();
  const podeAlterar = possuiAcessoTotal() || possuiPermissao('cooperados.alterar');

  useEffect(() => {
    async function loadData() {
      if (!id) return;
      setIsLoading(true);
      setError(null);
      try {
        const dto = await obterCooperadoDetalhe(id);
        setData(dto);
      } catch (err: any) {
        console.error(err);
        setError('Não foi possível carregar os detalhes do cooperado.');
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, [id]);

  if (isLoading) {
    return (
      <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto w-full focus:outline-none" tabIndex={-1}>
        <Skeleton className="h-24 w-full rounded-lg" />
        <Skeleton className="h-64 w-full rounded-lg" />
      </main>
    );
  }

  if (error || !data) {
    return (
      <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto w-full focus:outline-none" tabIndex={-1}>
        <Alert variant="error" title="Erro">
          {error || 'Cooperado não encontrado.'}
        </Alert>
        <Button variant="secondary" onClick={() => navigate(ROUTES.COOPERADOS)} className="w-max">
          Voltar para listagem
        </Button>
      </main>
    );
  }

  return (
    <main className="flex flex-col gap-6 p-6 max-w-4xl mx-auto w-full focus:outline-none" role="main" tabIndex={-1}>
      <PageHeader
        title={data.nome}
        description={data.codigo && data.codigo.toLowerCase() !== data.nome.toLowerCase() ? data.codigo : undefined}
        titleExtras={<StatusBadge status={data.desativado ? 'inativo' : 'ativo'} />}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Cooperados', href: ROUTES.COOPERADOS },
              { label: 'Detalhes' },
            ]}
          />
        }
        actions={
          <div className="flex items-center gap-3">
            <Button variant="secondary" onClick={() => navigate(ROUTES.COOPERADOS)}>
              Voltar
            </Button>
            {podeAlterar && (
              <Button variant="primary" onClick={() => navigate(`${createPath(ROUTES.COOPERADOS_DETALHES, { id: data.publicId })}/editar`)}>
                Editar
              </Button>
            )}
          </div>
        }
      />

      <div className="flex flex-col gap-3">
        <DetailsSection title="Identificação">
          <DescriptionList columns={4}>
            <DescriptionItem label="Tipo" value={data.tipo === 1 ? 'Cooperado' : 'Coordenador'} className="sm:col-span-2" />
            <DescriptionItem label="CPF" value={data.cpf} />
            <DescriptionItem label="Data de Nascimento" value={formatarDataOuVazio(data.dataNascimento)} />
          </DescriptionList>
        </DetailsSection>

        <DetailsSection title="Documentação">
          <DescriptionList columns={4}>
            <DescriptionItem label="RG" value={data.rg} />
            <DescriptionItem label="Órgão Emissor" value={data.orgaoEmissor} />
            <DescriptionItem label="Data de Emissão" value={formatarDataOuVazio(data.dataEmissaoRg)} />
            <DescriptionItem label="SUSEP" value={data.susep} />
            <DescriptionItem label="INSS" value={data.inss} />
            <DescriptionItem label="ISSQN" value={data.issqn} />
          </DescriptionList>
        </DetailsSection>

        <DetailsSection title="Contato">
          <DescriptionList columns={4}>
            <DescriptionItem label="E-mail" value={data.email} className="sm:col-span-2" />
            <DescriptionItem label="Telefone" value={formatarTelefone(data.telefone)} className="sm:col-span-2" />
          </DescriptionList>
        </DetailsSection>

        <DetailsSection title="Endereço">
          <DescriptionList columns={4}>
            <DescriptionItem label="CEP" value={formatarCep(data.cep)} />
            <DescriptionItem label="Logradouro" value={data.logradouro} className="sm:col-span-3" />
            <DescriptionItem label="Número" value={data.numero} />
            <DescriptionItem label="Complemento" value={data.complemento} className="sm:col-span-2" />
            <DescriptionItem label="Bairro" value={data.bairro} />
            <DescriptionItem label="UF" value={data.uf} />
            {/* Ideally city is joined in backend or resolved, for now showing ID */}
            <DescriptionItem label="Cidade ID" value={data.cidadeId?.toString()} className="sm:col-span-3" />
          </DescriptionList>
        </DetailsSection>

        <DetailsSection title="Dados Operacionais">
          <DescriptionList columns={4}>
            <DescriptionItem label="Número de Dependentes" value={data.numeroDependentes} />
            <DescriptionItem label="Data de Inscrição" value={formatarDataOuVazio(data.dataInscricao)} />
            <DescriptionItem label="Credenciado" value={data.credenciado ? 'Sim' : 'Não'} />
            {data.tipo === 1 && (
              <DescriptionItem label="ID Coordenador" value={data.coordenadorId?.toString()} />
            )}
          </DescriptionList>
        </DetailsSection>

        <DetailsSection title="Dados Bancários">
          <DescriptionList columns={4}>
            <DescriptionItem label="Banco ID" value={data.bancoId?.toString()} />
            <DescriptionItem label="Agência" value={data.agencia} />
            <DescriptionItem label="Conta Corrente" value={data.contaCorrente} className="sm:col-span-2" />
          </DescriptionList>
        </DetailsSection>

        {data.observacao && (
          <DetailsSection title="Observações">
            <p className="text-sm text-texto-secundario whitespace-pre-wrap">{data.observacao}</p>
          </DetailsSection>
        )}
      </div>
    </main>
  );
};
