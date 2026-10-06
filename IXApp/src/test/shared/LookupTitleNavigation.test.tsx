import React from 'react';
import { describe, expect, it } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { LookupNavigationLabel } from '@shared/components/lookups/LookupNavigationLabel';
import { LookupField } from '@shared/components/lookups/LookupField';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

describe('lookup title navigation', () => {
  it('opens the master page only after a double click', () => {
    render(<MemoryRouter initialEntries={['/form']}>
      <Routes>
        <Route path="/form" element={<LookupNavigationLabel label="Sales tax group" masterRoute="/foundation/tax-groups" />} />
        <Route path="/foundation/tax-groups" element={<div>Tax groups page</div>} />
      </Routes>
    </MemoryRouter>);

    const title = screen.getByRole('button', { name: 'Sales tax group: double-click to view details' });
    fireEvent.click(title);
    expect(screen.queryByText('Tax groups page')).not.toBeInTheDocument();
    fireEvent.doubleClick(title);
    expect(screen.getByText('Tax groups page')).toBeInTheDocument();
  });

  it('uses an explicit destination for a custom lookup', () => {
    render(<MemoryRouter initialEntries={['/form']}>
      <Routes>
        <Route path="/form" element={<LookupNavigationLabel label="Custom reference" masterRoute="/custom-master" />} />
        <Route path="/custom-master" element={<div>Custom master page</div>} />
      </Routes>
    </MemoryRouter>);

    fireEvent.doubleClick(screen.getByRole('button', { name: 'Custom reference: double-click to view details' }));
    expect(screen.getByText('Custom master page')).toBeInTheDocument();
  });

  it('shows a small master icon that navigates on a single click', () => {
    render(<MemoryRouter initialEntries={['/form']}>
      <Routes>
        <Route path="/form" element={<LookupNavigationLabel label="Sales tax group" masterRoute="/foundation/tax-groups" />} />
        <Route path="/foundation/tax-groups" element={<div>Tax groups page</div>} />
      </Routes>
    </MemoryRouter>);

    const icon = screen.getByRole('button', { name: 'Open Sales tax group details' });
    expect(icon).toHaveAttribute('title', 'Open details');
    fireEvent.click(icon);
    expect(screen.getByText('Tax groups page')).toBeInTheDocument();
  });

  it('does not show a navigation icon without a master route', () => {
    render(<MemoryRouter><LookupNavigationLabel label="Sales tax group" /></MemoryRouter>);
    expect(screen.getByText('Sales tax group')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Open Sales tax group details' })).toBeNull();
  });

  it('navigates from a shared lookup label without opening its dropdown', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(<QueryClientProvider client={client}><MemoryRouter initialEntries={['/form']}>
      <Routes>
        <Route path="/form" element={<LookupField name="priorityId" label="Priority" masterRoute="/workflow/priorities" options={[]} />} />
        <Route path="/workflow/priorities" element={<div>Priorities page</div>} />
      </Routes>
    </MemoryRouter></QueryClientProvider>);

    fireEvent.click(screen.getByRole('button', { name: 'Open Priority details' }));
    expect(screen.getByText('Priorities page')).toBeInTheDocument();
    expect(screen.queryByRole('listbox')).toBeNull();
  });

  it('does not infer a route from a shared lookup field name', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(<QueryClientProvider client={client}><MemoryRouter>
      <LookupField name="priorityId" label="Priority" options={[]} />
    </MemoryRouter></QueryClientProvider>);

    expect(screen.queryByRole('button', { name: 'Open Priority details' })).toBeNull();
  });
});
