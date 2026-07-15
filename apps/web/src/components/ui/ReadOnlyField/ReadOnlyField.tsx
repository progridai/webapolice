import React from 'react';
import { LockIcon } from '../Icons';
import './ReadOnlyField.css';

export interface ReadOnlyFieldProps {
  value: string | number;
  id?: string;
  className?: string;
}

export const ReadOnlyField: React.FC<ReadOnlyFieldProps> = ({
  value,
  id,
  className = ''
}) => {
  return (
    <div className={`readonly-field-wrapper ${className}`}>
      <input
        type="text"
        className="readonly-field-input"
        id={id}
        value={value || '—'}
        readOnly
        aria-readonly="true"
        title="Este campo é de leitura"
      />
      <LockIcon size={14} className="readonly-field-icon" aria-hidden="true" />
    </div>
  );
};
