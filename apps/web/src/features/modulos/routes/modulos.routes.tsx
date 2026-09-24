import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { ModulosListPage } from '../pages/ModulosListPage';
import { ModuloFormPage } from '../pages/ModuloFormPage';

export const ModulosRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="" element={<ModulosListPage />} />
      <Route path="novo" element={<ModuloFormPage />} />
      <Route path=":publicId/editar" element={<ModuloFormPage />} />
    </Routes>
  );
};
