import React from 'react';

export interface DescriptionItemProps {
  label: string;
  value: React.ReactNode;
  icon?: React.ReactNode;
  className?: string;
}

export const DescriptionItem: React.FC<DescriptionItemProps> = ({
  label,
  value,
  icon,
  className = '',
}) => {
  return (
    <div className={`flex flex-col gap-1 ${className}`}>
      <dt className="text-sm font-medium text-slate-500 dark:text-slate-400 flex items-center gap-1.5">
        {icon && <span className="text-slate-400 dark:text-slate-500">{icon}</span>}
        {label}
      </dt>
      <dd className="text-base text-slate-900 dark:text-slate-50 break-words">
        {value === null || value === undefined || value === '' ? (
          <span className="text-slate-400 dark:text-slate-500 italic">Não informado</span>
        ) : (
          value
        )}
      </dd>
    </div>
  );
};

export interface DescriptionListProps {
  children: React.ReactNode;
  columns?: 1 | 2 | 3;
  className?: string;
}

export const DescriptionList: React.FC<DescriptionListProps> = ({
  children,
  columns = 2,
  className = '',
}) => {
  const getGridCols = () => {
    switch (columns) {
      case 1:
        return 'grid-cols-1';
      case 2:
        return 'grid-cols-1 sm:grid-cols-2';
      case 3:
        return 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3';
      default:
        return 'grid-cols-1 sm:grid-cols-2';
    }
  };

  return (
    <dl className={`grid gap-6 ${getGridCols()} ${className}`}>
      {children}
    </dl>
  );
};
