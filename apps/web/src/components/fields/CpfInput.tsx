import React, { forwardRef } from 'react';
import { IMaskInput } from 'react-imask';
import type { InputProps } from '../ui/Input';

export const CpfInput = forwardRef<HTMLInputElement, InputProps>(
  ({ className = '', error = false, onChange, value, ...props }, ref) => {
    return (
      <IMaskInput
        mask="000.000.000-00"
        unmask={false}
        inputRef={ref}
        className={`form-input ${error ? 'input-erro' : ''} ${className}`}
        onAccept={(value) => {
          if (onChange) {
            onChange({
              target: { name: props.name, value },
            } as React.ChangeEvent<HTMLInputElement>);
          }
        }}
        {...props}
        value={value === undefined ? undefined : String(value)}
      />
    );
  },
);

CpfInput.displayName = 'CpfInput';
