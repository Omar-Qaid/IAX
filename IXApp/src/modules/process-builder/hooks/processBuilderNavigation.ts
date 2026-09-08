import type { ProcessBuilderNavigationState } from '../store/useProcessBuilderStore';
export const navigationStorageKey = (builderId: string) =>
  `ixapp.processBuilder.navigation.${builderId}`;

export const readNavigationState = (builderId: string): ProcessBuilderNavigationState | null => {
  try {
    const value = sessionStorage.getItem(navigationStorageKey(builderId));
    return value ? (JSON.parse(value) as ProcessBuilderNavigationState) : null;
  } catch {
    return null;
  }
};
