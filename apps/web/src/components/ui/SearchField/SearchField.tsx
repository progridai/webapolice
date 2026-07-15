import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Input } from '../Input';
import { SearchIcon } from '../Icons';
import './SearchField.css';

export interface SearchFieldProps {
  id?: string;
  placeholder?: string;
  value?: string;
  onChange: (value: string) => void;
  debounceMs?: number;
  disabled?: boolean;
  className?: string;
  'aria-label'?: string;
}

export const SearchField: React.FC<SearchFieldProps> = ({
  id,
  placeholder = 'Buscar...',
  value = '',
  onChange,
  debounceMs = 500,
  disabled = false,
  className = '',
  'aria-label': ariaLabel,
}) => {
  const [localValue, setLocalValue] = useState(value);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const onChangeRef = useRef(onChange);

  // Keeps the ref current in an effect (not during render)
  useEffect(() => {
    onChangeRef.current = onChange;
  });

  // Sync external value reset (e.g., when parent clears filters) in an effect
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLocalValue(value);
  }, [value]);


  // Cleanup debounce on unmount
  useEffect(() => {
    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, []);

  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setLocalValue(newValue);

    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }
    debounceRef.current = setTimeout(() => {
      onChangeRef.current(newValue);
    }, debounceMs);
  }, [debounceMs]);

  const handleClear = useCallback(() => {
    setLocalValue('');
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }
    onChangeRef.current('');
  }, []);

  return (
    <div className={`search-field ${className}`}>
      <Input
        id={id}
        type="text"
        placeholder={placeholder}
        value={localValue}
        onChange={handleChange}
        disabled={disabled}
        icon={<SearchIcon />}
        aria-label={ariaLabel || placeholder}
      />
      {localValue && !disabled && (
        <button
          type="button"
          className="search-field-clear"
          onClick={handleClear}
          aria-label="Limpar busca"
        >
          <span aria-hidden="true">&times;</span>
        </button>
      )}
    </div>
  );
};
