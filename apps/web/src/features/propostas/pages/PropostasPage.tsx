import { useEffect } from 'react';
import { PageHeader } from '../../../components/ui';
import { PropostasConsulta } from '../components/PropostasConsulta';

export function PropostasPage() {
  useEffect(() => {
    document.title = 'Propostas | WebApolice';
  }, []);
  return (
    <main className="flex flex-col gap-4 w-full" tabIndex={-1}>
      <PageHeader
        title="Propostas"
        description="Consulta de propostas, condições registradas e beneficiários."
      />
      <PropostasConsulta />
    </main>
  );
}
