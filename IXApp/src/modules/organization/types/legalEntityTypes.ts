export interface LegalEntityAddress {
  id: string;
  location: number;
  locationId: string;
  description: string;
  address: string;
  primary: boolean;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  county: string;
  countryRegionId: string;
  districtName: string;
  validFrom: string | null;
  validTo: string | null;
  roles: string[];
}

export interface LegalEntityContact {
  id: string;
  location: number;
  locationId: string;
  description: string;
  type: string;
  number: string;
  extension: string;
  primary: boolean;
  roles: string[];
}

export interface LegalEntity {
  recId: number;
  party: number;
  dataArea: string;
  name: string;
  languageId: string | null;
  currencyCode: string | null;
  taxLicenseNum: string | null;
  federalTaxId: string | null;
  bankAccount: string | null;
  calendar: number | null;
  timeZone: string | null;
  memo: string | null;
  arabicName: string | null;
  localizedRegion: string | null;
  logo: string | null;
  reportLogo: string | null;
  /** Pending attachment changes; undefined means keep the current attachment. */
  logoFile?: File | null;
  reportLogoFile?: File | null;
  addresses: LegalEntityAddress[];
  contacts: LegalEntityContact[];
  /** UI fields represented in the D365 form but not yet exposed by CompanyInfoDto. */
  inHierarchy?: boolean;
  useForFinancialConsolidation?: boolean;
  useForFinancialElimination?: boolean;
  fullName?: string;
}

export interface LegalEntityRecord extends LegalEntity {
  id: string;
}

export interface LegalEntityRepository {
  list(signal?: AbortSignal): Promise<LegalEntityRecord[]>;
  create(entity: LegalEntityRecord): Promise<LegalEntityRecord>;
  update(entity: LegalEntityRecord): Promise<LegalEntityRecord>;
  delete(entity: LegalEntityRecord): Promise<void>;
}
