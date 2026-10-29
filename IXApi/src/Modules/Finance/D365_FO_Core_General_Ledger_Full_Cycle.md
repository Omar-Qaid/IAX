# D365 F&O Core General Ledger Full Cycle

Functional implementation and technical table impact guide

Prepared for Omar Hamood Saeed Qaid  
Reference date 28 October 2026

This guide treats “general ledger alignment” as the alignment of business accounting, D365 Finance configuration, journal processing, posted accounting data, reconciliation, and reporting. It is a blueprint and a sandbox training lab, not a record of changes made to an environment. Warehousing, inventory costing, production, and manufacturing are outside scope. The accounting examples are designed for a small service company.

The core lab uses only ledger accounts. Bank, tax, customer, vendor, fixed asset, budget, and group consolidation integrations are identified separately. A journal that debits a bank main account is not a bank subledger transaction. A ledger receivable is not a customer invoice. These distinctions determine which tables and reconciliation processes are affected.

Microsoft Learn references appear as [S01] and similar identifiers, with links at the end. Example account numbers, journal names, amounts, approval thresholds, and test cases are proposed lab designs. They are not Microsoft defaults. Menu labels vary by version and role; use navigation search if a path differs. Public CDM schemas describe historical application metadata, not a complete contract for every current release. Confirm fields, relations, enums, feature states, and extension behavior in the AOT for your target build.

## 1 Scope and business alignment

Build the base in this sequence: legal entity and calendar, currencies, chart of accounts, dimensions and account structures, ledger, journal controls, opening balances, daily postings, adjustments, reconciliation, period close, year end, and next year validation. A legal entity has one ledger; posting layers support additional accounting adjustments within that ledger. [S01, S02]

| Business question | D365 configuration or process | Required evidence |
|---|---|---|
| Which company owns this transaction | Legal entity and ledger | Correct company and ledger on posted entry |
| What happened economically | Main account and posting type | Account classification agrees with accounting policy |
| Which department incurred it | Financial dimension | Posted dimension agrees with responsibility |
| Which month owns the cost | Accounting date and accrual process | Correct period and reversal schedule |
| Who can approve and post | Security and journal workflow | Approval history and posting responsibility |
| Which currency is authoritative | Ledger currencies and rate types | Transaction and accounting amounts reconcile |
| Is a clearing balance explained | Ledger settlement and supporting schedule | Unsettled items equal account balance |
| Is the month complete | Close tasks and ledger calendar | Reconciliations signed off and period restricted |

Core acceptance is not simply “journal posts.” Each scenario must produce the expected account, amount, date, dimension, currency, layer, document reference, audit evidence, and report result.

## 2 Accounting fundamentals

Assets = Liabilities + Equity. Profit = Revenue − Expenses. Debits increase assets and expenses; credits increase liabilities, equity, and revenue. Reversals and correction postings can use different debit and credit presentations, so do not identify direction solely by a positive number on a report.

| Term | Meaning | Example |
|---|---|---|
| Legal entity | Accounting company | GL01 |
| Ledger | Company's accounting configuration | GL01 ledger in SAR |
| Chart of accounts | Main account catalogue | CORE |
| Main account | Economic classification | 611000 Rent expense |
| Financial dimension | Reporting classification | Department ADMIN |
| Ledger account | Main account with dimension values | 611000 ADMIN |
| Journal name | Entry template and controls | GEN |
| Journal batch | Header grouping entered lines | System journal number |
| Voucher | Balanced accounting transaction identifier | System voucher number |
| Posting layer | Accounting adjustment layer | Current |
| Default dimension | Financial dimension values without a main account | Department ADMIN |

A journal batch can contain many vouchers. A single journal line with an offset can generate two or more posted accounting lines. Tax, rounding, balancing, and other processing can create additional lines. Do not expect journal line count to equal posted account entry count.

## 3 Sandbox company and chart of accounts

Create a test legal entity GL01 named Core Services. Use SAR accounting currency, January to December fiscal year, January 2026 training period, and Current posting layer. Leave reporting currency out of the first exercise; add a second company for dual currency tests before posting its first transaction. Use fictional counterparties and evidence.

| Account | Name | Suggested main account type | Use |
|---|---|---|---|
| 110100 | Bank ledger control | Asset | Base lab cash balance only |
| 110200 | Petty cash | Asset | Optional petty cash example |
| 120100 | Receivable ledger control | Asset | GL only example or AR control when integrated |
| 130100 | Prepaid rent | Asset | Unexpired rent |
| 130200 | Prepaid insurance | Asset | Unexpired insurance |
| 140100 | Input tax | Asset | Optional recoverable tax |
| 150100 | Foreign currency deposit | Asset | Standalone FX lab |
| 160100 | Equipment cost | Asset | Optional manual asset accounting |
| 169100 | Accumulated depreciation | Asset or Balance sheet per approved design | Credit balance contra asset presented against equipment |
| 170100 | Due from GL02 | Asset | Intercompany receivable |
| 210100 | Payable ledger control | Liability | GL only example or AP control when integrated |
| 220100 | Accrued utilities | Liability | Unbilled expense |
| 220200 | Salaries payable | Liability | Payroll clearing |
| 230100 | Deferred revenue | Liability | Unearned customer cash |
| 240100 | Output tax | Liability | Optional tax collected |
| 240200 | Tax settlement payable | Liability | Optional net tax due |
| 250100 | Receipt clearing | Liability | Unidentified cash receipts |
| 270100 | Due to GL02 | Liability | Intercompany payable |
| 310100 | Share capital | Equity | Owner funding |
| 320100 | Retained earnings | Equity | Year end transfer |
| 399900 | Migration clearing | Equity or Balance sheet per migration design | Must clear to zero after migration |
| 410100 | Service revenue | Revenue | Earned service income |
| 611000 | Rent expense | Expense | Monthly occupancy expense |
| 612000 | Salaries expense | Expense | Payroll cost |
| 613000 | Utilities expense | Expense | Electricity and similar costs |
| 614000 | Bank charges | Expense | Bank fees |
| 615000 | Consulting expense | Expense | Service purchases |
| 616000 | Insurance expense | Expense | Monthly insurance recognition |
| 617000 | Depreciation expense | Expense | Optional manual depreciation |
| 618000 | Approved write off expense | Expense | Small differences |
| 710000 | Unrealized FX gain | Revenue | Revaluation gain |
| 810000 | Unrealized FX loss | Expense | Revaluation loss |

Balance sheet presentation and main account categories must identify contra assets correctly. A credit balance does not by itself make an account an economic liability. Have the accounting owner approve account type, category, report mapping, and year end treatment for each account.

Define Department values ADMIN, SALES, and SERVICE as a custom financial dimension for this lab. This avoids needing organizational department setup. In a real organization, decide whether the dimension should be backed by an operating unit instead. A custom dimension's backing values use DimensionFinancialTag; this is distinct from the Financial tags feature. [S03]

## 4 Setup in dependency order

### Step 1 Legal entity and fiscal calendar

1. In Organization administration, create or select the legal entity. Confirm its country, address, company ID, and accounting ownership.
2. Open General ledger > Calendars > Fiscal calendars. Create calendar COREJAN and fiscal years 2026 and 2027, with monthly operating periods.
3. After assigning the calendar to the ledger, use the Ledger calendars page to set January 2026 Open for the training users. Restrict other periods as appropriate.
4. Distinguish the shared definition of a calendar from the ledger specific status and module access to its periods. [S04]

Tables: CompanyInfo; FiscalCalendar; FiscalCalendarYear; FiscalCalendarPeriod; LedgerFiscalCalendarYear; LedgerFiscalCalendarPeriod. Module level access also has related configuration: inspect the target build rather than assuming LedgerFiscalCalendarPeriod alone explains every permission.

Example: a transaction dated 31 January must post into January. A transaction dated 1 February must follow February's access rules. Negative test: set January On hold and attempt to post; expect a period error. Restore it to Open for the exercise.

### Step 2 Currencies and exchange rates

1. In General ledger > Currencies, confirm SAR and any foreign currencies needed for later testing.
2. Define a rate type, such as CORE, with effective dated currency pairs and exchange rates.
3. Set accounting currency, optional reporting currency, and rate types on the ledger before transactions begin.
4. Define automatic rounding and currency gain or loss accounts where applicable. [S05]

Tables: Currency; ExchangeRateType; ExchangeRateCurrencyPair; ExchangeRate; Ledger. Automatic transaction account configuration is also read by posting; inspect the appropriate setup records in your build.

Example: for the independent FX test, 1 USD = 3.75 SAR initially and 3.80 SAR at month end. These are fictional training rates. D365 rate storage can use a conversion factor; do not assume the physical ExchangeRate field contains the human readable rate unchanged.

Test both the effective date and conversion direction. A missing rate should be resolved in setup, not hidden by an unsupported database edit.

### Step 3 Chart of accounts and main accounts

1. Open General ledger > Chart of accounts > Accounts > Chart of accounts.
2. Create CORE and add the required main accounts.
3. Set account types and categories. Apply legal entity overrides, active dates, fixed dimensions, and manual entry restrictions where needed.
4. Use categories and financial report definitions to present the accounts consistently. [S06]

Tables: LedgerChartOfAccounts; MainAccount; MainAccountCategory; MainAccountLegalEntity for relevant company overrides.

Example: prepaid rent is an asset; rent expense is an expense. If rent expense is incorrectly configured as a balance sheet account, year end can carry it forward instead of closing it into retained earnings.

### Step 4 Dimensions and account structures

1. Open General ledger > Chart of accounts > Dimensions > Financial dimensions.
2. Create Department, activate it through the supported process, and add ADMIN, SALES, and SERVICE.
3. In Configure account structures, create BS covering accounts 100000 through 399999, with Department optional.
4. Create PL covering accounts 400000 through 899999, with Department mandatory and limited to the three lab values.
5. Activate both structures and assign them to the ledger. Keep their main account ranges nonoverlapping. [S07]

Tables: DimensionAttribute; DimensionAttributeValue; DimensionFinancialTag for this custom dimension; DimensionHierarchy and related constraint records; DimensionAttributeValueCombination for resolved ledger combinations; DimensionAttributeValueSet and DimensionAttributeValueSetItem for default sets.

Example: 611000 ADMIN is valid. A blank Department on 611000 should fail under this proposed structure. A blank Department on 130100 can be valid. Test fixed dimensions too: a main account fixed to ADMIN can override an entered dimension at posting. Review the posted result. [S08]

Optional advanced rule: require a Purpose dimension for selected travel accounts. Optional balancing dimension: require each department to balance and configure automatic interunit debit and credit accounts. This creates additional accounting lines; introduce it in a separate test company so it does not change the simple base lab.

### Step 5 Ledger and parameters

1. Open General ledger > Ledger setup > Ledger.
2. Assign CORE, COREJAN, SAR, and the active BS and PL structures.
3. Review General ledger parameters for voucher handling, fiscal year close, ledger settlement, and other relevant controls.
4. Configure accounts for automatic transactions as needed. Record all decisions before opening balances are posted. [S05]

Tables: Ledger; LedgerParameters; associated ledger account structure links and automatic posting account setup. These are configuration changes, not posted business transactions.

Test: post a small balanced journal and confirm ledger, calendar, currency, and valid account combinations are all resolved correctly.

### Step 6 Journal names and number sequences

Create the following proposed journal names in General ledger > Journal setup > Journal names. Names are arbitrary; journal type and controls determine behavior. [S09]

| Name | Purpose | Proposed controls |
|---|---|---|
| OPEN | Migration balances | Dedicated access and migration references |
| GEN | Daily ledger entries | Standard approval and Current layer |
| ACCR | Accrual adjustments | Mandatory support and reversal date review |
| RECL | Corrections and reclassifications | Original voucher reference required |
| ALLOC | Allocation results | Relevant allocation journal type |
| IC | Intercompany entries | Approved company pairs |

Assign appropriate number sequences. Test journal batch numbers separately from voucher numbers. Do not assume they are interchangeable or that every journal gets only one voucher.

Tables: LedgerJournalName; NumberSequenceTable and related number sequence framework records; LedgerJournalTable for journal instances. Workflow definitions and runtime history are additional framework data when approval is used.

### Step 7 Security and workflow

Assign roles so preparers, approvers, and posters have the intended responsibilities. Define the general journal workflow if required and associate it with the journal name. A proposed lab threshold is an approval step for total debits above SAR 10,000; the actual workflow condition must be tested against the intended amount basis and currency.

Test draft, submitted, rejected, approved, and posted behavior with the actual user roles. Workflow approval is not accounting posting. Before posting there should be no final posted GL voucher for the draft journal. Validate and Simulate posting are useful but do not constitute business approval. [S09]

### Step 8 Reporting configuration

Create the Main account and Main account plus Department dimension sets. Enable or initialize balances using the controls available for your balance calculation feature. Define balance sheet and profit and loss reports with agreed account ranges and signs. Verify all reports use the intended company, dates, currencies, dimensions, and posting layers. [S10, S11]

Tables: DimensionHierarchy for dimension set definitions; either legacy DimensionFocusBalance data or the newer GeneralLedgerBalance and GeneralLedgerMainAccountBalance model, with related reporting dimension and processing records. Financial reporting has its own reporting infrastructure; these operational balance tables are not a universal list of all report storage.

## 5 Journal entry procedure used throughout the lab

1. Select company GL01.
2. Open General ledger > Journal entries > General journals, or the equivalent General journal page.
3. Create a batch using the required journal name. Enter a clear description.
4. Open Lines. Enter accounting date, account type Ledger, ledger account and dimensions, debit or credit, currency, and document reference.
5. Choose one representation: two or more explicit lines under the same voucher with no offsets, or a line with an offset account. Do not accidentally enter both representations for the same economic entry.
6. Confirm the voucher grouping, accounting currency balance, posting layer, and any reversal settings. Check sales tax only when deliberately testing the tax extension.
7. Validate, then Simulate posting where available. Review and correct errors.
8. Submit and complete approval if configured. Post using an authorized user.
9. Open Voucher transactions and drill to the entries. Compare the resulting ledger accounts, dates, dimensions, and currency amounts with the expected journal.
10. Record journal number, voucher, company, accounting date, and supporting evidence. [S09, S37]

For the two explicit lines below, use one voucher and the same accounting date:

| Date | Type | Account | Department | Debit SAR | Credit SAR |
|---|---|---|---|---:|---:|
| 2026-01-01 | Ledger | 110100 | Blank | 100000 | 0 |
| 2026-01-01 | Ledger | 310100 | Blank | 0 | 100000 |

Creating and saving this journal writes journal work data. Posting produces final accounting data. The ledger account combination can be resolved or created during entry or processing and reused if it already exists; it is not necessarily created only at posting.

## 6 Technical data model and posting impact

### 6.1 Primary table map

| Stage | Primary tables | Role |
|---|---|---|
| Journal template | LedgerJournalName | Defaults, journal type, controls |
| Entered journal | LedgerJournalTable | Batch header, name, posting state |
| Entered lines | LedgerJournalTrans | Date, voucher, account, offset, amounts and references |
| Posted header | GeneralJournalEntry | Ledger, accounting date, fiscal period, layer, journal identifiers |
| Posted lines | GeneralJournalAccountEntry | Posted account, main account, amounts, posting type, text |
| Voucher association | SubledgerVoucherGeneralJournalEntry | Voucher, company, date to posted entry association |
| Ledger combination | DimensionAttributeValueCombination | Resolved account identity with relevant dimensions |
| Default dimensions | DimensionAttributeValueSet and DimensionAttributeValueSetItem | Dimension defaults without the main account |
| Segment storage | DimensionAttributeValueGroupCombination, DimensionAttributeValueGroup, DimensionAttributeLevelValue | Links and segment values for combinations |
| Ledger settlement | LedgerTransSettlement | Settlement grouping and transaction references |
| Reporting balances | Feature dependent balance tables | Derived summaries of posted accounting |

Journal and posted schemas are documented separately in Microsoft metadata. [S12, S13, S14, S15] LedgerTransSettlement is documented as a settlement table. [S16] This map identifies primary impacts, not every insert, update, temporary record, workflow event, or batch log used internally.

### 6.2 Important journal fields

LedgerJournalTable includes the journal number and journal name; review Posted and workflow related state in the target build. LedgerJournalTrans commonly includes JournalNum, Voucher, TransDate, AccountType, LedgerDimension, OffsetAccountType, OffsetLedgerDimension, AmountCurDebit, AmountCurCredit, CurrencyCode, ExchRate, DefaultDimension, OffsetDefaultDimension, and Txt. Reversal and tax fields are also relevant in their respective scenarios. Check exact fields and defaulting methods in the AOT. [S12]

LedgerJournalTrans.LedgerDimension must be interpreted with AccountType. When the account type is Vendor, Customer, or Bank, the reference is a dynamic account identity for that account, not simply a main account with financial dimensions. DefaultDimension belongs to a different storage contract. [S17]

### 6.3 Posted fields and relations

GeneralJournalEntry records include RecId, Ledger, AccountingDate, FiscalCalendarPeriod, FiscalCalendarYear, PostingLayer, JournalNumber, and SubledgerVoucher information. GeneralJournalAccountEntry records include GeneralJournalEntry, LedgerDimension, MainAccount, AccountingCurrencyAmount, ReportingCurrencyAmount, TransactionCurrencyAmount, TransactionCurrencyCode, IsCredit, IsCorrection, PostingType, and Text. [S13, S14]

Read-only relation map:

- GeneralJournalAccountEntry.GeneralJournalEntry -> GeneralJournalEntry.RecId.
- GeneralJournalEntry.Ledger -> Ledger.RecId.
- GeneralJournalAccountEntry.LedgerDimension -> DimensionAttributeValueCombination.RecId.
- DimensionAttributeValueCombination.MainAccount -> MainAccount.RecId for main account backed combinations.
- GeneralJournalAccountEntry.MainAccount is also available in the documented schema; confirm its use in the target build.
- SubledgerVoucherGeneralJournalEntry.GeneralJournalEntry -> GeneralJournalEntry.RecId. Its Voucher, VoucherDataAreaId, and AccountingDate preserve voucher association context. [S15, S18]

These are relationships, not an instruction to join every table into one flattened result. Association rows can multiply amount rows. Aggregate posted accounting entries at their own grain before joining one to many references.

### 6.4 Table impact of a simple posted voucher

For the SAR 100,000 funding voucher, expect journal header and line records first. In a standard simple posting without tax or other generated entries, the posted accounting has a debit of 100,000 to 110100 and a credit of 100,000 to 310100, with the relevant posted header and voucher association. Header cardinality and supporting records can differ with posting options and source processing.

The journal header posting state changes on successful completion. Final journal work records may be subject to supported cleanup or archival policies; they are not the permanent accounting source for all reporting. GeneralJournalEntry and GeneralJournalAccountEntry are the principal posted ledger evidence.

The base ledger only transaction does not create a customer balance, vendor open invoice, bank transaction, fixed asset transaction, or tax detail merely because the main account has a corresponding name.

### 6.5 Technical investigation rules

Scope posted GL by Ledger and AccountingDate, with PostingLayer and currency as needed. GeneralJournalEntry and GeneralJournalAccountEntry are not ordinary company tables that can universally be filtered using DataAreaId. Company context follows the ledger; voucher associations also hold voucher company context.

Never join on Voucher alone across the system. Voucher strings can repeat across companies, dates, number sequences, and sources. Use the available ledger, voucher company, accounting date, source links, and journal context; verify the source's actual cardinality.

Do not use the legacy AX LedgerTrans table as the main posted ledger source for D365 Finance. Do not update posted accounting or dimension framework tables directly. Dimension combinations are shared and immutable framework structures; supported services must create or resolve them. [S03]

For standard postings, net signed accounting amount should be zero within the balanced accounting entry at the appropriate currency precision. Do not apply ABS to every amount or negate amounts a second time using IsCredit without understanding the representation. Correction entries can require separate debit and credit presentation logic. Inspect actual data and reconcile your export to Voucher transactions.

### 6.6 Read and write expectations by action

| Action | Primary configuration read | Primary business data written or changed | Financial effect |
|---|---|---|---|
| Save draft journal | Journal name, account and dimension setup, currencies | LedgerJournalTable and LedgerJournalTrans; dimension combinations may be resolved | No final posted GL balance |
| Validate or simulate | Ledger, period access, structures, accounts and rates | Validation messages and possible framework processing, not a final posted GL voucher | No final posted GL balance |
| Approve journal | Workflow and security setup | Workflow state and history | No final posted GL balance |
| Post ledger only journal | All applicable accounting and posting controls | GeneralJournalEntry, GeneralJournalAccountEntry, voucher associations, journal posting state and balance processing records | Posted debit and credit |
| Post reclassification | Same posting controls | New posted GL entries referencing new account combinations | Moves classification while preserving net amount |
| Settle equal same currency ledger items | Ledger settlement parameters and selected transactions | Settlement data including LedgerTransSettlement and applicable framework records | Usually matching only in this defined case |
| Process allocation | Allocation rule, source balances, destination and offset setup | Allocation proposal or journal and posted accounting when posted | Redistributes selected balances |
| Run GL FX revaluation | Account flags, rates and revaluation account setup | Revaluation history and posted GL voucher | Changes accounting or reporting valuation |
| Close fiscal year | Main account types, close template, dimensions and parameters | System generated opening and optional closing accounting plus close history | Carries BS and transfers net P&L |

These expectations are for tracing business outcomes. Use a controlled sandbox trace or debugger for an exact record level write inventory in your release; public schema documentation cannot establish every internal mutation or extension side effect.

## 7 Fully balanced January case study

All amounts are SAR. Tax, bank subledger, AP, AR, and fixed asset subledger are deliberately excluded from these base transactions. For every case below, use the standard journal procedure, then review the posted account entries. Use the specified Department on P&L lines; balance sheet Department may be blank under the proposed structures.

All normal manual cases affect LedgerJournalTable and LedgerJournalTrans before posting and the posted GL tables described in section 6 after posting. Specific additional impacts appear below.

### Case A Owner funding on 1 January

Business event: owner deposits 100,000 into the company's funds.

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 110100 Bank ledger control | Blank | 100000 | 0 |
| 310100 Share capital | Blank | 0 | 100000 |

Use OPEN for this startup example. Expected bank ledger and capital balances are 100,000 debit and credit. This is new company funding, not migration of prior profit.

### Case B Cash service revenue on 5 January

Business event: services worth 20,000 have been delivered and cash received.

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 110100 Bank ledger control | Blank | 20000 | 0 |
| 410100 Service revenue | SERVICE | 0 | 20000 |

Use GEN. Bank ledger becomes 120,000 debit. Revenue is 20,000 credit. No CustTrans is created because account type is Ledger.

### Case C Annual prepaid rent on 6 January

Business event: pay 12,000 covering January through December.

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 130100 Prepaid rent | Blank | 12000 | 0 |
| 110100 Bank ledger control | Blank | 0 | 12000 |

This establishes an asset; it does not recognize the entire year as January expense. Bank ledger becomes 108,000 debit.

### Case D Recognize January rent on 31 January

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 611000 Rent expense | ADMIN | 1000 | 0 |
| 130100 Prepaid rent | Blank | 0 | 1000 |

Prepaid rent becomes 11,000 debit. January rent expense is 1,000. Either post this as a controlled monthly journal or configure a ledger accrual scheme and review its generated schedule. Do not post the manual recognition as well as the generated recognition. Accrual schemes are supported for distributing costs and revenue over periods. [S19]

### Case E Accrue utilities on 31 January

Business event: January utilities are estimated at 2,000; the bill has not arrived.

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 613000 Utilities expense | ADMIN | 2000 | 0 |
| 220100 Accrued utilities | Blank | 0 | 2000 |

Use ACCR and configure a reversing entry dated 1 February through the supported journal reversal settings. Confirm the date is open and inspect the resulting reversal behavior. Do not assume that saving a reversal date alone guarantees an unattended future batch will run. [S09]

January ends with a 2,000 accrual. February reversal: debit 220100 for 2,000 and credit 613000 for 2,000. If the actual bill of 2,200 is paid directly through the ledger in February, debit utilities 2,200 and credit bank ledger 2,200. The combined February expense is 200. These February entries are excluded from the January totals.

### Case F Salary accrual on 25 January

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 612000 Salaries expense | ADMIN | 8000 | 0 |
| 220200 Salaries payable | Blank | 0 | 8000 |

Payroll calculations, employee deductions, and payroll systems are outside this lab. This is the approved summary accounting entry.

### Case G Pay salaries on 28 January

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 220200 Salaries payable | Blank | 8000 | 0 |
| 110100 Bank ledger control | Blank | 0 | 8000 |

Salaries payable is zero. Salary expense remains 8,000. Bank ledger becomes 100,000 debit. The accrual and payment on 220200 can be ledger settled if the account is configured for settlement.

### Case H Receive advance service income on 15 January

Business event: receive 6,000 for services to be delivered over three months.

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 110100 Bank ledger control | Blank | 6000 | 0 |
| 230100 Deferred revenue | Blank | 0 | 6000 |

This is a liability until the service is earned. Bank ledger becomes 106,000 debit.

### Case I Earn one month of advance income on 31 January

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 230100 Deferred revenue | Blank | 2000 | 0 |
| 410100 Service revenue | SERVICE | 0 | 2000 |

Deferred revenue becomes 4,000 credit. Revenue now includes 22,000 credit. The accounting policy must determine the actual earning pattern; equal monthly recognition is a lab assumption.

### Case J Receive unidentified cash on 20 January

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 110100 Bank ledger control | Blank | 3000 | 0 |
| 250100 Receipt clearing | Blank | 0 | 3000 |

Bank ledger becomes 109,000 debit. Clearing has a 3,000 credit awaiting evidence.

### Case K Identify the cash on 22 January

Evidence confirms that the 3,000 relates to services already earned, not an unposted customer invoice in another subledger.

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 250100 Receipt clearing | Blank | 3000 | 0 |
| 410100 Service revenue | SERVICE | 0 | 3000 |

Clearing becomes zero and revenue becomes 25,000 credit. Select the two 250100 entries for ledger settlement. In this simple same currency balanced example, settlement changes matching data, not the account balance. LedgerTransSettlement is a primary table to inspect; features can add other processing. [S16, S20]

### Case L Bank fee on 31 January

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 614000 Bank charges | ADMIN | 100 | 0 |
| 110100 Bank ledger control | Blank | 0 | 100 |

Final bank ledger is 108,900 debit. A real bank implementation should use account type Bank or the appropriate bank process for this accounting and reconcile the bank transactions.

### Case M Allocate rent across departments

After Case D, allocate ADMIN rent to SALES 60 percent and SERVICE 40 percent.

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 611000 Rent expense | SALES | 600 | 0 |
| 611000 Rent expense | SERVICE | 400 | 0 |
| 611000 Rent expense | ADMIN | 0 | 1000 |

Total rent remains 1,000. ADMIN rent becomes zero. Use a controlled manual journal for the first test; then configure an active ledger allocation rule with source, destinations, offset, and percentages that generates this intended result. Review whether the request creates a proposal or posts according to the selected options. Rules can use other methods, including a basis, where needed. [S21, S22]

Additional setup tables include LedgerAllocationRule and related destination, source, and basis records. Generated allocation journals follow their configured posting process. Do not automatically assume every scheduled allocation run is safe to repeat; verify what it selects and whether it would allocate the same source twice.

### Case N Correct the utilities department

Utilities belong to SERVICE rather than ADMIN. Reclassify the dimension without changing the amount owed:

| Account | Department | Debit | Credit |
|---|---|---:|---:|
| 613000 Utilities expense | SERVICE | 2000 | 0 |
| 613000 Utilities expense | ADMIN | 0 | 2000 |

Total utilities remain 2,000, the accrual liability stays 2,000, and profit does not change. The original voucher remains part of the audit trail. Ensure the next month's reversing and actual expense entries use the intended Department; correct the planned reversal handling if the original reversing accrual still uses ADMIN.

## 8 Expected trial balance and financial statements

The following is the January closing balance by main account after Cases A through N. Reclassification and allocation net to zero at main account level.

| Account | Debit balance | Credit balance |
|---|---:|---:|
| 110100 Bank ledger control | 108900 | 0 |
| 130100 Prepaid rent | 11000 | 0 |
| 220100 Accrued utilities | 0 | 2000 |
| 230100 Deferred revenue | 0 | 4000 |
| 310100 Share capital | 0 | 100000 |
| 410100 Service revenue | 0 | 25000 |
| 611000 Rent expense | 1000 | 0 |
| 612000 Salaries expense | 8000 | 0 |
| 613000 Utilities expense | 2000 | 0 |
| 614000 Bank charges | 100 | 0 |
| Total | 131000 | 131000 |

Zero balance accounts: 220200 salaries payable and 250100 receipt clearing. Their transactions still exist and can be inspected even when the trial balance excludes zero balances.

Revenue is 25,000. Expenses are 11,100: rent 1,000, salaries 8,000, utilities 2,000, and bank charges 100. Profit is 13,900.

Assets are 119,900: bank ledger 108,900 and prepaid rent 11,000. Liabilities are 6,000: accrued utilities 2,000 and deferred revenue 4,000. Equity consists of share capital 100,000 and current profit 13,900. Therefore 119,900 = 6,000 + 113,900.

Department expense checks: ADMIN 8,100; SALES 600; SERVICE 2,400. Total departmental expense is 11,100. Revenue in this lab is SERVICE 25,000. A department report is not necessarily a balanced departmental balance sheet unless balancing dimension accounting has been configured.

Bank ledger bridge: 100,000 + 20,000 − 12,000 − 8,000 + 6,000 + 3,000 − 100 = 108,900. Keep the opening funding and all receipts and payments separately identifiable.

## 9 Additional core scenarios and edge cases

These are independent exercises. Do not add their balances to the January case study above.

### 9.1 Opening balance migration for an existing company

Obtain the signed source trial balance at the cutover date, decide whether cutover is at year start or midyear, and map every account, dimension, and currency. A midyear cutover generally needs an explicit decision on current year P&L detail and reporting; do not put all current year revenue and expense into retained earnings without that decision.

Example source balances: bank debit 50,000; prepaid expense debit 10,000; accrued liabilities credit 5,000; capital credit 40,000; retained earnings credit 15,000. Debits and credits are both 60,000. Load a controlled opening journal and validate its date and presentation; an imported migration journal is not automatically the same as the system generated Opening transaction created by year end close.

If AP, AR, or Bank are in scope, migrate their detailed balances through the approved subledger migration design. Reconcile their GL control accounts and remove duplicate amounts from the standalone GL load. Migration clearing must finish at zero. Post only after reconciling source and target totals and signoff.

### 9.2 Recurring journals and templates

For a fixed monthly 500 consulting charge, a periodic journal can generate debit 615000 ADMIN 500 and credit a suitable accrual liability 500. Review generated dates and voucher numbers every cycle. A template helps repeat entry; it does not prove the expense occurred and does not inherently post itself. Periodic journals and voucher templates are supported journal capabilities. [S09, S38]

### 9.3 Ledger accrual schedule

For 12,000 annual insurance, the intended economics are debit prepaid insurance 12,000, credit bank 12,000, then debit insurance expense 1,000 and credit prepaid insurance 1,000 for each of twelve months. Create an accrual scheme under Journal setup > Accrual schemes, set the appropriate accounts and frequency, apply it through Ledger accruals, and inspect Transactions before posting. The generated scheme uses its own replacement and deferral logic; compare the complete generated voucher schedule with the intended economics instead of adding a second manual copy. Primary setup includes LedgerAccrualTable and related schedule configuration. [S19]

Test a partial first month, nonmonthly frequency, final rounding residual, closed future period, and cancellation or correction of a bad schedule. Agree the amortization basis with accounting rather than assuming equal months always apply.

### 9.4 Reversal of an incorrect ledger voucher

Suppose an incorrect cash consulting entry is debit 615000 ADMIN 900 and credit 110100 900. Use the supported reversal action where the transaction is eligible: debit bank 900, credit consulting expense 900. Then post the correct 600 expense entry. Original, reversal, and replacement remain traceable.

Reverse an eligible posted journal or selected vouchers using the available action. If the original period is restricted, use an authorized open date consistent with accounting policy. Some transactions require correction in their source module, such as an invoice credit note; a GL reversal is not a universal way to undo every business document. [S23, S24]

### 9.5 Foreign currency revaluation

Record a USD 5,000 deposit in ledger account 150100 at 3.75 SAR per USD. The deposit's accounting amount is 18,750 SAR. For a simple ledger only purchase of the deposit, credit the cash ledger for the corresponding 18,750 SAR accounting amount using the supported currency entry approach. Review the full voucher rather than attempting to force every currency's transaction amounts into one arithmetic total.

At month end, 5,000 × 3.80 = 19,000 SAR. Intended adjustment: debit 150100 250, credit unrealized FX gain 710000 with a valid Department 250. The foreign currency principal remains USD 5,000.

Mark the account for GL foreign currency revaluation; set rate types and gain or loss accounts; run a preview for the correct ledger, date, currency, account, and layer; then post the approved results. Expected posted amount change is 250 SAR; inspect revaluation history and the linked voucher. GL revaluation posts the difference from the already adjusted accounting balance, rather than reversing all previous revaluation amounts every run. [S25]

Rerunning unchanged criteria and rates should not create the same economic gain again. Investigate any unexpected additional adjustment. At a later fictional rate of 3.70, the target amount is 18,500; the incremental adjustment from 19,000 is debit unrealized loss 500 and credit deposit 500. A later actual disposal or receipt must address realized and unrealized FX according to the relevant settlement and accounting process.

When bank, AR, or AP revaluation is used, exclude the corresponding control accounts from GL revaluation to avoid double revaluation. Do not classify arbitrary historical cost assets for revaluation just because their original transaction currency was foreign. [S25]

### 9.6 Reporting currency and posting layers

In a separate company with reporting currency configured before first posting, test a SAR 3,750 transaction whose intended reporting amount is USD 1,000 under the lab rate configuration. Verify all three amounts independently, including rounding and any reporting currency only adjustments. Reporting currency is not simply report formatting or a second legal entity. [S26]

Post a 100 expense adjustment in an agreed non Current layer. A Current only report should exclude it; a report that includes that adjustment layer should include it. Define the layers in every reconciliation and year end test. Do not mix an operational report with an adjusted statutory report and call the difference a missing transaction.

### 9.7 Clearing settlement with a residual

Credit clearing 1,000 for a receipt; subsequently debit clearing 800 after identifying part of it. The residual account balance is 200 credit. Use supported partial settlement behavior where available and configured. Otherwise follow the approved supported process for identifying the matched portion. Never remove the residual from an open item report merely because the full receipt has been manually marked in a spreadsheet.

If an approved 5 debit residual must be written off, post debit write off expense 5 and credit the clearing account 5 before completing the relevant matching. For a credit residual the direction is reversed or another economic classification is needed. Foreign currency or out of balance ledger settlement can create gain or loss entries depending on features; “settlement never creates accounting” is not a universal rule. [S20]

Ledger settlement and AP or AR invoice settlement are distinct. Current releases provide feature dependent links and partial settlement options, so inspect your parameters and test how they interact with year end. [S20, S27]

### 9.8 Intercompany accounting

GL01 pays 1,000 on behalf of GL02. Intended GL01 entry: debit due from GL02 1,000; credit bank 1,000. Intended GL02 entry: debit consulting expense ADMIN 1,000; credit due to GL01 1,000.

Create due from and due to accounts, journal names in both companies, and the originating and destination relationship under General ledger intercompany setup. Configure reciprocal origin if either company can initiate entries. Use an intercompany journal and review the generated accounting in each company. Reconcile partner, date, currency, and amount. Main setup includes LedgerInterCompany and the configured accounts and journals; confirm exact relations in the AOT. [S28]

This is optional for a one company GL implementation. Department balancing within one company is a separate function and does not create an intercompany receivable between legal entities.

### 9.9 Interunit balancing within a company

In a separate test, debit expense SALES 1,000 and credit cash ADMIN 1,000 with Department as the balancing dimension. Expected additional economics: SALES needs a balancing credit 1,000, and ADMIN needs a balancing debit 1,000, posted to the configured interunit accounts. The company still balances overall, and each Department balances after generated interunit entries. Test the exact generated accounts and dimensions. [S05, S08]

### 9.10 Equipment and depreciation using GL only

Independent example: debit equipment cost 12,000, credit bank 12,000; monthly debit depreciation expense 200, credit accumulated depreciation 200. This is manual accounting with an external asset schedule. It does not create an asset master, asset book, depreciation proposal, or AssetTrans. If asset register and automatic depreciation are required, introduce Fixed assets and its posting profiles instead.

### 9.11 Suspended accounts and invalid dates

Test an account suspended for the company, a dimension value outside its valid date, a missing required dimension, and a journal dated in a restricted period. Resolve the appropriate configuration or entry error; do not disable all financial controls to make an individual voucher post. Retest the original case after the correction.

### 9.12 Amounts and rounding

Test a voucher with multiple debit lines and one credit line, a voucher with multiple currencies, a small exchange difference, and a zero amount line. A batch total of zero does not prove each voucher is valid. Use permitted rounding configuration and inspect generated rounding accounts. Never rely on silent plugs to a suspense account as a routine balancing method.

### 9.13 Group consolidation and elimination

For a single legal entity, this is outside the base implementation. For a group, agree account mappings, calendar alignment, ownership treatment, reporting currency translation, intercompany matching, and elimination rules before combining results. Financial reporting and consolidation company processes offer different approaches. A consolidated report is not an extra legal entity ledger by default. [S39]

Using the 1,000 intercompany example, the group must eliminate the due from asset and due to liability: conceptually debit due to 1,000 and credit due from 1,000 in the consolidation or elimination context. Do not post that group elimination into both subsidiaries' operating ledgers. If an intercompany service also produced group internal revenue and expense, eliminate those matching amounts separately as appropriate. Test that group net results remove internal balances without removing external costs or cash.

### 9.14 Budgets and cash flow reporting

Budget planning and budget control are separate optional requirements. A budget amount is not an actual GeneralJournalAccountEntry. Keep actual, budget, and forecast data sources distinct and agree a comparison grain of company, account, dimensions, currency, and period before building a budget versus actual report.

For cash flow reporting, classify cash movements by the approved operating, investing, and financing policies. In the base lab, owner funding is a financing inflow, the service receipts are operating inflows, and rent, salaries, and bank fees are operating outflows under the simplified lab policy. The bank ledger bridge explains the net movement; a statutory cash flow statement requires the full agreed report mapping and accounting treatment. Do not count noncash accruals as additional cash payments.

## 10 Optional finance subledger integrations

These extensions do not require warehousing or manufacturing. Enable them when the business needs detailed records rather than ledger only balances. Their source tables below are primary examples, not exhaustive posting maps.

| Extension | Example economics | Additional primary records to inspect | Reconciliation |
|---|---|---|---|
| Bank | Receipt debit bank, credit revenue | BankAccountTable, BankAccountTrans plus posted GL | Bank transaction balance to bank control account and statement |
| AP | Service invoice debit expense, credit payable; payment debit payable, credit bank | VendTable, VendTrans, VendTransOpen, VendSettlement; invoice tables depend on journal or document path | Vendor open balances and aging to AP control |
| AR | Service invoice debit receivable, credit revenue; receipt debit bank, credit receivable | CustTable, CustTrans, CustTransOpen, CustSettlement; invoice records depend on source | Customer open balances and aging to AR control |
| Tax | Purchase debit expense and recoverable input tax, credit payable or bank | TaxTable, TaxGroupHeading, TaxGroupData, TaxItemGroupHeading, TaxOnItem, TaxLedgerAccountGroup, TaxTrans and applicable accounting links | Tax transaction detail to GL tax accounts and return |
| Fixed assets | Acquisition and periodic depreciation | AssetTable, AssetBook, AssetTrans and posting configuration | Asset book balances to GL cost and accumulated depreciation |

Source documents can also generate SubledgerJournalEntry and SubledgerJournalAccountEntry before transfer to posted GL, with accounting distributions for relevant document types. The precise path depends on the document. A manual general journal is not evidence that every source document framework table must be populated. [S02]

### Tax examples

Use an illustrative 15 percent tax rate solely to test arithmetic and posting. This does not establish the applicable treatment, deductibility, reporting, e invoicing, or filing obligations of a real Saudi transaction.

Purchase for 1,000 plus fully recoverable 150 tax: debit consulting expense 1,000; debit input tax 150; credit bank or vendor 1,150. Sale for 2,000 plus 300 tax: debit bank or customer 2,300; credit service revenue 2,000; credit output tax 300.

Configure sales tax codes, effective rates, sales tax groups, item sales tax groups, settlement periods, and ledger posting groups using the applicable tax setup. Tax codes selected through the relevant group intersection, tax direction, inclusive or exclusive amounts, and deductibility determine the result. Review calculated tax before posting. Sales tax ledger posting groups determine the main accounts used for calculated tax. [S29, S30, S31]

For 100 percent nondeductible tax under a simple expense treatment, the same purchase can economically debit expense 1,150 and credit the counterparty 1,150, rather than recognizing a recoverable 150 asset. Actual configuration and accounting may vary. Test a credit note reversing the taxable document through its supported source process, a tax exempt case, a zero rate case, and an inclusive tax amount.

For the simple input 150 and output 300 balances, a conceptual settlement is debit output tax 300, credit input tax 150, and credit tax settlement payable 150. Use the tax settlement process and its generated voucher for the actual implementation; do not duplicate it with this conceptual manual journal. Payment is debit tax settlement payable 150, credit bank 150.

Typing a debit directly to an input tax main account does not automatically create complete TaxTrans detail or a correct tax return. Use the supported tax adjustment process for tax corrections.

## 11 Period close procedure

1. Confirm cutoff and complete authorized entries for the period, including integrated finance modules if used.
2. Review unposted, failed, or pending approval journals and batch results. Decide which belong in this month.
3. Review source to GL transfer where relevant and reconcile subledger control accounts.
4. Reconcile cash and bank evidence, clearing accounts, accrued expenses, prepayments, deferred revenue, payroll clearing, and any intercompany balances.
5. Post approved accruals, recognition entries, corrections, and allocations.
6. Run relevant currency revaluation after balances and exchange rates are ready, using each module's proper process.
7. Complete applicable ledger settlements and explain remaining items. If adjustments change balances, revisit the affected reconciliations and revaluation.
8. Check trial balance, P&L, balance sheet, dimensions, layer filters, and reporting currency balances. Confirm balance processing is current.
9. Obtain accounting signoff. Set the completed period On hold or restrict access to the approved closing group, according to the close policy.
10. Use the Financial period close workspace if needed to assign and track tasks. Setting up a close task does not perform or approve the accounting itself. [S32]

Use On hold for a normal reversible posting restriction. Permanently closed cannot be reopened; do not use it as the ordinary month end switch. Review module and user group access as well as overall period status. [S32]

January evidence pack: posted voucher listing; 131,000 debit and credit closing balance totals; 13,900 profit; prepaid rent rollforward showing 11,000; accrued utilities schedule showing 2,000; deferred revenue schedule showing 4,000; bank ledger bridge showing 108,900; cleared salary and receipt accounts; approval and restriction evidence.

## 12 Year end and next year opening

Before closing 2026, complete the full fiscal year's accounting and reconciliations, create 2027 in the fiscal calendar, verify account types and opening account mappings, and define the year end template and retained earnings account. Review dimension transfer settings and the treatment of reruns in General ledger parameters. Validate year end close where available. [S33, S34]

Run General ledger > Period close > Year end close for the correct company, fiscal year, template, dimensions, and voucher. Inspect the generated batch and voucher results. Closing a month does not automatically perform this annual transfer.

For a simplified training company where Cases A through N are the only 2026 movements, expected 2027 opening economics are:

| Account | Opening debit | Opening credit |
|---|---:|---:|
| 110100 Bank ledger control | 108900 | 0 |
| 130100 Prepaid rent | 11000 | 0 |
| 220100 Accrued utilities | 0 | 2000 |
| 230100 Deferred revenue | 0 | 4000 |
| 310100 Share capital | 0 | 100000 |
| 320100 Retained earnings | 0 | 13900 |
| Total | 119900 | 119900 |

New year P&L opening balances are zero. Balance sheet accounts carry forward according to setup, and P&L net profit flows to retained earnings. A system generated Opening transaction is created; an optional Closing transaction depends on parameters. Do not assume the prior year's P&L must show physical zero balances without considering transaction type and closing settings. [S33]

This year end illustration intentionally assumes no February through December activity; it is a mechanical lab result, not a recommendation to omit the remaining rent recognition or other real business activity.

Test a late approved 500 expense posted to 2026 after an initial close. If nothing else changes, retained earnings brought forward should become 13,400 credit after the supported rerun. Understand whether the configuration replaces prior generated entries or retains them and transfers the delta. Do not manually post a second 13,900 opening profit. Review settlement awareness and fiscal dimension handling before and after the rerun. [S27, S33]

## 13 Imports and developer implementation

Use standard setup and journals before writing custom code. For high volume journal import, Microsoft documents the General journal data entity, AOT name LedgerJournalEntity, through Data management with set based processing. It has no public OData collection in that documented entity. Synchronous integrations can use supported journal header and line entities when appropriate. Creating journal records does not itself complete approval or posting. [S35, S36]

Proposed import payload fields include journal name, batch identifier, line number, voucher grouping, transaction date, account type, account display value, currency, debit, credit, description, and offset information when used. Use the actual entity template exported from the target environment; field names and mandatory columns must match that template.

Configure Financial dimension configuration for integrating applications for ledger and default dimension display formats. A journal's account display value must follow the integration format, not an assumed visual segment order. Test blank segments and separators, account type, and defaulting for both account and offset. [S35]

Implement external batch identifiers, duplicate detection, validation feedback, controlled posting, retry status, and post success reconciliation. A transport retry must not silently create a duplicate economic transaction. Record a mapping from external transaction to journal, company, voucher, and accounting date.

When X++ is necessary, inspect the standard journal framework, including LedgerJournalEngine and the standard journal checking and posting path such as LedgerJournalCheckPost, in the target build. Invoke required account modification and defaulting behavior rather than filling raw RecId values from guessed strings. Use supported dimension services. Do not create GeneralJournalEntry or GeneralJournalAccountEntry rows directly, and do not patch dimension storage. Exact extension points and signatures require inspection of the actual environment; this guide does not claim a compiled custom posting implementation.

## 14 Troubleshooting by symptom

| Symptom | Investigate first | Evidence |
|---|---|---|
| Account combination invalid | Structure activation, ledger assignment, dimension constraints, dates | Entered combination compared with active structure |
| Required Department missing | Journal and account defaults, integration segment order | Resolved account and validation message |
| Posted Department differs | Fixed dimension and account override behavior | Journal vs posted account entry |
| Cannot post approved journal | Period access, posting permission, account restrictions, rates | Posting validation and batch message |
| Journal imported but not visible in TB | Posting state and actual posted voucher | Journal header and posted GL records |
| Journal posted but report looks stale | Company, dates, layer, dimension set, balance processing status | Voucher inquiry vs current balance timestamp |
| TB duplicates rows | Dimension set changes and balance model | Supported dimension set maintenance and troubleshooting |
| Foreign amount wrong | Rate type, pair, effective date, conversion factor, fixed rates | Transaction and accounting amount calculation |
| FX gain duplicated | GL and subledger selection, prior revaluation history | Account flags and revaluation vouchers |
| Bank GL differs from bank transactions | Manual ledger postings to bank control, date or currency filters | BankAccountTrans vs ledger entries |
| AP or AR does not reconcile | Direct control postings, transfer status, migration duplicates | Open item report and posted control account |
| Clearing balance zero but items open | Matching not performed, wrong matching scope | Ledger settlement inquiry and remaining transactions |
| Year end profit carried incorrectly | Account type, retained earnings setup, template, layer and rerun settings | Close validation and generated opening voucher |

The newer balance feature uses process automation and indicates data freshness. Investigate status before assuming a missing posted voucher. Legacy and newer tables differ; rebuilding balances is a supported maintenance action for a diagnosed issue, not a routine response to every report question. [S10, S11]

## 15 Acceptance test catalogue

Record test ID, company, feature state, input, expected entries, actual journal and voucher, report evidence, and pass or fail. Run failures deliberately in a sandbox.

| ID | Test | Expected result |
|---|---|---|
| T01 | New ledger setup | Correct chart, active structures, calendar, currencies |
| T02 | Missing required dimension | Posting blocked with meaningful validation |
| T03 | Invalid dimension value | Combination rejected |
| T04 | Fixed Department | Posted value follows configured fixed behavior |
| T05 | Suspended main account | Relevant entry rejected |
| T06 | On hold period | Unauthorized posting rejected |
| T07 | Restricted user group | Only intended close users can post |
| T08 | Missing FX rate | Posting cannot silently use an unintended rate |
| T09 | Two line ledger voucher | Correct debit, credit, date and layer |
| T10 | Line with offset | Same intended economics without duplicate lines |
| T11 | Multi voucher batch | Each voucher is valid independently |
| T12 | Unbalanced voucher | Validation or posting blocks it |
| T13 | Workflow rejection | Journal returns for correction without final accounting |
| T14 | Approval and posting | Approval history and posted voucher both present |
| T15 | Simulate posting | No final posted accounting created by simulation |
| T16 | Startup funding | Bank and capital each 100000 |
| T17 | Service cash receipt | Bank and revenue correct |
| T18 | Prepayment | Asset established without full immediate expense |
| T19 | Monthly recognition | Expense 1000 and prepaid reduction 1000 |
| T20 | Accrual | Expense and liability 2000 |
| T21 | Reversal and actual bill | Correct dates, Department, and net expense |
| T22 | Salary clearing | Liability settles to zero after payment |
| T23 | Deferred revenue | Only earned portion recognized |
| T24 | Receipt identification | Clearing reclassified to correct economics |
| T25 | Same currency ledger settlement | Matched items removed from open analysis without changing balance |
| T26 | Partial settlement | Residual remains explainable under supported feature behavior |
| T27 | Allocation percentages | Destination and offset totals match source |
| T28 | Basis allocation | Approved driver and period determine split |
| T29 | Allocation rerun | No unintended repeat distribution |
| T30 | Dimension reclassification | Profit unchanged and correct departments updated |
| T31 | Eligible GL reversal | Original and reversal retained and net correct |
| T32 | Unsupported source reversal | Correct source document correction path used |
| T33 | FX revaluation | 250 gain under the fictional rate example |
| T34 | FX unchanged rerun | No duplicate 250 economic gain |
| T35 | Accounting and reporting currencies | Independent expected amounts and balanced accounting |
| T36 | Posting layer report | Current and adjusted reports explain their difference |
| T37 | Intercompany | Each company balances and partner accounts reconcile |
| T38 | Balancing dimension | Configured interunit lines generated correctly |
| T39 | Journal import segment order | Correct resolved accounts and dimensions |
| T40 | Duplicate import retry | Duplicate economic transaction prevented or quarantined |
| T41 | Failed batch | Errors visible and successful scope understood before retry |
| T42 | January reconciliation | TB 131000 each side and profit 13900 |
| T43 | Month restriction | Closed month controlled without accidental permanent close |
| T44 | Year end first run | Opening balance sheet correct and P&L transferred |
| T45 | Year end adjustment rerun | Retained earnings reflects final adjusted result once |
| T46 | Settlement across year boundary | Feature aware open items and opening balances reconcile |
| T47 | Bank extension | Bank subledger, GL, and statement reconcile |
| T48 | AP and AR extensions | Open items agree with control accounts without duplicate opening balances |
| T49 | Tax positive and reversal cases | Tax detail and GL reconcile, including credit notes |
| T50 | New balance model | Reporting reconciles after background process completion |

Country specific tax, public sector accounting, regulated reporting, and every possible extension cannot be exhausted by one generic implementation. Treat this catalogue as the common core coverage and extend it for the actual release, features, legal entity requirements, and accounting policies.

## 16 Recommended build stages

| Stage | Deliverable | Exit check |
|---|---|---|
| 1 Accounting design | Chart, dimensions, recognition and close rules | Accounting owner approves design |
| 2 Configuration | Ledger, calendars, accounts, structures, journals and security | Positive and negative setup tests pass |
| 3 Posting proof | Cases A through N with vouchers and table trace | January balances reconcile exactly |
| 4 Periodic processing | Accrual, allocation, FX, settlement and corrections | Generated entries agree with expectations |
| 5 Reporting and close | Trial balance, statements, month and year close | Opening and final balances reconcile |
| 6 Integration | Controlled import and optional finance subledgers | Retry, control account and migration tests pass |

For a D365 developer or application support role, keep one evidence sheet per scenario: business event, menu and inputs, read and write table expectations, generated voucher, accounting equations, reconciliation, and failure diagnosis. This creates a reusable support and regression reference.

## 17 Microsoft references

- [S01 General ledger overview](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/general-ledger)
- [S02 Ledger and subledger accounting overview](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/ledger-subledger)
- [S03 Ledger account combinations and dimension storage](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/financial/ledgeraccountcombinations)
- [S04 Fiscal calendars years and periods](https://learn.microsoft.com/en-us/dynamics365/finance/budgeting/fiscal-calendars-fiscal-years-periods)
- [S05 Configure ledgers](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/configure-ledger)
- [S06 Plan the chart of accounts](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/plan-chart-of-accounts)
- [S07 Define financial dimensions](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/tasks/define-financial-dimensions)
- [S08 Financial dimensions and posting](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/Default-dimensions)
- [S09 General journal processing](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/general-journal-processing)
- [S10 Financial dimension sets](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/financial-dimension-sets)
- [S11 New financial dimension sets and balance table changes](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/financial-dimension-set-new)
- [S12 LedgerJournalTrans schema](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/worksheetline/ledgerjournaltrans)
- [S13 GeneralJournalEntry schema](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/ledger/transactionheader/generaljournalentry)
- [S14 GeneralJournalAccountEntry schema](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/ledger/transactionline/generaljournalaccountentry)
- [S15 SubledgerVoucherGeneralJournalEntry schema](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/ledger/transaction/subledgervouchergeneraljournalentry)
- [S16 LedgerTransSettlement schema](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/ledger/transaction/ledgertranssettlement)
- [S17 Dynamic accounts and default dimensions](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/financial/system-generated-accounts-default-dimensions)
- [S18 DimensionAttributeValueCombination schema](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/financialdimensions/main/dimensionattributevaluecombination)
- [S19 Accruals overview](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/accruals-overview)
- [S20 Ledger settlements](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/ledger-settlements)
- [S21 Process allocations](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/process-allocations)
- [S22 LedgerAllocationRule schema](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/ledger/main/ledgerallocationrule)
- [S23 Reverse journal posting](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/reverse-journal-posting)
- [S24 Transactions that cannot be reversed](https://learn.microsoft.com/en-us/troubleshoot/dynamics-365/finance/general-ledger/cant-reverse-transactions)
- [S25 General ledger foreign currency revaluation](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/foreign-currency-revaluation-general-ledger)
- [S26 Dual currency](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/dual-currency)
- [S27 Settlement awareness and year end close](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/awareness-between-ledger-settlement-year-end-close)
- [S28 Intercompany accounting setup](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/intercompany-accounting-setup)
- [S29 Sales tax ledger posting groups](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/tasks/set-up-ledger-posting-groups-sales-tax)
- [S30 Sales tax calculation on general journal lines](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/sales-tax-calculation-on-general-journal)
- [S31 Set up sales tax codes](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/tasks/set-up-sales-tax-codes)
- [S32 Close the general ledger at period end](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/close-general-ledger-at-period-end)
- [S33 Year end close](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/year-end-close)
- [S34 Year end close validation](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/yec-valid)
- [S35 General journal data entity](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/entity-general-journal)
- [S36 Journal header entity](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/entity-general-journal-ledgerjournalheader)
- [S37 Create and validate journals](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/tasks/create-validate-journals)
- [S38 Post periodic journals](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/tasks/post-periodic-journals)
- [S39 Financial consolidations and currency translation](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/financial-consolidations-currency-translation)

Use these references to validate current product behavior, then confirm implementation details against the actual environment. The worked journals and balances in this guide are original training examples.
