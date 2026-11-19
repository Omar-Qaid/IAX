import React from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';

type Record = { id: string; name: string };
const config: EnterpriseListDetailsConfig<Record> = {
  dataSource: { type: 'static', records: [{ id: '1', name: 'Navigation record' }] },
  createRecord: () => ({ id: '', name: '' }),
  getPrimaryText: (record) => record.name,
  getValues: (record) => ({ name: record.name }),
  setValues: (record, values) => ({ ...record, name: String(values.name) }),
  headerFields: [],
  subtitle: 'Document details',
  status: 'Open',
  sections: [
    { id: 'lines', tab: 'lines', title: 'Document lines', content: <Draft /> },
    { id: 'header', tab: 'header', title: 'Document header', content: <span>Header fields</span> },
  ],
};
function Draft() {
  const [value, setValue] = React.useState('Uncommitted line');
  return (
    <input
      aria-label="Line draft"
      value={value}
      onChange={(event) => setValue(event.target.value)}
    />
  );
}

describe('enterprise setup page defaults', () => {
  it.each(['Customer payment terms', 'Tax codes', 'Tax groups', 'Exchange rates'])(
    'renders %s without document tabs and shows every detail section',
    (title) => {
      render(<ListDetailsPage variant="enterprise" title={title} config={config} />);
      expect(screen.getByRole('heading', { name: title })).toBeInTheDocument();
      expect(screen.queryByRole('tab', { name: 'Lines' })).toBeNull();
      expect(screen.queryByRole('tab', { name: 'Header' })).toBeNull();
      expect(screen.getByText('Header fields')).toBeVisible();
      expect(screen.getByRole('textbox', { name: 'Line draft' })).toBeVisible();
      expect(screen.queryByText('Navigation record')).toBeNull();
    }
  );
});
