import React from 'react';
import './EmptyState.css';

export interface EmptyStateProps extends React.HTMLAttributes<HTMLDivElement> {
  title: string;
  description: string;
  icon?: React.ReactNode;
  action?: React.ReactNode;
  secondaryAction?: React.ReactNode;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title,
  description,
  icon,
  action,
  secondaryAction,
  className = '',
  ...props
}) => {
  return (
    <div className={`table-empty-state-component ${className}`} {...props}>
      {icon && <div className="empty-state-icon-wrapper">{icon}</div>}
      <h3 className="empty-state-title">{title}</h3>
      <p className="empty-state-desc">{description}</p>
      {(action || secondaryAction) && (
        <div className="empty-state-actions mt-3">
          {secondaryAction}
          {action}
        </div>
      )}
    </div>
  );
};
export default EmptyState;
