import { create } from 'zustand';
import { DEFAULT_COMPANY } from '@core/constants/appConstants';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

interface CompanyStoreState {
  currentCompany: string;
  setCompany: (companyCode: string) => void;
}

const getInitialCompany = (): string =>
  globalThis.localStorage?.getItem(STORAGE_KEYS.COMPANY) || DEFAULT_COMPANY;

export const useCompanyStore = create<CompanyStoreState>((set) => ({
  currentCompany: getInitialCompany(),
  setCompany: (companyCode) => {
    globalThis.localStorage?.setItem(STORAGE_KEYS.COMPANY, companyCode);
    set({ currentCompany: companyCode });
  },
}));
