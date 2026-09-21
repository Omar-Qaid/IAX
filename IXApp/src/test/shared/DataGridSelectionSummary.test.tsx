import React from 'react';
import { afterEach, describe, expect, it } from 'vitest';
import { act, cleanup, render, screen } from '@testing-library/react';
import i18n from '@core/localization/i18n';
import { DataGridSelectionSummary } from '@shared/components/data-grid/DataGridSelectionSummary';

afterEach(async () => {
  cleanup();
  await i18n.changeLanguage('en');
});

describe('grid row count localization', () => {
  it.each([
    [0, 'لا توجد صفوف'],
    [1, 'صف واحد'],
    [2, 'صفان'],
    [3, '3 صفوف'],
    [12, '12 صفاً'],
    [100, '100 صف'],
  ])('renders the Arabic plural for %i rows', async (count, expected) => {
    await i18n.changeLanguage('ar');
    render(
      <DataGridSelectionSummary loadedRows={count} totalRowCount={count} filteredRows={count} />
    );
    expect(screen.getByText(expected)).toBeInTheDocument();
  });

  it('updates a fully loaded server count when the language changes', async () => {
    await i18n.changeLanguage('en');
    render(
      <DataGridSelectionSummary serverSide loadedRows={12} totalRowCount={12} filteredRows={12} />
    );
    expect(screen.getByText('12 ROWS')).toBeInTheDocument();
    await act(() => i18n.changeLanguage('ar'));
    expect(screen.getByText('12 صفاً')).toBeInTheDocument();
  });
});
