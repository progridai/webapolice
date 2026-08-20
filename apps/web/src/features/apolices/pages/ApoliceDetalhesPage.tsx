import React, { useEffect, useRef, useState } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { 
  Button, 
  Alert, 
  Skeleton, 
  Breadcrumbs,
  PageHeader,
  StatusBadge
} from '../../../components/ui';
import { useApoliceDetalhe } from '../hooks/useApoliceDetalhe';

import { ResumoTab } from '../components/tabs/ResumoTab';
import { RamosTab } from '../components/tabs/RamosTab';
import { ConfiguracoesTab } from '../components/tabs/ConfiguracoesTab';
import { SubestipulantesTab } from '../components/tabs/SubestipulantesTab';
import { VidasTab } from '../components/tabs/VidasTab';
import { UniversoPermitidoTab } from '../components/tabs/UniversoPermitidoTab';
import { HistoricoTab } from '../components/tabs/HistoricoTab';

type TabKey = 'resumo' | 'ramos' | 'configuracoes' | 'subestipulantes' | 'vidas' | 'universo' | 'historico';

const TABS: { key: TabKey; label: string }[] = [
  { key: 'resumo', label: 'Resumo' },
  { key: 'ramos', label: 'Ramos' },
  { key: 'configuracoes', label: 'Configurações' },
  { key: 'subestipulantes', label: 'Subestipulantes' },
  { key: 'vidas', label: 'Vidas' },
  { key: 'universo', label: 'Universo Permitido' },
  { key: 'historico', label: 'Histórico' },
];

import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { ROUTES } from '../../../app/routes/routePaths';

export const ApoliceDetalhesPage: React.FC = () => {
  const { publicId } = useParams<{ publicId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const mainRef = useRef<HTMLElement>(null);
  
  const [activeTab, setActiveTab] = useState<TabKey>('resumo');
  const { data, isLoading, error, retry } = useApoliceDetalhe(publicId);
  const { possuiPermissao } = useAuthorization();
  const podeEditar = possuiPermissao('apolices.alterar');

  useEffect(() => {
    if (!isLoading && mainRef.current) {
      mainRef.current.focus();
    }
  }, [isLoading]);

  useEffect(() => {
    if (data) {
      document.title = `Detalhes da Apólice: ${data.nome} | WebApolice`;
    } else {
      document.title = 'Detalhes da Apólice | WebApolice';
    }
  }, [data]);

  const handleVoltar = () => {
    if (location.state?.fromListagem) {
      navigate('/apolices', { state: location.state });
    } else {
      navigate('/apolices');
    }
  };

  if (isLoading) {
    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-[1440px] mx-auto focus:outline-none" aria-busy="true">
        <div className="mb-6">
          <Skeleton className="w-32 h-10 mb-4" />
          <Skeleton className="w-full h-32" />
        </div>
        <Skeleton className="w-full h-[400px]" />
      </main>
    );
  }

  if (error) {
    return (
      <main ref={mainRef} tabIndex={-1} className="p-6 max-w-[1440px] mx-auto focus:outline-none">
        <Button variant="ghost" onClick={handleVoltar} className="mb-4" aria-label="Voltar para listagem">
          &larr; Voltar para apólices
        </Button>
        <Alert
          variant="error"
          title="Erro ao carregar detalhes"
          role="alert"
          action={
            <Button variant="primary" size="small" onClick={retry}>
              Tentar novamente
            </Button>
          }
        >
          {error.message || 'Não foi possível carregar os dados da apólice.'}
        </Alert>
      </main>
    );
  }

  if (!data) return null;

  return (
    <main ref={mainRef} tabIndex={-1} className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">
      <PageHeader
        title={`Apólice ${data.nome}`}
        titleExtras={<StatusBadge status={data.ativo ? 'ativo' : 'inativo'} label={data.status} />}
        description={`${data.estipulanteNome} • ${data.seguradoraNome}`}
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Apólices', href: '/apolices' },
              { label: 'Visão 360°' }
            ]}
          />
        }
        actions={
          <div className="flex gap-2">
            {podeEditar && (
              <Button variant="outline" onClick={() => navigate(ROUTES.APOLICE_EDITAR.replace(':publicId', publicId!))}>
                Editar Apólice
              </Button>
            )}
            <Button variant="ghost" onClick={handleVoltar}>Voltar</Button>
          </div>
        }
      />

      <div className="flex flex-col gap-4">
        {/* Navigation Tabs */}
        <div className="border-b border-borda">
          <nav className="-mb-px flex space-x-6 overflow-x-auto" aria-label="Tabs">
            {TABS.map((tab) => (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key)}
                className={`
                  whitespace-nowrap py-3 px-1 border-b-2 font-medium text-sm transition-colors
                  ${activeTab === tab.key
                    ? 'border-marca-principal text-marca-principal'
                    : 'border-transparent text-texto-secundario hover:text-texto-principal hover:border-borda-forte'
                  }
                `}
                aria-current={activeTab === tab.key ? 'page' : undefined}
              >
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Tab Content */}
        <div className="pt-2">
          {activeTab === 'resumo' && <ResumoTab apolice={data} />}
          {activeTab === 'ramos' && <RamosTab apolice={data} onMutateSucesso={retry} />}
          {activeTab === 'configuracoes' && <ConfiguracoesTab apolice={data} />}
          {activeTab === 'subestipulantes' && <SubestipulantesTab />}
          {activeTab === 'vidas' && <VidasTab publicId={data.publicId} />}
          {activeTab === 'universo' && <UniversoPermitidoTab />}
          {activeTab === 'historico' && <HistoricoTab />}
        </div>
      </div>
    </main>
  );
};
