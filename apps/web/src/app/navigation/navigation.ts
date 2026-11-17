import type React from 'react';
import { ROUTES } from '../routes/routePaths';
import {
  BookOpenIcon,
  ClipboardListIcon,
  HomeIcon,
  PaletteIcon,
  SettingsIcon,
  ShieldIcon,
  UserIcon,
  UsersIcon,
  BriefcaseIcon,
} from '../../components/ui/Icons';

export interface NavItem {
  label: string;
  path: string;
  icon: React.ComponentType<{ size?: number; className?: string }>;
  moduloCodigo?: string;
  permissaoCodigo?: string;
  recursoCodigo?: string;
  somenteOperador?: boolean;
  requiresEnvFlag?: boolean;
}

export interface NavGroup {
  label?: string;
  items: NavItem[];
}

export const NAV_GROUPS: NavGroup[] = [
  {
    items: [
      {
        label: 'Início',
        path: ROUTES.APP,
        icon: HomeIcon,
      },
    ],
  },
  {
    label: 'Operação',
    items: [
      {
        label: 'Clientes',
        path: ROUTES.CLIENTES,
        icon: UsersIcon,
        moduloCodigo: 'CLIENTES',
      },
      {
        label: 'Apólices',
        path: ROUTES.APOLICES,
        icon: ClipboardListIcon,
        moduloCodigo: 'APOLICES',
        permissaoCodigo: 'apolices.visualizar',
      },
      {
        label: 'Propostas',
        path: ROUTES.PROPOSTAS,
        icon: ClipboardListIcon,
        moduloCodigo: 'APOLICES',
        permissaoCodigo: 'propostas.visualizar',
        recursoCodigo: 'PROPOSTAS',
      },
    ],
  },
  {
    label: 'Parceiros e Estrutura',
    items: [
      {
        label: 'Seguradoras',
        path: ROUTES.SEGURADORAS,
        icon: BriefcaseIcon,
        permissaoCodigo: 'seguradoras.visualizar',
      },
      {
        label: 'Corretoras',
        path: ROUTES.CORRETORAS,
        icon: BriefcaseIcon,
        permissaoCodigo: 'corretoras.visualizar',
      },
      {
        label: 'Estipulantes',
        path: ROUTES.ESTIPULANTES,
        icon: BriefcaseIcon,
        moduloCodigo: 'ESTIPULANTES',
        permissaoCodigo: 'estipulantes.visualizar',
      },
      {
        label: 'Subestipulantes',
        path: ROUTES.SUBESTIPULANTES,
        icon: BriefcaseIcon,
        permissaoCodigo: 'subestipulantes.visualizar',
      },
      {
        label: 'Cooperados',
        path: ROUTES.COOPERADOS,
        icon: UsersIcon,
        moduloCodigo: 'COOPERADOS',
        permissaoCodigo: 'cooperados.visualizar',
      },
    ],
  },
  {
    label: 'Configurações do Seguro',
    items: [
      {
        label: 'Ramos',
        path: ROUTES.RAMOS,
        icon: BriefcaseIcon,
        moduloCodigo: 'APOLICES',
        permissaoCodigo: 'ramos.visualizar',
      },
      {
        label: 'Coberturas',
        path: ROUTES.COBERTURAS,
        icon: BookOpenIcon,
        moduloCodigo: 'CADASTRO',
        permissaoCodigo: 'coberturas.visualizar',
      },
      {
        label: 'Módulos Globais',
        path: ROUTES.MODULOS,
        icon: SettingsIcon,
        permissaoCodigo: 'modulos.visualizar',
      },
      {
        label: 'Convênios de Cobrança',
        path: ROUTES.CONVENIOS_COBRANCA,
        icon: BookOpenIcon,
        moduloCodigo: 'CADASTRO',
        permissaoCodigo: 'convenios_cobranca.visualizar',
      },
    ],
  },
  {
    label: 'Segurança',
    items: [
      {
        label: 'Usuários',
        path: ROUTES.SEGURANCA_USUARIOS,
        icon: UserIcon,
        moduloCodigo: 'SEGURANCA',
        permissaoCodigo: 'seguranca.usuarios.visualizar',
      },
      {
        label: 'Perfis',
        path: ROUTES.SEGURANCA_PERFIS,
        icon: ShieldIcon,
        moduloCodigo: 'SEGURANCA',
        permissaoCodigo: 'seguranca.perfis.visualizar',
      },
      {
        label: 'Catálogo de Permissões',
        path: ROUTES.SEGURANCA_CATALOGO,
        icon: BookOpenIcon,
        moduloCodigo: 'SEGURANCA',
        permissaoCodigo: 'seguranca.catalogo.visualizar',
      },
      {
        label: 'Auditoria',
        path: ROUTES.SEGURANCA_AUDITORIA,
        icon: ClipboardListIcon,
        moduloCodigo: 'SEGURANCA',
        permissaoCodigo: 'seguranca.auditoria.visualizar',
      },
    ],
  },
  {
    label: 'Sistema',
    items: [
      {
        label: 'Módulos do Sistema',
        path: ROUTES.SEGURANCA_MODULOS,
        icon: SettingsIcon,
        somenteOperador: true,
      },
      {
        label: 'Design System',
        path: ROUTES.DESIGN_SYSTEM,
        icon: PaletteIcon,
        somenteOperador: true,
        requiresEnvFlag: true,
      },
    ],
  },
];
