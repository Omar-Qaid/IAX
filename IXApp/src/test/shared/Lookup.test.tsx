import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';

const sampleOptions: LookupOption[] = [
  { id: '1', code: 'US', name: 'United States' },
  { id: '2', code: 'CA', name: 'Canada' },
];

describe('LookupField', () => {
  it('renders input with label and opens lookup dialog on click', () => {
    render(
      <LookupField
        name="country"
        label="Country"
        options={sampleOptions}
      />
    );

    const input = screen.getByLabelText(/Country/i);
    expect(input).toBeDefined();

    fireEvent.click(input);
    expect(screen.getByText('Select Country')).toBeDefined();
    expect(screen.getByText('US - United States')).toBeDefined();
  });
});
