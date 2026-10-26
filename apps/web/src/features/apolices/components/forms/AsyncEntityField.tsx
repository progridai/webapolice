import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Controller } from 'react-hook-form';
import type { Control } from 'react-hook-form';
import type { ApoliceFormValues } from '../../schemas/apoliceForm.schema';
import { Alert, Button, Input, Spinner } from '../../../../components/ui';
import { useConsultaApolice } from '../../hooks/useConsultaApolice';

// O atraso agrupa a digitação; abortar a consulta também cancela o timer.
function aguardarBusca(delay: number, signal: AbortSignal): Promise<void> {
  return new Promise((resolve, reject) => {
    const cancel = () => {
      clearTimeout(timer);
      reject(new DOMException('Busca cancelada', 'AbortError'));
    };
    const timer = setTimeout(() => {
      signal.removeEventListener('abort', cancel);
      resolve();
    }, delay);
    if (signal.aborted) cancel();
    else signal.addEventListener('abort', cancel, { once: true });
  });
}

export interface AsyncEntityOption {
  value: string;
  label: string;
}

type FieldAccessibilityProps = Pick<
  React.InputHTMLAttributes<HTMLInputElement>,
  'id' | 'aria-describedby' | 'aria-invalid'
>;

interface EntitySearchFieldProps extends FieldAccessibilityProps {
  placeholder?: string;
  error?: boolean;
  disabled?: boolean;
  value: string;
  initialLabel?: string;
  onChange: (id: string) => void;
  onBlur?: () => void;
  searcher: (
    term: string,
    signal?: AbortSignal,
  ) => Promise<AsyncEntityOption[]>;
  searchDelay?: number;
  emptyMessage?: string;
  clearSearchOnFocus?: boolean;
}

export const EntitySearchField: React.FC<EntitySearchFieldProps> = ({
  id,
  placeholder,
  error,
  disabled,
  value,
  initialLabel,
  onChange,
  onBlur,
  searcher,
  searchDelay = 350,
  emptyMessage = 'Nenhum resultado encontrado.',
  clearSearchOnFocus = true,
  ...accessibilityProps
}) => {
  const [selected, setSelected] = useState<
    (AsyncEntityOption & { initialLabel?: string }) | null
  >(null);
  const [interaction, setInteraction] = useState<{
    value: string;
    initialLabel?: string;
    term: string;
  } | null>(null);
  const wrapperRef = useRef<HTMLDivElement>(null);
  // Ajuste condicionado ao contexto, sem efeito de sincronização de estado.
  if (
    interaction &&
    (disabled ||
      interaction.value !== value ||
      interaction.initialLabel !== initialLabel)
  ) {
    setInteraction(null);
  }
  if (
    selected &&
    (selected.value !== value || selected.initialLabel !== initialLabel)
  ) {
    setSelected(null);
  }
  const selectedLabel = value
    ? selected?.value === value && selected.initialLabel === initialLabel
      ? selected.label
      : initialLabel || ''
    : '';
  // O rascunho pertence ao valor externo atual; resets não reutilizam buscas antigas.
  const current =
    interaction?.value === value && interaction.initialLabel === initialLabel
      ? interaction
      : null;
  const isOpen = current !== null && !disabled;
  const term = isOpen ? current.term : '';
  const load = useMemo(() => {
    if (!isOpen) return null;
    return async (signal: AbortSignal) => {
      await aguardarBusca(searchDelay, signal);
      return searcher(term, signal);
    };
  }, [isOpen, term, searcher, searchDelay]);
  const {
    data,
    error: searchError,
    isLoading: loading,
    refetch,
  } = useConsultaApolice(load, 'Não foi possível carregar as opções.');

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        event.target instanceof Node &&
        !wrapperRef.current?.contains(event.target)
      ) {
        setInteraction(null);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = (option: AsyncEntityOption) => {
    setSelected({ ...option, initialLabel });
    setInteraction(null);
    onChange(option.value);
  };

  return (
    <div
      ref={wrapperRef}
      className="relative w-full"
      onBlur={(event) => {
        if (
          !(event.relatedTarget instanceof Node) ||
          !wrapperRef.current?.contains(event.relatedTarget)
        ) {
          setInteraction(null);
        }
      }}
      onKeyDown={(event) => {
        if (event.key === 'Escape') setInteraction(null);
      }}
    >
      <div className="relative">
        <Input
          id={id}
          {...accessibilityProps}
          type="text"
          placeholder={placeholder || 'Buscar...'}
          value={isOpen ? current.term : selectedLabel}
          onChange={(event) => {
            const nextValue = value ? '' : value;
            setSelected(null);
            setInteraction({
              value: nextValue,
              initialLabel,
              term: event.target.value,
            });
            if (value) onChange('');
          }}
          onFocus={() =>
            setInteraction({
              value,
              initialLabel,
              term: clearSearchOnFocus ? '' : selectedLabel,
            })
          }
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
      {isOpen && (
        <div className="absolute z-50 w-full mt-1 bg-fundo-elevado border border-borda rounded shadow-lg max-h-60 overflow-y-auto">
          {searchError ? (
            <Alert variant="error" title="Erro ao buscar registros">
              Não foi possível carregar as opções.
              <Button
                type="button"
                variant="secondary"
                size="small"
                onClick={() => void refetch()}
              >
                Tentar novamente
              </Button>
            </Alert>
          ) : !loading && !data?.length ? (
            <div className="p-3 text-sm text-texto-secundario text-center">
              {emptyMessage}
            </div>
          ) : (
            !loading && (
              <ul className="py-1">
                {data?.map((option) => (
                  <li key={option.value}>
                    <Button
                      type="button"
                      variant="text"
                      className={`w-full text-left justify-start hover:bg-fundo-superficie ${option.value === value ? 'bg-fundo-superficie font-medium' : ''}`}
                      onMouseDown={(event) => {
                        event.preventDefault();
                        handleSelect(option);
                      }}
                      onClick={() => handleSelect(option)}
                    >
                      <span className="text-texto-principal">
                        {option.label}
                      </span>
                    </Button>
                  </li>
                ))}
              </ul>
            )
          )}
        </div>
      )}
    </div>
  );
};

interface AsyncEntityFieldProps extends FieldAccessibilityProps {
  name: 'estipulanteId' | 'seguradoraId' | 'corretoraId';
  control: Control<ApoliceFormValues, unknown, ApoliceFormValues>;
  disabled?: boolean;
  placeholder?: string;
  initialLabel?: string;
  searcher: (
    term: string,
    signal?: AbortSignal,
  ) => Promise<AsyncEntityOption[]>;
}

export const AsyncEntityField: React.FC<AsyncEntityFieldProps> = ({
  name,
  control,
  placeholder,
  initialLabel,
  searcher,
  disabled,
  id,
  ...accessibilityProps
}) => {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <EntitySearchField
          id={id || name}
          {...accessibilityProps}
          disabled={disabled}
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
