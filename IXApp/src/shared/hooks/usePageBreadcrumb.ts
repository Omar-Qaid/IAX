import { useLayoutEffect } from 'react';
import { useLocation } from 'react-router-dom';
import {
  registerPageBreadcrumb,
  type PageBreadcrumbConfig,
} from '@shared/services/pageBreadcrumbStore';

/** Publish page-owned breadcrumb metadata without coupling a pattern to the app shell. */
export function usePageBreadcrumb(
  config: PageBreadcrumbConfig | false | undefined,
  defaultLabel: string
): void {
  const { pathname } = useLocation();
  const disabled = config === false;
  const listPath = (config && config.listPath) || pathname.replace(/\/[^/]+\/?$/, '') || '/';
  const labelKey = (config && config.labelKey) || undefined;
  const label = (config && config.label) || defaultLabel;
  const explicit = Boolean(config);
  useLayoutEffect(() => {
    if (disabled || listPath === pathname || (!labelKey && !label)) return;
    return registerPageBreadcrumb({ pathname, listPath, labelKey, label, explicit });
  }, [disabled, pathname, listPath, labelKey, label, explicit]);
}
