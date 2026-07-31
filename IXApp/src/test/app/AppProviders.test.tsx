import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';

describe('AppProviders', () => {
  it('renders children correctly within provider tree', () => {
    render(
      <AppProviders>
        <div data-testid="test-child">App Ready</div>
      </AppProviders>
    );
    expect(screen.getByTestId('test-child')).toBeDefined();
    expect(screen.getByTestId('test-child').textContent).toBe('App Ready');
  });
});
