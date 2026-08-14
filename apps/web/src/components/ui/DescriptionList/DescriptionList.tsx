import React from 'react';
import './DescriptionList.css';

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
    <div className={`desc-item ${className}`}>
      <dt className="desc-item-label">
        {icon && <span className="desc-item-label-icon">{icon}</span>}
        {label}{typeof label === 'string' && !label.endsWith(':') ? ':' : ''}
      </dt>
      <dd className="desc-item-value">
        {value === null || value === undefined || value === '' ? (
          <span className="desc-item-empty">Não informado</span>
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
  density?: 'compact' | 'comfortable';
  className?: string;
}

export const DescriptionList: React.FC<DescriptionListProps> = ({
  children,
  columns = 2,
  density = 'compact',
  className = '',
}) => {
  const getGridColsClass = () => {
    switch (columns) {
      case 1:
        return 'desc-list-cols-1';
      case 2:
        return 'desc-list-cols-2';
      case 3:
        return 'desc-list-cols-3';
      default:
        return 'desc-list-cols-2';
    }
  };

  return (
    <dl className={`desc-list-grid ${getGridColsClass()} desc-list-density-${density} ${className}`}>
      {children}
    </dl>
  );
};
