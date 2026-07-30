import { ApiError } from '@core/api/apiError';

export function mapErrorToMessage(error: unknown): string {
  if (error instanceof ApiError) {
    if (Object.keys(error.validationErrors).length > 0) {
      const firstField = Object.keys(error.validationErrors)[0]!;
      const firstMsg = error.validationErrors[firstField]?.[0];
      return `${error.message}: ${firstField} - ${firstMsg}`;
    }
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return 'An unexpected error occurred. Please try again.';
}
