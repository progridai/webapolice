import React, { forwardRef } from 'react';
import { IMaskInput } from 'react-imask';
import type { InputProps } from '../ui/Input';

export const PhoneInput = forwardRef<HTMLInputElement, InputProps>(
  ({ className = '', error = false, onChange, ...props }, ref) => {
    return (
      <IMaskInput
        mask={[
          { mask: '(00) 0000-0000' },
          { mask: '(00) 00000-0000' }
        ]}
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

PhoneInput.displayName = 'PhoneInput';
