import { TAX_ROUTE_PATHS } from '@modules/finance/tax/routes/taxRoutePaths';
import {
  listRecordLoader,
  pagedRecordLoader,
  registerRecordPreview,
} from '@shared/components/record-preview';
import { markupTransApi } from '../charges/api/markupTransApi';
import { FOUNDATION_ROUTE_PATHS } from '../routes/foundationRoutePaths';

/** Record preview entities (D365 hover cards) owned by Foundation. */
export const FOUNDATION_RECORD_PREVIEWS = {
  CURRENCY: 'currency',
  SALES_TAX_GROUP: 'taxGroup',
  ITEM_SALES_TAX_GROUP: 'taxItemGroup',
  TAX_AUTHORITY: 'taxAuthority',
  TAX_LEDGER_DIMENSION: 'taxLedgerDimension',
  TAX_ACCOUNT_GROUP: 'taxAccountGroup',
  TAX_PERIOD: 'taxPeriod',
  TAX_CODE: 'taxCode',
  DELIVERY_MODE: 'dlvMode',
  DELIVERY_TERMS: 'dlvTerm',
  REFERENCE_OPTION: 'referenceOption',
  SALES_CHARGE_CODE: 'salesChargeCode',
} as const;

registerRecordPreview<{ code: string; name: string }>(FOUNDATION_RECORD_PREVIEWS.REFERENCE_OPTION, {
  fromOption: (option) => ({ code: option.code, name: option.name ?? '' }),
  fields: [
    { labelKey: 'fields.code', defaultLabel: 'Code', value: 'code', lookup: true },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
  ],
});

registerRecordPreview(FOUNDATION_RECORD_PREVIEWS.SALES_CHARGE_CODE, {
  masterRoute: FOUNDATION_ROUTE_PATHS.CHARGES_CODES,
  load: listRecordLoader(
    (signal) => markupTransApi.codes('sales', signal),
    (record) => record.markupCode
  ),
  fields: [
    {
      labelKey: 'documentCharges.fields.chargesCode',
      defaultLabel: 'Charges code',
      value: 'markupCode',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'txt' },
  ],
});

registerRecordPreview<{ code: string; name: string }>(
  FOUNDATION_RECORD_PREVIEWS.TAX_LEDGER_DIMENSION,
  {
    fromOption: (option) => ({ code: option.code, name: option.name ?? '' }),
    fields: [
      { labelKey: 'fields.mainAccount', defaultLabel: 'Main account', value: 'code', lookup: true },
      { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
    ],
  }
);

registerRecordPreview<{ taxAccountGroup: string; name: string }>(
  FOUNDATION_RECORD_PREVIEWS.TAX_ACCOUNT_GROUP,
  {
    masterRoute: TAX_ROUTE_PATHS.TAX_ACCOUNT_GROUPS,
    load: pagedRecordLoader('/v1/TaxLedgerAccountGroup', 'TaxAccountGroup'),
    fromOption: (option) => ({ taxAccountGroup: option.code, name: option.name ?? '' }),
    fields: [
      {
        labelKey: 'taxAccountGroup.fields.code',
        defaultLabel: 'Ledger posting group',
        value: 'taxAccountGroup',
        lookup: true,
      },
      { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
    ],
  }
);
registerRecordPreview<{ taxPeriod: string; name: string }>(FOUNDATION_RECORD_PREVIEWS.TAX_PERIOD, {
  masterRoute: TAX_ROUTE_PATHS.TAX_PERIODS,
  load: pagedRecordLoader('/v1/TaxPeriodHead', 'TaxPeriod'),
  fromOption: (option) => ({ taxPeriod: option.code, name: option.name ?? '' }),
  fields: [
    {
      labelKey: 'taxTable.fields.settlementPeriod',
      defaultLabel: 'Settlement period',
      value: 'taxPeriod',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
  ],
});
registerRecordPreview<{ taxCode: string; taxName: string }>(FOUNDATION_RECORD_PREVIEWS.TAX_CODE, {
  masterRoute: TAX_ROUTE_PATHS.TAX_CODES,
  load: pagedRecordLoader('/v1/TaxTable', 'TaxCode'),
  fromOption: (option) => ({ taxCode: option.code, taxName: option.name ?? '' }),
  fields: [
    {
      labelKey: 'taxTable.fields.code',
      defaultLabel: 'Sales tax code',
      value: 'taxCode',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'taxName' },
  ],
});

// Server DTO fields shown by the cards (the paged loader returns raw DTOs).
interface CurrencyPreview {
  currencyCode: string;
  txt: string;
}
interface TaxGroupPreview {
  taxGroup: string;
  taxGroupName: string;
}
interface TaxItemGroupPreview {
  taxItemGroup: string;
  name: string;
}
interface DeliveryCodePreview {
  code: string;
  txt: string;
}
interface TaxAuthorityPreview {
  taxAuthority: string;
  name: string;
}

registerRecordPreview<CurrencyPreview>(FOUNDATION_RECORD_PREVIEWS.CURRENCY, {
  masterRoute: FOUNDATION_ROUTE_PATHS.CURRENCIES,
  load: pagedRecordLoader('/v1/Currency', 'CurrencyCode'),
  fromOption: (option) => ({ currencyCode: option.code, txt: option.name ?? '' }),
  fields: [
    { labelKey: 'fields.currency', defaultLabel: 'Currency', value: 'currencyCode', lookup: true },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'txt' },
  ],
});

registerRecordPreview<TaxGroupPreview>(FOUNDATION_RECORD_PREVIEWS.SALES_TAX_GROUP, {
  masterRoute: TAX_ROUTE_PATHS.TAX_GROUPS,
  load: pagedRecordLoader('/v1/TaxGroup', 'TaxGroup'),
  fromOption: (option) => ({ taxGroup: option.code, taxGroupName: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.salesTaxGroup',
      defaultLabel: 'Sales tax group',
      value: 'taxGroup',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'taxGroupName' },
  ],
});

registerRecordPreview<TaxItemGroupPreview>(FOUNDATION_RECORD_PREVIEWS.ITEM_SALES_TAX_GROUP, {
  masterRoute: TAX_ROUTE_PATHS.TAX_ITEM_GROUPS,
  load: pagedRecordLoader('/v1/TaxItemGroup', 'TaxItemGroup'),
  fromOption: (option) => ({ taxItemGroup: option.code, name: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.itemSalesTaxGroup',
      defaultLabel: 'Item sales tax group',
      value: 'taxItemGroup',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
  ],
});

registerRecordPreview<TaxAuthorityPreview>(FOUNDATION_RECORD_PREVIEWS.TAX_AUTHORITY, {
  masterRoute: TAX_ROUTE_PATHS.TAX_AUTHORITIES,
  load: pagedRecordLoader('/v1/TaxAuthorityAddress', 'TaxAuthority'),
  fromOption: (option) => ({ taxAuthority: option.code, name: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.taxAuthority',
      defaultLabel: 'Tax authority',
      value: 'taxAuthority',
      lookup: true,
    },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'name' },
  ],
});

// Modes and terms of delivery have no master form yet, so they declare no masterRoute.
registerRecordPreview<DeliveryCodePreview>(FOUNDATION_RECORD_PREVIEWS.DELIVERY_MODE, {
  load: pagedRecordLoader('/v1/DlvMode', 'Code'),
  fromOption: (option) => ({ code: option.code, txt: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.deliveryMode',
      defaultLabel: 'Mode of delivery',
      value: 'code',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'txt' },
  ],
});

registerRecordPreview<DeliveryCodePreview>(FOUNDATION_RECORD_PREVIEWS.DELIVERY_TERMS, {
  load: pagedRecordLoader('/v1/DlvTerm', 'Code'),
  fromOption: (option) => ({ code: option.code, txt: option.name ?? '' }),
  fields: [
    {
      labelKey: 'customerQuickCreate.fields.deliveryTerms',
      defaultLabel: 'Delivery terms',
      value: 'code',
      lookup: true,
    },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'txt' },
  ],
});
