import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface ReleasedProductRecord {
  id: string;
  itemNumber: string;
  name: string;
  nameAliasLocalized: string;
  nameAlias: string;
  productType: string;
  productSubtype: string;
  salesPrice: number;
  purchasePrice: number;
  salesPriceQuantity: number;
  purchasePriceQuantity: number;
  salesTaxItemGroupId: string;
  purchaseTaxItemGroupId: string;
  costPrice: number;
  itemGroupId: string;
  modelGroupId: string;
  storageDimensionGroupId: string;
  trackingDimensionGroupId: string;
  inventoryUnit: string;
  purchaseUnit: string;
  salesUnit: string;
  purchaseToInventoryFactor: number;
  salesToInventoryFactor: number;
  defaultOrderSettings: ReleasedProductDefaultOrderSettings;
}

export interface ReleasedProductGroupAssignment {
  itemGroupId: string; modelGroupId: string;
  storageDimensionGroupId: string; trackingDimensionGroupId: string;
}
export interface ReleasedProductUnitAssignment {
  inventoryUnit: string; purchaseUnit: string; salesUnit: string;
  groups?: ReleasedProductGroupAssignment;
  purchaseToInventoryFactor: number; salesToInventoryFactor: number;
  commercial?: ReleasedProductCommercialAssignment;
  defaultOrderSettings?: ReleasedProductDefaultOrderSettings;
}
export interface ReleasedProductCommercialAssignment {
  purchasePrice: number; purchasePriceQuantity: number; purchaseTaxItemGroupId: string;
  salesPrice: number; salesPriceQuantity: number; salesTaxItemGroupId: string;
}

export interface CreateSharedProductInput {
  productNumber: string;
  searchName: string;
  name: string;
  description: string;
  languageId: string;
}

export interface ReleaseProductInput {
  productId: string;
  itemNumber: string;
  nameAlias: string;
}

export interface ConfigureReleasedProductInput {
  inventoryUnit: string;
  purchaseUnit: string;
  salesUnit: string;
  purchaseToInventoryFactor: number;
  salesToInventoryFactor: number;
  inventoryPrice: number;
  purchasePrice: number;
  salesPrice: number;
  priceUnit: number;
  purchaseTaxItemGroupId: string;
  salesTaxItemGroupId: string;
  itemGroupId: string;
  modelGroupId: string;
  storageDimensionGroupId: string;
  trackingDimensionGroupId: string;
  defaultOrderSettings?: ReleasedProductDefaultOrderSettings;
}

export interface ReleasedProductOrderModuleSettings {
  siteId: string; warehouseId: string; standardQuantity: number;
  minimumQuantity: number; maximumQuantity: number;
  multipleQuantity: number; leadTimeDays: number; stopped: boolean;
}
export interface ReleasedProductDefaultOrderSettings {
  defaultOrderType: number;
  purchase: ReleasedProductOrderModuleSettings;
  sales: ReleasedProductOrderModuleSettings;
  inventory: ReleasedProductOrderModuleSettings;
}

export interface ReleasedProductValidation {
  isValid: boolean;
  errors: string[];
}
export interface ReleasedProductLookupOption { value: string; label: string }
export interface ReleasedProductLookups {
  units: ReleasedProductLookupOption[]; itemGroups: ReleasedProductLookupOption[];
  modelGroups: ReleasedProductLookupOption[]; storageDimensionGroups: ReleasedProductLookupOption[];
  trackingDimensionGroups: ReleasedProductLookupOption[]; taxItemGroups: ReleasedProductLookupOption[];
  vendors: ReleasedProductLookupOption[]; sites: ReleasedProductLookupOption[];
  warehouses: ReleasedProductLookupOption[];
}
export interface CreateReleasedProductInput {
  productNumber: string; name: string; nameAlias: string; searchName: string;
  itemNumber: string; itemNameAlias: string; sku: string; buyerGroupId: string;
  vendorAccount: string; bomUnit: string; configuration?: ConfigureReleasedProductInput;
}
export interface ReleaseCompanyRecord { id: string; code: string; name: string; nameAlias: string }

const endpoint = '/v1/InventTable/released-products';

export const releasedProductApi = {
  async list(signal?: AbortSignal): Promise<ReleasedProductRecord[]> {
    const response = await apiClient.get<ApiResponse<ReleasedProductRecord[]>>(endpoint, { signal });
    if (!response.data.success || !Array.isArray(response.data.data)) {
      throw new ApiError(response.data.message || 'The released products response did not contain data.', 500);
    }
    return response.data.data;
  },
  async lookups(signal?: AbortSignal): Promise<ReleasedProductLookups> {
    const response = await apiClient.get<ApiResponse<ReleasedProductLookups>>(`${endpoint}/lookups`, { signal });
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'Released product lookups could not be loaded.', 400);
    return response.data.data;
  },
  async create(input: CreateReleasedProductInput): Promise<ReleasedProductRecord> {
    const response = await apiClient.post<ApiResponse<ReleasedProductRecord>>(`${endpoint}/create`, input);
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The released product could not be created.', 400);
    return response.data.data;
  },

  async createShared(input: CreateSharedProductInput): Promise<ReleasedProductRecord> {
    const response = await apiClient.post<ApiResponse<ReleasedProductRecord>>(`${endpoint}/shared-products`, input);
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The product could not be created.', 400);
    return response.data.data;
  },

  async release(input: ReleaseProductInput): Promise<ReleasedProductRecord> {
    const response = await apiClient.post<ApiResponse<ReleasedProductRecord>>(`${endpoint}/release`, input);
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The product could not be released.', 400);
    return response.data.data;
  },
  async releaseToCompany(input: ReleaseProductInput, company: string): Promise<ReleasedProductRecord> {
    const response = await apiClient.post<ApiResponse<ReleasedProductRecord>>(`${endpoint}/release`, input, { headers: { 'X-Company': company } });
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The product could not be released.', 400);
    return response.data.data;
  },
  async companies(signal?: AbortSignal): Promise<ReleaseCompanyRecord[]> {
    const response = await apiClient.get<ApiResponse<Array<{ recId: number; dataArea: string; name: string; arabicName?: string | null }>>>('/v1/CompanyInfo', { signal });
    if (!response.data.success || !Array.isArray(response.data.data)) throw new ApiError(response.data.message || 'Legal entities could not be loaded.', 500);
    return response.data.data.map((row) => ({ id: String(row.recId), code: row.dataArea, name: row.name, nameAlias: row.arabicName ?? '' }));
  },

  async configure(id: string, input: ConfigureReleasedProductInput): Promise<ReleasedProductRecord> {
    const response = await apiClient.put<ApiResponse<ReleasedProductRecord>>(`${endpoint}/${encodeURIComponent(id)}/configuration`, input);
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The product could not be configured.', 400);
    return response.data.data;
  },

  async assignGroups(id: string, input: ReleasedProductGroupAssignment): Promise<ReleasedProductRecord> {
    const response = await apiClient.put<ApiResponse<ReleasedProductRecord>>(`${endpoint}/${encodeURIComponent(id)}/groups`, input);
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The product groups could not be assigned.', 400);
    return response.data.data;
  },

  async assignUnits(id: string, input: ReleasedProductUnitAssignment): Promise<ReleasedProductRecord> {
    const response = await apiClient.put<ApiResponse<ReleasedProductRecord>>(`${endpoint}/${encodeURIComponent(id)}/units`, input);
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The product units could not be assigned.', 400);
    return response.data.data;
  },

  async validate(id: string, signal?: AbortSignal): Promise<ReleasedProductValidation> {
    const response = await apiClient.get<ApiResponse<ReleasedProductValidation>>(`${endpoint}/${encodeURIComponent(id)}/validate`, { signal });
    if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'The product could not be validated.', 400);
    return response.data.data;
  },
};
