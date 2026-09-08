import { useRef, useState } from 'react';

/** Runs an action at most once at a time, including calls before React rerenders. */
export function useAsyncAction(action: () => Promise<void>, onError: (error: unknown) => void) {
  const pending = useRef(false);
  const [running, setRunning] = useState(false);

  const run = async () => {
    if (pending.current) return;
    pending.current = true;
    setRunning(true);
    try {
      await action();
    } catch (error) {
      onError(error);
    } finally {
      pending.current = false;
      setRunning(false);
    }
  };

  return { run, running };
}
