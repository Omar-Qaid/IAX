export const queryKeys = {
  customers: {
    all: ['customers'] as const,
    lists: () => [...queryKeys.customers.all, 'list'] as const,
    list: (params: Record<string, unknown>) => [...queryKeys.customers.lists(), params] as const,
    details: () => [...queryKeys.customers.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.customers.details(), id] as const,
  },
  customerGroups: {
    all: ['customerGroups'] as const,
    lists: () => [...queryKeys.customerGroups.all, 'list'] as const,
    list: (params?: Record<string, unknown>) => [...queryKeys.customerGroups.lists(), params] as const,
    detail: (id: string) => [...queryKeys.customerGroups.all, 'detail', id] as const,
  },
  salesOrders: {
    all: ['salesOrders'] as const,
    lists: () => [...queryKeys.salesOrders.all, 'list'] as const,
    list: (params: Record<string, unknown>) => [...queryKeys.salesOrders.lists(), params] as const,
    details: () => [...queryKeys.salesOrders.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.salesOrders.details(), id] as const,
  },
  currencies: {
    all: ['currencies'] as const,
    lists: () => [...queryKeys.currencies.all, 'list'] as const,
    list: (params?: Record<string, unknown>) => [...queryKeys.currencies.lists(), params] as const,
    detail: (id: string) => [...queryKeys.currencies.all, 'detail', id] as const,
  },
  settings: {
    all: ['settings'] as const,
    current: () => [...queryKeys.settings.all, 'current'] as const,
  },
};
