import React, { forwardRef, useEffect, useRef } from 'react';
import './Checkbox.css';

export interface CheckboxProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type'> {
  indeterminate?: boolean;
  error?: boolean;
  label?: React.ReactNode;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ className = '', indeterminate = false, error = false, label, id, ...props }, ref) => {
    const defaultRef = useRef<HTMLInputElement>(null);
    const resolvedRef = (ref || defaultRef) as React.RefObject<HTMLInputElement>;
    const generatedId = React.useId();
    const inputId = id || `checkbox-${generatedId}`;

    useEffect(() => {
      if (resolvedRef.current) {
        resolvedRef.current.indeterminate = indeterminate;
      }
    }, [indeterminate, resolvedRef]);

    const checkboxMarkup = (
      <div className={`checkbox-wrapper ${error ? 'checkbox-erro' : ''} ${className}`}>
        <input
          ref={resolvedRef}
          type="checkbox"
          id={inputId}
          {...props}
        />
        <span className="checkbox-custom" aria-hidden="true"></span>
      </div>
    );

    if (label) {
      return (
        <label htmlFor={inputId} className="checkbox-container">
          {checkboxMarkup}
          <span className="checkbox-label-text">{label}</span>
        </label>
      );
    }

    return checkboxMarkup;
  }
);

Checkbox.displayName = 'Checkbox';
export default Checkbox;
