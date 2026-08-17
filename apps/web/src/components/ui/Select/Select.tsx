import React, { forwardRef } from 'react';
import './Select.css';

export interface OptionItem {
  label: string;
  value: string | number;
}

export interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  error?: boolean;
  placeholder?: string;
  options?: OptionItem[];
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ children, className = '', error = false, placeholder, options, ...props }, ref) => {
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
        {options
          ? options.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))
          : children}
      </select>
    );
  }
);

Select.displayName = 'Select';
export default Select;
