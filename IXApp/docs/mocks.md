# Mock Services & Data Documentation (`src/mocks`)

## 1. Purpose and Responsibilities
The `mocks` layer provides typed mock datasets and in-memory mock service repositories for **IXApp**. It enables complete offline development, UI prototyping, and automated unit testing without requiring an active ASP.NET Core REST API backend.

Mock services implement the exact same TypeScript contracts as production API services, allowing seamless transitions when toggling `VITE_ENABLE_MOCK_API=true` or `false`.

---

## 2. Folder Structure
```text
src/mocks/
├── data/                      # Realistic domain mock datasets
│   ├── currencies.ts          # MOCK_CURRENCIES (USD, EUR, SAR, AED, GBP)
│   ├── customerGroups.ts      # MOCK_CUSTOMER_GROUPS (Major, Retail, Wholesale)
│   ├── customers.ts           # MOCK_CUSTOMERS (Contoso, Fabrikam, Northwind)
│   ├── logistics.ts           # MOCK_COUNTRY_REGIONS, MOCK_STATES, MOCK_CITIES, MOCK_COUNTIES
│   └── salesOrders.ts         # MOCK_SALES_ORDERS & line items
└── repositories/              # Stateful in-memory mock repositories
    ├── mockCurrencyRepository.ts
    ├── mockCustomerGroupRepository.ts
    ├── mockCustomerRepository.ts
    └── mockSalesOrderRepository.ts
```

---

## 3. Mock Service Contract Pattern
Features interact with services behind a single interface. The service layer resolves mock or HTTP mode dynamically:

```ts
// Service Resolver Pattern
export const currencyService = {
  getPaged: async (params: PaginationParameters): Promise<PagedResult<Currency>> => {
    if (environment.enableMockApi) {
      await mockDelay(300); // Simulate network latency
      return mockCurrencyRepository.getPaged(params);
    }
    const { data } = await apiClient.get<PagedResult<Currency>>('/currencies', { params });
    return data;
  },
  
  getById: async (id: string | number): Promise<Currency | null> => {
    if (environment.enableMockApi) {
      return mockCurrencyRepository.getById(id);
    }
    const { data } = await apiClient.get<Currency>(`/currencies/${id}`);
    return data;
  },
};
```

---

## 4. In-Memory Mock Repository Capabilities
Mock repositories maintain state during the application session:
- **Filtering & Search:** Case-insensitive search across code and description fields.
- **Pagination:** Slices data array based on `pageNumber` and `pageSize`, returning total records and total pages.
- **CRUD Operations:** Adds new records with auto-generated IDs, updates existing entities, and removes deleted records.
- **Latency Simulation:** Includes optional `mockDelay(ms)` helper to test loading indicators and button loading states.

---

## 5. Environment Configuration
Mock mode is controlled via environment variables in `.env` / `.env.development`:
```env
VITE_ENABLE_MOCK_API=true
```

When set to `true`, services route requests to in-memory mock repositories. When set to `false`, services call live ASP.NET Core REST API endpoints via `apiClient`.

---

## 6. Best Practices & Rules
- **Contract Parity:** Mock datasets must strictly match domain interfaces (`Customer`, `SalesOrder`, `LogisticsPostalAddress`).
- **Realistic Data:** Use realistic enterprise names, ISO country codes, valid dates, and formatted currency symbols instead of generic strings like `"test1"`, `"test2"`.
- **Immutability:** Repository functions should return cloned object copies to prevent unintended mutation of initial mock arrays.

---

## 7. Common Mistakes
- **Mistake:** Exporting raw mutable arrays directly and modifying them inside page components.
- **Correction:** Route all mutations through mock repository methods (`mockCustomerRepository.save(data)`) to ensure dirty state and table refreshes behave realistically.

---

## 8. Decision Rules & Checklist
- [ ] Does the mock record fulfill all mandatory fields of the entity interface?
- [ ] Is `mockDelay()` included to test loading spinners?
- [ ] Does `getPaged()` calculate `totalPages` and `hasNextPage` accurately?
