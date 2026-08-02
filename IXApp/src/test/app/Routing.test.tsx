import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AuthProvider } from '@core/auth/AuthProvider';
import { RouteGuard } from '@app/routes/RouteGuard';
import { AppLayout } from '@app/layouts/AppLayout';
import { Route, Routes } from 'react-router-dom';

describe('Routing Architecture', () => {
  it('renders protected child component when authorized', () => {
    render(
      <AuthProvider>
        <MemoryRouter initialEntries={['/dashboard']}>
          <RouteGuard>
            <div>Protected Content</div>
          </RouteGuard>
        </MemoryRouter>
      </AuthProvider>
    );

    expect(screen.getByText('Protected Content')).toBeDefined();
  });

  it('renders nested module pages through the application layout outlet', async () => {
    render(
      <AuthProvider>
        <MemoryRouter initialEntries={['/accounts-receivable/customers']}>
          <Routes>
            <Route element={<AppLayout />}>
              <Route path="/accounts-receivable/customers" element={<div>Customers route content</div>} />
            </Route>
          </Routes>
        </MemoryRouter>
      </AuthProvider>,
    );

    expect(await screen.findByText('Customers route content')).toBeDefined();
  });
});
