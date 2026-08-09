export const currencyQueryKeys = {
  all: ['currencies'] as const,
  lists: () => [...currencyQueryKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) => [...currencyQueryKeys.lists(), params] as const,
  detail: (id: string) => [...currencyQueryKeys.all, 'detail', id] as const,
};
