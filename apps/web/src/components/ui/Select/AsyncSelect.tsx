import { forwardRef } from 'react';
import ReactAsyncSelect from 'react-select/async';
import type { GroupBase, SelectInstance, StylesConfig } from 'react-select';

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

export const AsyncSelect = forwardRef<
  SelectInstance<AsyncOptionItem, false>,
  AsyncSelectProps
>(
  (
    {
      id,
      name,
      placeholder = 'Selecione...',
      error,
      loadOptions,
      value,
      onChange,
      onBlur,
      defaultOptions = true,
    },
    ref,
  ) => {
    const customStyles: StylesConfig<
      AsyncOptionItem,
      false,
      GroupBase<AsyncOptionItem>
    > = {
      control: (provided, state) => ({
        ...provided,
        minHeight: '40px',
        backgroundColor: 'var(--cor-fundo-superficie)',
        borderColor: error
          ? 'var(--cor-erro)'
          : state.isFocused
            ? 'var(--cor-foco)'
            : 'var(--cor-borda-forte)',
        boxShadow: state.isFocused
          ? error
            ? '0 0 0 1px var(--cor-erro)'
            : '0 0 0 1px var(--cor-foco)'
          : 'none',
        borderRadius: '0.375rem',
        '&:hover': {
          borderColor: error
            ? 'var(--cor-erro)'
            : state.isFocused
              ? 'var(--cor-foco)'
              : 'var(--cor-borda-forte)',
        },
      }),
      menu: (provided) => ({
        ...provided,
        backgroundColor: 'var(--cor-fundo-elevado)',
        border: '1px solid var(--cor-borda)',
        boxShadow: 'var(--sombra-card)',
        zIndex: 50,
      }),
      option: (provided, state) => ({
        ...provided,
        backgroundColor: state.isSelected
          ? 'var(--cor-marca-principal)'
          : state.isFocused
            ? 'var(--cor-fundo-aplicacao)'
            : 'transparent',
        color: state.isSelected
          ? 'var(--cor-sobre-marca-principal)'
          : 'var(--cor-texto-principal)',
        cursor: 'pointer',
        '&:active': {
          backgroundColor: 'var(--cor-marca-principal)',
          color: 'var(--cor-sobre-marca-principal)',
        },
      }),
      singleValue: (provided) => ({
        ...provided,
        color: 'var(--cor-texto-principal)',
      }),
      input: (provided) => ({
        ...provided,
        color: 'var(--cor-texto-principal)',
      }),
      placeholder: (provided) => ({
        ...provided,
        color: 'var(--cor-texto-secundario)',
      }),
      noOptionsMessage: (provided) => ({
        ...provided,
        color: 'var(--cor-texto-secundario)',
      }),
      loadingMessage: (provided) => ({
        ...provided,
        color: 'var(--cor-texto-secundario)',
      }),
    };

    return (
      <ReactAsyncSelect<AsyncOptionItem, false>
        ref={ref}
        inputId={id}
        name={name}
        cacheOptions
        defaultOptions={defaultOptions}
        loadOptions={loadOptions}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        placeholder={placeholder}
        styles={customStyles}
        noOptionsMessage={() => 'Nenhum resultado encontrado'}
        loadingMessage={() => 'Buscando...'}
        classNamePrefix="react-select"
      />
    );
  },
);

AsyncSelect.displayName = 'AsyncSelect';
export default AsyncSelect;
