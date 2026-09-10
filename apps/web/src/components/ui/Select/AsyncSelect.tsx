import React, { forwardRef } from 'react';
import ReactAsyncSelect from 'react-select/async';
import type { GroupBase, StylesConfig } from 'react-select';

export interface AsyncOptionItem {
  label: string;
  value: string;
}

export interface AsyncSelectProps {
  id?: string;
  name?: string;
  placeholder?: string;
  error?: boolean;
  loadOptions: (inputValue: string) => Promise<AsyncOptionItem[]>;
  value?: AsyncOptionItem | null;
  onChange?: (val: AsyncOptionItem | null) => void;
  onBlur?: () => void;
  defaultOptions?: boolean | AsyncOptionItem[];
}

export const AsyncSelect = forwardRef<any, AsyncSelectProps>(
  ({ id, name, placeholder = 'Selecione...', error, loadOptions, value, onChange, onBlur, defaultOptions = true }, ref) => {

    const customStyles: StylesConfig<AsyncOptionItem, false, GroupBase<AsyncOptionItem>> = {
      control: (provided, state) => ({
        ...provided,
        minHeight: '40px',
        backgroundColor: 'var(--bg-principal)',
        borderColor: error ? 'var(--status-erro)' : (state.isFocused ? 'var(--marca-primaria)' : 'var(--borda-forte)'),
        boxShadow: state.isFocused ? (error ? '0 0 0 1px var(--status-erro)' : '0 0 0 1px var(--marca-primaria)') : 'none',
        borderRadius: '0.375rem',
        '&:hover': {
          borderColor: error ? 'var(--status-erro)' : (state.isFocused ? 'var(--marca-primaria)' : 'var(--borda-forte)'),
        },
      }),
      menu: (provided) => ({
        ...provided,
        backgroundColor: 'var(--bg-secundario)',
        border: '1px solid var(--borda-suave)',
        boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        zIndex: 50,
      }),
      option: (provided, state) => ({
        ...provided,
        backgroundColor: state.isSelected
          ? 'var(--marca-primaria)'
          : state.isFocused
            ? 'var(--bg-terciario)'
            : 'transparent',
        color: state.isSelected
          ? '#fff'
          : 'var(--texto-principal)',
        cursor: 'pointer',
        '&:active': {
          backgroundColor: 'var(--marca-primaria-dark)',
        },
      }),
      singleValue: (provided) => ({
        ...provided,
        color: 'var(--texto-principal)',
      }),
      input: (provided) => ({
        ...provided,
        color: 'var(--texto-principal)',
      }),
      placeholder: (provided) => ({
        ...provided,
        color: 'var(--texto-terciario)',
      }),
      noOptionsMessage: (provided) => ({
        ...provided,
        color: 'var(--texto-secundario)',
      }),
      loadingMessage: (provided) => ({
        ...provided,
        color: 'var(--texto-secundario)',
      }),
    };

    return (
      <ReactAsyncSelect
        ref={ref}
        inputId={id}
        name={name}
        cacheOptions
        defaultOptions={defaultOptions}
        loadOptions={loadOptions}
        value={value}
        onChange={onChange as any}
        onBlur={onBlur}
        placeholder={placeholder}
        styles={customStyles}
        noOptionsMessage={() => 'Nenhum resultado encontrado'}
        loadingMessage={() => 'Buscando...'}
        classNamePrefix="react-select"
      />
    );
  }
);

AsyncSelect.displayName = 'AsyncSelect';
export default AsyncSelect;
