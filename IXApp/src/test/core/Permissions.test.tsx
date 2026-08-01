import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AuthProvider } from '@core/auth/AuthProvider';
import { PermissionGuard } from '@core/auth/PermissionGuard';

describe('Permission and Security Architecture', () => {
  it('renders children when user possesses required permission', () => {
    render(
      <AuthProvider>
        <PermissionGuard permission="customers:read">
          <div>Authorized Button</div>
        </PermissionGuard>
      </AuthProvider>
    );

    expect(screen.getByText('Authorized Button')).toBeDefined();
  });
});
