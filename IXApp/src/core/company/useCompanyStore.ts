import { create } from 'zustand';
import { DEFAULT_COMPANY } from '@core/constants/appConstants';
import { STORAGE_KEYS } from '@core/constants/storageKeys';
import { unsavedChanges } from '@core/navigation/unsavedChanges';

interface CompanyStoreState {
  currentCompany: string;
  setCompany: (companyCode: string) => boolean;
}

const getInitialCompany = (): string =>
  globalThis.localStorage?.getItem(STORAGE_KEYS.COMPANY) || DEFAULT_COMPANY;

export const useCompanyStore = create<CompanyStoreState>((set, get) => ({
  currentCompany: getInitialCompany(),
  setCompany: (companyCode) => {
    if (companyCode === get().currentCompany) return true;
    if (!unsavedChanges.confirmDiscard()) return false;
    globalThis.localStorage?.setItem(STORAGE_KEYS.COMPANY, companyCode);
    set({ currentCompany: companyCode });
    return true;
  },
}));
