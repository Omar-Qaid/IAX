import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AuthProvider } from '@core/auth/AuthProvider';
import { RouteGuard } from '@app/routes/RouteGuard';

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
});
