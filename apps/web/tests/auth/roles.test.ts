/**
 * tests/auth/roles.test.ts
 *
 * Testa os helpers de autorização centralizados.
 */
import { describe, it, expect } from 'vitest';
import { hasRole, hasAnyRole, hasAllRoles, APP_ROLES } from '../../src/auth/roles';

describe('Helpers de Autorização', () => {
  describe('APP_ROLES', () => {
    it('deve conter as roles admin, gestor e operador', () => {
      expect(APP_ROLES.ADMIN).toBe('admin');
      expect(APP_ROLES.GESTOR).toBe('gestor');
      expect(APP_ROLES.OPERADOR).toBe('operador');
    });
  });

  describe('hasRole', () => {
    it('retorna true quando o usuário possui a role', () => {
      expect(hasRole(['admin', 'gestor'], 'admin')).toBe(true);
    });

    it('retorna false quando o usuário não possui a role', () => {
      expect(hasRole(['gestor'], 'admin')).toBe(false);
    });

    it('retorna false para array vazio', () => {
      expect(hasRole([], 'admin')).toBe(false);
    });
  });

  describe('hasAnyRole', () => {
    it('retorna true quando possui ao menos uma role da lista', () => {
      expect(hasAnyRole(['operador'], ['admin', 'operador'])).toBe(true);
    });

    it('retorna false quando não possui nenhuma role da lista', () => {
      expect(hasAnyRole(['gestor'], ['admin', 'operador'])).toBe(false);
    });

    it('retorna false para arrays vazios', () => {
      expect(hasAnyRole([], ['admin'])).toBe(false);
    });

    it('retorna false quando lista de roles requeridas está vazia', () => {
      expect(hasAnyRole(['admin'], [])).toBe(false);
    });
  });

  describe('hasAllRoles', () => {
    it('retorna true quando possui todas as roles', () => {
      expect(hasAllRoles(['admin', 'gestor'], ['admin', 'gestor'])).toBe(true);
    });

    it('retorna false quando não possui todas as roles', () => {
      expect(hasAllRoles(['admin'], ['admin', 'gestor'])).toBe(false);
    });

    it('retorna true para lista de roles requeridas vazia', () => {
      // every() retorna true para array vazio — comportamento esperado
      expect(hasAllRoles(['admin'], [])).toBe(true);
    });
  });
});
