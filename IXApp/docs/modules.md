# Business Modules Layer Documentation (`src/modules`)

## 1. Purpose and Responsibilities
The `modules` layer contains the domain-specific business features for **IXApp**. Each module owns its domain pages, business components, domain hooks, feature services, validation schemas, and TypeScript interfaces.

Modules compose reusable page patterns from `@patterns` and generic UI components from `@shared` to deliver complete enterprise business capabilities.

---

## 2. Folder Structure
```text
src/modules/
├── index.ts                   # Centralized module public exports
├── auth/                      # Authentication module
│   └── pages/
│       └── LoginPage.tsx      # Login view component
├── dashboard/                 # Enterprise Dashboard workspace module
│   └── pages/
│       └── DashboardPage.tsx  # Workspace pattern dashboard
├── accounts-receivable/       # Accounts Receivable domain module
│   ├── index.ts               # Accounts Receivable module exports
│   ├── currencies/            # Currencies setup
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── pages/CurrenciesPage.tsx
│   │   ├── services/currencyService.ts
│   │   └── types/currency.ts
│   ├── customer-groups/       # Customer Groups setup
│   │   ├── pages/CustomerGroupListPage.tsx
│   │   └── services/customerGroupService.ts
│   ├── customers/             # Customers master data
│   │   ├── components/CustomerForm.tsx
│   │   ├── pages/CustomerListPage.tsx
│   │   └── services/customerService.ts
│   └── sales-orders/          # Sales Orders document processing
│       ├── pages/SalesOrdersPage.tsx
│       ├── pages/SalesOrderPage.tsx
│       └── services/salesOrderService.ts
└── system-administration/     # System setup & settings module
    └── pages/ApplicationSettingsPage.tsx
```

---

## 3. Standard Feature Folder Layout
Every feature within a business module follows this internal structure:
```text
feature-name/
├── components/                # Domain-specific components
├── hooks/                     # Custom React Query & form hooks
├── pages/                     # Feature page components
├── services/                  # Feature API service functions
├── validation/                # Zod schema definitions
├── types/                     # Feature TypeScript interfaces
├── constants/                 # Feature constants & query keys
└── index.ts                   # Public feature exports
```

---

## 4. Representative Modules & Pages

### 4.1 Accounts Receivable (`@modules/accounts-receivable`)
- **`CurrenciesPage.tsx`:** Validates `SimpleListPage` pattern. Manages Currency code, Name, Symbol, Decimals, and Active state with page-level Save/Cancel.
- **`CustomerGroupListPage.tsx`:** Manages Customer Group ID, Name, Default Currency, and Payment Terms using the Currency lookup.
- **`CustomerListPage.tsx`:** Implements the dense customer list pattern with command actions, field-aware filtering, and a bilingual customer grid.
- **`SalesOrdersPage.tsx` & `SalesOrderPage.tsx`:** Validates `DocumentPage` pattern. Features order header form, lines DataGrid with real-time totals calculation, and process actions (Confirm, Post, Cancel).

### 4.2 Dashboard (`@modules/dashboard`)
- **`DashboardPage.tsx`:** Validates `WorkspacePage` pattern. Displays KPI summary tiles (Total Customers, Open Sales Orders, Monthly Sales, Overdue Balance), recent orders DataGrid, and quick links.

### 4.3 System Administration (`@modules/system-administration`)
- **`ApplicationSettingsPage.tsx`:** Validates `MasterFormPage` pattern. Features tabbed setup sections for General settings, Localization, UI Preferences, and API Configuration.

---

## 5. Domain Service Architecture
Module services wrap API calls or mock repositories behind a clean contract:
```ts
export const customerService = {
  getPaged: async (params: PaginationParameters): Promise<PagedResult<Customer>> => {
    if (environment.enableMockApi) return mockCustomerRepository.getPaged(params);
    const { data } = await apiClient.get<PagedResult<Customer>>('/customers', { params });
    return data;
  },
  getById: async (id: string | number): Promise<Customer> => {
    if (environment.enableMockApi) return mockCustomerRepository.getById(id);
    const { data } = await apiClient.get<Customer>(`/customers/${id}`);
    return data;
  },
  save: async (customer: Customer): Promise<Customer> => {
    if (environment.enableMockApi) return mockCustomerRepository.save(customer);
    const { data } = await apiClient.post<Customer>('/customers', customer);
    return data;
  },
};
```

---

## 6. Dependency & Isolation Rules
- **Allowed Dependencies:** `@modules` $\rightarrow$ `@patterns`, `@shared`, `@core`.
- **Forbidden Dependencies:** 
  - `@modules` must **never** be imported by `@shared`, `@patterns`, or `@core`.
  - **Cross-Module Prohibition:** One business module must **never** import directly from another business module (`accounts-receivable` must not import from `accounts-payable`).

---

## 7. Cross-Module Communication
When a feature in Module A requires data or services from Module B:
1. Move the shared type or contract to `@core/types` or `@shared/types`.
2. Move the shared lookup service to `@shared/services` or resolve via backend REST API endpoints.

---

## 8. Best Practices
- Every module must export its public interface via `index.ts`.
- Page components inside modules should remain thin orchestrators that delegate rendering to `@patterns` and `@shared` components.
- Domain validation schemas must be defined with Zod inside `@modules/module-name/validation/`.

---

## 9. Do's and Don'ts
- **DO:** Keep domain logic and feature services inside their respective module folders.
- **DO:** Use `environment.enableMockApi` inside services to support seamless switching to backend endpoints.
- **DON'T:** Create direct cross-module imports between business features.
- **DON'T:** Put raw Axios calls inside React page components.

---

## 10. Decision Rules & Checklist
- [ ] Is the feature placed inside the correct domain module folder?
- [ ] Are public feature exports declared in `index.ts`?
- [ ] Does the service fallback cleanly to mock data when `VITE_ENABLE_MOCK_API=true`?
- [ ] Are Zod validation schemas typed and aligned with entity interfaces?

---

## 11. Performance Considerations
- Module pages are lazy-loaded in `routeConfig.tsx` to ensure code-splitting per business area.
- Query invalidation in domain hooks is strictly scoped to the feature's query key prefix (e.g. `['customers']`).
