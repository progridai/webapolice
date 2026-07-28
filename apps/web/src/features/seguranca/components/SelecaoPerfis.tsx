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
      <p className="text-sm text-slate-500 dark:text-slate-400">
        Nenhum perfil disponível.
      </p>
    );
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
      {perfis.map((perfil) => {
        const isSelecionado = selecionados.includes(perfil.publicId);
        const isDisabled = disabled || !perfil.ativo;

        return (
          <div
            key={perfil.publicId}
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              padding: '8px',
              borderRadius: '6px',
              border: '1px solid',
              borderColor: isSelecionado ? 'var(--cor-marca-principal-borda)' : 'var(--cor-borda)',
              backgroundColor: isSelecionado ? 'var(--cor-marca-principal-suave)' : 'transparent',
              opacity: isDisabled ? 0.5 : 1,
              transition: 'all 0.2s ease'
            }}
          >
            <Checkbox
              label={
                <span style={{ display: 'flex', flexDirection: 'column', marginLeft: '4px' }}>
                  <span style={{ fontSize: 'var(--fonte-tamanho-sm)', fontWeight: 500, color: 'var(--cor-texto-principal)' }}>
                    {perfil.nome}
                  </span>
                  <span style={{ fontSize: 'var(--fonte-tamanho-xs)', color: 'var(--cor-texto-secundario)', fontFamily: 'monospace' }}>
                    {perfil.codigo}
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
