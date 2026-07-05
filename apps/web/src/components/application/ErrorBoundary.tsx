/**
 * ErrorBoundary.tsx
 *
 * Error Boundary para captura de erros inesperados de renderização React.
 *
 * Deve:
 * - Capturar falhas de componentes filhos
 * - Exibir página segura sem detalhes técnicos
 * - Permitir tentar novamente (resetar o estado)
 * - Permitir retornar ao início
 * - Não exibir stack traces ao usuário
 * - Preservar hook para observabilidade futura (ex: Sentry)
 */
import React from 'react';
import { EmptyState } from '../ui/EmptyState';
import { Button } from '../ui/Button';
import { ErrorIcon } from '../ui/Icons';
import { ENV } from '../../app/config/env';
import './ErrorBoundary.css';

interface ErrorBoundaryProps {
  children: React.ReactNode;
  /** Elemento de fallback customizado (opcional) */
  fallback?: React.ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  errorId: string | null;
}

export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, errorId: null };
  }

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true, errorId: `err-${Date.now()}` };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo): void {
    // Hook para observabilidade futura (ex: Sentry.captureException)
    // Não logar o erro completo para evitar expor dados sensíveis
    if (ENV.IS_DEV) {
      console.error('[ErrorBoundary] Erro capturado:', error.name, error.message);
      console.error('[ErrorBoundary] Component stack:', info.componentStack);
    }
  }

  handleReset = (): void => {
    this.setState({ hasError: false, errorId: null });
  };

  handleGoHome = (): void => {
    this.setState({ hasError: false, errorId: null });
    window.location.hash = '#/app';
  };

  render(): React.ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div className="error-boundary-container" role="alert" aria-live="assertive">
          <EmptyState
            title="Algo deu errado"
            description="Ocorreu um erro inesperado. Nossa equipe foi notificada. Tente novamente ou retorne ao início."
            icon={<ErrorIcon size={48} aria-hidden="true" />}
            action={
              <div className="error-boundary-actions">
                <Button variant="primary" onClick={this.handleReset}>
                  Tentar novamente
                </Button>
                <Button variant="secondary" onClick={this.handleGoHome}>
                  Voltar ao início
                </Button>
              </div>
            }
          />
        </div>
      );
    }

    return this.props.children;
  }
}
