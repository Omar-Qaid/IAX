import { describe, expect, it } from 'vitest';
import { getLogicalDrawerAnchor } from '@shared/hooks/useLogicalDrawerAnchor';

describe('getLogicalDrawerAnchor', () => {
  it.each([
    ['start', 'left'],
    ['end', 'right'],
  ] as const)('maps logical %s to MUI anchor %s', (placement, expected) => {
    expect(getLogicalDrawerAnchor(placement)).toBe(expected);
  });
});
