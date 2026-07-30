/**
 * Application-wide constants.
 */

/** Layout dimensions */
export const LAYOUT = {
    TOPBARHEIGHT: 40,
    SIDEBARWIDTH: 260,
    SIDEBARMINIWIDTH: 48,
    DRAWER_WIDTH: 380,
    SETTINGS_WIDTH: 340,
} as const;

/** Breakpoint values (matching MUI defaults) */
export const BREAKPOINTS = {
    xs: 0,
    sm: 600,
    md: 900,
    lg: 1200,
    xl: 1536,
} as const;

/** Z-index layers */
export const Z_INDEX = {
    sidebar: 1200,
    topBar: 1201,
    drawer: 1200,
    modal: 1300,
    tooltip: 1500,
} as const;

/** Common color tokens */
export const COLORS = {
    primary: '#0078d4',
    primaryHover: '#106ebe',
    textPrimary: '#1e293b',
    textSecondary: '#64748b',
    textMuted: '#94a3b8',
    border: '#edebe9',
    borderHover: '#cbd5e1',
    bgPage: '#f5f5f5',
    bgCard: '#ffffff',
    bgHover: '#f8fafc',
    success: '#059669',
    successBg: '#ecfdf5',
    warning: '#d97706',
    warningBg: '#fffbeb',
    error: '#dc2626',
    errorBg: '#fef2f2',
    info: '#2563eb',
    infoBg: '#eff6ff',
} as const;

/**
 * Centralized accent/status palette used by feature dashboards and charts.
 * This is a fixed (Tailwind-derived) palette intentionally independent of the
 * theme presets. Reference these tokens instead of hardcoding hex literals.
 */
export const PALETTE = {
    blue: '#3b82f6',
    indigo: '#6366f1',
    emerald: '#10b981',
    green: '#16a34a',
    red: '#ef4444',
    amber: '#f59e0b',
    orange: '#f97316',
    sky: '#0ea5e9',
    violet: '#7c3aed',
    // Neutral / slate scale
    ink: '#1e293b',
    slate: '#64748b',
    slateMuted: '#94a3b8',
    border: '#e2e8f0',
    borderStrong: '#cbd5e1',
    surfaceMuted: '#f8fafc',
    surfaceSubtle: '#f1f5f9',
} as const;

/** Pagination defaults */
export const PAGINATION = {
    defaultPageSize: 25,
    pageSizeOptions: [10, 25, 50, 100],
} as const;

/** Animation durations (ms) */
export const ANIMATION = {
    fast: 150,
    normal: 250,
    slow: 350,
} as const;

// ─── API Endpoints ───────────────────────────────────────────────────────────
/** Centralized API route constants — prevents typos and enables global rename. */
export const API_ENDPOINTS = {
    LOGIN: '/Auth/login',
    ME: '/Auth/me',
    REFRESH_TOKEN: '/Auth/refresh-token',
    EXTERNAL_PROVIDERS: '/Auth/external-providers',
    AR_CUSTOMERS: '/Customer',
    AR_CUSTOMER_GROUPS: '/CustomerGroup',
    AR_CURRENCIES: '/Currency',
    AR_PAYMENT_TERMS: '/PaymTerm',
    AR_EXCHANGE_RATES: '/CurrencyExchangeRate',
    AR_SALES_ORDERS: '/SalesOrder',
    AR_SALES_LINES: '/SalesLine',
    AR_INVOICES: '/CustInvoiceJour',
    AR_INVOICE_LINES: '/CustInvoiceTrans',
    CUST_TRANS: '/CustTrans',
    CUST_SETTLEMENT: '/CustSettlement',
    CUSTOMER_PAYMENTS: '/CustomerPayment',
    CUST_LEDGER: '/CustLedger',
    CUST_LEDGER_ACCOUNTS: '/CustLedgerAccounts',
    INVENTORY_ONHAND: '/Inventory/onhand',
    INVENTORY_TRANSACTIONS: '/InventTransaction',
    AP_VENDORS: '/Vendor',
    AP_VENDOR_GROUPS: '/VendorGroup',
    AP_PURCH_ORDERS: '/PurchTable',
    AUDIT_LOG: '/SysAuditLog',
    COMPANIES: '/OrgCompany',
    DEPARTMENTS: '/OrgDepartment',
    OCCUPATIONS: '/OrgOccupation',
    NATIONALITIES: '/OrgNationality',
    GENDERS: '/OrgGender',
    EMPLOYEES: '/OrgEmployee',
    ANNOUNCEMENTS: '/OrgAnnouncement',
    ATTACHMENTS: '/OrgAttachment',
    INVENT_ITEMS: '/InventItem',
    INVENT_ITEM_GROUPS: '/InventItemGroup',
    INVENT_UOMS: '/InventUOM',
    INVENT_TRANSACTIONS: '/InventTransaction',
    INVENT_JOURNALS: '/InventJournal',
    WF_CATEGORIES: '/WfCategory',
    WF_PRIORITIES: '/WfPriority',
    WF_PROCESSES: '/WfProcess',
    WF_STEPS: '/WfStep',
    WF_ACTIVITY_TYPES: '/WfActivityType',
    WF_ACTIVITIES: '/WfActivity',
    WF_ACTIVITY_CONTROLS: '/WfActivityControl',
    WF_OPERATORS: '/WfOperator',
    WF_STATUSES: '/WfStatus',
    WF_CONTROLS: '/WfControl',
    WF_PERFORMERS: '/WfPerformer',
    WF_REQUESTS: '/WfRequest',
    WF_REQUEST_CONTROLS: '/WfRequestControl',
    WF_TRANSITIONS: '/WfTransition',
    WF_VARIABLES: '/WfVariable',
    WF_DATA_TYPES: '/WfDataType',
    WF_REQUEST_MAPPING_VARIABLES: '/WfRequestMappingVariable',
    WF_ACTIVITY_MAPPING_VARIABLES: '/WfActivityMappingVariable',
    WF_REQUEST_CONTROLS_VALIDATIONS: '/WfRequestControlsValidation',
    WF_ACTIVITY_CONTROLS_VALIDATIONS: '/WfActivityControlsValidation',
    WF_REQUEST_CONTROL_OPTIONS: '/WfRequestControlsOption',
    WF_ACTIVITY_CONTROL_OPTIONS: '/WfActivityControlsOption',
    USERS: '/User',
    ROLES: '/Role',
    JOBS: '/OrgJob',
    SHOWROOMS: '/OrgShowroom',
    MANAGEMENT_LEVELS: '/OrgManagementLevel',
    EMPLOYEE_MANAGERS: '/OrgEmployeeManager',
    USER_GROUPS: '/OrgEmployeeGroup',
    USER_CATEGORIES: '/OrgEmployeeCategory',
    NUMBER_SEQUENCES: '/SysNumberSequence',
    PERMISSIONS: '/AppPermission',
    NOTIFICATIONS: '/SysNotification',
    NOTIFICATION_TEMPLATES: '/SysNotificationTemplate',
    NOTIFICATION_PREFERENCES: '/SysNotificationPreference',
    BACKGROUND_JOBS: '/SysBackgroundJob',
    CHAT: '/SysChat',
} as const;

/** SignalR / WebSocket hub paths (appended to the server root URL, not the API base URL). */
export const REALTIME = {
    HUB_PATH: '/hubs/realtime',
} as const;


// ─── Application Config ──────────────────────────────────────────────────────
/** Runtime config values referenced across the app. */
export const APP_CONFIG = {
    /** Server-side pagination page size */
    PAGE_SIZE: 100,
    INITIAL_PAGE_SIZE: 20,
    NEXT_PAGE_SIZE: 10,
    /** Default toast notification duration (ms) */
    TOAST_DURATION: 4000,
} as const;

/** Centralized grid filter operators for UI display and internal logic. */
export const GRID_FILTER_OPERATORS = [
    { label: 'is exactly', value: 'equals' },
    { label: 'is not', value: 'notEquals' },
    { label: 'contains', value: 'contains' },
    { label: 'does not contain', value: 'doesNotContain' },
    { label: 'begins with', value: 'startsWith' },
    { label: 'ends with', value: 'endsWith' },
    { label: 'is one of', value: 'in' },
    { label: 'after', value: 'gt' },
    { label: 'before', value: 'lt' },
    { label: 'matches', value: 'matches' },
] as const;
