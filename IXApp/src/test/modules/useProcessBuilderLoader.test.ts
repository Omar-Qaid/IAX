import { act, renderHook, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useProcessBuilderLoader } from '@modules/process-builder/hooks/useProcessBuilderLoader';
import {
  useProcessBuilderStore,
  createProcessBuilderDocument,
} from '@modules/process-builder/store/useProcessBuilderStore';
import * as api from '@modules/process-builder/api/processBuilderApi';

const mocks = vi.hoisted(() => ({ notifyError: vi.fn(), t: (key: string) => key }));
vi.mock('@shared/hooks/useNotifications', () => ({
  useNotifications: () => ({ notifyError: mocks.notifyError }),
}));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: mocks.t }),
}));
vi.mock('@modules/process-builder/api/processBuilderApi', () => ({
  getProcessCodeMetadata: vi.fn(),
  getVariableCodeMetadata: vi.fn(),
  getStepCodeMetadata: vi.fn(),
  getActivityCodeMetadata: vi.fn(),
  getRequestControlCodeMetadata: vi.fn(),
  loadProcessBuilder: vi.fn(),
}));

beforeEach(() => {
  vi.resetAllMocks();
  localStorage.clear();
  sessionStorage.clear();
  useProcessBuilderStore.getState().initialize(createProcessBuilderDocument('new'));
  vi.mocked(api.getProcessCodeMetadata).mockResolvedValue({
    manual: true,
    mode: 'manual',
    available: true,
    previewCode: null,
    message: null,
  });
  for (const getMetadata of [
    api.getVariableCodeMetadata,
    api.getStepCodeMetadata,
    api.getActivityCodeMetadata,
    api.getRequestControlCodeMetadata,
  ]) {
    vi.mocked(getMetadata).mockResolvedValue({
      manual: true,
      mode: 'manual',
      available: true,
      previewCode: null,
      message: null,
    });
  }
});

describe('useProcessBuilderLoader', () => {
  it('loads section metadata even when process code preview fails', async () => {
    vi.mocked(api.getProcessCodeMetadata).mockRejectedValue(new Error('Preview unavailable'));
    const { result } = renderHook(() => useProcessBuilderLoader('new'));
    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current).toMatchObject({
      manualVariableCode: true,
      manualStepCode: true,
      manualActivityCode: true,
      manualRequestControlCode: true,
    });
    expect(mocks.notifyError).toHaveBeenCalledTimes(1);
  });

  it('preserves unsaved edits when the translation function changes', async () => {
    const { result, rerender } = renderHook(() => useProcessBuilderLoader('new'));
    await waitFor(() => expect(result.current.loading).toBe(false));
    act(() => useProcessBuilderStore.getState().updateProcess({ name: 'Unsaved edit' }));
    mocks.t = (key: string) => `translated:${key}`;
    rerender();
    expect(api.getProcessCodeMetadata).toHaveBeenCalledTimes(1);
    expect(useProcessBuilderStore.getState().document.name).toBe('Unsaved edit');
  });
});
