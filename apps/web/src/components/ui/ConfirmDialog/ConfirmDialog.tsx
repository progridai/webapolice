import React from 'react';
import { Modal } from '../Modal/Modal';
import { Button } from '../Button/Button';
import { ErrorIcon, AlertIcon } from '../Icons';
import './ConfirmDialog.css';

export interface ConfirmDialogProps {
  aberto: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description: React.ReactNode;
  confirmText?: string;
  cancelText?: string;
  variant?: 'danger' | 'primary';
  loading?: boolean;
}

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  aberto,
  onClose,
  onConfirm,
  title,
  description,
  confirmText = 'Confirmar',
  cancelText = 'Cancelar',
  variant = 'primary',
  loading = false,
}) => {
  const footer = (
    <>
      <Button variant="secondary" onClick={onClose} disabled={loading}>
        {cancelText}
      </Button>
      <Button
        variant={variant === 'danger' ? 'danger' : 'primary'}
        onClick={onConfirm}
        loading={loading}
      >
        {confirmText}
      </Button>
    </>
  );

  return (
    <Modal
      aberto={aberto}
      onClose={onClose}
      title={title}
      size="small"
      footer={footer}
    >
      <div className="confirm-dialog-content-row">
        <div className={variant === 'danger' ? 'confirm-dialog-icon-error' : 'confirm-dialog-icon-primary'}>
          {variant === 'danger' ? <ErrorIcon size={24} /> : <AlertIcon size={24} />}
        </div>
        <div className="confirm-dialog-desc">
          {description}
        </div>
      </div>
    </Modal>
  );
};
export default ConfirmDialog;
