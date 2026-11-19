import React from 'react';
import { describe, expect, it } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { act, render, screen, waitFor, within } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { PageBreadcrumbs } from '@app/navigation/PageBreadcrumbs';
import { MasterDetailPage } from '@patterns/master-detail/MasterDetailPage';
import {
  registerPageBreadcrumb,
  type PageBreadcrumbConfig,
} from '@shared/services/pageBreadcrumbStore';

function Fixture({
  path = '/generic-document/123',
  show = true,
  auto = false,
  breadcrumb = {
    listPath: '/accounts-receivable/free-text-invoices',
    labelKey: 'freeTextInvoices.freeTextInvoice',
  },
}: {
  path?: string;
  show?: boolean;
  auto?: boolean;
  breadcrumb?: PageBreadcrumbConfig | false;
}) {
  return (
    <MemoryRouter initialEntries={[path]}>
      <AppProviders>
        <PageBreadcrumbs />
        {show && (
          <MasterDetailPage
            breadcrumb={auto ? undefined : breadcrumb}
            records={[{ id: '123', title: 'Record 123' }]}
            onSelect={() => undefined}
            title="Record 123"
            subtitle="Journal name"
            filterLabel="Filter"
            emptyLabel="Empty"
            actionPane={null}
          />
        )}
      </AppProviders>
    </MemoryRouter>
  );
}

describe('pattern-owned shell breadcrumbs', () => {
  it('keeps explicit page metadata ahead of inferred metadata and preserves other owners on unmount', async () => {
    const view = render(<Fixture />);
    const nav = screen.getByRole('navigation', { name: 'Breadcrumbs' });
    await within(nav).findByText('Free text invoice');
    let release = () => {};
    try {
      act(() => {
        release = registerPageBreadcrumb({
          pathname: '/generic-document/123',
          listPath: '/accounts-receivable/free-text-invoices',
          label: 'Inferred caption',
          explicit: false,
        });
      });
      expect(within(nav).queryByText('Inferred caption')).toBeNull();
      view.rerender(<Fixture show={false} />);
      await within(nav).findByText('Inferred caption');
    } finally {
      act(() => release());
    }
    await waitFor(() => expect(within(nav).queryByText('Inferred caption')).toBeNull());
  });

  it('builds a hierarchy for a new pattern page without a hardcoded detail route', async () => {
    render(<Fixture />);
    const nav = screen.getByRole('navigation', { name: 'Breadcrumbs' });
    expect(await within(nav).findByText('Free text invoice')).toBeInTheDocument();
    expect(within(nav).getByRole('link', { name: 'Accounts Receivable' })).toBeInTheDocument();
    expect(
      nav.querySelector('a[href="/accounts-receivable/free-text-invoices"]')
    ).toBeInTheDocument();
  });

  it('updates the document label and clears the override when the pattern unmounts', async () => {
    const view = render(<Fixture />);
    const nav = screen.getByRole('navigation', { name: 'Breadcrumbs' });
    await within(nav).findByText('Free text invoice');
    view.rerender(
      <Fixture
        breadcrumb={{
          listPath: '/accounts-receivable/free-text-invoices',
          labelKey: 'freeTextInvoices.newInvoice',
        }}
      />
    );
    await within(nav).findByText('New free text invoice');
    expect(within(nav).queryByText('Free text invoice')).toBeNull();
    view.rerender(<Fixture show={false} />);
    await waitFor(() => expect(within(nav).queryByText('New free text invoice')).toBeNull());
    expect(nav.querySelector('a[href="/accounts-receivable/free-text-invoices"]')).toBeNull();
  });

  it('infers a registered parent list for a newly added master-details page', async () => {
    render(<Fixture path="/inventory/journal-names/123" auto />);
    const nav = screen.getByRole('navigation', { name: 'Breadcrumbs' });
    await within(nav).findByText('Journal name');
    expect(nav.querySelector('a[href="/inventory/journal-names"]')).toBeInTheDocument();
  });
});
