import { useEffect, useEffectEvent } from 'react';

/** Cancel the page only after nested editors and overlays have handled Escape. */
export function useEscapeCancel(enabled: boolean, onCancel?: () => void): void {
  const cancel = useEffectEvent(() => onCancel?.());
  const hasCancel = Boolean(onCancel);
  useEffect(() => {
    if (!enabled || !hasCancel) return;
    const pending = new Set<ReturnType<typeof setTimeout>>();
    const handleKeyDown = (event: KeyboardEvent) => {
      if (
        event.key !== 'Escape' ||
        event.defaultPrevented ||
        event.repeat ||
        event.isComposing ||
        event.ctrlKey ||
        event.altKey ||
        event.metaKey
      )
        return;
      // Capture this before an overlay unmounts in response to the same key.
      if (
        Array.from(
          document.querySelectorAll(
            '[role="dialog"], [role="alertdialog"], [role="listbox"], [role="menu"]'
          )
        ).some((overlay) => !overlay.closest('[aria-hidden="true"], [hidden]'))
      )
        return;
      const timer = setTimeout(() => {
        pending.delete(timer);
        if (event.defaultPrevented) return;
        event.preventDefault();
        cancel();
      }, 0);
      pending.add(timer);
    };
    window.addEventListener('keydown', handleKeyDown, true);
    return () => {
      window.removeEventListener('keydown', handleKeyDown, true);
      pending.forEach(clearTimeout);
    };
  }, [enabled, hasCancel]);
}
