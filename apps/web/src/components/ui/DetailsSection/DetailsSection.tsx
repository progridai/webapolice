import React from 'react';
import { Card } from '../Card';

export interface DetailsSectionProps {
  title: string;
  description?: string;
  icon?: React.ReactNode;
  action?: React.ReactNode;
  children: React.ReactNode;
  emptyState?: React.ReactNode;
  isEmpty?: boolean;
  className?: string;
}

export const DetailsSection: React.FC<DetailsSectionProps> = ({
  title,
  description,
  icon,
  action,
  children,
  emptyState,
  isEmpty = false,
  className = '',
}) => {
  return (
    <Card className={`p-4 flex flex-col gap-2 ${className}`}>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
        <div className="flex items-start gap-3">
          {icon && (
            <div className="mt-1 text-texto-secundario">
              {icon}
            </div>
          )}
          <div>
            <h3 className="text-lg font-semibold text-texto-principal">
              {title}
            </h3>
            {description && (
              <p className="text-sm text-texto-secundario mt-1">
                {description}
              </p>
            )}
          </div>
        </div>
        {action && (
          <div className="flex-shrink-0">
            {action}
          </div>
        )}
      </div>

      <div className="flex-grow">
        {isEmpty && emptyState ? (
          <div className="text-sm text-texto-secundario py-2">
            {emptyState}
          </div>
        ) : isEmpty ? (
          <p className="text-sm text-texto-secundario py-2">
            Nenhuma informação disponível.
          </p>
        ) : (
          children
        )}
      </div>
    </Card>
  );
};
