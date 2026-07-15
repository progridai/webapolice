import React from 'react';
import './FormGrid.css';

export interface FormGridProps {
  children: React.ReactNode;
  className?: string;
}

export const FormGrid: React.FC<FormGridProps> = ({ children, className = '' }) => {
  return (
    <div className={`form-grid ${className}`}>
      {children}
    </div>
  );
};
