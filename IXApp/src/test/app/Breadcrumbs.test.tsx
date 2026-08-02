import React from 'react';
import { describe, expect, it } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { render, screen } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { PageBreadcrumbs } from '@shared/components/page/PageBreadcrumbs';

describe('localized route breadcrumbs', () => {
  it('uses route metadata for dynamic sales-order paths', () => {
    render(
      <MemoryRouter initialEntries={['/accounts-receivable/sales-orders/so-101']}>
        <AppProviders><PageBreadcrumbs /></AppProviders>
      </MemoryRouter>,
    );
    expect(screen.getByText('Accounts Receivable')).toBeDefined();
    expect(screen.getByText('Sales orders')).toBeDefined();
    expect(screen.getByText('Sales order')).toBeDefined();
  });
});
