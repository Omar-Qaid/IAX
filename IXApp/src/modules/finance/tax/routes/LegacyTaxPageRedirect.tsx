import { Navigate, useLocation } from 'react-router-dom';
import { LEGACY_TAX_ROUTE_PATHS, TAX_ROUTE_PATHS } from './taxRoutePaths';

export function LegacyTaxPageRedirect() {
  const location = useLocation();
  const pathname = location.pathname.replace(/\/+$/, '').toLowerCase();
  const key = (Object.keys(LEGACY_TAX_ROUTE_PATHS) as (keyof typeof LEGACY_TAX_ROUTE_PATHS)[]).find(
    (name) => LEGACY_TAX_ROUTE_PATHS[name].toLowerCase() === pathname
  );
  return key ? (
    <Navigate
      replace
      to={`${TAX_ROUTE_PATHS[key]}${location.search}${location.hash}`}
      state={location.state}
    />
  ) : null;
}
