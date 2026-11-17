import React from 'react';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AppProviders } from '@app/providers/AppProviders';
import { apiClient } from '@core/api/apiClient';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { LookupDisplayField } from '@shared/components/lookups/LookupDisplayField';
import {
  RecordPreview,
  pagedRecordLoader,
  registerRecordPreview,
} from '@shared/components/record-preview';
import {
  POSTAL_ADDRESS_RECORD_PREVIEW,
  type PostalAddressPreview,
} from '@shared/components/logistics/postalAddressRecordPreview';
import '@app/configuration/recordPreviews';

type Group = { custGroupId: string; name: string; paymTermId: string };
const groups: Record<string, Group> = {
  CONTR: { custGroupId: 'CONTR', name: 'Contractors', paymTermId: 'Net30' },
};
const load = vi.fn(async (key: string) => groups[key] ?? null);

registerRecordPreview<Group>('testGroup', {
  masterRoute: '/test/customer-groups',
  load,
  fields: [
    { labelKey: 'test.group', defaultLabel: 'Customer group', value: 'custGroupId', lookup: true },
    { labelKey: 'test.description', defaultLabel: 'Description', value: 'name' },
    { labelKey: 'test.terms', defaultLabel: 'Terms of payment', value: 'paymTermId', lookup: true },
  ],
});

afterEach(() => {
  load.mockClear();
  vi.restoreAllMocks();
});

const hover = (element: Element) => fireEvent.mouseEnter(element.closest('span') ?? element);

describe('RecordPreview', () => {
  it('renders the value unchanged for unregistered entities or empty keys', () => {
    render(
      <AppProviders>
        <RecordPreview entity="unknown" recordKey="X">
          <span>plain</span>
        </RecordPreview>
        <RecordPreview entity="testGroup" recordKey="">
          <span>empty</span>
        </RecordPreview>
      </AppProviders>
    );
    expect(screen.getByText('plain').parentElement?.tagName).not.toBe('SPAN');
    expect(screen.getByText('empty')).toBeDefined();
  });

  it('loads nothing until hovered, then shows the D365 card', async () => {
    render(
      <AppProviders>
        <RecordPreview entity="testGroup" recordKey="CONTR">
          <span>CONTR</span>
        </RecordPreview>
      </AppProviders>
    );
    expect(load).not.toHaveBeenCalled();
    hover(screen.getByText('CONTR'));
    const card = await screen.findByRole('tooltip');
    expect(load).toHaveBeenCalledWith('CONTR', expect.anything());
    await waitFor(() => expect(card).toHaveTextContent('Contractors'));
    expect(card).toHaveTextContent('Customer group');
    expect(card).toHaveTextContent('Terms of payment');
    expect(card).toHaveTextContent('Net30');
  });

  it('opens on keyboard focus and closes on Escape', async () => {
    render(
      <AppProviders>
        <RecordPreview entity="testGroup" recordKey="CONTR">
          <input aria-label="Group" defaultValue="CONTR" />
        </RecordPreview>
      </AppProviders>
    );
    const field = screen.getByRole('textbox', { name: 'Group' });
    fireEvent.focus(field);
    await screen.findByRole('tooltip');
    expect(field.closest('[aria-describedby]')).not.toBeNull();
    fireEvent.keyDown(field, { key: 'Escape' });
    await waitFor(() => expect(screen.queryByRole('tooltip')).toBeNull());
  });

  it('stays open while the pointer moves onto the card', async () => {
    render(
      <AppProviders>
        <RecordPreview entity="testGroup" recordKey="CONTR">
          <span>CONTR</span>
        </RecordPreview>
      </AppProviders>
    );
    const value = screen.getByText('CONTR');
    hover(value);
    const card = await screen.findByRole('tooltip');
    fireEvent.mouseLeave(value.closest('span')!);
    fireEvent.mouseEnter(card.firstElementChild!);
    await new Promise((resolve) => setTimeout(resolve, 250));
    expect(screen.getByRole('tooltip')).toBeDefined();
    fireEvent.mouseLeave(card.firstElementChild!);
    await waitFor(() => expect(screen.queryByRole('tooltip')).toBeNull());
  });

  it('says so when the record does not exist', async () => {
    render(
      <AppProviders>
        <RecordPreview entity="testGroup" recordKey="MISSING">
          <span>MISSING</span>
        </RecordPreview>
      </AppProviders>
    );
    hover(screen.getByText('MISSING'));
    expect(await screen.findByText('No details found.')).toBeDefined();
  });
});

describe('generic loader', () => {
  it('asks the BaseController /paged endpoint for one record by its key', async () => {
    const get = vi
      .spyOn(apiClient, 'get')
      .mockResolvedValue({ data: { success: true, data: [{ custGroupId: 'CONTR' }] } } as never);
    const record = await pagedRecordLoader('/v1/CustGroup', 'CustGroupId')('CONTR');
    expect(record).toEqual({ custGroupId: 'CONTR' });
    const [url, config] = get.mock.calls[0] as [string, { params: URLSearchParams }];
    expect(url).toBe('/v1/CustGroup/paged');
    expect(config.params.get('PageSize')).toBe('1');
    expect(config.params.get('Filters[0].Field')).toBe('CustGroupId');
    expect(config.params.get('Filters[0].Operator')).toBe('equals');
    expect(config.params.get('Filters[0].Value')).toBe('CONTR');
  });

  it('returns null when nothing matches', async () => {
    vi.spyOn(apiClient, 'get').mockResolvedValue({ data: { success: true, data: [] } } as never);
    expect(await pagedRecordLoader('/v1/CustGroup', 'CustGroupId')('NONE')).toBeNull();
  });
});

describe('where it appears', () => {
  it('previews a grid column declared with preview', async () => {
    type Customer = { id: string; group: string };
    const columns: ColumnDef<Customer>[] = [
      { field: 'group', headerName: 'Customer group', preview: { entity: 'testGroup' } },
    ];
    render(
      <AppProviders>
        <DataGrid<Customer> rows={[{ id: '1', group: 'CONTR' }]} columns={columns} height={200} hideToolbar />
      </AppProviders>
    );
    hover(screen.getByText('CONTR'));
    const card = await screen.findByRole('tooltip');
    await waitFor(() => expect(card).toHaveTextContent('Contractors'));
  });

  it('previews a form lookup by its masterRoute, with no per-field setup', async () => {
    render(
      <AppProviders>
        <LookupDisplayField label="Customer group" value="CONTR" masterRoute="/test/customer-groups" />
      </AppProviders>
    );
    await act(async () => {
      hover(screen.getAllByText('CONTR')[0]);
    });
    const card = await screen.findByRole('tooltip');
    await waitFor(() => expect(card).toHaveTextContent('Contractors'));
  });
});

describe('registered master-data cards (D365: identifying fields only)', () => {
  it('shows an address the page already holds, without loading it', async () => {
    const address: PostalAddressPreview = {
      location: 'Head office',
      validFrom: '2025-01-26T07:41:11Z',
      address: 'Jeddah, Al Hamra 23536',
    };
    const get = vi.spyOn(apiClient, 'get');
    render(
      <AppProviders>
        <RecordPreview entity={POSTAL_ADDRESS_RECORD_PREVIEW} recordKey="5637144576" record={address}>
          <span>Head office</span>
        </RecordPreview>
      </AppProviders>
    );
    hover(screen.getByText('Head office'));
    const card = await screen.findByRole('tooltip');
    expect(card).toHaveTextContent('Location');
    expect(card).toHaveTextContent('Effective');
    expect(card).toHaveTextContent('2025');
    expect(card).toHaveTextContent('Jeddah, Al Hamra 23536');
    expect(get).not.toHaveBeenCalled();
  });

  it('shows only the name for a customer account', async () => {
    vi.spyOn(apiClient, 'get').mockResolvedValue({
      data: {
        success: true,
        data: [{ recId: 1, accountNumber: 'C-0000003', name: 'Abdullah Rashid Al-Qarni Est.', customerGroupId: 'RETAIL', currencyCode: 'SAR' }],
      },
    } as never);
    render(
      <AppProviders>
        <RecordPreview entity="customer" recordKey="C-0000003">
          <span>C-0000003</span>
        </RecordPreview>
      </AppProviders>
    );
    hover(screen.getByText('C-0000003'));
    const card = await screen.findByRole('tooltip');
    await waitFor(() => expect(card).toHaveTextContent('Abdullah Rashid Al-Qarni Est.'));
    expect(card).toHaveTextContent('Name');
    expect(card).not.toHaveTextContent('RETAIL');
    expect(card).not.toHaveTextContent('SAR');
  });

  it('renders an address value unchanged when the page has no address data', () => {
    render(
      <AppProviders>
        <RecordPreview entity={POSTAL_ADDRESS_RECORD_PREVIEW} recordKey="1">
          <span>Plain address</span>
        </RecordPreview>
      </AppProviders>
    );
    expect(screen.getByText('Plain address').closest('[aria-describedby]')).toBeNull();
  });

  it.each([
    ['inventSite', '/v1/InventSite/paged', 'SiteId', { siteId: 'DAM', name: 'Dammam site' }, ['Site', 'DAM', 'Name', 'Dammam site']],
    ['inventLocation', '/v1/InventLocation/paged', 'InventLocationId', { inventLocationId: 'DAM', name: 'Dammam Central Warehouse' }, ['Warehouse', 'Dammam Central Warehouse']],
    ['taxGroup', '/v1/TaxGroup/paged', 'TaxGroup', { taxGroup: 'VAT15%', taxGroupName: 'Value Added TAX 15%' }, ['Sales tax group', 'VAT15%', 'Description', 'Value Added TAX 15%']],
    ['currency', '/v1/Currency/paged', 'CurrencyCode', { currencyCode: 'SAR', txt: 'Saudi riyal' }, ['Currency', 'Saudi riyal']],
    ['paymentTerms', '/v1/PaymTerm/paged', 'PaymTermId', { paymTermId: 'Net30', description: '30 days' }, ['Terms of payment', '30 days']],
    ['customerGroup', '/v1/CustGroup/paged', 'CustGroupId', { custGroupId: 'CONTR', name: 'Contractors' }, ['Customer group', 'Description', 'Contractors']],
  ])('shows the %s card with its key and description only', async (entity, endpoint, keyField, dto, expected) => {
    const get = vi
      .spyOn(apiClient, 'get')
      .mockResolvedValue({ data: { success: true, data: [dto] } } as never);
    const key = String(Object.values(dto)[0]);
    render(
      <AppProviders>
        <RecordPreview entity={entity} recordKey={key}>
          <span>value-{entity}</span>
        </RecordPreview>
      </AppProviders>
    );
    hover(screen.getByText(`value-${entity}`));
    const card = await screen.findByRole('tooltip');
    await waitFor(() => expect(card).toHaveTextContent(String(expected.at(-1))));
    expected.forEach((text) => expect(card).toHaveTextContent(text));
    const [url, config] = get.mock.calls[0] as [string, { params: URLSearchParams }];
    expect(url).toBe(endpoint);
    expect(config.params.get('Filters[0].Field')).toBe(keyField);
    // Exactly two rows (label + value each), like the D365 cards: key and description.
    expect(card.querySelectorAll('p')).toHaveLength(4);
  });
});
