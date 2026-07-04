/**
 * AppSidebar.tsx
 *
 * Barra lateral de navegação — visível em desktop/tablet.
 * Recolhida em mobile (substituída pelo MobileNavigation).
 */
import React from 'react';
import { AppNavigation } from './AppNavigation';

export const AppSidebar: React.FC = () => {
  return (
    <aside className="app-sidebar" aria-label="Menu lateral de navegação">
      <AppNavigation />
    </aside>
  );
};
