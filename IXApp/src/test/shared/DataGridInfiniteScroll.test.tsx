import React from 'react';
import { act, fireEvent, render, renderHook, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AppProviders } from '@app/providers/AppProviders';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { DataGridMobileBody } from '@shared/components/data-grid/DataGridMobileBody';
import { ScrollToTopButton } from '@shared/components/data-grid/ScrollToTopButton';
import { useLoadMore } from '@shared/components/data-grid/hooks/useLoadMore';
import { useGridDataSource } from '@shared/components/data-grid/hooks/useGridDataSource';
import type { ColumnDef, DataGridHandle, FetchRowsParams } from '@shared/components/data-grid/types';

type Item = { id: number; name: string; active: boolean };
const columns: ColumnDef<Item>[] = [
  { field: 'name', headerName: 'Name' },
  { field: 'active', headerName: 'Active', type: 'boolean' },
];
const items = (count: number, from = 0): Item[] =>
  Array.from({ length: count }, (_, i) => ({ id: from + i, name: `Item ${from + i}`, active: true }));

/** A scroll container sitting at the bottom of its content. */
const bottomedContainer = () => {
  const el = document.createElement('div');
  Object.defineProperties(el, {
    scrollHeight: { configurable: true, value: 1000 },
    clientHeight: { configurable: true, value: 400 },
    scrollTop: { configurable: true, writable: true, value: 600 },
  });
  return { current: el };
};

afterEach(() => vi.useRealTimers());

describe('useLoadMore', () => {
  type Props = { rowCount: number; loading: boolean; hasMore?: boolean };
  const setup = (loadNextPage: () => void, initial: Props) => {
    const ref = bottomedContainer();
    const hook = renderHook(
      ({ rowCount, loading, hasMore = true }: Props) =>
        useLoadMore({ rowCount, hasMore, loading, rowHeight: 32, loadNextPage, scrollContainerRef: ref }),
      { initialProps: initial }
    );
    return hook;
  };

  it('requests one page at a time and the next one once rows arrive', () => {
    const loadNextPage = vi.fn();
    const { result, rerender } = setup(loadNextPage, { rowCount: 50, loading: false });
    expect(loadNextPage).toHaveBeenCalledTimes(1);

    act(() => result.current.onScroll());
    rerender({ rowCount: 50, loading: true });
    act(() => result.current.onScroll());
    expect(loadNextPage).toHaveBeenCalledTimes(1);

    rerender({ rowCount: 100, loading: false });
    expect(loadNextPage).toHaveBeenCalledTimes(2);
  });

  it('does not retry a failed page in a loop, only on a later user scroll', () => {
    vi.useFakeTimers();
    const loadNextPage = vi.fn();
    const { result, rerender } = setup(loadNextPage, { rowCount: 50, loading: false });
    rerender({ rowCount: 50, loading: true });
    rerender({ rowCount: 50, loading: false }); // failed: no new rows
    rerender({ rowCount: 50, loading: false });
    expect(loadNextPage).toHaveBeenCalledTimes(1);

    act(() => result.current.onScroll()); // inside the back-off
    expect(loadNextPage).toHaveBeenCalledTimes(1);

    vi.advanceTimersByTime(2_500);
    act(() => result.current.onScroll());
    expect(loadNextPage).toHaveBeenCalledTimes(2);
  });

  it('stops when there is nothing more to load', () => {
    const loadNextPage = vi.fn();
    setup(loadNextPage, { rowCount: 50, loading: false, hasMore: false });
    expect(loadNextPage).not.toHaveBeenCalled();
  });
});

describe('useGridDataSource paging', () => {
  it('asks again for a page that failed instead of skipping it', () => {
    const onFetchRows = vi.fn<(params: FetchRowsParams) => void>();
    const { result, rerender } = renderHook(
      ({ loaded }: { loaded: number }) =>
        useGridDataSource({
          enabled: true,
          initialSort: [],
          initialFilters: [],
          pageSize: 50,
          loadedRowCount: loaded,
          onFetchRows,
          onScrollReset: () => undefined,
        }),
      { initialProps: { loaded: 50 } }
    );
    act(() => result.current.loadNextPage());
    act(() => result.current.loadNextPage()); // the first attempt failed: still 50 rows
    expect(onFetchRows.mock.calls.map(([p]) => p.page)).toEqual([1, 1]);

    rerender({ loaded: 100 });
    act(() => result.current.loadNextPage());
    expect(onFetchRows.mock.calls.at(-1)![0].page).toBe(2);
  });
});

describe('mobile cards', () => {
  const renderCards = (props: Partial<React.ComponentProps<typeof DataGridMobileBody<Item>>> = {}) => {
    const scrollContainerRef = { current: document.createElement('div') };
    return render(
      <AppProviders>
        <DataGridMobileBody<Item>
          rows={items(3)}
          columns={columns}
          getRowId={(row) => row.id}
          scrollContainerRef={scrollContainerRef}
          {...props}
        />
      </AppProviders>
    );
  };

  it('renders cards before the list is measured', () => {
    renderCards();
    expect(screen.getByText('Item 0')).toBeDefined();
    expect(screen.getByText('Item 2')).toBeDefined();
  });

  it('shows numeric row IDs as selected when selection arrives as strings', () => {
    renderCards({ columns: [columns[0]], selectionMode: 'multiple', selectedIds: ['1'] });
    const boxes = screen.getAllByRole('checkbox') as HTMLInputElement[];
    expect(boxes.map((box) => box.checked)).toEqual([false, true, false]);
  });

  it('toggles a numeric row off when it was selected as a string', () => {
    const onSelectionChange = vi.fn();
    renderCards({ columns: [columns[0]], selectionMode: 'multiple', selectedIds: ['1'], onSelectionChange });
    fireEvent.click(screen.getAllByRole('checkbox')[1]);
    expect(onSelectionChange).toHaveBeenCalledWith([]);
  });

  it('shows a loading-more indicator while the next page loads', () => {
    renderCards({ loading: true, hasMore: true });
    expect(screen.getByText(/loading/i)).toBeDefined();
  });
});

describe('scroll control', () => {
  it('offers back-to-top after scrolling two viewports and scrolls up', async () => {
    const el = document.createElement('div');
    Object.defineProperty(el, 'clientHeight', { configurable: true, value: 300 });
    el.scrollTo = vi.fn() as unknown as typeof el.scrollTo;
    document.body.appendChild(el);
    render(
      <AppProviders>
        <ScrollToTopButton scrollContainerRef={{ current: el }} />
      </AppProviders>
    );
    expect(screen.queryByRole('button', { name: 'Back to top' })).toBeNull();

    el.scrollTop = 900;
    fireEvent.scroll(el);
    const button = await screen.findByRole('button', { name: 'Back to top' });
    fireEvent.click(button);
    expect(el.scrollTo).toHaveBeenCalledWith(expect.objectContaining({ top: 0 }));
    el.remove();
  });

  it('exposes scrollToRow for loaded records only', async () => {
    const ref = React.createRef<DataGridHandle>();
    render(
      <AppProviders>
        <DataGrid<Item> ref={ref} rows={items(40)} columns={columns} height={300} hideToolbar />
      </AppProviders>
    );
    await waitFor(() => expect(ref.current).not.toBeNull());
    let found: boolean | undefined;
    act(() => {
      found = ref.current!.scrollToRow(30);
    });
    expect(found).toBe(true);
    expect(ref.current!.scrollToRow(999)).toBe(false);
    expect(() => ref.current!.scrollToTop()).not.toThrow();
  });
});
