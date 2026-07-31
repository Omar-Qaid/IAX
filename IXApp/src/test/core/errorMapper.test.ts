import { describe, it, expect } from 'vitest';
import { mapErrorToMessage } from '@core/errors/errorMapper';
import { ApiError } from '@core/api/apiError';

describe('errorMapper', () => {
  it('maps ApiError correctly', () => {
    const error = new ApiError('Unauthorized', 401);
    const mapped = mapErrorToMessage(error);
    expect(mapped).toBe('Unauthorized');
  });

  it('maps generic Error correctly', () => {
    const error = new Error('Network Error');
    const mapped = mapErrorToMessage(error);
    expect(mapped).toBe('Network Error');
  });
});
