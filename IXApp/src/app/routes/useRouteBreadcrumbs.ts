import { useSyncExternalStore } from 'react';
import { useLocation } from 'react-router-dom';
import {
  getPageBreadcrumbSnapshot,
  getServerPageBreadcrumbSnapshot,
  subscribePageBreadcrumbs,
} from '@shared/services/pageBreadcrumbStore';
import { getRouteBreadcrumbs, type BreadcrumbDefinition } from './routeMetadata';

/** Shell breadcrumbs prefer metadata published by the mounted page pattern. */
export function useRouteBreadcrumbs(): BreadcrumbDefinition[] {
  const { pathname, search } = useLocation();
  const pages = useSyncExternalStore(
    subscribePageBreadcrumbs,
    getPageBreadcrumbSnapshot,
    getServerPageBreadcrumbSnapshot
  );
  const fallback = getRouteBreadcrumbs(pathname, search);
  const matches = Array.from(pages.values())
    .reverse()
    .filter((item) => item.pathname === pathname);
  const page = matches.find((item) => item.explicit) ?? matches[0];
  if (!page || (!page.explicit && fallback.length > 3) || !page.listPath) return fallback;
  const parents = getRouteBreadcrumbs(page.listPath);
  if (parents.length < 3) return fallback;
  return [
    ...parents.map((item, index) =>
      index === parents.length - 1 ? { ...item, path: page.listPath } : item
    ),
    page.labelKey ? { labelKey: page.labelKey } : { labelKey: '', label: page.label },
  ];
}
