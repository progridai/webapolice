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
    <Card className={`p-6 flex flex-col gap-4 ${className}`}>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-2">
        <div className="flex items-start gap-3">
          {icon && (
            <div className="mt-1 text-slate-400 dark:text-slate-500">
              {icon}
            </div>
          )}
          <div>
            <h3 className="text-lg font-semibold text-slate-900 dark:text-slate-50">
              {title}
            </h3>
            {description && (
              <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
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
          <div className="text-sm text-slate-500 dark:text-slate-400 py-2">
            {emptyState}
          </div>
        ) : isEmpty ? (
          <p className="text-sm text-slate-500 dark:text-slate-400 py-2">
            Nenhuma informação disponível.
          </p>
        ) : (
          children
        )}
      </div>
    </Card>
  );
};
