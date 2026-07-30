export interface Customer {
  id: string;
  accountNumber: string;
  name: string;
  nameAr?: string;
  customerGroupId: string;
  currencyCode: string;
  email?: string;
  phone?: string;
  status: 'active' | 'onHold' | 'blocked';
  creditLimit?: number;
  createdAt: string;
  modifiedAt?: string;
}

export const MOCK_CUSTOMERS: Customer[] = [
  {
    id: 'cust-101',
    accountNumber: 'US-001',
    name: 'Contoso Retail Americas',
    nameAr: 'كونتوسو للتجزئة أمريكا',
    customerGroupId: 'CG-MAJOR',
    currencyCode: 'USD',
    email: 'purchasing@contoso.com',
    phone: '+1 425 555 0100',
    status: 'active',
    creditLimit: 250000,
    createdAt: '2025-01-15T08:00:00Z',
  },
  {
    id: 'cust-102',
    accountNumber: 'US-002',
    name: 'Fabrikam Supplies Ltd.',
    nameAr: 'شركة فابريكام للتوريدات',
    customerGroupId: 'CG-WHOLESALE',
    currencyCode: 'USD',
    email: 'orders@fabrikam.com',
    phone: '+1 206 555 0188',
    status: 'active',
    creditLimit: 100000,
    createdAt: '2025-02-01T09:30:00Z',
  },
  {
    id: 'cust-103',
    accountNumber: 'EU-003',
    name: 'Northwind Traders International',
    nameAr: 'تجار نورث ويند العالمية',
    customerGroupId: 'CG-GOVT',
    currencyCode: 'EUR',
    email: 'info@northwind.eu',
    phone: '+44 20 7946 0912',
    status: 'onHold',
    creditLimit: 50000,
    createdAt: '2025-03-10T11:20:00Z',
  },
  {
    id: 'cust-104',
    accountNumber: 'SA-004',
    name: 'Al-Madina Enterprise Solutions',
    nameAr: 'حلول المدينة للمؤسسات',
    customerGroupId: 'CG-MAJOR',
    currencyCode: 'SAR',
    email: 'contact@almadina.sa',
    phone: '+966 11 456 7890',
    status: 'active',
    creditLimit: 500000,
    createdAt: '2025-04-05T14:15:00Z',
  },
];
