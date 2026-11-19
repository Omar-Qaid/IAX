# Customer Invoice action-pane readiness

Review date: 2026-11-16. The supplied image confirms the Invoice ribbon, not the layout of its destination forms.
The earlier user instruction requires confirming new D365 forms before creating them; the existing Customer enterprise pattern should be reused when those forms are confirmed.

| Action                                            | Verified implementation                                                                                                                                                                                                                                                          | Readiness                                                                                                                            |
| ------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| New > Free text invoice                           | `FreeTextInvoiceController` at `/api/v1/CustInvoiceTable` supports list, get, draft create/update/delete, correction and posting. `FreeTextInvoiceDtos.cs` exposes customer/invoice account, dates, currency, payment setup, posting profile, dimensions, tax and invoice lines. | Backend available; no frontend page or registered route. Need opened D365 form screenshots covering header and lines.                |
| Journals > Invoice journal                        | `CustInvoiceJour` is populated by sales/free-text posting. `CustInvoiceJourDto` has no invoice-history fields, and `SalesInvoiceController` disables inherited read actions with `NonAction`.                                                                                    | Needs a posted-invoice read projection/endpoint and a confirmed frontend inquiry. Do not link it to drafts or customer transactions. |
| Set up > Recurring invoices                       | No confirmed frontend/customer-template assignment endpoint pair found.                                                                                                                                                                                                          | Requires a separate template and customer recurrence workflow.                                                                       |
| Journals > Bill of exchange                       | No implemented frontend inquiry destination found.                                                                                                                                                                                                                               | Unavailable for this basic frontend wiring.                                                                                          |
| Related information > Period statistics           | No existing frontend period-statistics inquiry found.                                                                                                                                                                                                                            | Need the inquiry screenshot to establish period controls and measures. Do not invent statistics.                                     |
| Related information > COD outstanding             | No existing frontend COD inquiry found.                                                                                                                                                                                                                                          | Do not substitute all open customer balances for COD-specific records.                                                               |
| Related information > Bill of exchange statistics | No existing frontend inquiry destination found.                                                                                                                                                                                                                                  | Unavailable for this basic frontend wiring.                                                                                          |
| Prepayment > Apply                                | Settlement/prepayment backend logic exists, but no matching frontend Apply action/dialog or confirmed menu-item behavior.                                                                                                                                                        | Need the opened Apply dialog and its intended process before wiring a financial action.                                              |

The legacy `AR_INVOICES = /CustInvoiceJour` constant is not a registered page and must not be used as a working destination.
The disabled Invoice command and invoice-copy sections in SalesOrderDetailsPage are not a posted Invoice journal page.
No UI actions were enabled, posting logic changed, or database records modified during this readiness review.

## Intended shared implementation

- Use enterprise `ListDetailsPage` for the confirmed free-text invoice form, with existing shared line grids, lookups, permissions and company-scoped data sources.
- Reuse existing backend draft and posting services; creating a draft must remain separate from posting.
- Use the existing shared read-only list pattern for the confirmed posted Invoice journal, with a real company/customer-scoped `CustInvoiceJour` projection.
- Build the customer Invoice ribbon through the same shared action-pane components as Customer and Sell; expose only implemented, permission-authorized destinations.

## Primary references

- [Recurring invoices](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/set-up-process-recurring-invoices): templates, customer assignment, generation and posting are separate stages.
- [Bills of exchange](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/set-up-bills-exchange): journal inquiries and statistics follow the bill-of-exchange posting process and its setup.
- [Prepayments](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/prepayments-invoices-vs-prepayments): customer prepayments can be settled against invoices; this does not by itself identify the screenshot's Apply menu item.

## List implementation after the supplied list screenshot

`CustFreeInvoiceListPage` now uses the sales-order list enterprise pattern at `/accounts-receivable/free-text-invoices`. The customer Invoice ribbon has a Related information > Free text invoices list destination. This does not represent creating a new invoice or the complete posted Invoice journal. See [Free text invoice list scope](free-text-invoice-list.md).

## Details implementation after the supplied header-and-lines screenshot

The list New/Edit actions and invoice links now open `CustFreeInvoiceDetailsPage`, using the same enterprise master/details shell as SalesOrderDetailsPage. Supported drafts can be created, edited and saved; posting remains a separate confirmed action. See [Free text invoice details scope](free-text-invoice-details.md). The customer ribbon list link above remains list navigation.
