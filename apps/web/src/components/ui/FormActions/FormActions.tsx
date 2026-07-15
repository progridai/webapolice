import React from 'react';
import './FormActions.css';

export interface FormActionsProps {
  children: React.ReactNode;
  className?: string;
  sticky?: boolean;
}

export const FormActions: React.FC<FormActionsProps> = ({ children, className = '', sticky = true }) => {
  return (
    <div className={`form-actions ${sticky ? 'form-actions-sticky' : ''} ${className}`}>
      <div className="form-actions-content">
        {children}
      </div>
    </div>
  );
};
