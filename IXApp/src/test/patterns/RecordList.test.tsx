import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
import { RecordList } from '@patterns/list-details/ListDetailsPage';

const records = Array.from({ length: 12 }, (_, index) => ({
  id: String(index + 1),
  name: `Record ${index + 1}`,
}));

describe('RecordList infinite scrolling', () => {
  it('renders records in batches and loads the next batch near the scroll boundary', () => {
    render(
      <RecordList
        records={records}
        selectedId="1"
        editing={false}
        query=""
        filterVisible={false}
        filterLabel="Filter"
        batchSize={5}
        getPrimaryText={(record) => record.name}
        onQueryChange={vi.fn()}
        onSelect={vi.fn()}
      />
    );

    expect(screen.getByText('Record 5')).toBeDefined();
    expect(screen.queryByText('Record 6')).toBeNull();

    const scrollContainer = screen.getByRole('status', { name: 'Loading more records' })
      .parentElement as HTMLElement;
    Object.defineProperties(scrollContainer, {
      scrollHeight: { configurable: true, value: 500 },
      clientHeight: { configurable: true, value: 300 },
      scrollTop: { configurable: true, value: 100 },
    });
    fireEvent.scroll(scrollContainer);

    expect(screen.getByText('Record 10')).toBeDefined();
    expect(screen.queryByText('Record 11')).toBeNull();
  });
});
