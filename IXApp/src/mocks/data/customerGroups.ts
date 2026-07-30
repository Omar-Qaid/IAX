export interface CustomerGroup {
  id: string;
  groupId: string;
  name: string;
  description: string;
  defaultCurrency: string;
  paymentTerms: string;
  active: boolean;
}

export const MOCK_CUSTOMER_GROUPS: CustomerGroup[] = [
  {
    id: 'cg-1',
    groupId: 'CG-MAJOR',
    name: 'Major Key Accounts',
    description: 'Tier 1 strategic enterprise accounts with special credit terms',
    defaultCurrency: 'USD',
    paymentTerms: 'Net 60',
    active: true,
  },
  {
    id: 'cg-2',
    groupId: 'CG-WHOLESALE',
    name: 'Wholesale Distributors',
    description: 'Bulk commercial product purchasing partners',
    defaultCurrency: 'USD',
    paymentTerms: 'Net 30',
    active: true,
  },
  {
    id: 'cg-3',
    groupId: 'CG-GOVT',
    name: 'Government & Public Sector',
    description: 'Municipal, state, and national government procurement',
    defaultCurrency: 'EUR',
    paymentTerms: 'Net 90',
    active: true,
  },
  {
    id: 'cg-4',
    groupId: 'CG-RETAIL',
    name: 'Standard Retail Direct',
    description: 'Standard end-user retail customers',
    defaultCurrency: 'USD',
    paymentTerms: 'Immediate',
    active: true,
  },
];
