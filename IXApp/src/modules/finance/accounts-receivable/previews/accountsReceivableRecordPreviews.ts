import { queryClient } from '@core/api/queryClient';
import { customerPostingAccountApi } from '../setup/api/customerPostingProfileApi';
import { GENERAL_LEDGER_ROUTE_PATHS } from '../../general-ledger/routes/generalLedgerRoutePaths';
import { localizedName } from '@shared/utilities/localizedName';
import {
  RECORD_PREVIEW_STALE_TIME_MS,
  listRecordLoader,
  pagedRecordLoader,
  registerRecordPreview,
} from '@shared/components/record-preview';
import {
  customerQuickCreateApi,
  type CustomerRecord,
} from '../customer/api/customerQuickCreateApi';
import { customerQueryKeys } from '../queries/accountsReceivableQueryKeys';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';

/** Record preview entities (D365 hover cards) owned by Accounts receivable. */
export const ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS = {
  CUSTOMER: 'customer',
  CUSTOMER_GROUP: 'customerGroup',
  PAYMENT_TERMS: 'paymentTerms',
  PAYMENT_METHOD: 'customerPaymentMethod',
  POSTING_PROFILE: 'customerPostingProfile',
  TRADE_JOURNAL_NAME: 'tradeAgreementJournalName',
  LEDGER_DIMENSION: 'customerLedgerDimension',
} as const;

registerRecordPreview(ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.LEDGER_DIMENSION, {
  masterRoute: GENERAL_LEDGER_ROUTE_PATHS.MAIN_ACCOUNTS,
  load: listRecordLoader(customerPostingAccountApi.ledgerAccounts, (record) => String(record.id)),
  fields: [
    { labelKey: 'fields.mainAccount', defaultLabel: 'Main account', value: 'code', lookup: true },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
  ],
});

registerRecordPreview<{ postingProfile: string; name: string }>(
  ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.POSTING_PROFILE,
  {
    masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_POSTING_PROFILES,
    load: pagedRecordLoader('/v1/CustLedger', 'PostingProfile'),
    fromOption: (option) => ({ postingProfile: option.code, name: option.name ?? '' }),
    fields: [
      {
        labelKey: 'freeTextInvoices.postingProfile',
        defaultLabel: 'Posting profile',
        value: 'postingProfile',
        lookup: true,
      },
      { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
    ],
  }
);

registerRecordPreview<{ journalName: string; name: string }>(
  ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.TRADE_JOURNAL_NAME,
  {
    masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.TRADE_AGREEMENT_JOURNAL_NAMES,
    load: pagedRecordLoader('/v1/PriceDiscAdmName', 'JournalName'),
    fields: [
      {
        labelKey: 'priceDiscount.columns.journalName',
        defaultLabel: 'Journal name',
        value: 'journalName',
        lookup: true,
      },
      { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
    ],
  }
);

// Server DTO fields shown by the cards (the paged loader returns raw DTOs).
interface CustGroupPreview {
  custGroupId: string;
  name: string;
}
interface PaymTermPreview {
  paymTermId: string;
  description: string;
}
interface PaymModePreview {
  paymMode: string;
  name: string;
}

// Like D365, a card shows only what identifies the record, not the whole record.

registerRecordPreview<CustomerRecord>(ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER, {
  masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS,
  // The customer name lives on the party record and only the customer list
  // returns it; the list is loaded once and reused for every preview.
  load: listRecordLoader(
    (signal) =>
      queryClient.fetchQuery({
        queryKey: customerQueryKeys.list({ source: 'record-preview' }),
        queryFn: () => customerQuickCreateApi.list(signal),
        staleTime: RECORD_PREVIEW_STALE_TIME_MS,
      }),
    (customer) => customer.accountNumber
  ),
  fields: [
    {
      labelKey: 'fields.customerName',
      defaultLabel: 'Name',
      value: (customer, { isRtl }) =>
        localizedName({ name: customer.name, nameAlias: customer.nameAr }, isRtl),
      lookup: true,
    },
  ],
});

registerRecordPreview<CustGroupPreview>(ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER_GROUP, {
  masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_GROUPS,
  load: pagedRecordLoader('/v1/CustGroup', 'CustGroupId'),
  fromOption: (option) => ({ custGroupId: option.code, name: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.customerGroup',
      defaultLabel: 'Customer group',
      value: 'custGroupId',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
  ],
});

registerRecordPreview<PaymTermPreview>(ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.PAYMENT_TERMS, {
  masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_TERMS,
  load: pagedRecordLoader('/v1/PaymTerm', 'PaymTermId'),
  fromOption: (option) => ({ paymTermId: option.code, description: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.termsOfPayment',
      defaultLabel: 'Terms of payment',
      value: 'paymTermId',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'description' },
  ],
});

registerRecordPreview<PaymModePreview>(ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.PAYMENT_METHOD, {
  masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_METHODS,
  load: pagedRecordLoader('/v1/CustPaymMode', 'PaymMode'),
  fromOption: (option) => ({ paymMode: option.code, name: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.methodOfPayment',
      defaultLabel: 'Method of payment',
      value: 'paymMode',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
  ],
});
