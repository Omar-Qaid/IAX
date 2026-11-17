import { registerRecordPreview } from '@shared/components/record-preview';

/** Record preview entity for postal addresses (D365: Location, Effective, Address). */
export const POSTAL_ADDRESS_RECORD_PREVIEW = 'postalAddress';

/**
 * What the address card shows. Addresses are read in the context of their owner
 * (an order, a party), so pages pass this record to `RecordPreview` instead of
 * the preview loading it.
 */
export interface PostalAddressPreview {
  /** Location name (the address description). */
  location: string;
  /** Start of validity, ISO date or date-time. */
  validFrom?: string | null;
  /** Formatted full address. */
  address: string;
}

registerRecordPreview<PostalAddressPreview>(POSTAL_ADDRESS_RECORD_PREVIEW, {
  fields: [
    { labelKey: 'fields.location', defaultLabel: 'Location', value: 'location', lookup: true },
    {
      labelKey: 'fields.effective',
      defaultLabel: 'Effective',
      value: (address, { language }) => {
        if (!address.validFrom) return '';
        const date = new Date(address.validFrom);
        return Number.isNaN(date.getTime()) ? address.validFrom : date.toLocaleString(language);
      },
    },
    { labelKey: 'fields.address', defaultLabel: 'Address', value: 'address' },
  ],
});
