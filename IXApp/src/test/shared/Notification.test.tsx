import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { EmptyState } from '@shared/components/feedback/EmptyState';

describe('Feedback components', () => {
  it('renders loading state indicator', () => {
    render(<LoadingState message="Fetching data..." />);
    expect(screen.getByText('Fetching data...')).toBeDefined();
  });

  it('renders empty state message', () => {
    render(<EmptyState title="No records found" message="Try adjusting your filters" />);
    expect(screen.getByText('No records found')).toBeDefined();
  });
});
