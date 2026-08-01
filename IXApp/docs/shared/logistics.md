# Logistics Components Documentation (`src/shared/components/logistics`)

## 1. Purpose and Responsibilities
The `logistics` sub-system provides Microsoft Dynamics 365 Finance & Operations-style slide-out side drawers for managing physical postal addresses (`LogisticsPostalAddressDrawer`) and electronic contact channels (`LogisticsElectronicAddressDrawer`). 

It includes cascading geography selection hooks ($\text{Country} \rightarrow \text{State} \rightarrow \text{City}/\text{County}$), date validity ranges (`validFrom`, `validTo`), primary flags, contact type pickers (`Phone`, `Email`, `URL`, `Fax`, `Telex`, `InstantMessage`), and bilingual LTR/RTL support.

---

## 2. Folder Structure
```text
src/shared/components/logistics/
├── LogisticsPostalAddressDrawer.tsx     # Physical postal address right-side drawer
├── LogisticsElectronicAddressDrawer.tsx # Electronic contact channel right-side drawer
└── index.ts                             # Public exports for logistics module
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` prefixed with `Logistics` (e.g., `LogisticsPostalAddressDrawer.tsx`, `LogisticsElectronicAddressDrawer.tsx`).
- **Interfaces:** `LogisticsPostalAddressDrawerProps`, `LogisticsElectronicAddressDrawerProps`.

---

## 4. Components & Drawers

### 4.1 `LogisticsPostalAddressDrawer`
Slide-out right drawer panel for creating or editing physical postal addresses.
- **Cascading Geography:** Selecting a `countryRegionId` automatically fetches states via `useStates(countryRegionId)`. Changing country clears `state`, `city`, and `county`. Selecting `state` fetches cities via `useCities(state)` and counties via `useCounties(state)`.
- **Defaults:** Automatically sets `validFrom` to today's date (`YYYY-MM-DD`) and `validTo` to `2154-12-31` matching D365 ERP validity rules.
- **Fields:** Description, Roles (`Business`, `Delivery`, `Invoice`, `Home`, `Payment`), Country, State, City, District, Street, Building, Zip Code, Post Box, County, Primary, Primary for Country.

### 4.2 `LogisticsElectronicAddressDrawer`
Slide-out right drawer panel for creating or editing electronic contact channels.
- **Types:** `Phone`, `Email`, `URL`, `Fax`, `Telex`, `InstantMessage`.
- **Fields:** Description, Type, Locator (phone number or email address), Extension, Primary switch.

---

## 5. Hooks & Integrations
Consumes custom logistics hooks from `@shared/hooks/useLogisticsAddress`:
- `useCountryRegions()`
- `useStates(countryRegionId)`
- `useCities(stateId)`
- `useCounties(stateId)`

---

## 6. Services & APIs
Connects to geography lookup endpoints or typed mock datasets (`@mocks/data/logistics.ts`).

---

## 7. State Management
Drawer state is managed locally via `useState`. Upon clicking Save, validated objects of type `LogisticsPostalAddress` or `LogisticsElectronicAddress` are passed to parent callback `onSave(address)`.

---

## 8. Design Patterns
- **Slide-Out Drawer Pattern:** Uses MUI `<Drawer anchor="right">` to allow editing addresses without leaving the current record page.
- **Cascading Dropdown Pattern:** Parent dropdown selection resets and triggers query fetches for dependent child dropdowns.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@shared/hooks/useLogisticsAddress`, `@shared/types/logistics`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Always validate mandatory fields (`description`, `countryRegionId`, `locator`) before executing `onSave`.
- Ensure icon buttons use specific path imports (`@mui/icons-material/Close`).

---

## 11. Do's and Don'ts
- **DO:** Reset dependent dropdown selections (`state`, `city`) when user changes `countryRegionId`.
- **DON'T:** Allow `validFrom` to be later than `validTo`.

---

## 12. Code Example
```tsx
<LogisticsPostalAddressDrawer
  open={isAddressDrawerOpen}
  initialData={selectedAddress}
  onClose={() => setIsAddressDrawerOpen(false)}
  onSave={(updatedAddress) => {
    handleSaveAddress(updatedAddress);
    setIsAddressDrawerOpen(false);
  }}
/>
```

---

## 13. Decision Rules & Checklist
- [ ] Is cascading reset logic verified when changing country?
- [ ] Are dates formatted cleanly in `YYYY-MM-DD` string format?
- [ ] Are mandatory field validation errors highlighted in red?
