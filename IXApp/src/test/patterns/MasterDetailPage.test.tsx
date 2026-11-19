import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { DocumentFieldLabel } from '@shared/components/fields/DocumentFieldLabel';
import {
  MasterDetailField,
  MasterDetailFieldGroup,
  MasterDetailFieldGroups,
  MasterDetailLines,
  MasterDetailPage,
  MasterDetailSection,
  MasterDetailTabs,
} from '@patterns/master-detail';

const records = [
  { id: '1', title: 'SO-001', subtitle: 'CUST-100 - Consulting' },
  { id: '2', title: 'SO-002', subtitle: 'CUST-200 - Retail' },
];

const renderPage = (props: Partial<React.ComponentProps<typeof MasterDetailPage>> = {}) => {
  const onSelect = vi.fn();
  render(
    <AppProviders>
      <MemoryRouter>
        <MasterDetailPage
          records={records}
          selectedId="1"
          onSelect={onSelect}
          title="SO-001 : Consulting"
          subtitle="Sales order details"
          viewLabel="Standard view"
          status="Backorder"
          filterLabel="Filter"
          emptyLabel="No records"
          actionPane={<button type="button">Edit</button>}
          tabs={
            <MasterDetailTabs
              ariaLabel="Views"
              value="lines"
              onChange={() => undefined}
              tabs={[
                { value: 'lines', label: 'Lines' },
                { value: 'header', label: 'Header' },
              ]}
            />
          }
          {...props}
        >
          <MasterDetailSection title="Sales order header" summary={['CUST-100', 'SAR']}>
            <MasterDetailFieldGroups>
              <MasterDetailFieldGroup title="Warehouse">
                <MasterDetailField
                  label="Site"
                  value="JED"
                  masterRoute="/inventory/sites"
                  previewOption={{ code: 'JED', name: 'Jeddah' }}
                />
                <MasterDetailField label="Release status" value="Open" />
                <MasterDetailField
                  label="Notes"
                  value=""
                  editing
                  editor={<input aria-label="Notes" />}
                />
              </MasterDetailFieldGroup>
            </MasterDetailFieldGroups>
          </MasterDetailSection>
          <MasterDetailSection title="Sales order lines">
            <MasterDetailLines
              toolbarLabel="Line actions"
              toolbar={<button type="button">Add line</button>}
              grid={<div>grid</div>}
            />
          </MasterDetailSection>
        </MasterDetailPage>
      </MemoryRouter>
    </AppProviders>
  );
  return { onSelect };
};

describe('MasterDetailPage', () => {
  it('uses sales-order heading and status typography', () => {
    renderPage();
    expect(screen.getByRole('heading', { name: 'SO-001 : Consulting' })).toHaveStyle({
      'font-size': '16px',
      'font-weight': '600',
      'line-height': '1.4',
    });
    expect(screen.getByText('Backorder')).toHaveStyle({ 'font-size': '12px' });
  });

  it('uses sales-order caption sizing and lookup label spacing', () => {
    render(
      <AppProviders>
        <DocumentFieldLabel>Plain label</DocumentFieldLabel>
        <DocumentFieldLabel lookup>Lookup label</DocumentFieldLabel>
      </AppProviders>
    );
    expect(screen.getByText('Plain label')).toHaveStyle({
      'font-size': '11px',
      'line-height': '1.2',
    });
    expect(screen.getByText('Lookup label')).toHaveStyle({
      'font-size': '10px',
      'margin-bottom': '6px',
    });
  });

  it('keeps the title and view tabs outside the constrained details scroll region', () => {
    renderPage();
    const viewport = document.querySelector('[data-master-detail-viewport]');
    const scrollRegion = document.querySelector('[data-details-scroll-region]');
    const fixedHeader = document.querySelector('[data-details-fixed-header]');
    expect(viewport).toHaveStyle({ display: 'flex', minHeight: '0', overflow: 'hidden' });
    expect(scrollRegion).toHaveStyle({
      overflowY: 'auto',
      minHeight: '0',
      overscrollBehaviorY: 'contain',
    });
    expect(fixedHeader).toHaveStyle({ flexShrink: '0' });
    expect(fixedHeader).toContainElement(screen.getByRole('tablist', { name: 'Views' }));
    expect(scrollRegion).not.toContainElement(
      screen.getByRole('heading', { name: 'SO-001 : Consulting' })
    );
    expect(scrollRegion).toContainElement(screen.getByText('grid'));
  });

  it('renders the D365 details layout: pane, breadcrumb, title, status, tabs and sections', () => {
    renderPage();
    expect(screen.getByRole('heading', { name: 'SO-001 : Consulting' })).toBeInTheDocument();
    expect(screen.getByText('Sales order details | Standard view')).toBeInTheDocument();
    expect(screen.getByText('Backorder')).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Lines' })).toBeInTheDocument();
    expect(screen.getByRole('group', { name: 'Warehouse' })).toBeInTheDocument();
    expect(screen.getByText('CUST-100', { selector: 'span' })).toBeInTheDocument();
    expect(screen.getByRole('toolbar', { name: 'Line actions' })).toBeInTheDocument();
    expect(screen.getByRole('status')).toHaveTextContent('Tab / Enter');
  });

  it('filters and selects records in the navigation pane', () => {
    const { onSelect } = renderPage();
    fireEvent.change(screen.getByPlaceholderText('Filter'), { target: { value: 'retail' } });
    expect(screen.queryByText('SO-001', { selector: 'p' })).toBeNull();
    fireEvent.click(screen.getByText('SO-002'));
    expect(onSelect).toHaveBeenCalledWith('2');
  });

  it('locks the navigation pane to the open record while editing', () => {
    renderPage({ editing: true });
    const other = screen.getByText('SO-002').closest('[role="button"]');
    expect(other).toHaveAttribute('aria-disabled', 'true');
  });

  it('shows master fields with open details, plain fields as text, and editors in edit mode', () => {
    renderPage();
    expect(screen.getByRole('button', { name: 'Open Site details' })).toBeInTheDocument();
    expect(screen.getByText('Open')).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: 'Notes' })).toBeInTheDocument();
  });

  it('shows loading and error states instead of the content', () => {
    const onRetry = vi.fn();
    renderPage({ error: 'Could not load', onRetry });
    expect(screen.getByText('Could not load')).toBeInTheDocument();
    expect(screen.queryByText('Sales order header')).toBeNull();
  });

  it('applies the advanced filter from the right rail', () => {
    renderPage({
      advancedFilter: {
        fields: [{ id: 'customer', label: 'Customer', getValue: (record) => record.subtitle }],
      },
    });
    // The rail's Filter button is the last one named Filter (the first toggles the record list).
    fireEvent.click(screen.getAllByRole('button', { name: /^Filter$/ }).at(-1)!);
    const filterPane = screen.getByRole('heading', { name: 'Filters' }).parentElement!
      .parentElement!;
    const panel = within(filterPane).getAllByRole('textbox').at(-1)!;
    fireEvent.change(panel, { target: { value: 'retail' } });
    fireEvent.click(screen.getByRole('button', { name: 'Apply' }));
    expect(screen.queryByText('SO-001', { selector: 'p' })).toBeNull();
    expect(screen.getByText('SO-002')).toBeInTheDocument();
  });

  it('shows related information for the open record', () => {
    renderPage({
      relatedInformation: {
        title: 'Customer',
        sections: [
          {
            id: 'c',
            label: 'Statistics',
            content: <span>Balance 1,000</span>,
            defaultExpanded: true,
          },
        ],
      },
    });
    fireEvent.click(screen.getByRole('button', { name: /Information/i }));
    expect(screen.getByText('Balance 1,000')).toBeInTheDocument();
  });

  it('renders field-config sections with the shared engine, in view and edit mode', () => {
    const sections = [
      {
        id: 'general',
        title: 'General',
        groups: [
          {
            id: 'g',
            title: 'Delivery',
            fields: [{ name: 'deliveryName', label: 'Delivery name', type: 'text' as const }],
          },
        ],
      },
    ];
    renderPage({ sections, values: { deliveryName: 'Main' } });
    expect(screen.getByRole('group', { name: 'Delivery' })).toBeInTheDocument();
    expect(screen.getByText('Main')).toBeInTheDocument();
  });

  it('has the Options menu and locks utilities while editing', () => {
    renderPage({ recordTableName: 'SalesTable', editing: true });
    expect(screen.getByRole('button', { name: /options/i })).toBeDisabled();
  });
});

it('provides shared document views and Search and locks Search during editing', () => {
  const onChange = vi.fn();
  const onSearch = vi.fn();
  renderPage({ tabs: undefined, viewTabs: { value: 'lines', onChange }, onSearch });
  expect(screen.getByRole('tab', { name: 'Lines' })).toHaveAttribute('aria-selected', 'true');
  fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
  expect(onChange).toHaveBeenCalledWith('header');
  fireEvent.click(screen.getByRole('button', { name: 'Search' }));
  expect(onSearch).toHaveBeenCalledOnce();
});

it('disables the shared document Search command while editing', () => {
  renderPage({ tabs: undefined, editing: true, onSearch: vi.fn() });
  expect(screen.getByRole('button', { name: 'Search' })).toBeDisabled();
});

it('opens and focuses navigation Search for a record form without line filters', async () => {
  renderPage({ tabs: undefined, searchNavigation: true, defaultNavigationOpen: false });
  expect(screen.getByRole('button', { name: 'Toggle record list' })).toHaveAttribute(
    'aria-pressed',
    'false'
  );
  fireEvent.click(screen.getByRole('button', { name: 'Search' }));
  const filter = await screen.findByLabelText('Filter');
  await waitFor(() => expect(filter).toHaveFocus());
  fireEvent.change(filter, { target: { value: 'Retail' } });
  expect(screen.getByText('CUST-200 - Retail')).toBeInTheDocument();
  expect(screen.queryByText('CUST-100 - Consulting')).toBeNull();
});
