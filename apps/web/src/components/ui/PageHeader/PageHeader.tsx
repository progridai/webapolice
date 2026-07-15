import React from 'react';
import './PageHeader.css';

export interface PageHeaderProps {
  title: string;
  description?: string;
  icon?: React.ReactNode;
  breadcrumbs?: React.ReactNode;
  actions?: React.ReactNode;
  children?: React.ReactNode;
}

export const PageHeader: React.FC<PageHeaderProps> = ({
  title,
  description,
  icon,
  breadcrumbs,
  actions,
  children
}) => {
  return (
    <div className="page-header">
      {breadcrumbs && <div className="page-header-breadcrumbs">{breadcrumbs}</div>}
      
      <div className="page-header-content">
        <div className="page-header-title-group">
          {icon && <div className="page-header-icon" aria-hidden="true">{icon}</div>}
          <div className="page-header-text">
            <h1 className="page-header-title">{title}</h1>
            {description && <p className="page-header-description">{description}</p>}
          </div>
        </div>

        {actions && (
          <div className="page-header-actions">
            {actions}
          </div>
        )}
      </div>

      {children && <div className="page-header-extras">{children}</div>}
    </div>
  );
};
