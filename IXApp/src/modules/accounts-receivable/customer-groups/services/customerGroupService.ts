import { MOCK_CUSTOMER_GROUPS, type CustomerGroup } from '@mocks/data/customerGroups';

let mockState = [...MOCK_CUSTOMER_GROUPS];

export const customerGroupService = {
  async getCustomerGroups(): Promise<CustomerGroup[]> {
    await new Promise((r) => setTimeout(r, 150));
    return [...mockState];
  },

  async saveCustomerGroups(groups: CustomerGroup[]): Promise<CustomerGroup[]> {
    await new Promise((r) => setTimeout(r, 300));
    mockState = [...groups];
    return mockState;
  },

  async deleteCustomerGroup(id: string): Promise<void> {
    await new Promise((r) => setTimeout(r, 200));
    mockState = mockState.filter((cg) => cg.id !== id);
  },
};
