export const PERMISSIONS = {
  DASHBOARD_VIEW: 'dashboard.view',
  
  CUSTOMER_VIEW: 'customer.view',
  CUSTOMER_CREATE: 'customer.create',
  CUSTOMER_UPDATE: 'customer.update',
  CUSTOMER_DELETE: 'customer.delete',
  
  CUSTOMER_GROUP_VIEW: 'customerGroup.view',
  CUSTOMER_GROUP_MANAGE: 'customerGroup.manage',
  
  SALES_ORDER_VIEW: 'salesOrder.view',
  SALES_ORDER_CREATE: 'salesOrder.create',
  SALES_ORDER_UPDATE: 'salesOrder.update',
  SALES_ORDER_CONFIRM: 'salesOrder.confirm',
  SALES_ORDER_POST: 'salesOrder.post',
  
  CURRENCY_VIEW: 'currency.view',
  CURRENCY_MANAGE: 'currency.manage',

  LEGAL_ENTITY_VIEW: 'legalEntity.view',
  LEGAL_ENTITY_MANAGE: 'legalEntity.manage',
  
  SETTINGS_VIEW: 'settings.view',
  SETTINGS_UPDATE: 'settings.update',
} as const;

export type PermissionCode = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
