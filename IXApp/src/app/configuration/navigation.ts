export interface ModuleNavLink {
    label: string;
    path?: string;
    icon?: string;
    expandable?: boolean;
    /** If set, link is only shown when user has View permission for this module+resource. Omit for always-visible links. */
    permission?: { module: string; resource: string };
}

export interface ModuleNavSection {
    id: string;
    title: string;
    links: ModuleNavLink[];
    bordered?: boolean;
}

export interface ModuleNavConfig {
    moduleId: string;
    label: string;
    icon: string;
    defaultPath: string;
    matchPath: string;
    sections: ModuleNavSection[];
}

export const MODULE_NAV_CONFIGS: Record<string, ModuleNavConfig> = {
    'mod-AccountsReceivable': {
        moduleId: 'mod-AccountsReceivable',
        label: 'nav.accounts_receivable',
        icon: 'receipt',
        defaultPath: '/accounts-receivable/customers',
        matchPath: '/accounts-receivable',
        sections: [
            {
                id: 'Customer',
                title: 'nav.customers',
                links: [
                    { label: 'nav.customers', path: '/accounts-receivable/customers', permission: { module: 'AccountsReceivable', resource: 'Customers' } },
                    { label: 'nav.customer_groups', path: '/accounts-receivable/customer-groups', permission: { module: 'AccountsReceivable', resource: 'CustomerGroups' } },
                    { label: 'nav.cust_transactions', path: '/accounts-receivable/transactions', permission: { module: 'AccountsReceivable', resource: 'Transactions' } },
                ],
            },
            {
                id: 'Reports',
                title: 'nav.reports',
                links: [
                    { label: 'nav.financial_reports', path: '/accounts-receivable/financial-reports' },
                ],
            },
            {
                id: 'SalesOrder',
                title: 'nav.orders',
                links: [
                    { label: 'nav.pos', path: '/accounts-receivable/pointOfSale', permission: { module: 'AccountsReceivable', resource: 'SalesOrders' } },
                    { label: 'nav.sales_order', path: '/accounts-receivable/sales-orders', permission: { module: 'AccountsReceivable', resource: 'SalesOrders' } },
                    { label: 'nav.returns_order' },
                    { label: 'Packing Slips', path: '/accounts-receivable/packing-slips', permission: { module: 'AccountsReceivable', resource: 'PackingSlips' } },
                ],
            },
            {
                id: 'SalesInvoices',
                title: 'nav.invoices',
                links: [
                    { label: 'nav.sales_invoices', path: '/accounts-receivable/invoices', permission: { module: 'AccountsReceivable', resource: 'Invoices' } },
                    { label: 'nav.shipped_orders' },
                    { label: 'nav.returns' },
                ],
            },
            {
                id: 'Payments',
                title: 'Payments',
                links: [
                    { label: 'Payment Journals', path: '/accounts-receivable/payments', permission: { module: 'AccountsReceivable', resource: 'Payments' } },
                ],
            },
            {
                id: 'inquiriesandreports',
                title: 'nav.inquiries_and_reports',
                links: [
                    { label: 'nav.sales_reports' },
                    { label: 'nav.order_analytics' },
                    { label: 'nav.customer_insights' },
                ],
            },
            {
                id: 'Setup',
                title: 'nav.setup',
                links: [
                    { label: 'erp.currencies', path: '/accounts-receivable/currencies', permission: { module: 'AccountsReceivable', resource: 'Currencies' } },
                    { label: 'Customer Posting Profiles', path: '/accounts-receivable/posting-profiles', permission: { module: 'AccountsReceivable', resource: 'PostingProfiles' } },
                    { label: 'Terms of payment', path: '/accounts-receivable/payment-terms', permission: { module: 'AccountsReceivable', resource: 'PaymTerm' } },
                ],
            },
        ],
    },
    'mod-AccountsPayable': {
        moduleId: 'mod-AccountsPayable',
        label: 'nav.accounts_payable',
        icon: 'payments',
        defaultPath: '/accounts-payable/vendors',
        matchPath: '/accounts-payable',
        sections: [
            {
                id: 'Vendors',
                title: 'nav.vendors',
                links: [
                    { label: 'nav.vendors', path: '/accounts-payable/vendors', permission: { module: 'AccountsPayable', resource: 'Vendors' } },
                    { label: 'nav.vendor_groups', path: '/accounts-payable/vendor-groups', permission: { module: 'AccountsPayable', resource: 'VendorGroups' } },
                ],
            },
            {
                id: 'PurchaseOrders',
                title: 'nav.orders',
                links: [
                    { label: 'nav.purchase_orders', path: '/accounts-payable/purchase-orders', permission: { module: 'AccountsPayable', resource: 'Vendors' } },
                ],
            },
            {
                id: 'Invoices',
                title: 'nav.invoices',
                links: [
                    { label: 'nav.vendor_invoices' },
                    { label: 'nav.payments' },
                ],
            },
            {
                id: 'Reports',
                title: 'nav.inquiries_and_reports',
                links: [
                    { label: 'nav.purchase_reports' },
                    { label: 'nav.vendor_analytics' },
                ],
            },
        ],
    },
    'mod-GeneralLedger': {
        moduleId: 'mod-GeneralLedger',
        label: 'nav.general_ledger',
        icon: 'ledger',
        defaultPath: '/foundation/currencies',
        matchPath: '/foundation',
        sections: [
            {
                id: 'Currencies',
                title: 'nav.currencies',
                links: [
                    { label: 'nav.currencies', path: '/foundation/currencies', permission: { module: 'GeneralLedger', resource: 'Currencies' } },
                    { label: 'nav.exchange_rate_types', path: '/erp-shared/exchange-rate-types', permission: { module: 'GeneralLedger', resource: 'ExchangeRateTypes' } },
                    { label: 'nav.currency_exchange_rates', path: '/erp-shared/currency-exchange-rates', permission: { module: 'GeneralLedger', resource: 'ExchangeRates' } },
                ],
            },
            {
                id: 'Setup',
                title: 'nav.setup',
                links: [
                    { label: 'nav.ledger_setup', path: '/erp-shared/ledger-setup' },
                    { label: 'nav.currency_setup', path: '/erp-shared/currency-setup' },
                ],
            },
        ],
    },
    'mod-Organization': {
        moduleId: 'mod-Organization',
        label: 'nav.organization',
        icon: 'corporate',
        defaultPath: '/organization/departments',
        matchPath: '/organization',
        sections: [
            {
                id: 'Company',
                title: 'nav.structure',
                links: [
                    { label: 'nav.departments', path: '/organization/departments', permission: { module: 'Organization', resource: 'Departments' } },
                    { label: 'nav.occupations', path: '/organization/occupations', permission: { module: 'Organization', resource: 'Occupations' } },
                    { label: 'nav.jobs', path: '/organization/jobs', permission: { module: 'Organization', resource: 'Jobs' } },
                    { label: 'nav.showrooms', path: '/organization/showrooms', permission: { module: 'Organization', resource: 'Showrooms' } },
                    { label: 'nav.hierarchy_classification', path: '/organization/hierarchy-classification', permission: { module: 'Organization', resource: 'Showrooms' } },
                    { label: 'nav.employees', path: '/organization/employees', permission: { module: 'Organization', resource: 'Employees' } },
                    { label: 'nav.announcements', path: '/organization/announcements', permission: { module: 'Organization', resource: 'Announcements' } },
                ],
            },
            {
                id: 'HR',
                title: 'nav.hr_metadata',
                links: [
                    { label: 'nav.nationalities', path: '/organization/nationalities', permission: { module: 'Organization', resource: 'Nationalities' } },
                    { label: 'nav.genders', path: '/organization/genders', permission: { module: 'Organization', resource: 'Genders' } },
                ],
            },
        ],
    },
    'mod-Inventory': {
        moduleId: 'mod-Inventory',
        label: 'nav.inventory',
        icon: 'inventory',
        defaultPath: '/inventory/items',
        matchPath: '/inventory',
        sections: [
            {
                id: 'Items',
                title: 'nav.items_management',
                links: [
                    { label: 'nav.items', path: '/inventory/items', permission: { module: 'Inventory', resource: 'Items' } },
                    { label: 'nav.item_groups', path: '/inventory/item-groups', permission: { module: 'Inventory', resource: 'ItemGroups' } },
                    { label: 'nav.uoms', path: '/inventory/uoms', permission: { module: 'Inventory', resource: 'UOM' } },
                    { label: 'nav.inventory_transactions', path: '/inventory/transactions', permission: { module: 'Inventory', resource: 'Transactions' } },
                    { label: 'nav.inventory_journals', path: '/inventory/journals', permission: { module: 'Inventory', resource: 'Journals' } },
                    { label: 'nav.pricing_sandbox', path: '/inventory/pricing-sandbox', permission: { module: 'Inventory', resource: 'Items' } },
                ],
            },
        ],
    },
    'mod-SystemAdministration': {
        moduleId: 'mod-SystemAdministration',
        label: 'nav.system_administration',
        icon: 'admin',
        defaultPath: '/system-administration/settings',
        matchPath: '/system-administration',
        sections: [
            {
                id: 'Users',
                title: 'nav.users',
                links: [
                    { label: 'nav.all_users', path: '/system-administration/users', permission: { module: 'SystemAdministration', resource: 'Users' } },
                    { label: 'nav.online_users', path: '/system-administration/online-users', permission: { module: 'SystemAdministration', resource: 'Users' } },
                    { label: 'nav.user_classification', path: '/system-administration/user-classification', permission: { module: 'SystemAdministration', resource: 'UserGroups' } },
                ],
            },
            {
                id: 'Security',
                title: 'nav.security',
                links: [
                    { label: 'nav.assign_users_to_roles', path: '/system-administration/assign-roles', permission: { module: 'SystemAdministration', resource: 'Roles' } },
                    { label: 'nav.permissions', path: '/system-administration/permissions', permission: { module: 'SystemAdministration', resource: 'Permissions' } },
                    { label: 'nav.settings', path: '/system-administration/settings' },
                ],
            },
            {
                id: 'Notifications',
                title: 'nav.notifications',
                links: [
                    { label: 'nav.notification_center', path: '/notifications/center' },
                    { label: 'nav.notification_settings', path: '/notifications/settings' },
                    { label: 'nav.notification_templates', path: '/notifications/templates' },
                    { label: 'nav.notification_dashboard', path: '/notifications/dashboard' },
                ],
            },
            {
                id: 'BatchJobs',
                title: 'nav.batch_jobs',
                links: [
                    { label: 'nav.background_jobs', path: '/background-jobs' },
                    { label: 'nav.jobs_dashboard', path: '/background-jobs/dashboard' },
                ],
            },
            {
                id: 'System',
                title: 'nav.system',
                links: [
                    { label: 'nav.chat', path: '/chat' },
                ],
            },
        ],
    },
    'mod-Workflow': {
        moduleId: 'mod-Workflow',
        label: 'nav.workflow',
        icon: 'workflow',
        defaultPath: '/workflow/processes',
        matchPath: '/workflow',
        sections: [
            {
                id: 'ProcessManagement',
                title: 'nav.workflow',
                links: [
                    { label: 'nav.processes', path: '/workflow/processes', permission: { module: 'Workflow', resource: 'Processes' } },
                    { label: 'nav.process_builder', path: '/workflow/process-builder', permission: { module: 'Workflow', resource: 'ProcessBuilder' } },
                    { label: 'nav.steps', path: '/workflow/steps', permission: { module: 'Workflow', resource: 'Steps' } },
                    { label: 'nav.activities', path: '/workflow/activities', permission: { module: 'Workflow', resource: 'Activities' } },
                    { label: 'nav.transitions', path: '/workflow/transitions', permission: { module: 'Workflow', resource: 'Transitions' } },
                    { label: 'nav.requests', path: '/workflow/requests', permission: { module: 'Workflow', resource: 'Requests' } },
                ],
            },
            {
                id: 'Configuration',
                title: 'nav.configuration',
                links: [
                    { label: 'nav.categories', path: '/workflow/categories', permission: { module: 'Workflow', resource: 'Categories' } },
                    { label: 'nav.priorities', path: '/workflow/priorities', permission: { module: 'Workflow', resource: 'Priorities' } },
                    { label: 'nav.performers', path: '/workflow/performers', permission: { module: 'Workflow', resource: 'Performers' } },
                    { label: 'nav.variables', path: '/workflow/variables', permission: { module: 'Workflow', resource: 'Variables' } },
                    { label: 'nav.controls', path: '/workflow/controls', permission: { module: 'Workflow', resource: 'Controls' } },
                    { label: 'nav.activity_controls', path: '/workflow/activity-controls', permission: { module: 'Workflow', resource: 'ActivityControls' } },
                    { label: 'nav.request_controls', path: '/workflow/request-controls', permission: { module: 'Workflow', resource: 'RequestControls' } },
                    { label: 'nav.activity_types', path: '/workflow/activity-types', permission: { module: 'Workflow', resource: 'ActivityTypes' } },
                    { label: 'nav.operators', path: '/workflow/operators', permission: { module: 'Workflow', resource: 'Operators' } },
                ],
            },
            {
                id: 'Security',
                title: 'nav.security',
                links: [
                    { label: 'nav.user_groups', path: '/workflow/user-groups' },
                    { label: 'nav.user_categories', path: '/workflow/user-categories' },
                ],
            },
            {
                id: 'System',
                title: 'nav.system',
                links: [
                    { label: 'nav.audit_logs', path: '/system/audit', permission: { module: 'System', resource: 'AuditLog' } },
                    { label: 'nav.number_sequences', path: '/system/number-sequences', permission: { module: 'System', resource: 'NumberSequences' } },
                ],
            },
        ],
    },
};
