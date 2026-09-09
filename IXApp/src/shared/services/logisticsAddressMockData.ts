import type { CountryRegion, State, City, County } from '@shared/types/logistics';

export const MOCK_COUNTRY_REGIONS: CountryRegion[] = [
  { countryRegionId: 'USA', isoCode: 'US', name: 'United States' },
  { countryRegionId: 'SAU', isoCode: 'SA', name: 'Saudi Arabia' },
  { countryRegionId: 'ARE', isoCode: 'AE', name: 'United Arab Emirates' },
  { countryRegionId: 'GBR', isoCode: 'GB', name: 'United Kingdom' },
];

export const MOCK_STATES: State[] = [
  { stateId: 'CA', name: 'California', countryRegionId: 'USA' },
  { stateId: 'NY', name: 'New York', countryRegionId: 'USA' },
  { stateId: 'TX', name: 'Texas', countryRegionId: 'USA' },
  { stateId: 'RIY', name: 'Riyadh Province', countryRegionId: 'SAU' },
  { stateId: 'MKH', name: 'Makkah Region', countryRegionId: 'SAU' },
  { stateId: 'DXB', name: 'Dubai', countryRegionId: 'ARE' },
];

export const MOCK_CITIES: City[] = [
  { cityKey: 'LA', name: 'Los Angeles', countryRegionId: 'USA', stateId: 'CA' },
  { cityKey: 'SF', name: 'San Francisco', countryRegionId: 'USA', stateId: 'CA' },
  { cityKey: 'NYC', name: 'New York City', countryRegionId: 'USA', stateId: 'NY' },
  { cityKey: 'HOU', name: 'Houston', countryRegionId: 'USA', stateId: 'TX' },
  { cityKey: 'RUH', name: 'Riyadh', countryRegionId: 'SAU', stateId: 'RIY' },
  { cityKey: 'JED', name: 'Jeddah', countryRegionId: 'SAU', stateId: 'MKH' },
];

export const MOCK_COUNTIES: County[] = [
  { countyId: 'LAD', name: 'Los Angeles County', countryRegionId: 'USA', stateId: 'CA' },
  { countyId: 'SFC', name: 'San Francisco County', countryRegionId: 'USA', stateId: 'CA' },
  { countyId: 'NYC', name: 'New York County', countryRegionId: 'USA', stateId: 'NY' },
  { countyId: 'RDC', name: 'Riyadh District', countryRegionId: 'SAU', stateId: 'RIY' },
];
