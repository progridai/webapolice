import { lazy, Suspense } from 'react';
import { PageLoading } from '../../../components/application/PageLoading';

const Page = lazy(() =>
  import('../pages/PropostasPage').then((m) => ({ default: m.PropostasPage })),
);

export function PropostasRoutePage() {
  return (
    <Suspense fallback={<PageLoading />}>
      <Page />
    </Suspense>
  );
}
