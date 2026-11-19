import React from 'react';
import { act, renderHook, waitFor } from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { queryClient } from '@core/api/queryClient';
import {
  useMasterDetailDocument,
  type MasterDetailDocumentSource,
  type MasterDetailDocumentRecord,
} from '@patterns/master-detail';
import type { DataGridHandle } from '@shared/components/data-grid/types';

type Line = { id: string; quantity: number };
type Document = MasterDetailDocumentRecord<Line>;
const document: Document = { id: '1', title: 'Document 1', lines: [{ id: 'line-1', quantity: 1 }] };
function Wrapper({ children }: { children: React.ReactNode }) {
  return (
    <AppProviders>
      <MemoryRouter>{children}</MemoryRouter>
    </AppProviders>
  );
}
function source(): MasterDetailDocumentSource<Document> {
  return {
    queryKey: ['master-detail-test'],
    get: vi.fn().mockResolvedValue(document),
    list: vi.fn().mockResolvedValue([document]),
    validate: vi.fn((record: Document) => {
      if (record.lines[0].quantity <= 0) throw new Error('Positive quantity required');
    }),
    save: vi.fn(async (_company: string, record: Document) => record),
  };
}
beforeEach(() => queryClient.clear());

it('retains invalid drafts and includes the latest grid commit in document Save', async () => {
  const api = source();
  const { result } = renderHook(
    () =>
      useMasterDetailDocument<Line, Document>({
        company: 'dat',
        documentId: '1',
        source: api,
        canEdit: true,
      }),
    { wrapper: Wrapper }
  );
  await waitFor(() => expect(result.current.document).toBeDefined());
  act(() => result.current.beginEdit());
  act(() =>
    result.current.update((current) => ({ ...current, lines: [{ id: 'line-1', quantity: -1 }] }))
  );
  await act(async () => {
    await result.current.save();
  });
  expect(result.current.error).toBe('Positive quantity required');
  expect(result.current.editing).toBe(true);
  expect(api.save).not.toHaveBeenCalled();
  result.current.grid.current = {
    saveEdit: vi.fn(async () => {
      result.current.update((current) => ({ ...current, lines: [{ id: 'line-1', quantity: 3 }] }));
      return true;
    }),
    cancelEdit: vi.fn(),
  } as unknown as DataGridHandle;
  await act(async () => {
    await result.current.save();
  });
  expect(api.save).toHaveBeenCalledWith(
    'dat',
    expect.objectContaining({ lines: [{ id: 'line-1', quantity: 3 }] })
  );
  expect(result.current.editing).toBe(false);
  expect(result.current.document?.lines[0].quantity).toBe(3);
});

it('blocks Save and tab changes on a rejected grid edit and cancels the row with the page', async () => {
  const api = source();
  const { result } = renderHook(
    () =>
      useMasterDetailDocument<Line, Document>({
        company: 'dat',
        documentId: '1',
        source: api,
        canEdit: true,
      }),
    { wrapper: Wrapper }
  );
  await waitFor(() => expect(result.current.document).toBeDefined());
  act(() => result.current.beginEdit());
  const cancelEdit = vi.fn();
  result.current.grid.current = {
    saveEdit: vi.fn().mockResolvedValue(false),
    cancelEdit,
  } as unknown as DataGridHandle;
  await act(async () => {
    await result.current.save();
    await result.current.switchTab('header');
  });
  expect(api.save).not.toHaveBeenCalled();
  expect(result.current.editing).toBe(true);
  expect(result.current.tab).toBe('lines');
  act(() => result.current.cancel());
  expect(cancelEdit).toHaveBeenCalledOnce();
  expect(result.current.editing).toBe(false);
});

it('does not enter Edit without permission', async () => {
  const api = source();
  const { result } = renderHook(
    () =>
      useMasterDetailDocument<Line, Document>({
        company: 'dat',
        documentId: '1',
        source: api,
        canEdit: false,
      }),
    { wrapper: Wrapper }
  );
  await waitFor(() => expect(result.current.document).toBeDefined());
  act(() => result.current.beginEdit());
  await act(async () => {
    await result.current.save();
  });
  expect(result.current.editing).toBe(false);
  expect(api.save).not.toHaveBeenCalled();
});

it('locks repeated Save and Cancel while a request is pending and retains API failures', async () => {
  const api = source();
  let rejectSave!: (reason: Error) => void;
  api.save = vi.fn(
    () =>
      new Promise<Document>((_resolve, reject) => {
        rejectSave = reject;
      })
  );
  const { result } = renderHook(
    () =>
      useMasterDetailDocument<Line, Document>({
        company: 'dat',
        documentId: '1',
        source: api,
        canEdit: true,
      }),
    { wrapper: Wrapper }
  );
  await waitFor(() => expect(result.current.document).toBeDefined());
  act(() => result.current.beginEdit());
  let pending!: Promise<void>;
  await act(async () => {
    pending = result.current.save();
    await Promise.resolve();
  });
  expect(result.current.busy).toBe(true);
  act(() => result.current.cancel());
  await act(async () => {
    await result.current.save();
  });
  expect(api.save).toHaveBeenCalledOnce();
  expect(result.current.editing).toBe(true);
  await act(async () => {
    rejectSave(new Error('Save failed'));
    await pending;
  });
  expect(result.current.error).toBe('Save failed');
  expect(result.current.editing).toBe(true);
  expect(result.current.busy).toBe(false);
});
