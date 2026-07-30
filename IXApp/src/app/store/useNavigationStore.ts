import { create } from 'zustand';

interface NavigationState {
  isDrawerOpen: boolean;
  activeModuleId: string | null;
  toggleDrawer: () => void;
  setDrawerOpen: (open: boolean) => void;
  setActiveModule: (moduleId: string | null) => void;
}

export const useNavigationStore = create<NavigationState>((set) => ({
  isDrawerOpen: true,
  activeModuleId: 'accounts-receivable',
  toggleDrawer: () => set((state) => ({ isDrawerOpen: !state.isDrawerOpen })),
  setDrawerOpen: (open: boolean) => set({ isDrawerOpen: open }),
  setActiveModule: (moduleId: string | null) => set({ activeModuleId: moduleId }),
}));
