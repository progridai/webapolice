/**
 * SelecaoPermissoes.tsx
 *
 * Componente reutilizável para seleção agrupada de permissões.
 * Agrupamento: Módulo → Recurso → Permissão (checkboxes).
 * Usa apenas os componentes do design system existentes: Checkbox, DetailsSection.
 */
import React from 'react';
import { Checkbox } from '../../../components/ui/Checkbox';
import { DetailsSection } from '../../../components/ui';
import type { CatalogoModuloDto } from '../types/seguranca.types';

interface SelecaoPermissoesProps {
  catalogo: CatalogoModuloDto[];
  selecionados: string[]; // publicIds das permissões selecionadas
  onChange: (selecionados: string[]) => void;
  disabled?: boolean;
}

export const SelecaoPermissoes: React.FC<SelecaoPermissoesProps> = ({
  catalogo,
  selecionados,
  onChange,
  disabled = false,
}) => {
  const toggle = (publicId: string) => {
    if (selecionados.includes(publicId)) {
      onChange(selecionados.filter((id) => id !== publicId));
    } else {
      onChange([...selecionados, publicId]);
    }
  };

  const toggleRecurso = (permissaoIds: string[]) => {
    const todosAtivos = permissaoIds.every((id) => selecionados.includes(id));
    if (todosAtivos) {
      onChange(selecionados.filter((id) => !permissaoIds.includes(id)));
    } else {
      const novos = permissaoIds.filter((id) => !selecionados.includes(id));
      onChange([...selecionados, ...novos]);
    }
  };

  if (catalogo.length === 0) {
    return (
      <p className="text-sm text-slate-500 dark:text-slate-400">
        Nenhuma permissão disponível no catálogo.
      </p>
    );
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
      {catalogo.map((modulo) => (
        <DetailsSection key={modulo.publicId} title={modulo.nome}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            {modulo.recursos.map((recurso) => {
              const ids = recurso.permissoes.map((p) => p.publicId);
              const todos = ids.every((id) => selecionados.includes(id));
              const alguns = ids.some((id) => selecionados.includes(id));

              return (
                <div key={recurso.publicId} className="pl-2">
                  {/* Nível de recurso — seleciona/desmarca todas as permissões */}
                  <div className="mb-1">
                    <Checkbox
                      label={
                        <span className="font-medium text-sm text-slate-700 dark:text-slate-300">
                          {recurso.nome}
                        </span>
                      }
                      checked={todos}
                      indeterminate={alguns && !todos}
                      onChange={() => toggleRecurso(ids)}
                      disabled={disabled}
                    />
                  </div>

                  {/* Permissões individuais */}
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '4px', paddingLeft: '24px' }}>
                    {recurso.permissoes.map((perm) => (
                      <Checkbox
                        key={perm.publicId}
                        label={
                          <span className="text-sm text-slate-600 dark:text-slate-400">
                            {perm.nome}
                            {perm.descricao && (
                              <span className="ml-1 text-xs text-slate-400 dark:text-slate-500">
                                — {perm.descricao}
                              </span>
                            )}
                          </span>
                        }
                        checked={selecionados.includes(perm.publicId)}
                        onChange={() => toggle(perm.publicId)}
                        disabled={disabled}
                      />
                    ))}
                  </div>
                </div>
              );
            })}
          </div>
        </DetailsSection>
      ))}
    </div>
  );
};
