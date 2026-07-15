import React from 'react';
import './ResultsSummary.css';

export interface ResultsSummaryProps {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  className?: string;
}

export const ResultsSummary: React.FC<ResultsSummaryProps> = ({
  currentPage,
  pageSize,
  totalItems,
  className = '',
}) => {
  if (totalItems === 0) return null;

  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalItems);

  return (
    <div className={`results-summary ${className}`} aria-live="polite">
      Exibindo {startItem}–{endItem} de {totalItems} {totalItems === 1 ? 'registro' : 'registros'}
    </div>
  );
};
