import { useEffect, useState } from 'react';
import { Button, PageHeader } from '../../../components/ui';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { PropostasConsulta } from '../components/PropostasConsulta';
import { NovaPreProposta } from '../components/NovaPreProposta';

export function PropostasPage() {
  const { possuiPermissao, possuiModulo, possuiRecurso } = useAuthorization();
  const [criando, setCriando] = useState(false);
  const [ocupado, setOcupado] = useState(false);
  const [propostaCriada, setPropostaCriada] = useState<string>();
  const podeCriar =
    possuiModulo('APOLICES') &&
    possuiRecurso('PROPOSTAS') &&
    possuiPermissao('propostas.visualizar') &&
    possuiPermissao('propostas.inserir');
  useEffect(() => {
    document.title = 'Propostas | WebApolice';
  }, []);
  return (
    <main className="flex flex-col gap-4 w-full" tabIndex={-1}>
      <PageHeader
        title="Propostas"
        description="Consulta de propostas, condições registradas e beneficiários."
        actions={
          podeCriar && !criando ? (
            <Button disabled={ocupado} onClick={() => setCriando(true)}>
              Nova pré proposta
            </Button>
          ) : undefined
        }
      />
      {criando ? (
        <NovaPreProposta
          onCancelar={() => setCriando(false)}
          onCriada={(id) => {
            setPropostaCriada(id);
            setCriando(false);
          }}
        />
      ) : (
        <PropostasConsulta
          key={propostaCriada ?? 'consulta'}
          initialPublicId={propostaCriada}
          onBusy={setOcupado}
        />
      )}
    </main>
  );
}
