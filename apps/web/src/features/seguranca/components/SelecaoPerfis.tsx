/**
 * SelecaoPerfis.tsx
 *
 * Componente reutilizável para seleção de múltiplos perfis no formulário de usuários.
 * Exibe nome, código e status. Não permite selecionar perfis inativos.
 * Usa apenas Checkbox do design system.
 */
import React from 'react';
import { Checkbox } from '../../../components/ui/Checkbox';
import { StatusBadge } from '../../../components/ui';
import type { PerfilDto } from '../types/seguranca.types';

interface SelecaoPerfisProps {
  perfis: PerfilDto[];
  selecionados: string[]; // publicIds
  onChange: (selecionados: string[]) => void;
  disabled?: boolean;
}

export const SelecaoPerfis: React.FC<SelecaoPerfisProps> = ({
  perfis,
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

  if (perfis.length === 0) {
    return (
      <p className="text-sm text-texto-secundario">
        Nenhum perfil disponível.
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-2">
      {perfis.map((perfil) => {
        const isSelecionado = selecionados.includes(perfil.publicId);
        const isDisabled = disabled || !perfil.ativo;

        return (
          <div
            key={perfil.publicId}
            className={`flex items-center justify-between p-2 rounded-md border transition-all ${isSelecionado ? 'border-primary-borda bg-primary-suave' : 'border-borda bg-transparent'} ${isDisabled ? 'opacity-50 pointer-events-none' : ''}`}
          >
            <Checkbox
              label={
                <span className="flex items-baseline gap-2">
                  <span className="text-sm font-semibold text-texto-principal">
                    {perfil.nome}
                  </span>
                </span>
              }
              checked={isSelecionado}
              onChange={() => !isDisabled && toggle(perfil.publicId)}
              disabled={isDisabled}
            />
            <StatusBadge status={perfil.ativo ? 'ativo' : 'inativo'} />
          </div>
        );
      })}
    </div>
  );
};
