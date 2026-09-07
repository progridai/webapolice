import React, { forwardRef } from 'react';
import { Input } from '../ui/Input';
import type { InputProps } from '../ui/Input';

export const DateInput = forwardRef<HTMLInputElement, InputProps>(
  (props, ref) => {
    return (
      <Input
        type="date"
        ref={ref}
        {...props}
      />
    );
  }
);

DateInput.displayName = 'DateInput';
