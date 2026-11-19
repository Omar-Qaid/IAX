import React from 'react';
import { describe, expect, it } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
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

describe('enterprise sales-order page defaults', () => {
  it('provides the title/status header, Lines/Header tabs and collapsed navigation without page markup', () => {
    render(<ListDetailsPage variant="enterprise" title="Document 001" config={config} />);
    expect(screen.getByRole('heading', { name: 'Document 001' })).toBeInTheDocument();
    expect(screen.getByText('Document details | Standard view')).toBeInTheDocument();
    expect(screen.getByText('Open')).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Lines' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByRole('tab', { name: 'Header' })).toBeInTheDocument();
    expect(screen.queryByText('Navigation record')).toBeNull();
    expect(screen.queryByText('Header fields')).not.toBeVisible();
  });

  it('keeps nested drafts mounted when switching document views', () => {
    render(
      <ListDetailsPage
        variant="enterprise"
        title="Document 001"
        config={{ ...config, initialMode: 'edit' }}
      />
    );
    fireEvent.change(screen.getByRole('textbox', { name: 'Line draft' }), {
      target: { value: 'Changed line' },
    });
    fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
    expect(screen.getByText('Header fields')).toBeVisible();
    expect(screen.queryByRole('textbox', { name: 'Line draft' })).toBeNull();
    fireEvent.click(screen.getByRole('tab', { name: 'Lines' }));
    expect(screen.getByRole('textbox', { name: 'Line draft' })).toHaveValue('Changed line');
  });
});
