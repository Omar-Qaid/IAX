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

  WF_PROCESS_VIEW: 'Workflow.Processes.View',
  WF_PROCESS_CREATE: 'Workflow.Processes.Create',
  WF_PROCESS_EDIT: 'Workflow.Processes.Edit',
  WF_PROCESS_DELETE: 'Workflow.Processes.Delete',

  WF_CATEGORY_VIEW: 'Workflow.Categories.View',
  WF_CATEGORY_CREATE: 'Workflow.Categories.Create',
  WF_CATEGORY_EDIT: 'Workflow.Categories.Edit',
  WF_CATEGORY_DELETE: 'Workflow.Categories.Delete',

  WF_ACTIVITY_TYPE_VIEW: 'Workflow.ActivityTypes.View',
  WF_ACTIVITY_TYPE_CREATE: 'Workflow.ActivityTypes.Create',
  WF_ACTIVITY_TYPE_EDIT: 'Workflow.ActivityTypes.Edit',
  WF_ACTIVITY_TYPE_DELETE: 'Workflow.ActivityTypes.Delete',

  WF_CONTROL_VIEW: 'Workflow.Controls.View',
  WF_CONTROL_CREATE: 'Workflow.Controls.Create',
  WF_CONTROL_EDIT: 'Workflow.Controls.Edit',
  WF_CONTROL_DELETE: 'Workflow.Controls.Delete',

  WF_PRIORITY_VIEW: 'Workflow.Priorities.View',
  WF_PRIORITY_CREATE: 'Workflow.Priorities.Create',
  WF_PRIORITY_EDIT: 'Workflow.Priorities.Edit',
  WF_PRIORITY_DELETE: 'Workflow.Priorities.Delete',

  WF_VARIABLE_VIEW: 'Workflow.Variables.View',
  WF_VARIABLE_CREATE: 'Workflow.Variables.Create',
  WF_VARIABLE_EDIT: 'Workflow.Variables.Edit',
  WF_VARIABLE_DELETE: 'Workflow.Variables.Delete',

  LEGAL_ENTITY_VIEW: 'legalEntity.view',
  LEGAL_ENTITY_MANAGE: 'legalEntity.manage',

  SETTINGS_VIEW: 'settings.view',
  SETTINGS_UPDATE: 'settings.update',
} as const;

export type PermissionCode = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
