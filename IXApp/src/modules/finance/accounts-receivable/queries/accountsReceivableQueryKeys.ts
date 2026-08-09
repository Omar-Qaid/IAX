export const customerQueryKeys = {
  all: ['customers'] as const,
  lists: () => [...customerQueryKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...customerQueryKeys.lists(), params] as const,
  details: () => [...customerQueryKeys.all, 'detail'] as const,
  detail: (id: string) => [...customerQueryKeys.details(), id] as const,
};

export const customerGroupQueryKeys = {
  all: ['customerGroups'] as const,
  lists: () => [...customerGroupQueryKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) => [...customerGroupQueryKeys.lists(), params] as const,
  detail: (id: string) => [...customerGroupQueryKeys.all, 'detail', id] as const,
};

export const salesOrderQueryKeys = {
  all: ['salesOrders'] as const,
  lists: () => [...salesOrderQueryKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...salesOrderQueryKeys.lists(), params] as const,
  details: () => [...salesOrderQueryKeys.all, 'detail'] as const,
  detail: (id: string) => [...salesOrderQueryKeys.details(), id] as const,
};
