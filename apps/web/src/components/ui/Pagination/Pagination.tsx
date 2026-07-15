import React from 'react';
import { Button } from '../Button/Button';
import './Pagination.css';

export interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  disabled?: boolean;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  onPageChange,
  disabled = false,
}) => {
  const isFirstPage = currentPage === 1;
  const isLastPage = currentPage === totalPages || totalPages === 0;

  return (
    <div className="table-pagination-bar-component">
      <div className="pagination-controls-component">
        <Button
          variant="secondary"
          size="small"
          disabled={disabled || isFirstPage}
          onClick={() => onPageChange(currentPage - 1)}
          aria-label="Ir para a página anterior"
        >
          Anterior
        </Button>
        
        {Array.from({ length: totalPages }, (_, i) => i + 1)
          .filter(page => page === 1 || page === totalPages || Math.abs(page - currentPage) <= 1)
          .map((page, index, array) => {
            const showEllipsis = index > 0 && page - array[index - 1] > 1;
            
            return (
              <React.Fragment key={page}>
                {showEllipsis && <span className="pagination-ellipsis" aria-hidden="true">...</span>}
                <Button
                  variant={currentPage === page ? 'primary' : 'secondary'}
                  size="small"
                  disabled={disabled}
                  onClick={() => onPageChange(page)}
                  aria-current={currentPage === page ? 'page' : undefined}
                  aria-label={`Ir para a página ${page}`}
                >
                  {page}
                </Button>
              </React.Fragment>
            );
          })}

        <Button
          variant="secondary"
          size="small"
          disabled={disabled || isLastPage}
          onClick={() => onPageChange(currentPage + 1)}
          aria-label="Ir para a próxima página"
        >
          Próximo
        </Button>
      </div>
    </div>
  );
};
export default Pagination;
