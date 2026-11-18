# Scope

Foundation examples prepare all 68 mapped configuration/master entities. The explicit transaction entry point exercises the three transaction entities: TaxTrans, TaxJournalTrans and MarkupTrans. Preparation never posts documents. Transactions reuse the owning AP business services, with Foundation reports and reconciliation. No startup hook runs either entry point.

The opt-in transaction example seeds TaxTransGeneralJournalAccountEntry for the AP-04 posted input-tax fixture after validating its tax/voucher/dimension/currency/amount references. Configuration/master preparation does not create these rows. The seeder supports unique one-to-one input-tax matches only and leaves unverified enum/allocation and optional subledger references unset.
