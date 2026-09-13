import { afterEach, describe, expect, it, vi } from 'vitest';
import { unsavedChanges } from '@core/navigation/unsavedChanges';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

afterEach(() => vi.restoreAllMocks());
describe('company switching with unsaved editors', () => {
  it('keeps company and storage unchanged on cancel, and changes both on confirm', () => {
    useCompanyStore.setState({ currentCompany: 'USMF' });
    localStorage.setItem(STORAGE_KEYS.COMPANY, 'USMF');
    const unregister = unsavedChanges.register('Discard changes?');
    try {
      const confirm = vi.spyOn(window, 'confirm').mockReturnValue(false);
      expect(useCompanyStore.getState().setCompany('DEMF')).toBe(false);
      expect(useCompanyStore.getState().currentCompany).toBe('USMF');
      expect(localStorage.getItem(STORAGE_KEYS.COMPANY)).toBe('USMF');
      confirm.mockReturnValue(true);
      expect(useCompanyStore.getState().setCompany('DEMF')).toBe(true);
      expect(localStorage.getItem(STORAGE_KEYS.COMPANY)).toBe('DEMF');
    } finally {
      unregister();
    }
  });
  it('retains protection until every editor unregisters', () => {
    const first = unsavedChanges.register('First');
    const second = unsavedChanges.register('Second');
    first();
    expect(unsavedChanges.isDirty()).toBe(true);
    second();
    expect(unsavedChanges.isDirty()).toBe(false);
  });
});
