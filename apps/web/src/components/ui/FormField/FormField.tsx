import React from 'react';
import { ErrorIcon } from '../Icons';
import './FormField.css';

export interface FormFieldProps {
  label: React.ReactNode;
  required?: boolean;
  error?: string;
  hint?: string;
  id?: string;
  children: React.ReactElement<{ id?: string; 'aria-describedby'?: string; 'aria-invalid'?: string; className?: string }>;
}

export const FormField: React.FC<FormFieldProps> = ({
  label,
  required = false,
  error,
  hint,
  id: customId,
  children,
}) => {
  const generatedId = React.useId();
  const id = customId || (children.props as { id?: string }).id || `field-${generatedId}`;
  
  const errorId = `${id}-error`;
  const hintId = `${id}-hint`;
  
  let describedBy = '';
  if (error) describedBy += ` ${errorId}`;
  if (hint) describedBy += ` ${hintId}`;
  describedBy = describedBy.trim();

  const childProps = children.props as { className?: string };

  // Clona o filho para injetar os atributos de acessibilidade
  const child = React.cloneElement(children, {
    id,
    'aria-describedby': describedBy || undefined,
    'aria-invalid': error ? 'true' : undefined,
    className: `${childProps.className || ''} ${error ? 'input-erro' : ''}`.trim(),
  });

  return (
    <div className="form-group">
      <label htmlFor={id} className={`form-label ${required ? 'required' : ''}`}>
        {label}
      </label>
      {child}
      {error ? (
        <span className="form-error-msg" id={errorId} role="alert">
          <ErrorIcon /> {error}
        </span>
      ) : hint ? (
        <span className="form-helper" id={hintId}>
          {hint}
        </span>
      ) : null}
    </div>
  );
};
export default FormField;
