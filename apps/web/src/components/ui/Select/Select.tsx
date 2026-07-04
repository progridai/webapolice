import React, { forwardRef } from 'react';
import './Select.css';

export interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  error?: boolean;
  placeholder?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ children, className = '', error = false, placeholder, ...props }, ref) => {
    return (
      <select
        ref={ref}
        className={`form-select ${error ? 'input-erro' : ''} ${className}`}
        {...props}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}
        {children}
      </select>
    );
  }
);

Select.displayName = 'Select';
export default Select;
