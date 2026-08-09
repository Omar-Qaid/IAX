import { create } from 'zustand';
import { DEFAULT_COMPANY } from '@core/constants/appConstants';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

interface AppStoreState {
  currentCompany: string;
  setCompany: (companyCode: string) => void;
}

const getInitialCompany = (): string => {
  return localStorage.getItem(STORAGE_KEYS.COMPANY) || DEFAULT_COMPANY;
};

export const useAppStore = create<AppStoreState>((set) => ({
  currentCompany: getInitialCompany(),
  setCompany: (companyCode: string) => {
    localStorage.setItem(STORAGE_KEYS.COMPANY, companyCode);
    set({ currentCompany: companyCode });
  },
}));
