export type ElectronicAddressType = 'Phone' | 'Email' | 'URL' | 'Telex' | 'Fax' | 'InstantMessage';

export interface CountryRegion {
  countryRegionId: string;
  isoCode: string;
  name?: string;
}

export interface State {
  stateId: string;
  name: string;
  countryRegionId: string;
}

export interface City {
  cityKey: string;
  name: string;
  stateId: string;
}

export interface County {
  countyId: string;
  name: string;
  stateId: string;
}

export interface LogisticsPostalAddress {
  id?: string | number | null;
  locationId?: string;
  description: string;
  roles?: string[];
  validFrom: string;
  validTo: string;
  countryRegionId: string;
  state?: string;
  city?: string;
  district?: string;
  street?: string;
  building?: string;
  zipCode?: string;
  buildingComplement?: string;
  postBox?: string;
  county?: string;
  primary?: boolean;
  primaryForCountry?: boolean;
}

export interface LogisticsElectronicAddress {
  id?: string | number | null;
  locationId?: string;
  description: string;
  type: ElectronicAddressType;
  number: string;
  extension?: string;
  roles?: string[];
  primary?: boolean;
}
