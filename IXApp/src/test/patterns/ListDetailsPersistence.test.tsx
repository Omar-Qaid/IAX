import React from 'react';
import { TextField } from '@mui/material';
import { beforeEach, describe, expect, it } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
import { useListDetailsPage } from '@patterns/list-details/useListDetailsPage';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';

interface TestRecord {
  id: string;
  name: string;
}

const records: TestRecord[] = [
  { id: '1', name: 'Payment request' },
  { id: '2', name: 'Inventory request' },
];

function PersistenceProbe(): React.ReactElement {
  const config = React.useMemo<EnterpriseListDetailsConfig<TestRecord>>(
    () => ({
      filterStorageKey: 'test.requests',
      dataSource: { type: 'static', records },
      createRecord: () => ({ id: '', name: '' }),
      getPrimaryText: (record) => record.name,
      getValues: () => ({}),
      setValues: (record) => record,
      headerFields: [],
      sections: [],
      presentation: { mode: 'list' },
    }),
    []
  );
  const state = useListDetailsPage(config);

  return (
    <>
      <TextField
        label="Search requests"
        value={state.query}
        onChange={(event) => state.setQuery(event.target.value)}
      />
      {state.visibleRecords.map((record) => (
        <span key={record.id}>{record.name}</span>
      ))}
    </>
  );
}

describe('list-details search persistence', () => {
  beforeEach(() => window.localStorage.clear());

  it('restores the last search text after the page remounts', () => {
    const firstRender = render(<PersistenceProbe />);
    fireEvent.change(screen.getByRole('textbox', { name: 'Search requests' }), {
      target: { value: 'payment' },
    });

    expect(window.localStorage.getItem('ixapp.list-details.query.test.requests')).toBe('"payment"');

    firstRender.unmount();
    render(<PersistenceProbe />);

    expect(screen.getByRole('textbox', { name: 'Search requests' })).toHaveValue('payment');
  });
});
