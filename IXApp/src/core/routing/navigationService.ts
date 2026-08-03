export type NavigationListener = (path: string) => void;
const listeners = new Set<NavigationListener>();

function notify(path: string) { listeners.forEach(listener => listener(path)); }
export const navigationService = {
  navigate(path: string, options?: { replace?: boolean }) {
    if (typeof window === 'undefined') return;
    window.history[options?.replace ? 'replaceState' : 'pushState']({}, '', path);
    notify(path);
    window.dispatchEvent(new PopStateEvent('popstate'));
  },
  back() { if (typeof window !== 'undefined') window.history.back(); },
  currentPath() { return typeof window === 'undefined' ? '/' : `${window.location.pathname}${window.location.search}${window.location.hash}`; },
  subscribe(listener: NavigationListener) { listeners.add(listener); return () => { listeners.delete(listener); }; },
};
