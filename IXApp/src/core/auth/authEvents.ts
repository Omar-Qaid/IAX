export type AuthEvent = 'session-expired' | 'access-denied';

type AuthEventListener = (event: AuthEvent) => void;
const listeners = new Set<AuthEventListener>();

export const authEvents = {
  subscribe(listener: AuthEventListener): () => void {
    listeners.add(listener);
    return () => listeners.delete(listener);
  },
  emit(event: AuthEvent): void {
    listeners.forEach((listener) => listener(event));
  },
};
