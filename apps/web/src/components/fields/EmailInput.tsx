import React, { forwardRef } from 'react';
import { Input } from '../ui/Input';
import type { InputProps } from '../ui/Input';

export const EmailInput = forwardRef<HTMLInputElement, InputProps>(
  (props, ref) => {
    return (
      <Input
        type="email"
        ref={ref}
        {...props}
      />
    );
  }
);

EmailInput.displayName = 'EmailInput';
