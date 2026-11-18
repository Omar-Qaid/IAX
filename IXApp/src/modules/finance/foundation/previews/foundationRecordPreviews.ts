import { TAX_ROUTE_PATHS } from '@modules/finance/tax/routes/taxRoutePaths';
import { pagedRecordLoader, registerRecordPreview } from '@shared/components/record-preview';
import { FOUNDATION_ROUTE_PATHS } from '../routes/foundationRoutePaths';

/** Record preview entities (D365 hover cards) owned by Foundation. */
export const FOUNDATION_RECORD_PREVIEWS = {
  CURRENCY: 'currency',
  SALES_TAX_GROUP: 'taxGroup',
  ITEM_SALES_TAX_GROUP: 'taxItemGroup',
  TAX_AUTHORITY: 'taxAuthority',
  DELIVERY_MODE: 'dlvMode',
  DELIVERY_TERMS: 'dlvTerm',
} as const;

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
    { labelKey: 'fields.salesTaxGroup', defaultLabel: 'Sales tax group', value: 'taxGroup', lookup: true },
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
    { labelKey: 'fields.taxAuthority', defaultLabel: 'Tax authority', value: 'taxAuthority', lookup: true },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'name' },
  ],
});

// Modes and terms of delivery have no master form yet, so they declare no masterRoute.
registerRecordPreview<DeliveryCodePreview>(FOUNDATION_RECORD_PREVIEWS.DELIVERY_MODE, {
  load: pagedRecordLoader('/v1/DlvMode', 'Code'),
  fromOption: (option) => ({ code: option.code, txt: option.name ?? '' }),
  fields: [
    { labelKey: 'fields.deliveryMode', defaultLabel: 'Mode of delivery', value: 'code', lookup: true },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'txt' },
  ],
});

registerRecordPreview<DeliveryCodePreview>(FOUNDATION_RECORD_PREVIEWS.DELIVERY_TERMS, {
  load: pagedRecordLoader('/v1/DlvTerm', 'Code'),
  fromOption: (option) => ({ code: option.code, txt: option.name ?? '' }),
  fields: [
    { labelKey: 'customerQuickCreate.fields.deliveryTerms', defaultLabel: 'Delivery terms', value: 'code', lookup: true },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'txt' },
  ],
});
