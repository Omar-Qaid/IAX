# Logistics drawers

`LogisticsPostalAddressDrawer` and `LogisticsElectronicAddressDrawer` edit typed address/contact values without leaving the current record page.

The postal drawer supports roles, validity dates, primary flags, and cascading country/state/city/county selections through hooks in `shared/hooks/useLogisticsAddress.ts`. Changing a parent geography resets dependent values. The electronic drawer supports configured address types including phone, email, URL, fax, telex, and instant message.

The parent owns persistence through `onSave`; the drawers do not call a domain endpoint. Geography data currently comes from `shared/services/logisticsAddressMockData.ts` through the shared hook, not from `src/mocks/data/logistics.ts` (that file does not exist).

Validate required locator/address values and date ordering before accepting save. Verify both LTR and RTL drawer placement/content, and keep primary-address business constraints in the owning module.
