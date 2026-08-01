import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';

describe('Page Pattern Architecture', () => {
  it('renders ListDetailsPage pattern with title', () => {
    render(
      <AppProviders>
        <ListDetailsPage
          title="Customer Management"
          subtitle="Manage enterprise customers"
          selectedId="1"
          detailsPane={<div>Page Body Content</div>}
          dataGridProps={{
            columns: [{ field: 'code', headerName: 'Customer Code' }],
            rows: [{ id: '1', code: 'CUST-001' }],
          }}
        />
      </AppProviders>
    );

    expect(screen.getByText('Customer Management')).toBeDefined();
    expect(screen.getByText('Customer Code')).toBeDefined();
    expect(screen.getByText('Page Body Content')).toBeDefined();
  });
});
