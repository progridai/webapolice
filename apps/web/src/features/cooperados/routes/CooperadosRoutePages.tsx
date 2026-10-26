import { Suspense, lazy } from 'react';
import { PageLoading } from '../../../components/application/PageLoading';

const CooperadosListPage = lazy(() =>
  import('../pages/CooperadosListPage').then((m) => ({
    default: m.CooperadosListPage,
  })),
);
const NovoCooperadoPage = lazy(() =>
  import('../pages/NovoCooperadoPage').then((m) => ({
    default: m.NovoCooperadoPage,
  })),
);
const EditarCooperadoPage = lazy(() =>
  import('../pages/EditarCooperadoPage').then((m) => ({
    default: m.EditarCooperadoPage,
  })),
);
const CooperadoDetalhePage = lazy(() =>
  import('../pages/CooperadoDetalhePage').then((m) => ({
    default: m.CooperadoDetalhePage,
  })),
);

export function CooperadosListRoute() {
  return (
    <Suspense fallback={<PageLoading />}>
      <CooperadosListPage />
    </Suspense>
  );
}

export function NovoCooperadoRoute() {
  return (
    <Suspense fallback={<PageLoading />}>
      <NovoCooperadoPage />
    </Suspense>
  );
}

export function EditarCooperadoRoute() {
  return (
    <Suspense fallback={<PageLoading />}>
      <EditarCooperadoPage />
    </Suspense>
  );
}

export function CooperadoDetalheRoute() {
  return (
    <Suspense fallback={<PageLoading />}>
      <CooperadoDetalhePage />
    </Suspense>
  );
}
