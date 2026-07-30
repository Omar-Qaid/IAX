import { MOCK_CUSTOMERS, type Customer } from '@mocks/data/customers';

export interface CustomerQueryParameters {
  search?: string;
  status?: string;
  customerGroupId?: string;
}

export interface CustomerService {
  getCustomers(params?: CustomerQueryParameters): Promise<Customer[]>;
  getCustomer(id: string): Promise<Customer | null>;
  createCustomer(data: Omit<Customer, 'id' | 'createdAt'>): Promise<Customer>;
  updateCustomer(id: string, data: Partial<Customer>): Promise<Customer>;
  deleteCustomer(id: string): Promise<void>;
}

let mockState = [...MOCK_CUSTOMERS];

export const customerService: CustomerService = {
  async getCustomers(params) {
    await new Promise((r) => setTimeout(r, 200));
    let result = [...mockState];
    if (params?.search) {
      const q = params.search.toLowerCase();
      result = result.filter(
        (c) =>
          c.name.toLowerCase().includes(q) ||
          c.accountNumber.toLowerCase().includes(q) ||
          c.email?.toLowerCase().includes(q)
      );
    }
    if (params?.status) {
      result = result.filter((c) => c.status === params.status);
    }
    return result;
  },

  async getCustomer(id) {
    await new Promise((r) => setTimeout(r, 150));
    return mockState.find((c) => c.id === id) || null;
  },

  async createCustomer(data) {
    await new Promise((r) => setTimeout(r, 250));
    const newCust: Customer = {
      ...data,
      id: `cust-${Date.now()}`,
      createdAt: new Date().toISOString(),
    };
    mockState.unshift(newCust);
    return newCust;
  },

  async updateCustomer(id, data) {
    await new Promise((r) => setTimeout(r, 250));
    const idx = mockState.findIndex((c) => c.id === id);
    if (idx === -1) throw new Error('Customer not found');
    const updated = {
      ...mockState[idx]!,
      ...data,
      modifiedAt: new Date().toISOString(),
    };
    mockState[idx] = updated;
    return updated;
  },

  async deleteCustomer(id) {
    await new Promise((r) => setTimeout(r, 200));
    mockState = mockState.filter((c) => c.id !== id);
  },
};
