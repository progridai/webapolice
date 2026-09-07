import React, { forwardRef } from 'react';
import { IMaskInput } from 'react-imask';
import type { InputProps } from '../ui/Input';

export const CepInput = forwardRef<HTMLInputElement, InputProps>(
  ({ className = '', error = false, onChange, ...props }, ref) => {
    return (
      <IMaskInput
        mask="00000-000"
        unmask={false}
        inputRef={ref}
        className={`form-input ${error ? 'input-erro' : ''} ${className}`}
        onAccept={(value, mask) => {
          if (onChange) {
            onChange({ target: { name: props.name, value } } as React.ChangeEvent<HTMLInputElement>);
          }
        }}
        {...(props as any)}
      />
    );
  }
);

CepInput.displayName = 'CepInput';
