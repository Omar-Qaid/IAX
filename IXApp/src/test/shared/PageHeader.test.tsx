import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { PageHeader } from '@shared/components/page/PageHeader';

describe('PageHeader', () => {
  it('renders title and subtitle correctly', () => {
    render(
      <PageHeader title="Customer Groups" subtitle="Manage customer classification" />
    );

    expect(screen.getByText('Customer Groups')).toBeDefined();
    expect(screen.getByText('Manage customer classification')).toBeDefined();
  });
});
