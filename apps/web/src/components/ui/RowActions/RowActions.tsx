import React, { ReactNode } from 'react';
import { Button } from '../Button';
import { DropdownMenu, DropdownMenuItem } from '../DropdownMenu';
import { MoreVerticalIcon } from '../Icons';
import './RowActions.css';

export interface RowActionItem {
  label: string;
  icon?: ReactNode;
  onClick: () => void;
  disabled?: boolean;
  variant?: 'danger' | 'default';
}

export interface RowActionsProps {
  primaryAction?: {
    label: string;
    icon?: ReactNode;
    onClick: () => void;
    disabled?: boolean;
  };
  actions: RowActionItem[];
  ariaLabel?: string;
}

export const RowActions: React.FC<RowActionsProps> = ({ primaryAction, actions, ariaLabel = 'Ações' }) => {
  return (
    <div className="row-actions" role="group" aria-label={ariaLabel}>
      {primaryAction && (
        <Button
          variant="secondary"
          size="small"
          onClick={primaryAction.onClick}
          disabled={primaryAction.disabled}
        >
          {primaryAction.icon && <span className="row-actions-btn-icon" aria-hidden="true">{primaryAction.icon}</span>}
          {primaryAction.label}
        </Button>
      )}

      {actions.length > 0 && (
        <DropdownMenu
          align="right"
          trigger={
            <button
              type="button"
              className="row-actions-menu-trigger"
              aria-label="Mais ações"
            >
              <MoreVerticalIcon size={16} />
            </button>
          }
        >
          {actions.map((action, idx) => (
            <DropdownMenuItem
              key={idx}
              onClick={action.onClick}
              disabled={action.disabled}
              icon={action.icon}
              className={action.variant === 'danger' ? 'row-actions-item-danger' : ''}
            >
              {action.label}
            </DropdownMenuItem>
          ))}
        </DropdownMenu>
      )}
    </div>
  );
};

