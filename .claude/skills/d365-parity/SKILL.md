---
name: d365-parity
description: Build any Finance feature in IXApi as a BASIC-parity replica of the matching D365 F&O cycle — ERP essentials only, standard D365 table and field names, C# services that implement the documented behavior, no invented tables and no custom logic, no advanced modules. Use whenever work touches IXApi/src/Modules/Finance (posting, journals, invoices, payments, settlement, period close, inventory) or whenever a D365 cycle document asks for a cycle to be implemented.
---

# Basic D365 parity inside IXApi

IXApi is .NET, not D365. The rule is still: **implement the same cycle D365 implements**, with
D365's standard tables and fields, and C# services that carry out the documented behavior.

**Target basic D365 only — ERP essentials.** The standard happy path of each core cycle, working
correctly end to end. Not every option, not the advanced modules. "Basic" is the ceiling, not a
first step toward building everything: do not add an advanced feature because the code is nearby
or it would "complete" the module.

## The two hard prohibitions

1. **No tables of your own.** Use the D365 table that already owns the data, with its D365 name
   and field names (`PurchTable`, `SalesLine`, `CustTrans`, `VendTransOpen`, `InventTrans`,
   `InventTransOrigin`, `InventSum`, `LedgerJournalTable`, `LedgerJournalTrans`,
   `GeneralJournalEntry`, `GeneralJournalAccountEntry`, `SubledgerVoucherGeneralJournalEntry`,
   `DimensionAttributeValueCombination`, `LedgerTransSettlement`, …). If a cycle needs a table that
   does not exist yet in IXApi, add it under its **real D365 name with its real fields** — never a
   convenience table, a flat "state" table, or a helper column invented to make code easier.
2. **No logic of your own.** Behavior comes from the documented D365 process: posting profiles,
   fiscal period status, posting types, settlement rules, costing model. Do not substitute a
   simpler scheme because it is easier to code.

When something is genuinely ambiguous, say so and ask — do not fill the gap with an invention.

## What "basic" includes

| Area | Basic scope |
|---|---|
| Foundation | Legal entity, currencies and exchange rates, fiscal calendar and ledger period status, number sequences, payment terms, basic sales tax codes/groups and ledger posting groups |
| General ledger | Chart of accounts and main accounts, financial dimensions and ledger combinations, journal names, general journal entry and **posting**, posted entries and voucher association, reversal of a posted journal, trial balance read, period open/on hold |
| Accounts receivable | Customer, sales order, confirmation, packing slip, invoice, credit note/return, customer payment and settlement, customer transactions and open balances |
| Accounts payable | Vendor, purchase order, confirmation, product receipt, invoice, return, vendor payment and settlement, vendor transactions and open balances |
| Inventory | Items and released products, inventory dimensions, on-hand, reservation and picking, inventory journals (movement/adjustment/transfer/count), one costing model (FIFO or weighted average) and inventory close |

## What is out of basic scope

Do **not** build these unless the user explicitly asks for that feature by name:

ledger allocation rules, accrual schemes, foreign-currency revaluation, intercompany accounting,
interunit/balancing dimensions, consolidation and elimination, budgeting and budget control,
posting layers other than `Current`, year-end close, fixed assets, production and manufacturing,
advanced warehousing (WMS), catch weight, asset leasing, revenue recognition (RevRec),
subscription billing, 1099, localization extensions (e.g. ZATCA e-invoicing), bank
reconciliation and statement matching, workflow approval engines, financial reporting designers.

When a basic path meets one of these — a non-`Current` layer, a foreign currency needing
conversion, journal tax, an intercompany line — **refuse it with a message naming what is
missing**. Never approximate an advanced feature inside a basic path.

## How to work

1. **Find the cycle's documentation first.** Project cycle docs live in `IXApi/docs/` and
   `IXApi/src/Modules/Finance/*.md` (e.g. `D365_FO_Core_General_Ledger_Full_Cycle.md`). Read the
   scope section before planning: what the document **excludes** is out of scope, and adjacent
   work that looks useful is not a substitute for what was asked.
2. **Check what exists** before adding. Many entities, DTOs, validators and configurations are
   already in place; the gap is usually the service that performs the process.
3. **Follow the module's seven-folder feature layout**:
   `Entities / Configuration / Dtos / Validation / Interfaces / Services / Controllers`
   (architecture tests enforce it). A new entity goes in `Entities` with its
   `IEntityTypeConfiguration` (auto-discovered via `ApplyConfigurationsFromAssembly`), and needs a
   migration plus the model snapshot.
4. **Post only through the shared engine.** `Common/Posting/SubledgerPostingSupport.cs` is this
   project's equivalent of D365's posting framework: ledger/period resolution
   (`LoadLedgerContextAsync`), posting profiles `Table → Group → All`
   (`ResolveCustomerSummaryAsync`, `ResolveVendorSummaryAsync`, `ResolveInventPostingAsync`),
   item model group policy (`ResolveItemModelPolicyAsync`, `PostsPhysicalAsync`), tax resolution
   and allocation, dimension validation (`LoadDimensionAsync`), entry construction (`NewEntry`,
   `AccountEntry`) and the `sp_getapplock` concurrency lock (`LockAsync`).
   **Never hand-build `GeneralJournalEntry` / `GeneralJournalAccountEntry` anywhere else**, and
   never duplicate the lock, the period check or the profile resolution in a service.
5. **Resolve accounts from configuration, never from constants.** No hard-coded account numbers,
   no "default" account fallback. A missing posting profile is a 422, not a guess.
6. **Reject what is not implemented; never approximate it.** If the path does not do FX
   conversion, intercompany, tax calculation or a non-Ledger account type, throw
   `SubledgerPostingException` with a message naming what is missing. An approximated posting is
   worse than a refused one.
7. **Every posting runs inside one transaction with its lock**, under
   `Database.CreateExecutionStrategy()`, rolling back and clearing the change tracker on failure.
   Map `SubledgerPostingException` to 422.
8. **Verify the voucher balances on the posted signed amounts**, not on the entered lines.

## Checks before reporting done

- `dotnet build IXApi/IXApi.csproj` → 0 errors.
- `dotnet test IXApi/Tests/IXApi.Tests.csproj` and `IXApi/Tests/InventoryCycle.Tests.csproj`.
- Report pre-existing failures as pre-existing, with the evidence that they are unrelated.
- State plainly what of the cycle is done and what is not. Partial parity reported as complete is
  the failure mode this skill exists to prevent.
