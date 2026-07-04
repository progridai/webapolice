import React, { useEffect, useState } from 'react';
import { useClientes } from '../hooks/useClientes';
import { useClientesFilters } from '../hooks/useClientesFilters';
import { ClientesFilters } from '../components/ClientesFilters';
import { ClientesTable } from '../components/ClientesTable';
import { ClientesMobileList } from '../components/ClientesMobileList';
import { Pagination, EmptyState, Alert, Button, Skeleton } from '../../../components/ui';

export const ClientesListPage: React.FC = () => {
  const { filters, setFilters, clearFilters } = useClientesFilters();
  const { data, isLoading, error, retry } = useClientes(filters);
  const [isMobile, setIsMobile] = useState(false);

  // Define isMobile responsivamente para trocar tabela por lista
  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  // Seta título da página
  useEffect(() => {
    document.title = 'Clientes | webapolice';
  }, []);

  const handleSort = (column: string) => {
    setFilters({
      sortBy: column,
      direction: filters.sortBy === column && filters.direction === 'asc' ? 'desc' : 'asc'
    });
  };

  const handlePageChange = (page: number) => {
    setFilters({ page });
  };

  const hasActiveFilters = Boolean(filters.nome || filters.cpf || filters.status);
  const hasData = data && data.itens && data.itens.length > 0;

  return (
    <div className="flex flex-col gap-6" role="main">
      <header>
        <h1 className="text-2xl font-bold tracking-tight">Clientes</h1>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          Gerencie e consulte os clientes da plataforma.
        </p>
      </header>

      <section aria-label="Filtros de clientes">
        <ClientesFilters 
          filters={filters} 
          onFilterChange={setFilters} 
          onClearFilters={clearFilters}
          isLoading={isLoading && !data} 
        />
      </section>

      {error ? (
        <div className="flex flex-col items-start gap-4">
          <Alert
            variant="error"
            title="Não foi possível carregar os clientes"
          >
            {error.message}
          </Alert>
          <Button onClick={retry} size="sm">Tentar novamente</Button>
        </div>
      ) : isLoading && !data ? (
        <div className="space-y-4" aria-busy="true" aria-live="polite">
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
        </div>
      ) : !hasData ? (
        <EmptyState
          title={hasActiveFilters ? "Nenhum cliente encontrado" : "Nenhum cliente cadastrado"}
          description={
            hasActiveFilters 
              ? "Não encontramos nenhum cliente com os filtros informados."
              : "Ainda não existem clientes cadastrados na plataforma."
          }
          action={hasActiveFilters ? (
            <Button onClick={clearFilters}>Limpar filtros</Button>
          ) : undefined}
        />
      ) : (
        <>
          <div aria-live="polite" className="sr-only">
            Exibindo {data.itens.length} clientes.
          </div>
          
          {isMobile ? (
            <ClientesMobileList clientes={data.itens} />
          ) : (
            <ClientesTable 
              clientes={data.itens} 
              isLoading={isLoading} 
              sortBy={filters.sortBy}
              direction={filters.direction}
              onSort={handleSort}
            />
          )}

          {data.totalPaginas > 1 && (
            <div className="mt-4 flex justify-between items-center flex-col sm:flex-row gap-4">
              <span className="text-sm text-gray-500 dark:text-gray-400">
                Exibindo página {data.paginaAtual} de {data.totalPaginas} ({data.totalItens} total)
              </span>
              <Pagination
                currentPage={data.paginaAtual}
                totalPages={data.totalPaginas}
                onPageChange={handlePageChange}
                disabled={isLoading}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
};
