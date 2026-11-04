# Configured supplier quotes, discount setup and tax references

The current master quotes are local paper SAR 25/EA, local maintenance SAR 350/EA and USD
supplier paper USD 7/EA. They apply from January 1 through December 31 of the requested fiscal
year, with price unit 1 and minimum quantity 1, referencing APV2-DIM. These are illustrative
supplier quotations; the USD quote need not equal the local supplier's SAR quote.

Both suppliers and purchasing modules reference the existing APV2 tax groups. Their intersection
includes APV2-15, configured at 15 percent with a recoverable input-tax ledger reference. The
optional APV2-2 cash-discount setup is 2 percent within 10 days. No discount is applied and no tax
transaction is posted by this package. Document-level discounts, charges, cash-discount application
and tax posting remain future transaction work.

See [REAL-EXAMPLE.md](../../../REAL-EXAMPLE.md) for the illustrative SAR 3,680 exercise. Its
amounts explain the stored master quotes and settings; it is not a saved or posted invoice.
