import { ENV } from '../config/env';
import { useAuthorization } from '../../auth/AuthorizationProvider';
import { NAV_GROUPS, type NavItem } from './navigation';

export function useAppNavigation() {
  const {
    ehOperadorSistema,
    possuiModulo,
    possuiRecurso,
    possuiPermissao,
    possuiAcessoTotal,
  } = useAuthorization();

  const canViewItem = (item: NavItem) => {
    if (item.requiresEnvFlag && !ENV.ENABLE_DESIGN_SYSTEM) return false;
    if (item.somenteOperador && !ehOperadorSistema()) return false;
    if (
      item.recursoCodigo &&
      (!possuiRecurso(item.recursoCodigo) ||
        (item.moduloCodigo && !possuiModulo(item.moduloCodigo)))
    )
      return false;
    if (
      item.moduloCodigo &&
      !possuiAcessoTotal() &&
      !possuiModulo(item.moduloCodigo)
    )
      return false;
    if (
      item.permissaoCodigo &&
      !possuiAcessoTotal() &&
      !possuiPermissao(item.permissaoCodigo)
    )
      return false;
    return true;
  };

  return NAV_GROUPS.map((group) => ({
    ...group,
    items: group.items.filter(canViewItem),
  })).filter((group) => group.items.length > 0);
}
