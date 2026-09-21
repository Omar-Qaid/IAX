import React from 'react';
import { afterEach, describe, expect, it } from 'vitest';
import { cleanup } from '@testing-library/react';
import { act, fireEvent, render, screen } from '@test/testUtils';
import i18n from '@core/localization/i18n';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import { localizedName } from '@shared/utilities/localizedName';

const records = [{ id: '1', name: 'Area Manager', nameAlias: 'مدير المنطقة' }];

function LocalizedRecord() {
  const { isRtl } = useAppTranslation();
  return (
    <ListDetailsPage
      variant="enterprise"
      title="Roles"
      config={{
        dataSource: { type: 'static', records },
        createRecord: () => ({ id: '', name: '', nameAlias: '' }),
        getPrimaryText: (record) => localizedName(record, isRtl),
        getValues: () => ({}),
        setValues: (record) => record,
        headerFields: [{
          id: 'name', label: 'Name',
          getValue: (record) => record.name,
          getDisplayValue: (record) => localizedName(record, isRtl),
          setValue: (record, value) => ({ ...record, name: String(value) }),
        }],
        sections: [],
      }}
    />
  );
}

afterEach(async () => {
  cleanup();
  await i18n.changeLanguage('en');
});

describe('localized record display', () => {
  it('switches display names with language while preserving the original edit value', async () => {
    await i18n.changeLanguage('ar');
    render(<LocalizedRecord />);
    expect(await screen.findAllByText('مدير المنطقة')).not.toHaveLength(0);
    expect(screen.queryByText('Area Manager')).not.toBeInTheDocument();
    await act(() => i18n.changeLanguage('en'));
    expect(screen.getAllByText('Area Manager')).not.toHaveLength(0);
    await act(() => i18n.changeLanguage('ar'));
    fireEvent.click(screen.getByRole('button', { name: 'تعديل' }));
    expect(screen.getByRole('textbox', { name: 'Name' })).toHaveValue('Area Manager');
  });
});
