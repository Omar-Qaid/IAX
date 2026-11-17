import React from 'react';
import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { AppProviders } from '@app/providers/AppProviders';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { DataGridLookupField } from '@shared/components/lookups/DataGridLookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';

const sites: LookupOption[] = [
  { id: 'JED', code: 'JED', name: 'Jeddah' },
  { id: 'RUH', code: 'RUH', name: 'Riyadh' },
  { id: 'DMM', code: 'DMM', name: 'Dammam' },
];

function Harness({
  initial = null,
  options = sites,
  onChange,
}: {
  initial?: string | null;
  options?: LookupOption[];
  onChange?: (value: string | number | null) => void;
}) {
  const [value, setValue] = React.useState<string | number | null>(initial);
  return (
    <AppProviders>
      <DataGridLookupField
        name="site"
        label="Site"
        value={value}
        options={options}
        searchable
        lazyLoading={false}
        onChange={(next) => {
          const selected = Array.isArray(next) ? (next[0] ?? null) : (next ?? null);
          setValue(selected);
          onChange?.(selected);
        }}
      />
    </AppProviders>
  );
}

const input = () => screen.getByRole('combobox', { name: 'Site' }) as HTMLInputElement;
const type = (text: string) => {
  fireEvent.focus(input());
  fireEvent.change(input(), { target: { value: text } });
};

describe('grid lookup cell', () => {
  it('shows the code like the read-only cell', () => {
    render(<Harness initial="JED" />);
    expect(input().value).toBe('JED');
  });

  it('keeps the selected value visible when the list opens on focus or click', async () => {
    render(<Harness initial="JED" />);
    fireEvent.focus(input());
    fireEvent.mouseDown(input());
    fireEvent.keyDown(input(), { key: 'ArrowDown' });
    await screen.findByRole('listbox');
    expect(input().value).toBe('JED');
  });

  it('clears an optional value when its text is deleted and the cell is left', async () => {
    const onChange = vi.fn();
    render(<Harness initial="JED" onChange={onChange} />);
    type('');
    expect(input().value).toBe('');
    fireEvent.blur(input());
    await waitFor(() => expect(onChange).toHaveBeenCalledWith(null));
  });

  it('shows the D365 record preview when the value is focused', async () => {
    render(<Harness initial="JED" />);
    fireEvent.focus(input());
    const card = await screen.findByRole('tooltip');
    expect(card).toHaveTextContent('Site');
    expect(card).toHaveTextContent('JED');
    expect(card).toHaveTextContent('Jeddah');
    expect(input()).toHaveAttribute('aria-describedby', card.id);
  });

  it('hides the preview while typing and on Escape', async () => {
    render(<Harness initial="JED" />);
    fireEvent.focus(input());
    await screen.findByRole('tooltip');
    fireEvent.keyDown(input(), { key: 'Escape' });
    await waitFor(() => expect(screen.queryByRole('tooltip')).toBeNull());

    fireEvent.focus(input());
    await screen.findByRole('tooltip');
    fireEvent.change(input(), { target: { value: 'r' } });
    await waitFor(() => expect(screen.queryByRole('tooltip')).toBeNull());
  });

  it('shows no preview for an empty cell', async () => {
    render(<Harness />);
    fireEvent.focus(input());
    await new Promise((resolve) => setTimeout(resolve, 500));
    expect(screen.queryByRole('tooltip')).toBeNull();
  });

  it('leaves the value room to show in a narrow grid column', () => {
    render(<Harness initial="PCS" />);
    const style = getComputedStyle(input());
    // The form lookup reserves 60px for clear + popup buttons; in a ~100px
    // grid column that hid the value entirely. Cells keep only a small padding.
    expect(style.getPropertyValue('padding-inline-end')).not.toBe('60px');
    expect(style.getPropertyValue('padding-right')).not.toBe('60px');
    expect(input().value).toBe('PCS');
  });

  it('lists code and name side by side', async () => {
    render(<Harness />);
    fireEvent.keyDown(input(), { key: 'ArrowDown' });
    const option = await screen.findByRole('option', { name: /RUH/ });
    expect(option).toHaveTextContent('RUH');
    expect(option).toHaveTextContent('Riyadh');
  });

  it('takes the first match on Enter after typing', async () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);
    type('ridy'.slice(0, 2)); // "ri"
    await screen.findByRole('option', { name: /RUH/ });
    fireEvent.keyDown(input(), { key: 'Enter' });
    expect(onChange).toHaveBeenCalledWith('RUH');
  });

  it('takes the first match on Tab after typing (D365 type-ahead)', async () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);
    type('dam');
    await screen.findByRole('option', { name: /DMM/ });
    fireEvent.keyDown(input(), { key: 'Tab' });
    expect(onChange).toHaveBeenCalledWith('DMM');
  });

  it('never changes the value when tabbing through without typing', () => {
    const onChange = vi.fn();
    render(<Harness initial="JED" onChange={onChange} />);
    fireEvent.focus(input());
    fireEvent.keyDown(input(), { key: 'Tab' });
    expect(onChange).not.toHaveBeenCalled();
    expect(input().value).toBe('JED');
  });

  it('shows a stored code that is not loaded yet instead of a blank cell', () => {
    render(<Harness initial="MED" />);
    expect(input().value).toBe('MED');
  });

  it('renders large client lists in chunks', async () => {
    const many = Array.from({ length: 1000 }, (_, i) => ({
      id: `S${i}`,
      code: `S${String(i).padStart(4, '0')}`,
      name: `Site ${i}`,
    }));
    render(<Harness options={many} />);
    fireEvent.keyDown(input(), { key: 'ArrowDown' });
    const listbox = await screen.findByRole('listbox');
    expect(within(listbox).getAllByRole('option')).toHaveLength(100);
  });
});

describe('grid lookup inside an editing line', () => {
  type Line = { id: string; site: string };
  const columns: ColumnDef<Line>[] = [
    {
      field: 'site',
      headerName: 'Site',
      editable: true,
      renderEditCell: ({ value, onChange }) => (
        <DataGridLookupField
          name="site"
          label="Site"
          value={value as string}
          options={sites}
          lazyLoading={false}
          onChange={(next) => onChange(next)}
        />
      ),
    },
  ];

  it('closes the list on Esc without discarding the line', async () => {
    const ref = React.createRef<DataGridHandle>();
    render(
      <AppProviders>
        <DataGrid<Line>
          ref={ref}
          rows={[{ id: 'a', site: 'JED' }]}
          columns={columns}
          masterForm
          onRowSave={vi.fn()}
          height={300}
          hideToolbar
          hideFilterRow
        />
      </AppProviders>
    );
    act(() => ref.current!.startEditRow('a'));
    const combo = await screen.findByRole('combobox', { name: 'Site' });
    combo.focus();
    fireEvent.keyDown(combo, { key: 'ArrowDown' });
    await screen.findByRole('listbox');
    fireEvent.keyDown(combo, { key: 'Escape' });
    await waitFor(() => expect(screen.queryByRole('listbox')).toBeNull());
    expect(screen.getByRole('combobox', { name: 'Site' })).toBeDefined();
  });
});
