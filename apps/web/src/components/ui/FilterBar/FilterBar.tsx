import React, { ReactNode } from 'react';
import './FilterBar.css';

export interface FilterBarProps {
  children: ReactNode;
  className?: string;
}

export const FilterBar: React.FC<FilterBarProps> = ({ children, className = '' }) => {
  return (
    <div className={`filter-bar ${className}`}>
      {children}
    </div>
  );
};
