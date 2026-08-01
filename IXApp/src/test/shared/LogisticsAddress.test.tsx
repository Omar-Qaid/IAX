import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LogisticsPostalAddressDrawer } from '@shared/components/logistics/LogisticsPostalAddressDrawer';
import { LogisticsElectronicAddressDrawer } from '@shared/components/logistics/LogisticsElectronicAddressDrawer';

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

describe('Logistics Address Integration', () => {
  it('renders LogisticsPostalAddressDrawer and validates mandatory fields', async () => {
    const handleClose = vi.fn();
    const handleSave = vi.fn();
    const queryClient = createTestQueryClient();

    render(
      <QueryClientProvider client={queryClient}>
        <LogisticsPostalAddressDrawer
          open={true}
          onClose={handleClose}
          onSave={handleSave}
        />
      </QueryClientProvider>
    );

    expect(screen.getByText('New address')).toBeInTheDocument();

    const okButton = screen.getByRole('button', { name: 'OK' });
    fireEvent.click(okButton);

    await waitFor(() => {
      expect(handleSave).not.toHaveBeenCalled();
    });
  });

  it('renders LogisticsElectronicAddressDrawer and handles valid submission', async () => {
    const handleClose = vi.fn();
    const handleSave = vi.fn();
    const queryClient = createTestQueryClient();

    render(
      <QueryClientProvider client={queryClient}>
        <LogisticsElectronicAddressDrawer
          open={true}
          onClose={handleClose}
          onSave={handleSave}
        />
      </QueryClientProvider>
    );

    expect(screen.getByText('Contact information')).toBeInTheDocument();

    const descriptionInput = screen.getByLabelText('Description *');
    const numberInput = screen.getByLabelText('Contact number/address *');

    fireEvent.change(descriptionInput, { target: { value: 'Main Office Phone' } });
    fireEvent.change(numberInput, { target: { value: '+1 (555) 019-2831' } });

    const okButton = screen.getByRole('button', { name: 'OK' });
    fireEvent.click(okButton);

    await waitFor(() => {
      expect(handleSave).toHaveBeenCalledWith(
        expect.objectContaining({
          description: 'Main Office Phone',
          number: '+1 (555) 019-2831',
          type: 'Phone',
        })
      );
    });
  });
});
