import React from 'react';
import { describe, expect, it } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { EnterpriseListHeader } from '@shared/components/page/EnterpriseListHeader';
import { DocumentHeader } from '@shared/components/page/DocumentHeader';

describe('simple-list title typography', () => {
  it('updates the record title when the selected customer changes', async () => {
    render(
      <SimpleListPage
        title="All customers"
        columns={[{ field: 'name', headerName: 'Name' }]}
        dataSource={{
          type: 'controlled',
          rows: [
            { id: 'C001', name: 'Consulting customer' },
            { id: 'C002', name: 'Retail customer' },
          ],
        }}
        enterpriseConfig={{ getRecordTitle: (record) => `${record.id} | ${record.name}` }}
      />
    );
    expect(
      await screen.findByRole('heading', { name: 'C001 | Consulting customer' })
    ).toBeInTheDocument();
    fireEvent.click(screen.getByText('Retail customer'));
    expect(
      await screen.findByRole('heading', { name: 'C002 | Retail customer' })
    ).toBeInTheDocument();
    expect(screen.getByText('All customers | Standard view')).toBeInTheDocument();
  });

  it('uses the document title typography and blue context/view caption', () => {
    render(
      <>
        <EnterpriseListHeader
          pageTitle="Sales orders"
          contextLabel="Sales orders for CUST-100"
          viewLabel="Standard view"
        />
        <DocumentHeader
          title="SO-001 : Consulting Customer"
          subtitle="Sales order details"
          viewLabel="Standard view"
        />
      </>
    );
    const listTitle = screen.getByRole('heading', { name: 'Sales orders for CUST-100', level: 1 });
    const referenceTitle = screen.getByRole('heading', { name: 'SO-001 : Consulting Customer' });
    const listStyle = getComputedStyle(listTitle);
    const referenceStyle = getComputedStyle(referenceTitle);
    expect(listStyle.fontSize).toBe(referenceStyle.fontSize);
    expect(listStyle.fontWeight).toBe(referenceStyle.fontWeight);
    expect(listStyle.lineHeight).toBe(referenceStyle.lineHeight);
    expect(listTitle).toHaveStyle({ fontSize: '16px', fontWeight: 600 });

    const caption = getComputedStyle(screen.getByText('Sales orders | Standard view'));
    const referenceCaption = getComputedStyle(
      screen.getByText('Sales order details | Standard view')
    );
    expect(caption.fontSize).toBe(referenceCaption.fontSize);
    expect(caption.color).toBe(referenceCaption.color);
  });
});
