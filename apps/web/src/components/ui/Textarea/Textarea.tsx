import React, { forwardRef } from 'react';
import './Textarea.css';

export interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  error?: boolean;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ className = '', error = false, rows = 3, ...props }, ref) => {
    return (
      <textarea
        ref={ref}
        rows={rows}
        className={`form-textarea ${error ? 'input-erro' : ''} ${className}`}
        {...props}
      />
    );
  }
);

Textarea.displayName = 'Textarea';
export default Textarea;
