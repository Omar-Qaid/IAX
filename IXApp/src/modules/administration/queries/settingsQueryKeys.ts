export const settingsQueryKeys = {
  all: ['settings'] as const,
  global: () => [...settingsQueryKeys.all, 'global'] as const,
  user: () => [...settingsQueryKeys.all, 'user'] as const,
};
