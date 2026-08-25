import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, screen, waitFor } from '@testing-library/react';
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
    expect(screen.getByText('Dashboard image')).toBeInTheDocument();
    expect(screen.getByText('Report company logo image')).toBeInTheDocument();
    expect(screen.getByText('Dashboard company image type')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'View in hierarchy' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Registration IDs' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Registration ID search' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Electronic document properties' })).not.toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: 'Options' })).toHaveLength(1);
    await user.click(screen.getByRole('button', { name: 'Edit' }));
    const name = screen.getByDisplayValue('AlHayat Building Materials Company');
    await user.clear(name);
    await user.type(name, 'AlHayat Contract Test');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => expect(update).toHaveBeenCalledTimes(1));
    expect(update.mock.calls[0]?.[0].name).toBe('AlHayat Contract Test');
    expect(update.mock.calls[0]?.[0].dataArea).toBe('HBMC');
  });

  it('keeps uploaded report images as pending managed attachments until save', async () => {
    const user = userEvent.setup();
    const update = vi.spyOn(legalEntityMockRepository, 'update');
    const { container } = render(<LegalEntityPage />);
    await screen.findByText('Dashboard image');
    await user.click(screen.getByRole('button', { name: 'Edit' }));
    const inputs = container.querySelectorAll<HTMLInputElement>('input[type="file"]');
    expect(inputs).toHaveLength(2);
    fireEvent.change(inputs[1]!, { target: { files: [new File(['image'], 'report.png', { type: 'image/png' })] } });
    await waitFor(() => expect(screen.getByAltText('Report company logo')).toHaveAttribute('src', 'data:image/png;base64,aW1hZ2U='));
    await user.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() => expect(update).toHaveBeenCalledTimes(1));
    expect(update.mock.calls[0]?.[0].reportLogoFile).toMatchObject({
      name: 'report.png',
      type: 'image/png',
    });
  });
});
