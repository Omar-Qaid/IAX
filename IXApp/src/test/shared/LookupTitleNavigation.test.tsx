import React from 'react';
import { describe, expect, it } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { LookupTitleMenu } from '@shared/components/lookups/LookupTitleMenu';

describe('lookup title navigation', () => {
  it('opens the master page only after a double click', () => {
    render(<MemoryRouter initialEntries={['/form']}>
      <Routes>
        <Route path="/form" element={<LookupTitleMenu name="taxGroupId" label="Sales tax group" />} />
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
        <Route path="/form" element={<LookupTitleMenu name="customReference" label="Custom reference" masterRoute="/custom-master" />} />
        <Route path="/custom-master" element={<div>Custom master page</div>} />
      </Routes>
    </MemoryRouter>);

    fireEvent.doubleClick(screen.getByRole('button', { name: 'Custom reference: double-click to view details' }));
    expect(screen.getByText('Custom master page')).toBeInTheDocument();
  });
});
