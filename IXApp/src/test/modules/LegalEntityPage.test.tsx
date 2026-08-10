import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { legalEntityMockRepository } from '@modules/organization/adapters/legalEntityMockRepository';
import { LegalEntityPage } from '@modules/organization/pages/LegalEntityPage';

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
});

describe('LegalEntityPage', () => {
  it('loads and updates a legal entity through its module repository', async () => {
    const user = userEvent.setup();
    const update = vi.spyOn(legalEntityMockRepository, 'update');
    render(<LegalEntityPage />);

    expect((await screen.findAllByText('AlHayat Building Materials Company')).length).toBeGreaterThan(0);
    await user.click(screen.getByRole('button', { name: 'Edit' }));
    const name = screen.getByDisplayValue('AlHayat Building Materials Company');
    await user.clear(name);
    await user.type(name, 'AlHayat Contract Test');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => expect(update).toHaveBeenCalledTimes(1));
    expect(update.mock.calls[0]?.[0].name).toBe('AlHayat Contract Test');
    expect(update.mock.calls[0]?.[0].dataArea).toBe('HBMC');
  });
});
