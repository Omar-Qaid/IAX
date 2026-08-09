import React from 'react';
import { afterEach, describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import i18n from '@core/localization/i18n';

const sampleOptions: LookupOption[] = [
  { id: '1', code: 'US', name: 'United States' },
  { id: '2', code: 'CA', name: 'Canada' },
];

describe('LookupField', () => {
  afterEach(async () => {
    await i18n.changeLanguage('en');
  });

  it('renders input with label and opens lookup dialog on click', () => {
    render(<LookupField name="country" label="Country" options={sampleOptions} />);

    const input = screen.getByLabelText(/Country/i);
    expect(input).toBeDefined();

    fireEvent.click(input);
    expect(screen.getByText('Select Country')).toBeDefined();
    expect(screen.getByText('US - United States')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Close' })).toBeDefined();
  });

  it('localizes the lookup dialog defaults in Arabic', async () => {
    await i18n.changeLanguage('ar');
    render(<LookupField name="country" label="الدولة" options={sampleOptions} />);

    fireEvent.click(screen.getByLabelText('الدولة'));
    expect(screen.getByText('اختر الدولة')).toBeDefined();
    expect(screen.getByPlaceholderText('البحث في الخيارات…')).toBeDefined();
    expect(screen.getByRole('button', { name: 'إغلاق' })).toBeDefined();
  });
});
