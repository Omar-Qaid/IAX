# Vendor payment and customer collection through the bank

Implemented scenario `01`, run by `CashAndBankTransactionExamples`.

- **Procedure:** AP scenario `01` pays the vendor and AR scenario `01` collects from the customer, both through their payment services. The bank tables are never written directly.
- **Expected:** a `BankAccountTrans` row for each payment, on the bank account the method of payment names. For each account, the report gives the movement count, deposits and withdrawals. The bank's ledger account (`181001`) moves by the same net amount.
- **Out of scope:** reconciling against a bank statement.
