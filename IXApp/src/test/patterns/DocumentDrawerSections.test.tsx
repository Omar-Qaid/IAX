import { describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@test/testUtils';
import {
  DocumentCopyDrawer,
  type DocumentCopyLine,
  type DocumentCopySource,
} from '@patterns/document/DocumentCopyDrawer';
import { DocumentTotalsDrawer } from '@patterns/document/DocumentTotalsDrawer';

// Exercise the drawer's selection contract independently of virtualized grid rendering.
vi.mock('@shared/components/data-grid/DataGrid', () => ({
  DataGrid: ({
    rows,
    selectedIds,
    onSelectionChange,
  }: {
    rows: { id: string }[];
    selectedIds: (string | number)[];
    onSelectionChange: (ids: string[]) => void;
  }) => (
    <div>
      {rows.map((row) => (
        <button
          key={row.id}
          aria-pressed={selectedIds.includes(row.id)}
          onClick={() => onSelectionChange([row.id])}
        >
          {row.id}
        </button>
      ))}
    </div>
  ),
}));

describe('Document drawer sections', () => {
  it('preserves source/line selection across collapse and clears lines when the source changes', async () => {
    const copy = vi.fn();
    const sourceChange = vi.fn();
    const close = vi.fn();
    const sources: DocumentCopySource[] = ['S1', 'S2'].map((id) => ({
      id,
      documentNumber: id,
      account: 'C1',
      accountName: 'Customer',
      createdAt: '2026-11-19',
      currencyCode: 'SAR',
      sourceType: 'order',
    }));
    const lines: DocumentCopyLine[] = [
      {
        id: 'L1',
        itemId: 'I1',
        description: 'Item',
        quantity: 2,
        unit: 'ea',
        unitPrice: 10,
        netAmount: 20,
        discount: 0,
        discountPercent: 0,
        site: '',
        warehouse: '',
      },
    ];
    const props = {
      open: true,
      title: 'Copy document',
      sourceSectionLabel: 'Sources',
      additionalSections: ['Other sources'],
      sources,
      lines,
      onClose: close,
      onSourceChange: sourceChange,
      onCopy: copy,
    };
    const { rerender } = render(<DocumentCopyDrawer {...props} />);
    expect(screen.getByRole('button', { name: 'Other sources' })).toHaveAttribute(
      'aria-expanded',
      'false'
    );
    expect(screen.getByRole('button', { name: 'OK' })).toBeDisabled();
    fireEvent.click(screen.getByRole('button', { name: 'S1' }));
    expect(sourceChange).toHaveBeenCalledWith(sources[0]);
    fireEvent.click(screen.getByRole('button', { name: 'L1' }));
    fireEvent.click(screen.getByRole('button', { name: 'Sources' }));
    fireEvent.click(screen.getByRole('button', { name: 'Sources' }));
    expect(screen.getByRole('button', { name: 'L1' })).toHaveAttribute('aria-pressed', 'true');
    fireEvent.click(screen.getByRole('button', { name: 'OK' }));
    expect(copy).toHaveBeenCalledWith(['L1'], {
      quantityFactor: 1,
      invertSign: false,
      recalculatePrice: false,
      copyPrecisely: true,
    });
    fireEvent.click(screen.getByRole('button', { name: 'S2' }));
    await waitFor(() => expect(screen.getByRole('button', { name: 'OK' })).toBeDisabled());
    rerender(<DocumentCopyDrawer {...props} busy />);
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Close' })).toBeDisabled();
    expect(close).not.toHaveBeenCalled();
  });

  it('keeps caller-supplied totals and initial section expansion', () => {
    const close = vi.fn();
    render(
      <DocumentTotalsDrawer
        open
        title="Totals"
        onClose={close}
        sections={[
          {
            id: 'amounts',
            title: 'Amounts',
            columns: [
              {
                id: 'total',
                fields: [{ id: 'net', label: 'Net amount', value: '1,234.50', emphasized: true }],
              },
            ],
          },
          { id: 'details', title: 'More totals', defaultExpanded: false, columns: [] },
        ]}
      />
    );
    expect(screen.getByRole('button', { name: 'Calculation basis' })).toHaveAttribute(
      'aria-expanded',
      'true'
    );
    expect(screen.getByRole('button', { name: 'More totals' })).toHaveAttribute(
      'aria-expanded',
      'false'
    );
    fireEvent.click(screen.getByRole('button', { name: 'Amounts' }));
    fireEvent.click(screen.getByRole('button', { name: 'Amounts' }));
    expect(screen.getByText('1,234.50')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'OK' }));
    expect(close).toHaveBeenCalledOnce();
  });
});
