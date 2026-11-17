# Customer Sell action pane

The customer form uses the existing enterprise `ListDetailsPage` commands and shared ribbon component.
Only basic actions with implemented destinations are exposed. Permissions and saved-customer selection gate each destination; editing the customer disables ribbon commands through the shared page pattern.

| Group | Action | Behavior |
|---|---|---|
| New | Sales order | Opens `SalesOrderQuickCreate` with the selected customer and its payment, tax, currency, delivery, site and warehouse defaults. Persists only on Save through the existing sales-order API. |
| Related information | Orders > Sales orders | Opens the existing sales-order list filtered by the selected customer account. |
| Related information | Packing slips | Reuses `CustPackingSlipJournal` across the customer's orders. Loads posted journals through existing order-scoped endpoints, with at most five concurrent calls. Lines and order tracking use the selected journal's owning sales-order record ID. |
| Trade agreements | Trade agreement journals | Opens the existing global journal setup list. It is not a customer-filtered price agreement inquiry or a complete customer journal-line creation flow. |
| Set up | Price/discount groups | Opens the existing setup page when permitted. |
| Set up | Trade agreement journal names | Opens the existing setup page when permitted. |

No destination is fabricated for quotation creation/inquiries, leads, opportunities, sales agreements, customer price/discount inquiries, supplementary sales items, or unspecified Setup menu entries. A new customer-specific trade agreement requires a supported journal-line editor; the existing journal header list is not presented as that workflow.

Microsoft references:
- [Create and edit sales quotations](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/tasks/create-edit-sales-quotations): quotations have their own creation and follow-up flow; prices can come from valid trade agreements.
- [Sales agreements](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/sales-agreements): sales agreements represent purchasing commitments and have their own fulfillment and pricing behavior. They are distinct from trade agreement journal setup.

No order delivery, invoice posting, settlement, journal posting, schema change, or live database mutation is performed by these navigation actions.
