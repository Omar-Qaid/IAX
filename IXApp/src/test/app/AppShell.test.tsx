import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { AppShell } from '@app/shell/AppShell';

describe('AppShell', () => {
  it('renders top bar and content area', () => {
    render(
      <MemoryRouter>
        <AppProviders>
          <AppShell>
            <div data-testid="page-content">Dashboard Content</div>
          </AppShell>
        </AppProviders>
      </MemoryRouter>
    );

    expect(screen.getByTestId('page-content')).toBeDefined();
    expect(screen.getByTestId('page-content').textContent).toBe('Dashboard Content');
  });
});
