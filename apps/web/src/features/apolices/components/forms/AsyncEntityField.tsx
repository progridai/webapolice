import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Controller } from 'react-hook-form';
import type { Control } from 'react-hook-form';
import { Input, Spinner } from '../../../../components/ui';

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  useEffect(() => {
    const handler = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(handler);
  }, [value, delay]);
  return debouncedValue;
}

export interface AsyncEntityOption {
  value: string;
  label: string;
}

interface EntitySearchFieldProps {
  id?: string;
  placeholder?: string;
  error?: boolean;
  disabled?: boolean;
  value: string;
  initialLabel?: string;
  onChange: (id: string) => void;
  onBlur?: () => void;
  searcher: (term: string) => Promise<AsyncEntityOption[]>;
}

const EntitySearchField: React.FC<EntitySearchFieldProps> = ({
  id,
  placeholder,
  error,
  disabled,
  value,
  initialLabel,
  onChange,
  onBlur,
  searcher,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [options, setOptions] = useState<AsyncEntityOption[]>([]);
  const [loading, setLoading] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const [selectedLabel, setSelectedLabel] = useState<string | null>(null);
  const wrapperRef = useRef<HTMLDivElement>(null);
  const debouncedSearch = useDebounce(searchTerm, 350);
  const initialised = useRef(false);

  // Set label when editing (initialLabel provided from parent, no API call needed)
  useEffect(() => {
    if (value && initialLabel && !initialised.current) {
      setSelectedLabel(initialLabel);
      setSearchTerm(initialLabel);
      initialised.current = true;
    }
    if (!value) {
      setSelectedLabel(null);
      setSearchTerm('');
      initialised.current = false;
    }
  }, [value, initialLabel]);

  const fetchOptions = useCallback(async (term: string) => {
    setLoading(true);
    try {
      const result = await searcher(term);
      setOptions(result);
    } catch {
      setOptions([]);
    } finally {
      setLoading(false);
    }
  }, [searcher]);

  useEffect(() => {
    if (isOpen) {
      fetchOptions(debouncedSearch);
    }
  }, [debouncedSearch, isOpen, fetchOptions]);

  // Close on click outside
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
        setIsOpen(false);
        if (selectedLabel) {
          setSearchTerm(selectedLabel);
        } else {
          setSearchTerm('');
        }
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [selectedLabel]);

  const handleSelect = (opt: AsyncEntityOption) => {
    setSelectedLabel(opt.label);
    setSearchTerm(opt.label);
    setIsOpen(false);
    initialised.current = true;
    onChange(opt.value);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setIsOpen(true);
    if (selectedLabel) {
      setSelectedLabel(null);
      onChange('');
      initialised.current = false;
    }
  };

  const handleFocus = () => {
    setIsOpen(true);
    if (selectedLabel) {
      setSearchTerm('');
    }
  };

  return (
    <div ref={wrapperRef} className="relative w-full">
      <div className="relative">
        <Input
          id={id}
          type="text"
          placeholder={placeholder || 'Buscar...'}
          value={searchTerm}
          onChange={handleInputChange}
          onFocus={handleFocus}
          onBlur={onBlur}
          error={error}
          disabled={disabled}
          className="w-full pr-10"
          autoComplete="off"
        />
        {loading && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2">
            <Spinner size="small" />
          </div>
        )}
      </div>

      {isOpen && !disabled && (
        <div className="absolute z-50 w-full mt-1 bg-fundo-elevado border border-borda rounded shadow-lg max-h-60 overflow-y-auto">
          {options.length === 0 && !loading ? (
            <div className="p-3 text-sm text-texto-secundario text-center">
              Nenhum resultado encontrado.
            </div>
          ) : (
            <ul className="py-1">
              {options.map((opt) => (
                <li
                  key={opt.value}
                  className={`px-3 py-2 cursor-pointer text-sm hover:bg-fundo-superficie transition-colors ${
                    opt.value === value ? 'bg-fundo-superficie font-medium' : ''
                  }`}
                  onMouseDown={(e) => {
                    e.preventDefault();
                    handleSelect(opt);
                  }}
                >
                  <div className="text-texto-principal">{opt.label}</div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
};

interface AsyncEntityFieldProps {
  name: string;
  control: Control<any>;
  label?: string;
  placeholder?: string;
  initialLabel?: string;
  searcher: (term: string) => Promise<AsyncEntityOption[]>;
}

export const AsyncEntityField: React.FC<AsyncEntityFieldProps> = ({
  name,
  control,
  placeholder,
  initialLabel,
  searcher,
}) => {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <EntitySearchField
          id={name}
          placeholder={placeholder}
          error={!!fieldState.error}
          value={field.value || ''}
          initialLabel={initialLabel}
          onChange={field.onChange}
          onBlur={field.onBlur}
          searcher={searcher}
        />
      )}
    />
  );
};
