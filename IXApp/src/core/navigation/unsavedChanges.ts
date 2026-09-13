const drafts = new Map<symbol, string>();
const listeners = new Set<() => void>();
const notify = () => listeners.forEach((listener) => listener());
export const unsavedChanges = {
  subscribe(listener: () => void) {
    listeners.add(listener);
    return () => {
      listeners.delete(listener);
    };
  },
  isDirty: () => drafts.size > 0,
  register(message: string) {
    const id = Symbol('draft');
    drafts.set(id, message);
    notify();
    return () => {
      drafts.delete(id);
      notify();
    };
  },
  confirmDiscard: () => !drafts.size || window.confirm([...drafts.values()][0]),
};
