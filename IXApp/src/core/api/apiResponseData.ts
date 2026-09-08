import { ApiError } from './apiError';
import type { ApiResponse } from './apiResponse';

export function requireApiData<T>(response: ApiResponse<T>, resourceName: string): T {
  if (!response.success || response.data == null) {
    throw new ApiError(
      response.message || `The ${resourceName} response did not contain data.`,
      500
    );
  }
  return response.data;
}
