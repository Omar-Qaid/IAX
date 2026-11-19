export interface PageBreadcrumbConfig {
  /** Parent list route. Defaults to the current URL without its final segment. */
  listPath?: string;
  /** Translation key for the current document breadcrumb. */
  labelKey?: string;
  /** Already-localized caption, when no translation key is supplied. */
  label?: string;
}

export interface RegisteredPageBreadcrumb extends PageBreadcrumbConfig {
  pathname: string;
  explicit: boolean;
}

const empty = new Map<symbol, RegisteredPageBreadcrumb>();
let snapshot: ReadonlyMap<symbol, RegisteredPageBreadcrumb> = empty;
const listeners = new Set<() => void>();
export const getPageBreadcrumbSnapshot = () => snapshot;
export const getServerPageBreadcrumbSnapshot = () => empty;
export const subscribePageBreadcrumbs = (listener: () => void) => {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
};

/** Each page owns its registration; unmounting an old page cannot clear a newer one. */
export function registerPageBreadcrumb(value: RegisteredPageBreadcrumb): () => void {
  const owner = Symbol('page-breadcrumb');
  snapshot = new Map(snapshot).set(owner, value);
  listeners.forEach((listener) => listener());
  return () => {
    const next = new Map(snapshot);
    next.delete(owner);
    snapshot = next;
    listeners.forEach((listener) => listener());
  };
}
