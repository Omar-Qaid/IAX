import { act, renderHook } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useAsyncAction } from '@shared/hooks/useAsyncAction';

describe('useAsyncAction', () => {
  it('blocks duplicate calls before rerender and allows another call after completion', async () => {
    let finish!: () => void;
    const action = vi.fn(
      () =>
        new Promise<void>((resolve) => {
          finish = resolve;
        })
    );
    const { result } = renderHook(() => useAsyncAction(action, vi.fn()));
    let pending!: Promise<void>;
    act(() => {
      pending = result.current.run();
      void result.current.run();
    });
    expect(action).toHaveBeenCalledTimes(1);
    expect(result.current.running).toBe(true);
    await act(async () => {
      finish();
      await pending;
    });
    expect(result.current.running).toBe(false);
    await act(async () => {
      pending = result.current.run();
      finish();
      await pending;
    });
    expect(action).toHaveBeenCalledTimes(2);
  });

  it('reports errors and releases the pending state for retry', async () => {
    const error = new Error('Save failed');
    const action = vi.fn().mockRejectedValueOnce(error).mockResolvedValue(undefined);
    const onError = vi.fn();
    const { result } = renderHook(() => useAsyncAction(action, onError));
    await act(async () => {
      await result.current.run();
    });
    expect(onError).toHaveBeenCalledWith(error);
    expect(result.current.running).toBe(false);
    await act(async () => {
      await result.current.run();
    });
    expect(action).toHaveBeenCalledTimes(2);
    expect(onError).toHaveBeenCalledTimes(1);
  });
});
